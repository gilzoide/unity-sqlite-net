/*
 * This is free and unencumbered software released into the public domain.
 *
 * Anyone is free to copy, modify, publish, use, compile, sell, or
 * distribute this software, either in source code form or as a compiled
 * binary, for any purpose, commercial or non-commercial, and by any
 * means.
 *
 * In jurisdictions that recognize copyright laws, the author or authors
 * of this software dedicate any and all copyright interest in the
 * software to the public domain. We make this dedication for the benefit
 * of the public at large and to the detriment of our heirs and
 * successors. We intend this dedication to be an overt act of
 * relinquishment in perpetuity of all present and future rights to this
 * software under copyright law.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
 * OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * For more information, please refer to <http://unlicense.org/>
 */
#include <cstdarg>
#include <cstdio>
#include <cstring>
#include <string>
#include <sys/stat.h>
#include <unistd.h>
#include <vector>

#include "SQLiteVfs.h"

#include "idbvfs.h"

/// Used size for Indexed DB "disk sectors"
#ifndef DISK_SECTOR_SIZE
	#define DISK_SECTOR_SIZE 32
#endif

/// Indexed DB key used to store idbvfs file sizes
#define IDBVFS_SIZE_KEY "file_size"


#ifdef __EMSCRIPTEN__
#include <emscripten.h>
#define INLINE_JS(...) EM_ASM(__VA_ARGS__)
#else
#define INLINE_JS(...)
#endif


#ifdef TRACE
static void TRACE_LOG(const char *fmt, ...) {
	va_list args;
	va_start(args, fmt);
	static char _idbvfs_trace_log_buffer[1024];
	vsnprintf(_idbvfs_trace_log_buffer, sizeof(_idbvfs_trace_log_buffer), fmt, args);
#ifdef __EMSCRIPTEN__
	EM_ASM({ console.log(UTF8ToString($0)) }, _idbvfs_trace_log_buffer);
#else
	printf("%s\n", _idbvfs_trace_log_buffer);
#endif
	va_end(args);
}
#else
	#define TRACE_LOG(...)
#endif

using namespace sqlitevfs;

class IdbPage {
public:
	IdbPage() {}

	IdbPage(const char *dbname, const char *subfilename)
		: dbname(dbname)
		, filename(dbname)
	{
		filename.append("/");
		filename.append(subfilename);
	}

	IdbPage(const char *dbname, int page_number)
		: IdbPage(dbname, std::to_string(page_number).c_str())
	{
	}

	bool exists() const {
		if (FILE *f = fopen(filename.c_str(), "r")) {
			fclose(f);
			return true;
		}
		else {
			return false;
		}
	}

	int load_into(void *data, size_t data_size, sqlite3_int64 offset_in_page = 0) const {
		if (FILE *f = fopen(filename.c_str(), "r")) {
			if (offset_in_page > 0) {
				fseek(f, offset_in_page, SEEK_SET);
			}
			size_t read_bytes = fread(data, 1, data_size, f);
			fclose(f);
			return read_bytes;
		}
		else {
			return 0;
		}
	}

	int load_into(std::vector<uint8_t>& out_buffer, size_t data_size) const {
		out_buffer.resize(data_size);
		return load_into(out_buffer.data(), data_size);
	}

	int scan_into(const char *fmt, ...) const {
		if (FILE *f = fopen(filename.c_str(), "r")) {
			va_list args;
			va_start(args, fmt);
			int assigned_items = vfscanf(f, fmt, args);
			va_end(args);
			fclose(f);
			return assigned_items;
		}
		else {
			return 0;
		}
	}

	int store(const void *data, size_t data_size) const {
		mkdir(dbname, 0777);

		if (FILE *f = fopen(filename.c_str(), "w")) {
			size_t written_bytes = fwrite(data, 1, data_size, f);
			fclose(f);
			return written_bytes;
		}
		else {
			return 0;
		}
	}

	int store(const std::vector<uint8_t>& data) const {
		return store(data.data(), data.size());
	}

	int store(const std::string& data) const {
		return store(data.c_str(), data.size());
	}

	bool remove() const {
		return unlink(filename.c_str()) == 0;
	}

private:
	const char *dbname;
	std::string filename;
};

struct IdbFileSize : public IdbPage {
	IdbFileSize() : IdbPage() {}
	IdbFileSize(sqlite3_filename file_name, bool autoload = true) : IdbPage(file_name, IDBVFS_SIZE_KEY) {
		if (autoload) {
			load();
		}
	}

