/*
 * Copyright (c) 2024 Gil Barbosa Reis
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
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
        public static extern void Free(IntPtr ptr);

        [DllImport(LibraryPath, EntryPoint = "sqlite3_column_bytes16", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ColumnBytes16(IntPtr stmt, int index);

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport(LibraryPath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int idbvfs_register(int makeDefault);
#endif

        static SQLite3()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            idbvfs_register(1);
#endif
        }
    }

    public static class ISQLiteConnectionExtensions
    {
        public static int Insert<T>(this ISQLiteConnection connection, ref T obj)
        {
            object boxed = obj;
            int result = connection.Insert(boxed);
            obj = (T) boxed;
            return result;
        }

		public static int Insert<T>(this ISQLiteConnection connection, ref T obj, Type objType)
        {
            object boxed = obj;
            int result = connection.Insert(boxed, objType);
            obj = (T) boxed;
            return result;
        }

		public static int Insert<T>(this ISQLiteConnection connection, ref T obj, string extra)
        {
            object boxed = obj;
            int result = connection.Insert(boxed, extra);
            obj = (T) boxed;
            return result;
        }

		public static int Insert<T>(this ISQLiteConnection connection, ref T obj, string extra, Type objType)
        {
            object boxed = obj;
            int result = connection.Insert(boxed, extra, objType);
            obj = (T) boxed;
            return result;
        }
    }
}
