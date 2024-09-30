/** 
 * @file SQLiteVfs.hpp
 * Single header with classes for easily implementing SQLite VFS shims in C++11
 *
 * How to use it:
 * ```cpp
 * #include <iostream>
 * #include <SQLiteVfs.hpp>
 *
 * // 1. Implement your File shim, override any methods you want
 * struct MySQLiteFile : SQLiteFileImpl {
 *     int xRead(void *p, int iAmt, sqlite3_int64 iOfst) override {
 *         std::cout << "READING" << std::endl;
 *         return SQLiteFileImpl::xRead(p, iAmt, iOfst);
 *     }
 * }
 *
 * // 2. Implement your VFS shim, override any methods you want
 * struct MySQLiteVfs : SQLiteVfsImpl<MySQLiteFile> {
 *     int xOpen(sqlite3_filename zName, SQLiteFile<TFileImpl> *file, int flags, int *pOutFlags) override {
 *         std::cout << "OPENING FILE " << zName << std::endl;
 *         return SQLiteVfsImpl::xOpen(zName, file, flags, pOutFlags);
 *     }
 * }
 *
 * // 3. Register your VFS
 * static MySQLiteVfs my_vfs("myvfs");
 * int register_my_vfs() {
 *     return my_vfs.register_vfs(false);
 * }
 *
 * // 4. (optional) Unregister your VFS
 * void unregister_my_vfs() {
 *     my_vfs.unregister_vfs();
 * }
 * ```
 */
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
#ifndef __SQLITE_VFS_HPP__
#define __SQLITE_VFS_HPP__

#include <new>

#include "../sqlite-amalgamation/sqlite3.h"

namespace sqlitevfs {
	/**
	 * SQLite File implementation with virtual methods for C++.
	 *
	 * The default method implementations forward execution to `original_file`.
	 *
	 * You should not create objects of this type manually.
	 * Instead, you should subclass it, overriding any of the methods necessary, and pass your subclass to `SQLiteVfsImpl<>`.
	 * 
	 * @note Destructors will be called automatically by `SQLiteFile` right after `xClose` is called.
	 *
	 * @see https://sqlite.org/c3ref/file.html
	 */
	struct SQLiteFileImpl {
		/**
		 * File used by the default method implementations.
		 */
		sqlite3_file *original_file;

		/**
		 * Determine which functions are supported by this implementation.
		 *
		 * The default implementation returns `original_file`'s `iVersion`, or 1 if it is NULL.
		 * Override this to report a different version.
		 * @see https://sqlite.org/c3ref/io_methods.html
		 */
		virtual int iVersion() const {
			return original_file ? original_file->pMethods->iVersion : 1;
		}

