# Android setup
ANDROID_NDK_ROOT ?=
# macOS code signing setup
CODESIGN ?= codesign
MACOS_CODESIGN_SIGNATURE ?=
# Download GitHub Action releases
GITHUB_CLI_BIN ?= gh
GITHUB_REPO ?= gilzoide/unity-sqlite-net
RUN_ID ?=

ifeq ($(DEBUG),1)
	CFLAGS += -O0 -g -DDEBUG
else
	CFLAGS += -O3 -DNDEBUG
endif

BUILD_DIRS = \
	lib/windows/x86_64 lib/windows/x86 lib/windows/arm64 \
	lib/linux/x86_64 lib/linux/x86 \
	lib/macos \
	lib/android/arm64 lib/android/arm32 lib/android/x86 lib/android/x86_64

SQLITE_SRC = sqlite-amalgamation/sqlite3.c
SQLITE_NET_SRC = $(wildcard sqlite-net~/src/*.cs) sqlite-net~/LICENSE.txt
SQLITE_NET_DEST = ../Runtime/sqlite-net
SQLITE_NET_SED_SCRIPT = tools~/fix-library-path.sed

$(BUILD_DIRS):
	mkdir -p $@

.PRECIOUS: lib/%/sqlite3.o~
lib/%/sqlite3.o~: $(SQLITE_SRC) | lib/%
	$(CC) -c -o $@ $< $(CFLAGS)

# Windows
lib/windows/%/gilzoide-sqlite-net.dll: LINKFLAGS += -shared
lib/windows/%/gilzoide-sqlite-net.dll: lib/windows/%/sqlite3.o~ | lib/windows/%
	$(CC) -o $@ $^ $(LINKFLAGS)

# Linux
lib/linux/%/libgilzoide-sqlite-net.so: CFLAGS += -fPIC

# Note: The "-Wl,-Bsymbolic" flags prevent the dynamic linker from interposing the plugin’s calls to its 
# own exported symbols with identically-named symbols in other loaded libraries (like /usr/lib/libsqlite3.so.0)
# which can cause a Unity Editor crash otherwise.
lib/linux/%/libgilzoide-sqlite-net.so: LINKFLAGS += -shared -lm -Wl,-Bsymbolic

lib/linux/%/libgilzoide-sqlite-net.so: lib/linux/%/sqlite3.o~ | lib/linux/%
	$(CC) -o $@ $^ $(LINKFLAGS)

# macOS
lib/macos/libgilzoide-sqlite-net.dylib: CFLAGS += -arch arm64 -arch x86_64 -isysroot $(shell xcrun --show-sdk-path --sdk macosx)
lib/macos/libgilzoide-sqlite-net.dylib: LINKFLAGS += -shared -arch arm64 -arch x86_64
lib/macos/libgilzoide-sqlite-net.dylib: lib/macos/sqlite3.o~ | lib/macos
	$(CC) -o $@ $^ $(LINKFLAGS)
ifdef MACOS_CODESIGN_SIGNATURE
	$(CODESIGN) -s "$(MACOS_CODESIGN_SIGNATURE)" $@
endif

# Android
check-ndk-root:
ifndef ANDROID_NDK_ROOT
	$(error ANDROID_NDK_ROOT must be set for Android builds!)
endif

lib/android/%/libgilzoide-sqlite-net.so: CFLAGS += -fPIC
lib/android/%/libgilzoide-sqlite-net.so: LINKFLAGS += -shared -lm -Wl,-z,max-page-size=16384
lib/android/%/libgilzoide-sqlite-net.so: lib/android/%/sqlite3.o~ check-ndk-root | lib/android/%
	$(CC) -o $@ $< $(LINKFLAGS)

lib/android/arm64/libgilzoide-sqlite-net.so: CC = $(wildcard $(ANDROID_NDK_ROOT)/toolchains/llvm/prebuilt/*/bin/aarch64-linux-android21-clang)
lib/android/arm32/libgilzoide-sqlite-net.so: CC = $(wildcard $(ANDROID_NDK_ROOT)/toolchains/llvm/prebuilt/*/bin/armv7a-linux-androideabi21-clang)
lib/android/x86_64/libgilzoide-sqlite-net.so: CC = $(wildcard $(ANDROID_NDK_ROOT)/toolchains/llvm/prebuilt/*/bin/x86_64-linux-android21-clang)
lib/android/x86/libgilzoide-sqlite-net.so: CC = $(wildcard $(ANDROID_NDK_ROOT)/toolchains/llvm/prebuilt/*/bin/i686-linux-android21-clang)


# Source
$(SQLITE_NET_DEST)/%.cs: sqlite-net~/src/%.cs $(SQLITE_NET_SED_SCRIPT)
	cat $< | sed -E -f $(SQLITE_NET_SED_SCRIPT) > $@

$(SQLITE_NET_DEST)/License.txt: sqlite-net~/License.txt
	cp $< $@

# Targets
windows-x86_64: lib/windows/x86_64/gilzoide-sqlite-net.dll
windows-x86: lib/windows/x86/gilzoide-sqlite-net.dll
windows-arm64: lib/windows/arm64/gilzoide-sqlite-net.dll

windows-mingw-x86_64: CC = x86_64-w64-mingw32-gcc
windows-mingw-x86_64: LINKFLAGS += -static-libgcc
windows-mingw-x86_64: lib/windows/x86_64/gilzoide-sqlite-net.dll

windows-mingw-x86: CC = i686-w64-mingw32-gcc
windows-mingw-x86: LINKFLAGS += -static-libgcc
windows-mingw-x86: lib/windows/x86/gilzoide-sqlite-net.dll

windows-mingw-arm64: CC = aarch64-w64-mingw32-gcc
windows-mingw-arm64: LINKFLAGS += -static-libgcc
windows-mingw-arm64: lib/windows/arm64/gilzoide-sqlite-net.dll

linux-x86_64: lib/linux/x86_64/libgilzoide-sqlite-net.so

macos-universal: lib/macos/libgilzoide-sqlite-net.dylib

android-arm64: lib/android/arm64/libgilzoide-sqlite-net.so
android-arm32: lib/android/arm32/libgilzoide-sqlite-net.so
android-x86_64: lib/android/x86_64/libgilzoide-sqlite-net.so
android-x86: lib/android/x86/libgilzoide-sqlite-net.so

source: $(SQLITE_NET_DEST)/License.txt $(SQLITE_NET_DEST)/AssemblyInfo.cs $(SQLITE_NET_DEST)/SQLite.cs $(SQLITE_NET_DEST)/SQLiteAsync.cs

all-android: android-arm64 android-arm32 android-x86_64 android-x86
all-apple: macos-universal
all-linux: linux-x86_64
all-windows: windows-x86_64 windows-x86
all-windows-mingw: windows-mingw-x86_64 windows-mingw-x86
all-windows-llvm-mingw: windows-mingw-x86_64 windows-mingw-x86 windows-mingw-arm64

# Dockerized builds
docker-all-android:
	docker build -f tools~/Dockerfile.build.android --platform=linux/amd64 -t gilzoide-sqlite-net-build-android:latest $(DOCKER_BUILD_ARGS) .
	docker run --rm -v "$(CURDIR)":/src -w /src --platform=linux/amd64 gilzoide-sqlite-net-build-android:latest make all-android
docker-all-linux:
	docker build -f tools~/Dockerfile.build.linux --platform=linux/amd64 -t gilzoide-sqlite-net-build-linux:latest $(DOCKER_BUILD_ARGS) .
	docker run --rm -v "$(CURDIR)":/src -w /src --platform=linux/amd64 gilzoide-sqlite-net-build-linux:latest make all-linux
docker-all-windows:
	docker build -f tools~/Dockerfile.build.windows -t gilzoide-sqlite-net-build-windows:latest $(DOCKER_BUILD_ARGS) .
	docker run --rm -v "$(CURDIR)":/src -w /src gilzoide-sqlite-net-build-windows:latest make all-windows-mingw
docker-all-windows-llvm:
	docker run --rm -v "$(CURDIR)":/src -w /src mstorsjo/llvm-mingw:latest make all-windows-llvm-mingw

# Download builds from GitHub Actions
download-latest-build:
	$(eval TMPDIR := $(shell mktemp -d))
	$(GITHUB_CLI_BIN) run download $(RUN_ID) --repo $(GITHUB_REPO) -p gilzoide-sqlite-net-* -D $(TMPDIR)
	rsync -r $(TMPDIR)/gilzoide-sqlite-net-windows/* lib/windows/
	rsync -r $(TMPDIR)/gilzoide-sqlite-net-linux/* lib/linux/
	rsync -r $(TMPDIR)/gilzoide-sqlite-net-android/* lib/android/
	rsync -r $(TMPDIR)/gilzoide-sqlite-net-macos/* lib/macos/
	$(RM) -r $(TMPDIR)