	void load() {
		scan_into("%lu", &file_size);
		is_dirty = false;
	}

	size_t get() const {
		return file_size;
	}

	void set(size_t new_file_size) {
		if (new_file_size != file_size) {
			file_size = new_file_size;
			is_dirty = true;
		}
	}

	void update_if_greater(size_t new_file_size) {
		if (new_file_size > file_size) {
			set(new_file_size);
		}
	}

	bool sync() const {
		if (is_dirty) {
			return store(std::to_string(file_size)) > 0;
		}
		else {
			return true;
		}
	}

private:
	size_t file_size = 0;
	bool is_dirty = false;
};

struct IdbFile : public SQLiteFileImpl {
	sqlite3_filename file_name;
	IdbFileSize file_size;
	std::vector<uint8_t> journal_data;
	bool is_db;

	IdbFile() {}
	IdbFile(sqlite3_filename file_name, bool is_db) : file_name(file_name), file_size(file_name), is_db(is_db) {}

	int iVersion() const override {
		return 1;
	}

	int xClose() override {
		return SQLITE_OK;
	}

	int xRead(void *p, int iAmt, sqlite3_int64 iOfst) override {
		TRACE_LOG("READ %s %d @ %ld", file_name, iAmt, iOfst);
		if (iAmt + iOfst > file_size.get()) {
			TRACE_LOG("  > %d", false);
			return SQLITE_IOERR_SHORT_READ;
		}

		int result;
		if (is_db) {
			result = readDb(p, iAmt, iOfst);
		}
		else {
			result = readJournal(p, iAmt, iOfst);
		}
		TRACE_LOG("  > %d", result);
		return result;
	}

	int xWrite(const void *p, int iAmt, sqlite3_int64 iOfst) override {
		TRACE_LOG("WRITE %s %d @ %ld", file_name, iAmt, iOfst);
		int result;
		if (is_db) {
			result = writeDb(p, iAmt, iOfst);
		}
		else {
			result = writeJournal(p, iAmt, iOfst);
		}
		TRACE_LOG("  > %d", result);
		return result;
	}

	int xTruncate(sqlite3_int64 size) override {
		TRACE_LOG("TRUNCATE %s to %ld", file_name, size);
		file_size.set(size);
		TRACE_LOG("  > %d", true);
		return SQLITE_OK;
	}

	int xSync(int flags) override {
		TRACE_LOG("SYNC %s %d", file_name, flags);
		// journal data is stored in-memory and synced all at once
		if (!journal_data.empty()) {
			IdbPage file(file_name, 0);
			file.store(journal_data);
			file_size.set(journal_data.size());
		}
		bool success = file_size.sync();
		INLINE_JS({
			Module.idbvfsSyncfs();
		});
		TRACE_LOG("  > %d", success);
		return success ? SQLITE_OK : SQLITE_IOERR_FSYNC;
	}

	int xFileSize(sqlite3_int64 *pSize) override {
		TRACE_LOG("FILE SIZE %s", file_name);
		if (!journal_data.empty()) {
			*pSize = journal_data.size();
		}
		else {
			*pSize = file_size.get();
		}
		TRACE_LOG("  > %d", *pSize);
		return SQLITE_OK;
	}

	int xLock(int flags) override {
		return SQLITE_OK;
	}

	int xUnlock(int flags) override {
		return SQLITE_OK;
	}

	int xCheckReservedLock(int *pResOut) override {
		*pResOut = 0;
		return SQLITE_OK;
	}

	int xFileControl(int op, void *pArg) override {
		switch (op) {
			case SQLITE_FCNTL_VFSNAME:
				*(char **) pArg = sqlite3_mprintf("%z", IDBVFS_NAME);
				return SQLITE_OK;
		}
		return SQLITE_NOTFOUND;
	}

	int xSectorSize() override {
		return DISK_SECTOR_SIZE;
	}