		/// @see https://sqlite.org/c3ref/io_methods.html
		virtual int xClose() {
			return original_file->pMethods->xClose(original_file);
		}
		/// @see https://sqlite.org/c3ref/io_methods.html
		virtual int xRead(void *p, int iAmt, sqlite3_int64 iOfst) {
			return original_file->pMethods->xRead(original_file, p, iAmt, iOfst);
		}
		/// @see https://sqlite.org/c3ref/io_methods.html
		virtual int xWrite(const void *p, int iAmt, sqlite3_int64 iOfst) {
			return original_file->pMethods->xWrite(original_file, p, iAmt, iOfst);
		}
		/// @see https://sqlite.org/c3ref/io_methods.html
		virtual int xTruncate(sqlite3_int64 size) {
			return original_file->pMethods->xTruncate(original_file, size);
		}
		/// @see https://sqlite.org/c3ref/io_methods.html
		virtual int xSync(int flags) {
			return original_file->pMethods->xSync(original_file, flags);
		}
		/// @see https://sqlite.org/c3ref/io_methods.html
		virtual int xFileSize(sqlite3_int64 *pSize) {
			return original_file->pMethods->xFileSize(original_file, pSize);
		}
		/// @see https://sqlite.org/c3ref/io_methods.html
		virtual int xLock(int flags) {
			return original_file->pMethods->xLock(original_file, flags);
		}
		/// @see https://sqlite.org/c3ref/io_methods.html
		virtual int xUnlock(int flags) {
			return original_file->pMethods->xUnlock(original_file, flags);
		}
		/// @see https://sqlite.org/c3ref/io_methods.html
		virtual int xCheckReservedLock(int *pResOut) {
			return original_file->pMethods->xCheckReservedLock(original_file, pResOut);
		}
		/// @see https://sqlite.org/c3ref/io_methods.html
		virtual int xFileControl(int op, void *pArg) {
			return original_file->pMethods->xFileControl(original_file, op, pArg);
		}
		/// @see https://sqlite.org/c3ref/io_methods.html
		virtual int xSectorSize() {
			return original_file->pMethods->xSectorSize(original_file);
		}
		/// @see https://sqlite.org/c3ref/io_methods.html
		virtual int xDeviceCharacteristics() {
			return original_file->pMethods->xDeviceCharacteristics(original_file);
		}
		/* Methods above are valid for version 1 */
		/// @see https://sqlite.org/c3ref/io_methods.html
		virtual int xShmMap(int iPg, int pgsz, int flags, void volatile**pp) {
			return original_file->pMethods->xShmMap(original_file, iPg, pgsz, flags, pp);
		}
		/// @see https://sqlite.org/c3ref/io_methods.html
		virtual int xShmLock(int offset, int n, int flags) {
			return original_file->pMethods->xShmLock(original_file, offset, n, flags);
		}
		/// @see https://sqlite.org/c3ref/io_methods.html
		virtual void xShmBarrier() {
			return original_file->pMethods->xShmBarrier(original_file);
		}
		/// @see https://sqlite.org/c3ref/io_methods.html
		virtual int xShmUnmap(int deleteFlag) {
			return original_file->pMethods->xShmUnmap(original_file, deleteFlag);
		}
		/* Methods above are valid for version 2 */
		/// @see https://sqlite.org/c3ref/io_methods.html
		virtual int xFetch(sqlite3_int64 iOfst, int iAmt, void **pp) {
			return original_file->pMethods->xFetch(original_file, iOfst, iAmt, pp);
		}
		/// @see https://sqlite.org/c3ref/io_methods.html
		virtual int xUnfetch(sqlite3_int64 iOfst, void *p) {
			return original_file->pMethods->xUnfetch(original_file, iOfst, p);
		}
		/* Methods above are valid for version 3 */
		/* Additional methods may be added in future releases */
	};

	/**
	 * POD `sqlite3_file` subclass that forwards all invocations to an embedded object that inherits `SQLiteFileImpl`.
	 *
	 * You should not create objects of this type manually nor subclass it.
	 *
	 * @tparam TFileImpl  `SQLiteFileImpl` subclass
	 */
	template<typename TFileImpl>
	struct SQLiteFile : public sqlite3_file {
		using FileImpl = TFileImpl;

		/**
		 * SQLite IO methods populated by `SQLiteFile::setup`.
		 */
		sqlite3_io_methods methods;
		/**
		 * File implementation object of the `SQLiteFileImpl` subclass passed as template parameter to `SQLiteVfsImpl<>`.
		 */
		FileImpl implementation;
		sqlite3_file original_file[0];

		/**
		 * Setup internal state based on the `open_result` flag.
		 *
		 * This function is called automatically by `SQLiteVfs::wrap_xOpen`.
		 * If `open_result` is `SQLITE_OK`, `pMethods` will be populated.
		 * Otherwise, `pMethods` will be set to NULL and SQLite won't call them.
		 *
		 * @param open_result  A SQLite error code. Pass `SQLITE_OK` to fully setup the file object.
		 *                     Pass anything else to set `pMethods` to NULL.
		 */
		void setup(int open_result) {
			if (open_result == SQLITE_OK) {
				implementation.original_file = original_file;
				methods = {
					implementation.iVersion(),
					&wrap_xClose,
					&wrap_xRead,
					&wrap_xWrite,
					&wrap_xTruncate,
					&wrap_xSync,
					&wrap_xFileSize,
					&wrap_xLock,
					&wrap_xUnlock,
					&wrap_xCheckReservedLock,
					&wrap_xFileControl,
					&wrap_xSectorSize,
					&wrap_xDeviceCharacteristics,
					&wrap_xShmMap,
					&wrap_xShmLock,
					&wrap_xShmBarrier,
					&wrap_xShmUnmap,
					&wrap_xFetch,
					&wrap_xUnfetch,
				};
				pMethods = &methods;
			}
			else {
				pMethods = nullptr;
			}
		}
	
