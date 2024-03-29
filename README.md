# SQLite-net for Unity
[![openupm](https://img.shields.io/npm/v/com.gilzoide.sqlite-net?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.gilzoide.sqlite-net/)

This package provides the excelent [SQLite-net](https://github.com/praeclarum/sqlite-net) library for accessing [SQLite](https://sqlite.org/) databases in Unity.


## Features
- [SQLite-net v1.8.116](https://github.com/praeclarum/sqlite-net/tree/v1.8.116): both synchronous and asynchronous APIs are available
- [SQLite 3.45.0](https://sqlite.org/releaselog/3_45_0.html) prebuilt for the following platforms: Windows, Linux, macOS, iOS, tvOS, visionOS, Android and WebGL
- Uses Unity Memory Manager for tracking native allocations in Memory Profiler with the area name `SQLite-net` (Unity 2022.2+ only)
- `SQLiteConnection.Serialize` extension method for serializing a database to `byte[]` (reference: [SQLite Serialization](https://www.sqlite.org/c3ref/serialize.html)).
- `SQLiteConnection.Deserialize` extension method for deserializing memory (`byte[]`) into an open database (reference: [SQLite Deserialization](https://www.sqlite.org/c3ref/deserialize.html)).


## Optional packages
- [SQLite Asset](https://github.com/gilzoide/unity-sqlite-asset): read-only SQLite database assets for Unity with scripted importer for ".sqlite", ".sqlite2" and ".sqlite3" files
- [SQLite Asset - CSV](https://github.com/gilzoide/unity-sqlite-asset-csv): easily import ".csv" files as read-only SQLite database assets


## How to install
Either:
- Use the [openupm registry](https://openupm.com/) and install this package using the [openupm-cli](https://github.com/openupm/openupm-cli):
  ```
  openupm add com.gilzoide.sqlite-net
  ```
- Install using the [Unity Package Manager](https://docs.unity3d.com/Manual/upm-ui-giturl.html) with the following URL:
  ```
  https://github.com/gilzoide/unity-sqlite-net.git#1.0.0-preview5
  ```
- Clone this repository or download a snapshot of it directly inside your project's `Assets` or `Packages` folder.


## Usage example
The following code demonstrates some of SQLite-net's core functionality.
Check out [SQLite-net's Wiki](https://github.com/praeclarum/sqlite-net/wiki) for more complete documentation on how to use the library.
```cs
using SQLite;
using UnityEngine;

// The library contains simple attributes that you can use
// to control the construction of tables, ORM style
public class Player
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
}

public class TestSQLite : MonoBehaviour
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
```


## Modifications made to SQLite-net source code
- The value of `LibraryPath` was changed from `sqlite3` to `__Internal` in WebGL builds and `gilzoide-sqlite-net` for other platforms.
  This makes sure the prebuilt libraries are used instead of the ones provided by the system.
- `LibraryPath` is made public.
  This is useful for libraries that want to bind additional native SQLite functions via P/Invoke.
- `SQLiteConnection.Quote` is made public.
  This is be useful for libraries making raw queries.