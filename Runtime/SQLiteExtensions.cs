using System;
using System.Runtime.InteropServices;

namespace SQLite
{
    public static partial class SQLite3
    {
        [Flags]
        public enum SerializeFlags : uint
        {
            None,
            NoCopy = 0x001,  /* Do no memory allocations */
        }

        [Flags]
        public enum DeserializeFlags : uint
        {
            None,
            FreeOnClose = 1,  /* Call sqlite3_free() on close */
            Resizeable = 2,  /* Resize using sqlite3_realloc64() */
            ReadOnly = 4,  /* Database is read-only */
        }

        [DllImport(LibraryPath, EntryPoint = "sqlite3_serialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Serialize(IntPtr db, [MarshalAs(UnmanagedType.LPStr)] string zSchema, out long piSize, SerializeFlags mFlags);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_deserialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result Deserialize(IntPtr db, [MarshalAs(UnmanagedType.LPStr)] string zSchema, byte[] pData, long szDb, long szBuf, DeserializeFlags mFlags);
        
        [DllImport(LibraryPath, EntryPoint = "sqlite3_deserialize", CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern Result Deserialize(IntPtr db, [MarshalAs(UnmanagedType.LPStr)] string zSchema, void* pData, long szDb, long szBuf, DeserializeFlags mFlags);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_malloc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Malloc(int size);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_malloc64", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Malloc(long size);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_realloc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Realloc(IntPtr ptr, int size);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_realloc64", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Realloc(IntPtr ptr, long size);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_free", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Free(IntPtr ptr);
    }
}