	private:
		static int wrap_xClose(sqlite3_file *file) {
			int result = static_cast<SQLiteFile *>(file)->implementation.xClose();
			static_cast<SQLiteFile *>(file)->~SQLiteFile();
			return result;
		}
		static int wrap_xRead(sqlite3_file *file, void *p, int iAmt, sqlite3_int64 iOfst) {
			return static_cast<SQLiteFile *>(file)->implementation.xRead(p, iAmt, iOfst);
		}
		static int wrap_xWrite(sqlite3_file *file, const void *p, int iAmt, sqlite3_int64 iOfst) {
			return static_cast<SQLiteFile *>(file)->implementation.xWrite(p, iAmt, iOfst);
		}
		static int wrap_xTruncate(sqlite3_file *file, sqlite3_int64 size) {
			return static_cast<SQLiteFile *>(file)->implementation.xTruncate(size);
		}
		static int wrap_xSync(sqlite3_file *file, int flags) {
			return static_cast<SQLiteFile *>(file)->implementation.xSync(flags);
		}
		static int wrap_xFileSize(sqlite3_file *file, sqlite3_int64 *pSize) {
			return static_cast<SQLiteFile *>(file)->implementation.xFileSize(pSize);
		}
		static int wrap_xLock(sqlite3_file *file, int flags) {
			return static_cast<SQLiteFile *>(file)->implementation.xLock(flags);
		}
		static int wrap_xUnlock(sqlite3_file *file, int flags) {
			return static_cast<SQLiteFile *>(file)->implementation.xUnlock(flags);
		}
		static int wrap_xCheckReservedLock(sqlite3_file *file, int *pResOut) {
			return static_cast<SQLiteFile *>(file)->implementation.xCheckReservedLock(pResOut);
		}
		static int wrap_xFileControl(sqlite3_file *file, int op, void *pArg) {
			return static_cast<SQLiteFile *>(file)->implementation.xFileControl(op, pArg);
		}
		static int wrap_xSectorSize(sqlite3_file *file) {
			return static_cast<SQLiteFile *>(file)->implementation.xSectorSize();
		}
		static int wrap_xDeviceCharacteristics(sqlite3_file *file) {
			return static_cast<SQLiteFile *>(file)->implementation.xDeviceCharacteristics();
		}
		static int wrap_xShmMap(sqlite3_file *file, int iPg, int pgsz, int flags, void volatile**pp) {
			return static_cast<SQLiteFile *>(file)->implementation.xShmMap(iPg, pgsz, flags, pp);
		}
		static int wrap_xShmLock(sqlite3_file *file, int offset, int n, int flags) {
			return static_cast<SQLiteFile *>(file)->implementation.xShmLock(offset, n, flags);
		}
		static void wrap_xShmBarrier(sqlite3_file *file) {
			return static_cast<SQLiteFile *>(file)->implementation.xShmBarrier();
		}
		static int wrap_xShmUnmap(sqlite3_file *file, int deleteFlag) {
			return static_cast<SQLiteFile *>(file)->implementation.xShmUnmap(deleteFlag);
		}
		static int wrap_xFetch(sqlite3_file *file, sqlite3_int64 iOfst, int iAmt, void **pp) {
			return static_cast<SQLiteFile *>(file)->implementation.xFetch(iOfst, iAmt, pp);
		}
		static int wrap_xUnfetch(sqlite3_file *file, sqlite3_int64 iOfst, void *p) {
			return static_cast<SQLiteFile *>(file)->implementation.xUnfetch(iOfst, p);
		}
	};

	/**
	 * SQLite VFS implementation with virtual methods for C++.
	 *
	 * The default method implementations forward execution to `original_vfs`.
	 *
	 * You should not create objects of this type manually.
	 * Instead, you should subclass it, overriding any of the methods necessary, and pass your subclass to `SQLiteVfs<>`.
	 *
	 * @tparam TFileImpl  `SQLiteFileImpl` subclass
	 * @see https://sqlite.org/c3ref/vfs.html
	 */
	template<typename TFileImpl>
	struct SQLiteVfsImpl {
		using FileImpl = TFileImpl;
		
		/**
		 * VFS used by the default method implementations.
		 */
		sqlite3_vfs *original_vfs;
		
