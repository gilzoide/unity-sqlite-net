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
