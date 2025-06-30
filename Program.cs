namespace Folders_Sync;
class Program
{
    static async Task Main(string[] args)
    {
        var config = Config.Parse(args);
        Logger.Initialize(config.LogFilePath);

        if (!Directory.Exists(config.SourcePath))
        {
            Logger.Error($"Source directory '{config.SourcePath}' does not exist.");
            return;
        }

        var synchronizer = new Synchronizer(config.SourcePath, config.ReplicaPath);
        Logger.Info("Initial full synchronization...");
        try
        {
            await synchronizer.SynchronizeAsync();
        }
        catch (Exception ex)
        {
            Logger.Error($"Initial synchronization failed: {ex.Message}");
            return;
        }

        Logger.Info("Starting file system watcher for immediate updates...");
        synchronizer.StartWatching();

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
            Logger.Info("Shutdown requested, stopping...");
            synchronizer.StopWatching();
        };

        Logger.Info($"Entering periodic sync loop every {config.IntervalSeconds}s. Press Ctrl+C to exit.");
        try
        {
            while (!cts.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(config.IntervalSeconds), cts.Token);
                Logger.Info("Periodic full synchronization...");
                try
                {
                    await synchronizer.SynchronizeAsync();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Periodic sync failed: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the application is cancelled via Ctrl+C
        }

        Logger.Info("Service stopped.");
    }
}