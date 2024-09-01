EMCC ?= emcc
ANDROID_NDK_ROOT ?=
CODESIGN ?= codesign
MACOS_CODESIGN_SIGNATURE ?=

ifeq ($(DEBUG),1)
	CFLAGS += -O0 -g -DDEBUG
else
	CFLAGS += -O3 -DNDEBUG
endif

BUILD_DIRS = \
	lib/windows/x86_64 lib/windows/x86 lib/windows/arm64 \
	lib/linux/x86_64 lib/linux/x86 \
	lib/macos \
	lib/android/arm64 lib/android/arm32 lib/android/x86 lib/android/x86_64 \
	lib/webgl

SQLITE_SRC = sqlite-amalgamation/sqlite3.c
SQLITE_CFLAGS_SRC = tools~/sqlite-cflags.txt
SQLITE_NET_SRC = $(wildcard sqlite-net~/src/*.cs) sqlite-net~/LICENSE.txt
SQLITE_NET_DEST = ../Runtime/sqlite-net
SQLITE_NET_SED_SCRIPT = tools~/fix-library-path.sed

ifneq ($(findstring 4.,$(MAKE_VERSION)),)
	SQLITE_CFLAGS = $(file < $(SQLITE_CFLAGS_SRC))
else
	SQLITE_CFLAGS = $(shell cat $(SQLITE_CFLAGS_SRC))
endif
CFLAGS += $(SQLITE_CFLAGS)

$(BUILD_DIRS):
	mkdir -p $@

.PRECIOUS: lib/%/sqlite3.o~
lib/%/sqlite3.o~: .EXTRA_PREREQS = $(SQLITE_CFLAGS_SRC)
lib/%/sqlite3.o~: $(SQLITE_SRC) | lib/%
	$(CC) -c -o $@ $< $(CFLAGS)

# Windows
lib/windows/%/gilzoide-sqlite-net.dll: LINKFLAGS += -shared
lib/windows/%/gilzoide-sqlite-net.dll: lib/windows/%/sqlite3.o~ | lib/windows/%
	$(CC) -o $@ $^ $(LINKFLAGS)

# Linux
lib/linux/%/libgilzoide-sqlite-net.so: CFLAGS += -fPIC
lib/linux/%/libgilzoide-sqlite-net.so: LINKFLAGS += -shared -lm
lib/linux/%/libgilzoide-sqlite-net.so: lib/linux/%/sqlite3.o~ | lib/linux/%
	$(CC) -o $@ $^ $(LINKFLAGS)

# macOS
lib/macos/libgilzoide-sqlite-net.dylib: CFLAGS += -arch arm64 -arch x86_64 -isysroot $(shell xcrun --show-sdk-path --sdk macosx)
lib/macos/libgilzoide-sqlite-net.dylib: LINKFLAGS += -shared
lib/macos/libgilzoide-sqlite-net.dylib: lib/macos/sqlite3.o~ | lib/macos
	$(CC) -o $@ $^ $(LINKFLAGS)
ifdef MACOS_CODESIGN_SIGNATURE
	$(CODESIGN) -s "$(MACOS_CODESIGN_SIGNATURE)" $@
endif

# iOS/tvOS/visionOS compile flags
sqlite-amalgamation/sqlite3.c.meta: .EXTRA_PREREQS = $(SQLITE_CFLAGS_SRC)
sqlite-amalgamation/sqlite3.c.meta:
	sed -i '' -E -e 's/(\s*CompileFlags:).*/\1 $(SQLITE_CFLAGS)/' $@

# Android
check-ndk-root:
ifndef ANDROID_NDK_ROOT
	$(error ANDROID_NDK_ROOT must be set for Android builds!)
endif

lib/android/%/libgilzoide-sqlite-net.so: CFLAGS += -fPIC
lib/android/%/libgilzoide-sqlite-net.so: LINKFLAGS += -shared -lm
lib/android/%/libgilzoide-sqlite-net.so: lib/android/%/sqlite3.o~ check-ndk-root | lib/android/%
	$(CC) -o $@ $< $(LINKFLAGS)

lib/android/arm64/libgilzoide-sqlite-net.so: CC = $(wildcard $(ANDROID_NDK_ROOT)/toolchains/llvm/prebuilt/*/bin/aarch64-linux-android21-clang)
lib/android/arm32/libgilzoide-sqlite-net.so: CC = $(wildcard $(ANDROID_NDK_ROOT)/toolchains/llvm/prebuilt/*/bin/armv7a-linux-androideabi21-clang)
lib/android/x86_64/libgilzoide-sqlite-net.so: CC = $(wildcard $(ANDROID_NDK_ROOT)/toolchains/llvm/prebuilt/*/bin/x86_64-linux-android21-clang)
lib/android/x86/libgilzoide-sqlite-net.so: CC = $(wildcard $(ANDROID_NDK_ROOT)/toolchains/llvm/prebuilt/*/bin/i686-linux-android21-clang)

# WebGL
lib/webgl/libgilzoide-sqlite-net.bc: .EXTRA_PREREQS = $(SQLITE_CFLAGS_SRC)
lib/webgl/libgilzoide-sqlite-net.bc: $(SQLITE_SRC) | lib/webgl
	$(EMCC) -c -emit-llvm -o $@ $< $(CFLAGS)

# Source
$(SQLITE_NET_DEST)/%.cs: sqlite-net~/src/%.cs $(SQLITE_NET_SED_SCRIPT)
	cat $< | sed -f $(SQLITE_NET_SED_SCRIPT) > $@

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

webgl: lib/webgl/libgilzoide-sqlite-net.bc

source: $(SQLITE_NET_DEST)/License.txt $(SQLITE_NET_DEST)/AssemblyInfo.cs $(SQLITE_NET_DEST)/SQLite.cs $(SQLITE_NET_DEST)/SQLiteAsync.cs

all-android: android-arm64 android-arm32 android-x86_64 android-x86
all-apple: macos-universal sqlite-amalgamation/sqlite3.c.meta
all-linux: linux-x86_64
all-webgl: webgl
all-windows: windows-x86_64 windows-x86
all-windows-mingw: windows-mingw-x86_64 windows-mingw-x86
all-windows-llvm-mingw: windows-mingw-x86_64 windows-mingw-x86 windows-mingw-arm64

# Dockerized builds
docker-all-android:
	docker build -f tools~/Dockerfile.build.android -t gilzoide-sqlite-net-build-android:latest .
	docker run --rm -v "$(CURDIR)":/src -w /src --platform=linux/amd64 gilzoide-sqlite-net-build-android:latest make all-android
docker-all-linux:
	docker build -f tools~/Dockerfile.build.linux -t gilzoide-sqlite-net-build-linux:latest .
	docker run --rm -v "$(CURDIR)":/src -w /src --platform=linux/amd64 gilzoide-sqlite-net-build-linux:latest make all-linux
docker-all-webgl:
	docker build -f tools~/Dockerfile.build.webgl -t gilzoide-sqlite-net-build-webgl:latest .
	docker run --rm -v "$(CURDIR)":/src -w /src gilzoide-sqlite-net-build-webgl:latest make all-webgl
docker-all-windows:
	docker build -f tools~/Dockerfile.build.windows -t gilzoide-sqlite-net-build-windows:latest .
	docker run --rm -v "$(CURDIR)":/src -w /src gilzoide-sqlite-net-build-windows:latest make all-windows-mingw
docker-all-windows-llvm:
	docker run --rm -v "$(CURDIR)":/src -w /src mstorsjo/llvm-mingw:latest make all-windows-llvm-mingw
