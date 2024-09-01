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
#if UNITY_2018_1_OR_NEWER
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
#endif

namespace SQLite
{
    public static class SQLiteConnectionExtensions
    {
        public static byte[] Serialize(this SQLiteConnection db, string schema = null)
        {
            IntPtr buffer = SQLite3.Serialize(db.Handle, schema, out long size, SQLite3.SerializeFlags.None);
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
                SQLite3.Free(buffer);
            }
        }

        public static SQLiteConnection Deserialize(this SQLiteConnection db, byte[] buffer, string schema = null, SQLite3.DeserializeFlags flags = SQLite3.DeserializeFlags.None)
        {
            return Deserialize(db, buffer, buffer.LongLength, schema, flags);
        }

        public static SQLiteConnection Deserialize(this SQLiteConnection db, byte[] buffer, long usedSize, string schema = null, SQLite3.DeserializeFlags flags = SQLite3.DeserializeFlags.None)
        {
            SQLite3.Result result = SQLite3.Deserialize(db.Handle, schema, buffer, usedSize, buffer.LongLength, flags);
            if (result != SQLite3.Result.OK)
            {
                throw SQLiteException.New(result, SQLite3.GetErrmsg(db.Handle));
            }
            return db;
        }

#if UNITY_2018_1_OR_NEWER
        public static SQLiteConnection Deserialize(this SQLiteConnection db, NativeArray<byte> buffer, string schema = null, SQLite3.DeserializeFlags flags = SQLite3.DeserializeFlags.None)
        {
            return Deserialize(db, buffer, buffer.Length, schema, flags);
        }

        public static SQLiteConnection Deserialize(this SQLiteConnection db, NativeArray<byte> buffer, long usedSize, string schema = null, SQLite3.DeserializeFlags flags = SQLite3.DeserializeFlags.None)
        {
            SQLite3.Result result;
            unsafe
            {
                result = SQLite3.Deserialize(db.Handle, schema, buffer.GetUnsafePtr(), usedSize, buffer.Length, flags);
            }
            if (result != SQLite3.Result.OK)
            {
                throw SQLiteException.New(result, SQLite3.GetErrmsg(db.Handle));
            }
            return db;
        }
#endif

#if UNITY_2021_2_OR_NEWER
        public static SQLiteConnection Deserialize(this SQLiteConnection db, ReadOnlySpan<byte> buffer, string schema = null, SQLite3.DeserializeFlags flags = SQLite3.DeserializeFlags.None)
        {
            return Deserialize(db, buffer, buffer.Length, schema, flags);
        }

        public static SQLiteConnection Deserialize(this SQLiteConnection db, ReadOnlySpan<byte> buffer, long usedSize, string schema = null, SQLite3.DeserializeFlags flags = SQLite3.DeserializeFlags.None)
        {
            SQLite3.Result result;
            unsafe
            {
                fixed (void* ptr = buffer)
                {
                    result = SQLite3.Deserialize(db.Handle, schema, ptr, usedSize, buffer.Length, flags);
                }
            }
            if (result != SQLite3.Result.OK)
            {
                throw SQLiteException.New(result, SQLite3.GetErrmsg(db.Handle));
            }
            return db;
        }
#endif
    }
}
