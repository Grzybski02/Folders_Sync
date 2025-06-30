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