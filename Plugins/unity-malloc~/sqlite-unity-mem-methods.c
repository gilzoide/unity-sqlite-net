#include <stdlib.h>

#include "../sqlite-amalgamation-3440200~/sqlite3.h"
#include "IUnityInterface.h"
#include "IUnityLog.h"
#include "IUnityMemoryManager.h"

static IUnityLog *logger = NULL;
static IUnityMemoryManager *memory_manager = NULL;
static UnityAllocator *allocator = NULL;

#if DEBUG
	#include <stdio.h>
	static char _msg[1024];
	#define DEBUG_LOG(format, ...) \
		if (logger != NULL) { sprintf(_msg, format, ##__VA_ARGS__); UNITY_LOG(logger, _msg); }
#else
	#define DEBUG_LOG(...)
#endif

void xLogError(void *userdata, int error_code, const char *message) {
	UNITY_LOG_ERROR(logger, message);
}

#define ALIGNMENT \
	(8)

#define ROUND8(x)     (((x)+7)&~7)

#define GET_BASE_PTR(ptr) \
	(((int64_t *) ptr) - 1)

static void *xMalloc(int size) {
	int64_t *allocation = (int64_t *) memory_manager->Allocate(allocator, sizeof(int64_t) + size, ALIGNMENT, __FILE__, __LINE__);
	allocation[0] = size;
	return allocation + 1;
}

static void xFree(void *ptr) {
	memory_manager->Deallocate(allocator, GET_BASE_PTR(ptr), __FILE__, __LINE__);
}

static void *xRealloc(void *ptr, int new_size) {
	int64_t *allocation = (int64_t *) memory_manager->Reallocate(allocator, GET_BASE_PTR(ptr), sizeof(int64_t) + new_size, ALIGNMENT, __FILE__, __LINE__);
	if (allocation == NULL) {
		return NULL;
	}
	else {
		allocation[0] = new_size;
		return allocation + 1;
	}
}

static int xSize(void *ptr) {
	return *GET_BASE_PTR(ptr);
}

static int xRoundup(int value) {
	return ROUND8(value);
}

static int xInit(void *_) {
	return SQLITE_OK;
}

static void xShutdown(void *_) {
}

static sqlite3_mem_methods mem_methods = {
	&xMalloc,
	&xFree,
	&xRealloc,
	&xSize,
	&xRoundup,
	&xInit,
	&xShutdown,
	NULL,
};

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces * unityInterfaces) {
	logger = UNITY_GET_INTERFACE(unityInterfaces, IUnityLog);
	if (logger != NULL) {
		int rc = sqlite3_config(SQLITE_CONFIG_LOG, &xLogError, NULL);
		if (rc != SQLITE_OK) {
			DEBUG_LOG("[SQLite-net] SQLITE_CONFIG_LOG error: %d", rc);
		}
		else {
			DEBUG_LOG("[SQLite-net] SQLITE_CONFIG_LOG initialized");
		}
	}

	memory_manager = UNITY_GET_INTERFACE(unityInterfaces, IUnityMemoryManager);
	if (memory_manager != NULL) {
		allocator = memory_manager->CreateAllocator("SQLite-net", "SQLite Memory Allocator");
		int rc = sqlite3_config(SQLITE_CONFIG_MALLOC, &mem_methods);
		if (rc != SQLITE_OK) {
			DEBUG_LOG("[SQLite-net] SQLITE_CONFIG_MALLOC error: %d", rc);
		}
		else {
			DEBUG_LOG("[SQLite-net] SQLITE_CONFIG_MALLOC initialized");
		}
	}
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload() {
	if (memory_manager) {
		memory_manager->DestroyAllocator(allocator);
	}
	allocator = NULL;
	memory_manager = NULL;
	logger = NULL;
}