		/**
		 * Open the database.
		 *
		 * `file` is guaranteed to have been constructed using the default constructor.
		 * If you return `SQLITE_OK`, the `file` IO methods will be populated.
		 * Otherwise, IO methods will be set to NULL and `file` will be automatically destroyed.
		 *
		 * @see https://sqlite.org/c3ref/vfs.html
		 */
		virtual int xOpen(sqlite3_filename zName, SQLiteFile<TFileImpl> *file, int flags, int *pOutFlags) {
			return original_vfs->xOpen(original_vfs, zName, file->original_file, flags, pOutFlags);
		}
		/// @see https://sqlite.org/c3ref/vfs.html
		virtual int xDelete(const char *zName, int syncDir) {
			return original_vfs->xDelete(original_vfs, zName, syncDir);
		}
		/// @see https://sqlite.org/c3ref/vfs.html
		virtual int xAccess(const char *zName, int flags, int *pResOut) {
			return original_vfs->xAccess(original_vfs, zName, flags, pResOut);
		}
		/// @see https://sqlite.org/c3ref/vfs.html
		virtual int xFullPathname(const char *zName, int nOut, char *zOut) {
			return original_vfs->xFullPathname(original_vfs, zName, nOut, zOut);
		}
		/// @see https://sqlite.org/c3ref/vfs.html
		virtual void *xDlOpen(const char *zFilename) {
			return original_vfs->xDlOpen(original_vfs, zFilename);
		}
		/// @see https://sqlite.org/c3ref/vfs.html
		virtual void xDlError(int nByte, char *zErrMsg) {
			original_vfs->xDlError(original_vfs, nByte, zErrMsg);
		}
		/// @see https://sqlite.org/c3ref/vfs.html
		virtual void (*xDlSym(void *library, const char *zSymbol))(void) {
			return original_vfs->xDlSym(original_vfs, library, zSymbol);
		}
		/// @see https://sqlite.org/c3ref/vfs.html
		virtual void xDlClose(void *library) {
			return original_vfs->xDlClose(original_vfs, library);
		}
		/// @see https://sqlite.org/c3ref/vfs.html
		virtual int xRandomness(int nByte, char *zOut) {
			return original_vfs->xRandomness(original_vfs, nByte, zOut);
		}
		/// @see https://sqlite.org/c3ref/vfs.html
		virtual int xSleep(int microseconds) {
			return original_vfs->xSleep(original_vfs, microseconds);
		}
		/// @see https://sqlite.org/c3ref/vfs.html
		virtual int xCurrentTime(double *pResOut) {
			return original_vfs->xCurrentTime(original_vfs, pResOut);
		}
		/// @see https://sqlite.org/c3ref/vfs.html
		virtual int xGetLastError(int nByte, char *zOut) {
			return original_vfs->xGetLastError(original_vfs, nByte, zOut);
		}
		/*
		** The methods above are in version 1 of the sqlite_vfs object
		** definition.  Those that follow are added in version 2 or later
		*/
		/// @see https://sqlite.org/c3ref/vfs.html
		virtual int xCurrentTimeInt64(sqlite3_int64 *pResOut) {
			return original_vfs->xCurrentTimeInt64(original_vfs, pResOut);
		}
		/*
		** The methods above are in versions 1 and 2 of the sqlite_vfs object.
		** Those below are for version 3 and greater.
		*/
		/// @see https://sqlite.org/c3ref/vfs.html
		virtual int xSetSystemCall(const char *zName, sqlite3_syscall_ptr ptr) {
			return original_vfs->xSetSystemCall(original_vfs, zName, ptr);
		}
		/// @see https://sqlite.org/c3ref/vfs.html
		virtual sqlite3_syscall_ptr xGetSystemCall(const char *zName) {
			return original_vfs->xGetSystemCall(original_vfs, zName);
		}
		/// @see https://sqlite.org/c3ref/vfs.html
		virtual const char *xNextSystemCall(const char *zName) {
			return original_vfs->xNextSystemCall(original_vfs, zName);
		}
		/*
		** The methods above are in versions 1 through 3 of the sqlite_vfs object.
		** New fields may be appended in future versions.  The iVersion
		** value will increment whenever this happens.
		*/
	};

