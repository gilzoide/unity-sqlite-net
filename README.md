# SQLite-net for Unity
This package provides the excelent [SQLite-net](https://github.com/praeclarum/sqlite-net) library for accessing [SQLite](https://sqlite.org/) databases in Unity.

Check out [SQLite-net's Wiki](https://github.com/praeclarum/sqlite-net/wiki) for documentation on how to use the library.


## Features
- No extra installation steps: just download this package and that's it
- [SQLite-net v1.8.116](https://github.com/praeclarum/sqlite-net/tree/v1.8.116): both synchronous and asynchronous APIs are available
- [SQLite 3.44.2](https://sqlite.org/releaselog/3_44_2.html) prebuilt for the following platforms: Windows, Linux, macOS, iOS, tvOS, Android and WebGL


## How to install
Either:
- Install using the [Unity Package Manager](https://docs.unity3d.com/Manual/upm-ui-giturl.html) with the following URL:
  ```
  https://github.com/gilzoide/unity-sqlite-net.git
  ```
- Clone this repository or download a snapshot of it directly inside your project's `Assets` or `Packages` folder.


## Modifications made to SQLite-net
The only modification made to SQLite-net source code is the value of `LibraryPath` used in WebGL builds, which is set to `__Internal` instead of the default value of `sqlite3`.