EMCC ?= emcc
ANDROID_NDK_ROOT ?=

ifeq ($(DEBUG),1)
	CFLAGS += -O0 -g -DDEBUG
else
	CFLAGS += -O3 -DNDEBUG
endif

# SQLite recommended defines + USE_URI. Reference: https://www.sqlite.org/compile.html
CFLAGS += \
	-DSQLITE_DQS=0 \
	-DSQLITE_THREADSAFE=0 \
	-DSQLITE_DEFAULT_MEMSTATUS=0 \
	-DSQLITE_DEFAULT_WAL_SYNCHRONOUS=1 \
	-DSQLITE_LIKE_DOESNT_MATCH_BLOBS \
	-DSQLITE_MAX_EXPR_DEPTH=0 \
	-DSQLITE_OMIT_DECLTYPE \
	-DSQLITE_OMIT_DEPRECATED \
	-DSQLITE_OMIT_PROGRESS_CALLBACK \
	-DSQLITE_OMIT_SHARED_CACHE \
	-DSQLITE_USE_ALLOCA \
	-DSQLITE_OMIT_AUTOINIT \
	-DSQLITE_USE_URI

BUILD_DIRS = \
	build/windows/x86_64 build/windows/x86 \
	build/linux/x86_64 build/linux/x86 \
	build/macos build/ios build/tvos build/visionos \
	build/android/arm64 build/android/arm32 build/android/x86 build/android/x86_64 \
	build/webgl

SQLITE_SRC = sqlite-amalgamation-3440200~/sqlite3.c
SQLITE_NET_SRC = $(wildcard sqlite-net~/src/*.cs) sqlite-net~/LICENSE.txt
SQLITE_NET_DEST = sqlite-net

$(BUILD_DIRS):
	mkdir -p $@

# Windows
build/windows/%/sqlite3.dll: LINKFLAGS += -shared
build/windows/%/sqlite3.dll: $(SQLITE_SRC) | build/windows/%
	$(CC) -o $@ $< $(CFLAGS) $(LINKFLAGS)

# Linux
build/linux/%/libsqlite3.so: CFLAGS += -fPIC
build/linux/%/libsqlite3.so: LINKFLAGS += -shared
build/linux/%/libsqlite3.so: $(SQLITE_SRC) | build/linux/%
	$(CC) -o $@ $< $(CFLAGS) $(LINKFLAGS)

# macOS
build/macos/libsqlite3.dylib: CFLAGS += -arch arm64 -arch x86_64 -isysroot $(shell xcrun --show-sdk-path --sdk macosx)
build/macos/libsqlite3.dylib: LINKFLAGS += -shared
build/macos/libsqlite3.dylib: $(SQLITE_SRC) | build/macos
	$(CC) -o $@ $< $(CFLAGS) $(LINKFLAGS)

# iOS
build/ios/libsqlite3.dylib: CFLAGS += -arch arm64 -isysroot $(shell xcrun --show-sdk-path --sdk iphoneos)
build/ios/libsqlite3.dylib: LINKFLAGS += -shared
build/ios/libsqlite3.dylib: $(SQLITE_SRC) | build/ios
	$(CC) -o $@ $< $(CFLAGS) $(LINKFLAGS)

# tvOS
build/tvos/libsqlite3.dylib: CFLAGS += -arch arm64 -isysroot $(shell xcrun --show-sdk-path --sdk appletvos)
build/tvos/libsqlite3.dylib: LINKFLAGS += -shared
build/tvos/libsqlite3.dylib: $(SQLITE_SRC) | build/tvos
	$(CC) -o $@ $< $(CFLAGS) $(LINKFLAGS)

# visionOS
build/visionos/libsqlite3.dylib: CFLAGS += -arch arm64 -isysroot $(shell xcrun --show-sdk-path --sdk xros)
build/visionos/libsqlite3.dylib: LINKFLAGS += -shared
build/visionos/libsqlite3.dylib: $(SQLITE_SRC) | build/visionos
	$(CC) -o $@ $< $(CFLAGS) $(LINKFLAGS)


# Android
build/android/%/libsqlite3.so: CFLAGS += -fPIC
build/android/%/libsqlite3.so: LINKFLAGS += -shared
build/android/%/libsqlite3.so: $(SQLITE_SRC) | build/android/%
	$(CC) -o $@ $< $(CFLAGS) $(LINKFLAGS)

build/android/arm64/libsqlite3.so: CC = $(wildcard $(ANDROID_NDK_ROOT)/toolchains/llvm/prebuilt/*/bin/aarch64-linux-android21-clang)
build/android/arm32/libsqlite3.so: CC = $(wildcard $(ANDROID_NDK_ROOT)/toolchains/llvm/prebuilt/*/bin/armv7a-linux-androideabi19-clang)
build/android/x86_64/libsqlite3.so: CC = $(wildcard $(ANDROID_NDK_ROOT)/toolchains/llvm/prebuilt/*/bin/x86_64-linux-android21-clang)
build/android/x86/libsqlite3.so: CC = $(wildcard $(ANDROID_NDK_ROOT)/toolchains/llvm/prebuilt/*/bin/i686-linux-android19-clang)

# WebGL
build/webgl/libsqlite3.bc: $(SQLITE_SRC) src~/sqlite3.c | build/webgl
	$(EMCC) -o $@ $< $(CFLAGS) -c -emit-llvm

# Targets
windows-x86_64: build/windows/x86_64/sqlite3.dll
windows-x86: build/windows/x86/sqlite3.dll

windows-mingw-x86_64: CC = x86_64-w64-mingw32-gcc
windows-mingw-x86_64: LINKFLAGS += -static-libgcc
windows-mingw-x86_64: build/windows/x86_64/sqlite3.dll

windows-mingw-x86: CC = i686-w64-mingw32-gcc
windows-mingw-x86: LINKFLAGS += -static-libgcc
windows-mingw-x86: build/windows/x86/sqlite3.dll

linux-x86_64: build/linux/x86_64/libsqlite3

macos-universal: build/macos/libsqlite3.dylib
ios-universal: build/ios/libsqlite3.dylib
tvos-universal: build/tvos/libsqlite3.dylib
visionos-universal: build/visionos/libsqlite3.dylib

android-arm64: build/android/arm64/libsqlite3.so
android-arm32: build/android/arm32/libsqlite3.so
android-x86_64: build/android/x86_64/libsqlite3.so
android-x86: build/android/x86/libsqlite3.so

webgl: build/webgl/libsqlite3.bc

source: $(SQLITE_NET_SRC)
	cp $^ $(SQLITE_NET_DEST)

all-android: android-arm64 android-arm32 android-x86_64 android-x86
all-apple: macos-universal ios-universal tvos-universal visionos-universal
all-linux: linux-x86_64
all-webgl: webgl
all-windows: windows-x86_64 windows-x86
all-windows-mingw: windows-mingw-x86_64 windows-mingw-x86