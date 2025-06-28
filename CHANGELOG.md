# Changelog
## [Unreleased](https://github.com/gilzoide/unity-sqlite-net/compare/1.2.4...HEAD)
### Added
- Support for encrypting / decrypting databases by using [SQLite3 Multiple Ciphers](https://utelle.github.io/SQLite3MultipleCiphers/) implementation
- [SQLiteAsset](Runtime/SQLiteAsset.cs): read-only SQLite database Unity assets.
  Files with the extensions ".sqlite", ".sqlite2" and ".sqlite3" will be imported as SQLite database assets.
  ".csv" files can be imported as SQLite database assets by changing the importer to `SQLite.Editor.Csv.SQLiteAssetCsvImporter` in the Inspector.
- `SQLiteConnection.SerializeToAsset` extension method for serializing a database to an instance of `SQLiteAsset`.
- `SQLiteConnection.ImportCsvToTable` extension method for importing a CSV text stream as a new table inside the database.
- Support for importing ".sql" files as either a `TextAsset` or a `SQLiteAsset`.
- `SQLiteConnection.ExecuteScript` extension method for executing a SQL script with multiple statements with a single call.

### Changed
- Update SQLite to 3.50.1
- Update NDK version used to build Android binaries to r27c
- Specify minimum macOS version to 11.0 when building dylib


## [1.2.4](https://github.com/gilzoide/unity-sqlite-net/tree/1.2.4)
### Fixed
- Crash when used in the Unity Editor in Linux platform


## [1.2.3](https://github.com/gilzoide/unity-sqlite-net/tree/1.2.3)
### Fixed
- Support for Android 15 devices using 16KB memory page size (reference: https://developer.android.com/guide/practices/page-sizes)


## [1.2.2](https://github.com/gilzoide/unity-sqlite-net/tree/1.2.2)
### Changed
- Updated SQLite to 3.49.0
- Compile SQLite from source in WebGL platform

### Fixed
- Support for WebGL builds with any native build configuration
- Make all column-related attributes inherit Unity's `PreserveAttribute`, avoiding properties being stripped from builds


## [1.2.1](https://github.com/gilzoide/unity-sqlite-net/tree/1.2.1)
### Added
- Add support for updating a struct passed to `Insert` with overload accepting `ref T`

### Fixed
- Support for struct return types in queries


## [1.2.0](https://github.com/gilzoide/unity-sqlite-net/tree/1.2.0)
### Added
- GitHub Action that builds all native libraries
- Support for macOS with Intel CPU

### Changed
- Updated SQLite to 3.48.0
- Updated SQLite-net to v1.9.172


## [1.1.2](https://github.com/gilzoide/unity-sqlite-net/tree/1.1.2)
### Fixed
- Support for the async API in WebGL platform


## [1.1.1](https://github.com/gilzoide/unity-sqlite-net/tree/1.1.1)
### Fixed
- "duplicate column name" errors on `CreateTable` on builds with managed code stripping enabled


## [1.1.0](https://github.com/gilzoide/unity-sqlite-net/tree/1.1.0)
### Added
- Support for persisting data in WebGL builds using idbvfs
- SQLiteREPL sample
- SQLitePreparedStatement class, a low-level wrapper for `sqlite3_stmt`
- Support for code signing macOS shared library from Makefile

### Changed
- Updated SQLite to 3.46.1
- Change version of Emscripten used from latest to 1.40.1, so that the plugin works in Unity versions below 2021.2

### Fixed
- Remove usage of `--platform` on Dockerfile to work with newer versions of Docker


## [1.0.1](https://github.com/gilzoide/unity-sqlite-net/tree/1.0.1)
### Changed
- License first-party code under the MIT license


## [1.0.0](https://github.com/gilzoide/unity-sqlite-net/tree/1.0.0)
### Added
- SQLite 3.45.2, prebuilt for Windows, Linux, macOS, Android and WebGL, and built from source in iOS, tvOS and visionOS
- SQLite-net v1.8.116, both sync and async APIs
- `SQLiteConnection.Serialize` and `SQLiteConnection.Deserialize` extension methods
