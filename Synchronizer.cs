using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Folders_Sync;
class Synchronizer
{
    private readonly string _source;
    private readonly string _replica;
    private FileSystemWatcher _watcher;

    public Synchronizer(string source, string replica)
    {
        _source = Path.GetFullPath(source);
        _replica = Path.GetFullPath(replica);
    }

    public async Task SynchronizeAsync()
    {
        if (!Directory.Exists(_source))
            throw new DirectoryNotFoundException($"Source not found: {_source}");

        Directory.CreateDirectory(_replica);
        await MirrorAsync(_source, _replica);
        Cleanup(_source, _replica);
        Logger.Info("Full synchronization complete.");
    }

    public void StartWatching()
    {
        _watcher = new FileSystemWatcher(_source)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Size
        };
        _watcher.Created += OnCreated;
        _watcher.Changed += OnChanged;
        _watcher.Deleted += OnDeleted;
        _watcher.Renamed += OnRenamed;
        _watcher.EnableRaisingEvents = true;
    }

    public void StopWatching()
    {
        if (_watcher == null) return;
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
        _watcher = null;
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        try
        {
            var rel = Path.GetRelativePath(_source, e.FullPath);
            var dest = Path.Combine(_replica, rel);
            if (Directory.Exists(e.FullPath))
            {
                Directory.CreateDirectory(dest);
                Logger.Info($"Directory created: {dest}");
            }
            else if (File.Exists(e.FullPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dest));
                File.Copy(e.FullPath, dest, true);
                Logger.Info($"File created: {e.FullPath} -> {dest}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"OnCreated error: {ex.Message}");
        }
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            var rel = Path.GetRelativePath(_source, e.FullPath);
            var dest = Path.Combine(_replica, rel);
            if (File.Exists(e.FullPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dest));
                File.Copy(e.FullPath, dest, true);
                Logger.Info($"File changed: {e.FullPath} -> {dest}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"OnChanged error: {ex.Message}");
        }
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        try
        {
            var rel = Path.GetRelativePath(_source, e.FullPath);
            var dest = Path.Combine(_replica, rel);
            if (Directory.Exists(dest))
            {
                Directory.Delete(dest, true);
                Logger.Info($"Directory deleted: {dest}");
            }
            else if (File.Exists(dest))
            {
                File.Delete(dest);
                Logger.Info($"File deleted: {dest}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"OnDeleted error: {ex.Message}");
        }
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        try
        {
            var oldRel = Path.GetRelativePath(_source, e.OldFullPath);
            var newRel = Path.GetRelativePath(_source, e.FullPath);
            var oldDest = Path.Combine(_replica, oldRel);
            var newDest = Path.Combine(_replica, newRel);
            if (Directory.Exists(oldDest))
            {
                Directory.Move(oldDest, newDest);
                Logger.Info($"Directory renamed: {oldDest} -> {newDest}");
            }
            else if (File.Exists(oldDest))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(newDest));
                File.Move(oldDest, newDest);
                Logger.Info($"File renamed: {oldDest} -> {newDest}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"OnRenamed error: {ex.Message}");
        }
    }

    private async Task MirrorAsync(string src, string dest)
    {
        foreach (var file in Directory.EnumerateFiles(src, "*", SearchOption.TopDirectoryOnly))
        {
            var destFile = Path.Combine(dest, Path.GetFileName(file));
            if (!File.Exists(destFile) || !await HasSameContentAsync(file, destFile))
            {
                Directory.CreateDirectory(dest);
                File.Copy(file, destFile, true);
                Logger.Info($"Copied: {file} -> {destFile}");
            }
        }
        foreach (var dir in Directory.EnumerateDirectories(src, "*", SearchOption.TopDirectoryOnly))
        {
            var subDest = Path.Combine(dest, Path.GetFileName(dir));
            Directory.CreateDirectory(subDest);
            await MirrorAsync(dir, subDest);
        }
    }

    private void Cleanup(string src, string dest)
    {
        foreach (var file in Directory.EnumerateFiles(dest, "*", SearchOption.TopDirectoryOnly))
        {
            var srcFile = Path.Combine(src, Path.GetFileName(file));
            if (!File.Exists(srcFile))
            {
                File.Delete(file);
                Logger.Info($"Deleted file: {file}");
            }
        }
        foreach (var dir in Directory.EnumerateDirectories(dest, "*", SearchOption.TopDirectoryOnly))
        {
            var srcDir = Path.Combine(src, Path.GetFileName(dir));
            if (!Directory.Exists(srcDir))
            {
                Directory.Delete(dir, true);
                Logger.Info($"Deleted directory: {dir}");
            }
            else Cleanup(srcDir, dir);
        }
    }

    private static async Task<bool> HasSameContentAsync(string f1, string f2)
    {
        var i1 = new FileInfo(f1);
        var i2 = new FileInfo(f2);
        if (i1.Length != i2.Length) return false;
        using var h = MD5.Create();
        using var s1 = File.OpenRead(f1);
        using var s2 = File.OpenRead(f2);
        var b1 = await h.ComputeHashAsync(s1);
        var b2 = await h.ComputeHashAsync(s2);
        return b1.SequenceEqual(b2);
    }
}