// Compile-time definitions used to build SQLite
// This is a separate file so that IL2CPP can use the same flags on all platforms
// Feel free to change this based on your needs
#define SQLITE_USE_URI 1
#define SQLITE_DQS 0 
#define SQLITE_DEFAULT_MEMSTATUS 0 
#define SQLITE_DEFAULT_WAL_SYNCHRONOUS 1 
#define SQLITE_LIKE_DOESNT_MATCH_BLOBS 1
#define SQLITE_MAX_EXPR_DEPTH 0 
#define SQLITE_OMIT_DECLTYPE 1
#define SQLITE_OMIT_DEPRECATED 1
#define SQLITE_OMIT_PROGRESS_CALLBACK 1
#define SQLITE_OMIT_SHARED_CACHE 1
#define SQLITE_USE_ALLOCA 1
#define SQLITE_ENABLE_RTREE 1
#define SQLITE_ENABLE_MATH_FUNCTIONS 1
#define HAVE_ISNAN 1
#define SQLITE_ENABLE_GEOPOLY 1
#define SQLITE_ENABLE_FTS5 1
#define SQLITE_ENABLE_HIDDEN_COLUMNS 1
// Default temporary storage to in-memory, since TEMP databases are not encrypted
#define SQLITE_TEMP_STORE 2
