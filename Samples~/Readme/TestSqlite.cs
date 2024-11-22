using SQLite;
using UnityEngine;

namespace Gilzoide.SqliteNet.Samples.Readme
{
    // The library contains simple attributes that you can use
    // to control the construction of tables, ORM style
    public class Player
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class TestSqlite : MonoBehaviour
    {
        void Start()
        {
            // 1. Create a connection to the database.
            // The special ":memory:" in-memory database and
            // URIs like "file:///somefile" are also supported
            var db = new SQLiteConnection($"{Application.persistentDataPath}/MyDb.db");

            // 2. Once you have defined your entity, you can automatically
            // generate tables in your database by calling CreateTable
            db.CreateTable<Player>();

            // 3. You can insert rows in the database using Insert
            // The Insert call fills Id, which is marked with [AutoIncremented]
            var newPlayer = new Player
            {
                Name = "gilzoide",
            };
            db.Insert(newPlayer);
            Debug.Log($"Player new ID: {newPlayer.Id}");
            // Similar methods exist for Update and Delete.

            // 4.a The most straightforward way to query for data
            // is using the Table method. This can take predicates
            // for constraining via WHERE clauses and/or adding ORDER BY clauses
            var query = db.Table<Player>().Where(p => p.Name.StartsWith("g"));
            foreach (Player player in query)
            {
                Debug.Log($"Found player named {player.Name} with ID {player.Id}");
            }

            // 4.b You can also make queries at a low-level using the Query method
            var players = db.Query<Player>("SELECT * FROM Player WHERE Id = ?", 1);
            foreach (Player player in players)
            {
                Debug.Log($"Player with ID 1 is called {player.Name}");
            }

            // 5. You can perform low-level updates to the database using the Execute
            // method, for example for running PRAGMAs or VACUUM
            db.Execute("VACUUM");
        }
    }
}
