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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SQLite
{
    /// <summary>
    /// Low level SQLite prepared statement object.
    /// </summary>
    /// <remarks>
    /// Using this is the same as using prepared statments in the SQLite C API.
    /// You need to bind all arguments to the statement, manually step for each row and until done, get values from the
    /// returned columns, manually reset for subsequent executions, then dispose when not needed anymore.
    /// </remarks>
    public class SQLitePreparedStatement : IDisposable
    {
        private static readonly IntPtr SQLITE_STATIC = IntPtr.Zero;

        private SQLiteConnection _db;
        private IntPtr _preparedStatement;

        public SQLitePreparedStatement(SQLiteConnection db, string statement)
        {
            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            _db = db;
            _preparedStatement = SQLite3.Prepare2(db.Handle, statement);
        }

        ~SQLitePreparedStatement()
        {
            Dispose();
        }

        public SQLite3.Result Reset()
        {
            ThrowIfDisposed();
            return SQLite3.Reset(_preparedStatement);
        }

        public SQLite3.Result Bind(int index, bool value)
        {
            ThrowIfDisposed();
            return (SQLite3.Result) SQLite3.BindInt(_preparedStatement, index, value ? 1 : 0);
        }
        public SQLite3.Result Bind(string name, bool value)
        {
            ThrowIfDisposed();
            int index = SQLite3.BindParameterIndex(_preparedStatement, name);
            return Bind(index, value);
        }

        public SQLite3.Result Bind(int index, int value)
        {
            ThrowIfDisposed();
            return (SQLite3.Result) SQLite3.BindInt(_preparedStatement, index, value);
        }
        public SQLite3.Result Bind(string name, int value)
        {
            ThrowIfDisposed();
            int index = SQLite3.BindParameterIndex(_preparedStatement, name);
            return Bind(index, value);
        }

        public SQLite3.Result Bind(int index, long value)
        {
            ThrowIfDisposed();
            return (SQLite3.Result) SQLite3.BindInt64(_preparedStatement, index, value);
        }
        public SQLite3.Result Bind(string name, long value)
        {
            ThrowIfDisposed();
            int index = SQLite3.BindParameterIndex(_preparedStatement, name);
            return Bind(index, value);
        }

        public SQLite3.Result Bind(int index, float value)
        {
            ThrowIfDisposed();
            return (SQLite3.Result) SQLite3.BindDouble(_preparedStatement, index, value);
        }
        public SQLite3.Result Bind(string name, float value)
        {
            ThrowIfDisposed();
            int index = SQLite3.BindParameterIndex(_preparedStatement, name);
            return Bind(index, value);
        }

        public SQLite3.Result Bind(int index, double value)
        {
            ThrowIfDisposed();
            return (SQLite3.Result) SQLite3.BindDouble(_preparedStatement, index, value);
        }
        public SQLite3.Result Bind(string name, double value)
        {
            ThrowIfDisposed();
            int index = SQLite3.BindParameterIndex(_preparedStatement, name);
            return Bind(index, value);
        }

        public SQLite3.Result Bind(int index, string value)
        {
            ThrowIfDisposed();
            return (SQLite3.Result) SQLite3.BindText(_preparedStatement, index, value, value.Length * sizeof(char), SQLITE_STATIC);
        }
        public SQLite3.Result Bind(string name, string value)
        {
            ThrowIfDisposed();
            int index = SQLite3.BindParameterIndex(_preparedStatement, name);
            return Bind(index, value);
        }

        public SQLite3.Result Bind(int index, byte[] value)
        {
            ThrowIfDisposed();
            return (SQLite3.Result) SQLite3.BindBlob(_preparedStatement, index, value, value.Length, SQLITE_STATIC);
        }
        public SQLite3.Result Bind(string name, byte[] value)
        {
            ThrowIfDisposed();
            int index = SQLite3.BindParameterIndex(_preparedStatement, name);
            return Bind(index, value);
        }

        public int BindParameterIndex(string name)
        {
            ThrowIfDisposed();
            return SQLite3.BindParameterIndex(_preparedStatement, name);
        }

        public SQLite3.Result Step()
        {
            ThrowIfDisposed();
            var result = SQLite3.Step(_preparedStatement);
            if (result > SQLite3.Result.OK && result < SQLite3.Result.Row)
            {
                throw SQLiteException.New(result, SQLite3.GetErrmsg(_db.Handle));
            }
            return result;
        }

        public int GetColumnCount()
        {
            ThrowIfDisposed();
            return SQLite3.ColumnCount(_preparedStatement);
        }

        public string GetColumnName(int column)
        {
            ThrowIfDisposed();
            return SQLite3.ColumnName16(_preparedStatement, column);
        }

        public IEnumerable<string> EnumerateColumnNames()
        {
            for (int i = 0, columnCount = GetColumnCount(); i < columnCount; i++)
            {
                yield return GetColumnName(i);
            }
        }

        public IEnumerable<string> EnumerateColumnsAsText()
        {
            for (int i = 0, columnCount = GetColumnCount(); i < columnCount; i++)
            {
                yield return GetString(i);
            }
        }

        public bool GetBool(int column)
        {
            ThrowIfDisposed();
            return SQLite3.ColumnInt(_preparedStatement, column) != 0;
        }
        
        public int GetInt(int column)
        {
            ThrowIfDisposed();
            return SQLite3.ColumnInt(_preparedStatement, column);
        }

        public long GetLong(int column)
        {
            ThrowIfDisposed();
            return SQLite3.ColumnInt64(_preparedStatement, column);
        }

        public float GetFloat(int column)
        {
            ThrowIfDisposed();
            return (float) SQLite3.ColumnDouble(_preparedStatement, column);
        }

        public double GetDouble(int column)
        {
            ThrowIfDisposed();
            return SQLite3.ColumnDouble(_preparedStatement, column);
        }

        public string GetString(int column)
        {
            ThrowIfDisposed();
            IntPtr ptr = SQLite3.ColumnText16(_preparedStatement, column);
            if (ptr == IntPtr.Zero)
            {
                return null;
            }
            int sizeInBytes = SQLite3.ColumnBytes16(_preparedStatement, column);
            return Marshal.PtrToStringUni(ptr, sizeInBytes / sizeof(char));
        }

        public byte[] GetBytes(int column)
        {
            ThrowIfDisposed();
            IntPtr blob = SQLite3.ColumnBlob(_preparedStatement, column);
            if (blob == IntPtr.Zero)
            {
                return null;
            }
            int sizeInBytes = SQLite3.ColumnBytes(_preparedStatement, column);
            var value = new byte[sizeInBytes];
            Marshal.Copy(blob, value, 0, sizeInBytes);
            return value;
        }

        public void Dispose()
        {
            SQLite3.Finalize(_preparedStatement);
            _preparedStatement = IntPtr.Zero;
            _db = null;
        }

        private void ThrowIfDisposed()
        {
            if (_preparedStatement == IntPtr.Zero)
            {
                throw new ObjectDisposedException(nameof(_preparedStatement));
            }
        }
    }
}
