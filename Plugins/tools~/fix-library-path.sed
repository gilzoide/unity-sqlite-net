/const string LibraryPath/ {
	# Make LibraryPath public
	s/const string/public const string/
	# Rename native library "sqlite3" -> "gilzoide-sqlite-net"
	s/sqlite3/gilzoide-sqlite-net/
	# Use "__Internal" library path in WebGL builds
	i\
#if !UNITY_EDITOR && (UNITY_WEBGL || UNITY_IOS || UNITY_TVOS || UNITY_VISIONOS)
	i\
		public const string LibraryPath = "__Internal";
	i\
#else
	a\
#endif
}

# Fix constrain `sqlite3_win32*` functions to Windows only
# This avoids breaking IL2CPP builds with managed stripping level set to Minimal
/sqlite3_win32_/ {
	i\
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
	n
	a\
#endif
}
	

# Make Quote function public, for libraries making raw queries
s/static string Quote/public static string Quote/

# Make SQLite3 class partial, to extend in another file
s/class SQLite3/partial class SQLite3/