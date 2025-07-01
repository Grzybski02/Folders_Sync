using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//
// Represents validated command line options for the synchronizer.
//

namespace Folders_Sync;
class Config
{
    public string SourcePath { get; }
    public string ReplicaPath { get; }
    public int IntervalSeconds { get; }
    public string LogFilePath { get; }

    private Config(string src, string rep, int interval, string log)
    {
        SourcePath = src;
        ReplicaPath = rep;
        IntervalSeconds = interval;
        LogFilePath = log;
    }

    public static Config Parse(string[] args)
    {
        if (args.Length < 4)
            ShowUsageAndExit();

        var src = args[0];
        var rep = args[1];
        if (!int.TryParse(args[2], out var interval) || interval <= 0)
            ShowUsageAndExit();
        var log = args[3];
        return new Config(src, rep, interval, log);
    }

    private static void ShowUsageAndExit()
    {
        Console.WriteLine("Usage: Folders_Sync.exe <sourcePath> <replicaPath> <intervalSeconds> <logFilePath>");
        Environment.Exit(1);
    }
}