	/**
	 * POD `sqlite3_vfs` subclass that forwards all invocations to an embedded object that inherits `SQLiteVfsImpl`.
	 *
	 * You should not subclass this type.
	 * Pass your `SQLiteVfsImpl` subclass as template argument instead.
	 *
	 * @tparam TVfsImpl  `SQLiteVfsImpl` subclass
	 */
	template<typename TVfsImpl>
	struct SQLiteVfs : public sqlite3_vfs {
		using VfsImpl = TVfsImpl;
		using FileImpl = typename VfsImpl::FileImpl;

		/**
		 * VFS implementation object of the `SQLiteVfsImpl` subclass passed as template parameter to `SQLiteVfs<>`.
		 */
		VfsImpl implementation;

		/**
		 * Construct a named VFS with the default VFS as base.
		 *
		 * @param name  VFS name.
		 * @see SQLiteVfs(const char *, sqlite3_vfs *)
		 */
		SQLiteVfs(const char *name)
			: SQLiteVfs(name, (sqlite3_vfs *) nullptr)
		{	
		}

		/**
		 * Construct a named VFS with the VFS named `base_vfs_name` as base.
		 *
		 * @param name  VFS name.
		 * @param base_vfs_name  Base VFS name, used to find the base VFS using `sqlite3_vfs_find`.
		 * @see SQLiteVfs(const char *, sqlite3_vfs *)
		 */
		SQLiteVfs(const char *name, const char *base_vfs_name)
			: SQLiteVfs(name, sqlite3_vfs_find(base_vfs_name))
		{
		}

		/**
		 * Construct a named VFS with `original_vfs` as base VFS.
		 *
		 * The `original_vfs` will be forwarded to the `implementation`.
		 *
		 * @warning If a VFS is registered with a name that is NULL or an empty string, then the behavior is undefined.
		 * @param name  VFS name.
		 * @param original_vfs  Base VFS. If NULL, the default VFS will be used instead.
		 * @see SQLiteVfs(const char *, sqlite3_vfs *)
		 */
		SQLiteVfs(const char *name, sqlite3_vfs *original_vfs)
			: SQLiteVfs()
		{
			if (original_vfs == nullptr) {
				original_vfs = sqlite3_vfs_find(nullptr);
			}
			implementation.original_vfs = original_vfs;

			iVersion = original_vfs->iVersion;
			szOsFile = (int) sizeof(SQLiteFile<FileImpl>) + original_vfs->szOsFile;
			mxPathname = original_vfs->mxPathname;
			zName = name;
		}

		/**
		 * Unregisters the VFS, just to be sure.
		 * @see unregister_vfs
		 */
		~SQLiteVfs() {
			unregister_vfs();
		}

		/**
		 * Register the VFS in SQLite using `sqlite3_vfs_register`.
		 *
		 * The same VFS can be registered multiple times without injury.
		 * To make an existing VFS into the default VFS, register it again with `makeDefault` flag set.
		 *
		 * @param makeDefault  Whether the VFS will be the new default VFS.
		 * @see https://sqlite.org/c3ref/vfs_find.html
		 */
		int register_vfs(bool makeDefault) {
			return sqlite3_vfs_register(this, makeDefault);
		}
		
		/**
		 * Unregister the VFS in SQLite using `sqlite3_vfs_unregister`.
		 *
		 * If the default VFS is unregistered, another VFS is chosen as the default arbitrarily.
		 * @see https://sqlite.org/c3ref/vfs_find.html
		 */
		int unregister_vfs() {
			return sqlite3_vfs_unregister(this);
		}

		/**
		 * Whether this VFS is registered in SQLite, checked using `sqlite3_vfs_find`.
		 *
		 * @see https://sqlite.org/c3ref/vfs_find.html
		 */
		bool is_registered() const {
			return sqlite3_vfs_find(zName) == this;
		}

	private:
		SQLiteVfs()
			: implementation()
		{
			pNext = nullptr;
			pAppData = nullptr;
			xOpen = &wrap_xOpen;
			xDelete = &wrap_xDelete;
			xAccess = &wrap_xAccess;
			xFullPathname = &wrap_xFullPathname;
			xDlOpen = &wrap_xDlOpen;
			xDlError = &wrap_xDlError;
			xDlSym = &wrap_xDlSym;
			xDlClose = &wrap_xDlClose;
			xRandomness = &wrap_xRandomness;
			xSleep = &wrap_xSleep;
			xCurrentTime = &wrap_xCurrentTime;
			xGetLastError = &wrap_xGetLastError;
			xCurrentTimeInt64 = &wrap_xCurrentTimeInt64;
			xSetSystemCall = &wrap_xSetSystemCall;
			xGetSystemCall = &wrap_xGetSystemCall;
			xNextSystemCall = &wrap_xNextSystemCall;
		}
		
