# Folders_Sync

This utility mirrors a source directory to a replica directory and keeps them
synchronized. It accepts several command-line arguments:

```
Folders_Sync.exe <sourcePath> <replicaPath> <intervalSeconds> <logFilePath>
```

* `<sourcePath>` – path of the directory to watch for changes.
* `<replicaPath>` – destination directory that will be kept in sync with the source.
* `<intervalSeconds>` – how often a full synchronization runs in seconds (int > 0).
* `<logFilePath>` – file where log entries are written.

`<intervalSeconds>` controls the delay between periodic full sync cycles while
the program runs.

## Building 
Install the .NET 8 SDK and run:
```
dotnet build
```
This produces the executable under `bin/`.

## Example
Through running .NET 8 SDK:
```
dotnet run -- C:\data\source C:\data\replica 30 C:\data\log.txt
```

or through the command line and .exe:

```
\bin\Release\net8.0\Folders_Sync.exe C:\data\source C:\data\replica 30 C:\data\log.txt
```

The command above synchronizes `C:\data\source` with `C:\data\replica` every
30 seconds and logs to `C:\data\log.txt`.

## Contributing

Pull requests are welcome. Please ensure the project builds and formatting is
applied before submitting.

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.