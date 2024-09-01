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
using System.Text;
using SQLite;
using UnityEngine;
using UnityEngine.UI;

namespace Gilzoide.SqliteNet.Samples.REPL
{
    public class SQLiteREPL : MonoBehaviour
    {
        public string DbName = "SQLiteREPL.sqlite";
        public Text OutputText;

        private SQLiteConnection _connection;

        void Start()
        {
            _connection = new SQLiteConnection(DbName);
        }

        void OnDestroy()
        {
            _connection?.Dispose();
        }

        public void OnSubmitSQL(string sql)
        {
            DateTime timeBefore = DateTime.Now;
            try
            {
                using (var stmt = new SQLitePreparedStatement(_connection, sql))
                {
                    var sb = new StringBuilder();
                    sb.Append(string.Join("|", stmt.EnumerateColumnNames()));
                    if (sb.Length > 0)
                    {
                        sb.Append('\n');
                        for (int i = 0, count = sb.Length; i < count; i++)
                        {
                            sb.Append('-');
                        }
                    }
                    while (true)
                    {
                        switch (stmt.Step())
                        {
                            case SQLite3.Result.Row:
                            {
                                sb.Append('\n');
                                sb.Append(string.Join("|", stmt.EnumerateColumnsAsText()));
                                break;
                            }
                            
                            case SQLite3.Result.Done:
                                goto breakwhile;
                        }
                    }
                breakwhile:
                    OutputText.text = sb.Length > 0 ? sb.ToString() : "OK";
                }
            }
            catch (SQLiteException ex)
            {
                OutputText.text = "Error: " + ex.Message;
            }

            DateTime timeAfter = DateTime.Now;
            TimeSpan timeSpent = timeAfter - timeBefore;
            OutputText.text += $"\n\nTook: {timeSpent.TotalMilliseconds}ms";
        }
    }
}
