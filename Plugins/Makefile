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
	-DSQLITE_DEFAULT_MEMSTATUS=0 \
	-DSQLITE_DEFAULT_WAL_SYNCHRONOUS=1 \
	-DSQLITE_LIKE_DOESNT_MATCH_BLOBS \
	-DSQLITE_MAX_EXPR_DEPTH=0 \
	-DSQLITE_OMIT_DECLTYPE \
	-DSQLITE_OMIT_DEPRECATED \
	-DSQLITE_OMIT_PROGRESS_CALLBACK \
	-DSQLITE_OMIT_SHARED_CACHE \
	-DSQLITE_USE_ALLOCA \
	-DSQLITE_USE_URI

BUILD_DIRS = \
	lib/windows/x86_64 lib/windows/x86 \
	lib/linux/x86_64 lib/linux/x86 \
	lib/macos lib/ios lib/tvos lib/visionos \
	lib/android/arm64 lib/android/arm32 lib/android/x86 lib/android/x86_64 \
	lib/webgl

SQLITE_SRC = sqlite-amalgamation-3440200~/sqlite3.c
MEMORY_METHODS_SRC = unity-malloc~/sqlite-unity-mem-methods.c
SQLITE_NET_SRC = $(wildcard sqlite-net~/src/*.cs) sqlite-net~/LICENSE.txt
SQLITE_NET_DEST = sqlite-net

$(BUILD_DIRS):
	mkdir -p $@

# Windows
lib/windows/%/gilzoide-sqlite-net.dll: LINKFLAGS += -shared
lib/windows/%/gilzoide-sqlite-net.dll: $(SQLITE_SRC) $(MEMORY_METHODS_SRC) | lib/windows/%
	$(CC) -o $@ $^ $(CFLAGS) $(LINKFLAGS)

# Linux
lib/linux/%/gilzoide-sqlite-net.so: CFLAGS += -fPIC
lib/linux/%/gilzoide-sqlite-net.so: LINKFLAGS += -shared
lib/linux/%/gilzoide-sqlite-net.so: $(SQLITE_SRC) $(MEMORY_METHODS_SRC) | lib/linux/%
	$(CC) -o $@ $^ $(CFLAGS) $(LINKFLAGS)

# macOS
lib/macos/gilzoide-sqlite-net.dylib: CFLAGS += -arch arm64 -arch x86_64 -isysroot $(shell xcrun --show-sdk-path --sdk macosx)
lib/macos/gilzoide-sqlite-net.dylib: LINKFLAGS += -shared
lib/macos/gilzoide-sqlite-net.dylib: $(SQLITE_SRC) $(MEMORY_METHODS_SRC) | lib/macos
	$(CC) -o $@ $^ $(CFLAGS) $(LINKFLAGS)

# iOS
lib/ios/gilzoide-sqlite-net.dylib: CFLAGS += -arch arm64 -isysroot $(shell xcrun --show-sdk-path --sdk iphoneos)
lib/ios/gilzoide-sqlite-net.dylib: LINKFLAGS += -shared
lib/ios/gilzoide-sqlite-net.dylib: $(SQLITE_SRC) $(MEMORY_METHODS_SRC) | lib/ios
	$(CC) -o $@ $^ $(CFLAGS) $(LINKFLAGS)

# tvOS
lib/tvos/gilzoide-sqlite-net.dylib: CFLAGS += -arch arm64 -isysroot $(shell xcrun --show-sdk-path --sdk appletvos)
lib/tvos/gilzoide-sqlite-net.dylib: LINKFLAGS += -shared
lib/tvos/gilzoide-sqlite-net.dylib: $(SQLITE_SRC) $(MEMORY_METHODS_SRC) | lib/tvos
	$(CC) -o $@ $^ $(CFLAGS) $(LINKFLAGS)

# visionOS
lib/visionos/gilzoide-sqlite-net.dylib: CFLAGS += -arch arm64 -isysroot $(shell xcrun --show-sdk-path --sdk xros)
lib/visionos/gilzoide-sqlite-net.dylib: LINKFLAGS += -shared
lib/visionos/gilzoide-sqlite-net.dylib: $(SQLITE_SRC) $(MEMORY_METHODS_SRC) | lib/visionos
	$(CC) -o $@ $^ $(CFLAGS) $(LINKFLAGS)


# Android
lib/android/%/gilzoide-sqlite-net.so: CFLAGS += -fPIC
lib/android/%/gilzoide-sqlite-net.so: LINKFLAGS += -shared
lib/android/%/gilzoide-sqlite-net.so: $(SQLITE_SRC) $(MEMORY_METHODS_SRC) | lib/android/%
	$(CC) -o $@ $^ $(CFLAGS) $(LINKFLAGS)

lib/android/arm64/gilzoide-sqlite-net.so: CC = $(wildcard $(ANDROID_NDK_ROOT)/toolchains/llvm/prebuilt/*/bin/aarch64-linux-android21-clang)
lib/android/arm32/gilzoide-sqlite-net.so: CC = $(wildcard $(ANDROID_NDK_ROOT)/toolchains/llvm/prebuilt/*/bin/armv7a-linux-androideabi19-clang)
lib/android/x86_64/gilzoide-sqlite-net.so: CC = $(wildcard $(ANDROID_NDK_ROOT)/toolchains/llvm/prebuilt/*/bin/x86_64-linux-android21-clang)
lib/android/x86/gilzoide-sqlite-net.so: CC = $(wildcard $(ANDROID_NDK_ROOT)/toolchains/llvm/prebuilt/*/bin/i686-linux-android19-clang)

# WebGL
lib/webgl/gilzoide-sqlite-net.bc: $(SQLITE_SRC) | lib/webgl
	$(EMCC) -o $@ $^ $(CFLAGS) -c -emit-llvm

# Source
sqlite-net/%.cs: sqlite-net~/src/%.cs
	cat $< | sed -f fix-library-path.sed~ > $@

sqlite-net/License.txt: sqlite-net~/License.txt
	cp $< $@

# Targets
windows-x86_64: lib/windows/x86_64/gilzoide-sqlite-net.dll
windows-x86: lib/windows/x86/gilzoide-sqlite-net.dll

windows-mingw-x86_64: CC = x86_64-w64-mingw32-gcc
windows-mingw-x86_64: LINKFLAGS += -static-libgcc
windows-mingw-x86_64: lib/windows/x86_64/gilzoide-sqlite-net.dll

windows-mingw-x86: CC = i686-w64-mingw32-gcc
windows-mingw-x86: LINKFLAGS += -static-libgcc
windows-mingw-x86: lib/windows/x86/gilzoide-sqlite-net.dll

linux-x86_64: lib/linux/x86_64/gilzoide-sqlite-net.so

macos-universal: lib/macos/gilzoide-sqlite-net.dylib
ios-universal: lib/ios/gilzoide-sqlite-net.dylib
tvos-universal: lib/tvos/gilzoide-sqlite-net.dylib
visionos-universal: lib/visionos/gilzoide-sqlite-net.dylib

android-arm64: lib/android/arm64/gilzoide-sqlite-net.so
android-arm32: lib/android/arm32/gilzoide-sqlite-net.so
android-x86_64: lib/android/x86_64/gilzoide-sqlite-net.so
android-x86: lib/android/x86/gilzoide-sqlite-net.so

webgl: lib/webgl/gilzoide-sqlite-net.bc

source: sqlite-net/License.txt sqlite-net/AssemblyInfo.cs sqlite-net/SQLite.cs sqlite-net/SQLiteAsync.cs

all-android: android-arm64 android-arm32 android-x86_64 android-x86
all-apple: macos-universal ios-universal tvos-universal visionos-universal
all-linux: linux-x86_64
all-webgl: webgl
all-windows: windows-x86_64 windows-x86
all-windows-mingw: windows-mingw-x86_64 windows-mingw-x86
