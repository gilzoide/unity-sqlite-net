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
using NUnit.Framework;
using SQLite;

namespace Gilzoide.SqliteNet.Tests.Editor
{
    public class TestSerialization
    {
        private class Row
        {
            [PrimaryKey]
            public int Id { get; set; }
            public int Value { get; set; }
        }

        [Test, TestCase(100)]
        public void TestSQLiteSerialization(int quantity)
        {
            byte[] serializedDatabase;
            using(var db = new SQLiteConnection(""))
            {
                db.CreateTable<Row>();
                for (int i = 0; i < quantity; i++)
                {
                    int added = db.Insert(new Row
                    {
                        Id = i,
                        Value = i,
                    });
                    Assert.That(added, Is.EqualTo(1));
                }
                serializedDatabase = db.Serialize();
            }
            Assert.That(serializedDatabase, Is.Not.Null);

            using(var db = new SQLiteConnection(""))
            {
                db.Deserialize(serializedDatabase, flags: SQLite3.DeserializeFlags.ReadOnly);
                for (int i = 0; i < quantity; i++)
                {
                    Row row = db.Table<Row>().Where(r => r.Id == i).FirstOrDefault();
                    Assert.That(row, Is.Not.Null, $"Couldn't find row {i}");
                    Assert.That(row.Id, Is.EqualTo(row.Value));
                }
            }
        }
    }
}
