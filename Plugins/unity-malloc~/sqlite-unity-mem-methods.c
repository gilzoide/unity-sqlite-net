#include <stdlib.h>

#include "../sqlite-amalgamation-3440200~/sqlite3.h"
#include "IUnityInterface.h"
#include "IUnityMemoryManager.h"

static IUnityMemoryManager *memory_manager = NULL;
static UnityAllocator *allocator = NULL;

#define ALIGNMENT \
	(sizeof(size_t))

#define GET_BASE_PTR(ptr) \
	(((size_t *) ptr) - 1)

static void *xMalloc(int size) {
	size_t *allocation = (size_t *) memory_manager->Allocate(allocator, size + sizeof(size_t), ALIGNMENT, __FILE__, __LINE__);
	*allocation = size;
	return allocation + 1;
}

static void xFree(void *ptr) {
	memory_manager->Deallocate(allocator, GET_BASE_PTR(ptr), __FILE__, __LINE__);
}

static void *xRealloc(void *ptr, int new_size) {
	size_t *allocation = (size_t *) memory_manager->Reallocate(allocator, GET_BASE_PTR(ptr), new_size, ALIGNMENT, __FILE__, __LINE__);
	if (allocation == NULL) {
		return NULL;
	}
	else {
		*allocation = new_size;
		return allocation + 1;
	}
}

static int xSize(void *ptr) {
	return *GET_BASE_PTR(ptr);
}

static int xRoundup(int value) {
	int unaligned_bytes = value % ALIGNMENT;
	if (unaligned_bytes == 0) {
		return value;
	}
	else {
		return value + ALIGNMENT - unaligned_bytes;
	}
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
