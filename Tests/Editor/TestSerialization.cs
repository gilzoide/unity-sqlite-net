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
