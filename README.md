# SQLite-net for Unity
[![openupm](https://img.shields.io/npm/v/com.gilzoide.sqlite-net?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.gilzoide.sqlite-net/)

This package provides the excelent [SQLite-net](https://github.com/praeclarum/sqlite-net) library for accessing [SQLite](https://sqlite.org/) databases in Unity.


## Features
- [SQLite-net v1.9.172](https://github.com/praeclarum/sqlite-net/tree/v1.9.172)
  + Both synchronous and asynchronous APIs are available
  + `SQLiteConnection.Serialize` extension method for serializing a database to `byte[]` (reference: [SQLite Serialization](https://www.sqlite.org/c3ref/serialize.html)).
  + `SQLiteConnection.Deserialize` extension method for deserializing memory (`byte[]`, `NativeArray<byte>` or `ReadOnlySpan<byte>`) into an open database (reference: [SQLite Deserialization](https://www.sqlite.org/c3ref/deserialize.html)).
  + `SQLiteConnection.ImportCsvToTable` extension method for importing a CSV text stream as a new table inside the database.
- [SQLite3 Multiple Ciphers 2.1.3](https://github.com/utelle/SQLite3MultipleCiphers/releases/tag/v2.1.3) (based on [SQLite 3.50.1](https://sqlite.org/releaselog/3_50_1.html))
  + Supports encrypted databases
  + Enabled modules: [R\*Tree](https://sqlite.org/rtree.html), [Geopoly](https://sqlite.org/geopoly.html), [FTS5](https://sqlite.org/fts5.html), [Built-In Math Functions](https://www.sqlite.org/lang_mathfunc.html)
  + Supports Windows, Linux, macOS, WebGL, Android, iOS, tvOS and visionOS platforms
  + Supports persisting data in WebGL builds by using a [custom VFS backed by Indexed DB](https://github.com/gilzoide/idbvfs).
- [SQLiteAsset](Runtime/SQLiteAsset.cs): read-only SQLite database Unity assets.
  + Files with the extensions ".sqlite", ".sqlite2" and ".sqlite3" will be imported as SQLite database assets.
  + ".csv" files can be imported as SQLite database assets by changing the importer to `SQLite.Editor.SQLiteAssetCsvImporter` in the Inspector.
  + ".sql" files are imported as Text assets by default, but can be imported as SQLite database assets by changing the importer to `SQLite.Editor.SQLScriptDatabaseImporter`.
  + Use the `CreateConnection()` method for connecting to the database provided by the asset.
    Make sure to `Dispose()` of any connections you create.
  + SQLite assets may be loaded from Streaming Assets folder or from memory, depending on the value of their "Streaming Assets Path" property.
    + **Note**: Android and WebGL platforms don't support loading SQLite databases from Streaming Assets and will always load them in memory.
  + `SQLiteConnection.SerializeToAsset` extension method for serializing a database to an instance of `SQLiteAsset`.

## How to install
Either:
- Use the [openupm registry](https://openupm.com/) and install this package using the [openupm-cli](https://github.com/openupm/openupm-cli):
  ```
  openupm add com.gilzoide.sqlite-net
  ```
- Install using the [Unity Package Manager](https://docs.unity3d.com/Manual/upm-ui-giturl.html) with the following URL:
  ```
  https://github.com/gilzoide/unity-sqlite-net.git#1.3.2
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


## License
SQLite-net for Unity first-party code is licensed under the [MIT license](LICENSE.txt).

Third-party code:
- SQLite-net: [MIT license](Runtime/sqlite-net/LICENSE.txt)
- SQLite3 Multiple Ciphers: [MIT license](https://github.com/utelle/SQLite3MultipleCiphers/blob/main/LICENSE)

Database icons from [Solar Icons Set](https://www.figma.com/community/file/1166831539721848736/solar-icons-set), licensed under [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/)


## Modifications made to SQLite-net source code
- The value of `LibraryPath` was changed from `sqlite3` to `__Internal` in WebGL/iOS/tvOS/visionOS builds and `gilzoide-sqlite-net` for other platforms.
  This makes sure the prebuilt libraries are used instead of the ones provided by the system.
- `LibraryPath` is made public.
  This is useful for libraries that want to bind additional native SQLite functions via P/Invoke.
- `SQLiteConnection.Quote` is made public.
  This is useful for libraries making raw queries.
- `SQLite3.SetDirectory` is only defined in Windows platforms.
- Makes all column related attributes inherit `PreserveAttribute`, fixing errors on columns when managed code stripping is enabled.
- Changes the `TaskScheduler` used by the async API on WebGL to one that executes tasks on Unity's main thread.
- Fix support for struct return types in queries