	int xDeviceCharacteristics() override {
		return 0;
	}

private:
	int readDb(void *p, int iAmt, sqlite3_int64 iOfst) {
		int page_number;
		sqlite3_int64 offset_in_page;
		if (iOfst + iAmt >= 512) {
			if (iOfst % iAmt != 0) {
				return SQLITE_IOERR_READ;
			}
			page_number = iOfst / iAmt;
			offset_in_page = 0;
		} else {
			page_number = 0;
			offset_in_page = iOfst;
		}

		IdbPage page(file_name, page_number);
		int loaded_bytes = page.load_into((uint8_t*) p, iAmt, offset_in_page);
		if (loaded_bytes < iAmt) {
			return SQLITE_IOERR_SHORT_READ;
		}
		else {
			return SQLITE_OK;
		}
	}

	int readJournal(void *p, int iAmt, sqlite3_int64 iOfst) {
		if (journal_data.empty()) {
			size_t journal_size = file_size.get();
			if (journal_size > 0) {
				IdbPage page(file_name, 0);
				page.load_into(journal_data, journal_size);
			}
		}
		if (iAmt + iOfst > journal_data.size()) {
			return SQLITE_IOERR_SHORT_READ;
		}
		memcpy(p, journal_data.data() + iOfst, iAmt);
		return SQLITE_OK;
	}

	int writeDb(const void *p, int iAmt, sqlite3_int64 iOfst) {
		int page_number = iOfst ? iOfst / iAmt : 0;

		IdbPage page(file_name, page_number);
		int stored_bytes = page.store(p, iAmt);
		if (stored_bytes < iAmt) {
			return SQLITE_IOERR_WRITE;
		}

		file_size.update_if_greater(iAmt + iOfst);
		return SQLITE_OK;
	}

	int writeJournal(const void *p, int iAmt, sqlite3_int64 iOfst) {
		if (iAmt + iOfst > journal_data.size()) {
			journal_data.resize(iAmt + iOfst);
		}
		memcpy(journal_data.data() + iOfst, p, iAmt);
		return SQLITE_OK;
	}
};

struct IdbVfs : public SQLiteVfsImpl<IdbFile> {
	int xOpen(sqlite3_filename zName, SQLiteFile<IdbFile> *file, int flags, int *pOutFlags) override {
		TRACE_LOG("OPEN %s", zName);
		bool is_db = (flags & SQLITE_OPEN_MAIN_DB) || (flags & SQLITE_OPEN_TEMP_DB);
		file->implementation = IdbFile(zName, is_db);
		return SQLITE_OK;
	}

	int xDelete(const char *zName, int syncDir) override {
		TRACE_LOG("DELETE %s", zName);
		IdbFileSize file_size(zName, false);
		if (!file_size.remove()) {
			return SQLITE_IOERR_DELETE;
		}

		for (int i = 0; ; i++) {
			IdbPage page(zName, i);
			if (!page.remove()) {
				break;
			}
		}
		rmdir(zName);
		return SQLITE_OK;
	}

	int xAccess(const char *zName, int flags, int *pResOut) override {
		TRACE_LOG("ACCESS %s %d", zName, flags);
		switch (flags) {
			case SQLITE_ACCESS_EXISTS:
			case SQLITE_ACCESS_READWRITE:
			case SQLITE_ACCESS_READ:
				IdbFileSize file_size(zName, false);
				*pResOut = file_size.exists();
				TRACE_LOG("  > %d", *pResOut);
				return SQLITE_OK;
		}
		return SQLITE_NOTFOUND;
	}

#ifdef __EMSCRIPTEN__
	int xFullPathname(const char *zName, int nOut, char *zOut) override {
		TRACE_LOG("FULL PATH %s", zName);
		if (zName[0] == '/') {
			strncpy(zOut, zName, nOut);
		}
		else {
			snprintf(zOut, nOut, "/idbfs/%s", zName);
		}
		TRACE_LOG(" > %s", zOut);
		return SQLITE_OK;
	}
#endif
};

extern "C" {
	const char *IDBVFS_NAME = "idbvfs";

	int idbvfs_register(int makeDefault) {
		static SQLiteVfs<IdbVfs> idbvfs(IDBVFS_NAME);
		INLINE_JS({
			if (!Module.idbvfsSyncfs) {
				// Run FS.syncfs in a queue, to avoid concurrent execution errors
				var syncQueue = 0;
				function doSync() {
					FS.syncfs(false, function() {
						syncQueue--;
						if (syncQueue > 0) {
							doSync();
						}
					});
				}
				Module.idbvfsSyncfs = function() {
					syncQueue++;
					if (syncQueue == 1) {
						doSync();
					}
				};
			}
		});
		return idbvfs.register_vfs(makeDefault);
	}
}
