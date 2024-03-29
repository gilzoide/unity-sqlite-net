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
        public static extern IntPtr Serialize(
            IntPtr db,  /* The database connection */
            [MarshalAs(UnmanagedType.LPStr)] string zSchema,  /* Which DB to serialize. ex: "main", "temp", ... */
            out long piSize,  /* Write size of the DB here, if not NULL */
            SerializeFlags mFlags  /* Zero or more SQLITE_SERIALIZE_* flags */
        );

        [DllImport(LibraryPath, EntryPoint = "sqlite3_deserialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern Result Deserialize(
            IntPtr db,  /* The database connection */
            [MarshalAs(UnmanagedType.LPStr)] string zSchema,  /* Which DB to reopen with the deserialization */
            byte[] pData,  /* The serialized database content */
            long szDb,  /* Number bytes in the deserialization */
            long szBuf,  /* Total size of buffer pData[] */
            DeserializeFlags mFlags  /* Zero or more SQLITE_DESERIALIZE_* flags */
        );

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

        public static byte[] Serialize(IntPtr db, string schema = null)
        {
            IntPtr buffer = Serialize(db, schema, out long size, SerializeFlags.None);
            if (buffer == IntPtr.Zero)
            {
                return null;
            }
            try
            {
                var bytes = new byte[size];
                Marshal.Copy(buffer, bytes, 0, (int) size);
                return bytes;
            }
            finally
            {
                Free(buffer);
            }
        }

        public static Result Deserialize(IntPtr db, byte[] bytes, string schema = null, DeserializeFlags flags = DeserializeFlags.None)
        {
            return Deserialize(db, bytes, bytes.LongLength, schema, flags);
        }

        public static Result Deserialize(IntPtr db, byte[] bytes, long usedLength, string schema = null, DeserializeFlags flags = DeserializeFlags.None)
        {
            return Deserialize(db, schema, bytes, usedLength, bytes.LongLength, flags);
        }
    }
}