		static int wrap_xOpen(sqlite3_vfs *vfs, sqlite3_filename zName, sqlite3_file *raw_file, int flags, int *pOutFlags) {
			auto file = static_cast<SQLiteFile<FileImpl> *>(raw_file);
			new (file) SQLiteFile<FileImpl>();
			int result = static_cast<SQLiteVfs *>(vfs)->implementation.xOpen(zName, file, flags, pOutFlags);
			file->setup(result);
			if (result != SQLITE_OK) {
				file->~SQLiteFile<FileImpl>();
			}
			return result;
		}
		static int wrap_xDelete(sqlite3_vfs *vfs, const char *zName, int syncDir) {
			return static_cast<SQLiteVfs *>(vfs)->implementation.xDelete(zName, syncDir);
		}
		static int wrap_xAccess(sqlite3_vfs *vfs, const char *zName, int flags, int *pResOut) {
			return static_cast<SQLiteVfs *>(vfs)->implementation.xAccess(zName, flags, pResOut);
		}
		static int wrap_xFullPathname(sqlite3_vfs *vfs, const char *zName, int nOut, char *zOut) {
			return static_cast<SQLiteVfs *>(vfs)->implementation.xFullPathname(zName, nOut, zOut);
		}
		static void *wrap_xDlOpen(sqlite3_vfs *vfs, const char *zFilename) {
			return static_cast<SQLiteVfs *>(vfs)->implementation.xDlOpen(zFilename);
		}
		static void wrap_xDlError(sqlite3_vfs *vfs, int nByte, char *zErrMsg) {
			static_cast<SQLiteVfs *>(vfs)->implementation.xDlError(nByte, zErrMsg);
		}
		static void (*wrap_xDlSym(sqlite3_vfs *vfs, void *library, const char *zSymbol))(void) {
			return static_cast<SQLiteVfs *>(vfs)->implementation.xDlSym(library, zSymbol);
		}
		static void wrap_xDlClose(sqlite3_vfs *vfs, void *library) {
			return static_cast<SQLiteVfs *>(vfs)->implementation.xDlClose(library);
		}
		static int wrap_xRandomness(sqlite3_vfs *vfs, int nByte, char *zOut) {
			return static_cast<SQLiteVfs *>(vfs)->implementation.xRandomness(nByte, zOut);
		}
		static int wrap_xSleep(sqlite3_vfs *vfs, int microseconds) {
			return static_cast<SQLiteVfs *>(vfs)->implementation.xSleep(microseconds);
		}
		static int wrap_xCurrentTime(sqlite3_vfs *vfs, double *pResOut) {
			return static_cast<SQLiteVfs *>(vfs)->implementation.xCurrentTime(pResOut);
		}
		static int wrap_xGetLastError(sqlite3_vfs *vfs, int nByte, char *zOut) {
			return static_cast<SQLiteVfs *>(vfs)->implementation.xGetLastError(nByte, zOut);
		}
		static int wrap_xCurrentTimeInt64(sqlite3_vfs *vfs, sqlite3_int64 *pResOut) {
			return static_cast<SQLiteVfs *>(vfs)->implementation.xCurrentTimeInt64(pResOut);
		}
		static int wrap_xSetSystemCall(sqlite3_vfs *vfs, const char *zName, sqlite3_syscall_ptr ptr) {
			return static_cast<SQLiteVfs *>(vfs)->implementation.xSetSystemCall(zName, ptr);
		}
		static sqlite3_syscall_ptr wrap_xGetSystemCall(sqlite3_vfs *vfs, const char *zName) {
			return static_cast<SQLiteVfs *>(vfs)->implementation.xGetSystemCall(zName);
		}
		static const char *wrap_xNextSystemCall(sqlite3_vfs *vfs, const char *zName) {
			return static_cast<SQLiteVfs *>(vfs)->implementation.xNextSystemCall(zName);
		}
	};
}

#endif  // __SQLITE_VFS_HPP__
