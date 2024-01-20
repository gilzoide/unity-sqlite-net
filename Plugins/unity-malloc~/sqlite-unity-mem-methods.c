#include <stdlib.h>

#include "../sqlite-amalgamation-3440200~/sqlite3.h"
#include "IUnityInterface.h"
#include "IUnityMemoryManager.h"

static IUnityMemoryManager *memory_manager = NULL;
static UnityAllocator *allocator = NULL;

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
	memory_manager = UNITY_GET_INTERFACE(unityInterfaces, IUnityMemoryManager);
	if (memory_manager == NULL) {
		return;
	}

	allocator = memory_manager->CreateAllocator("SQLite-net", "SQLite Memory Allocator");
	sqlite3_config(SQLITE_CONFIG_MALLOC, &mem_methods);
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload() {
	if (memory_manager) {
		memory_manager->DestroyAllocator(allocator);
	}
	allocator = NULL;
	memory_manager = NULL;
}
