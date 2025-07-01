using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//
// Simple thread-safe logger that writes to console and a log file.
//

namespace Folders_Sync;
static class Logger
{
    private static StreamWriter _fileWriter;
    private static readonly object _lock = new object();

    public static void Initialize(string logFilePath)
    {
        _fileWriter = new StreamWriter(logFilePath, append: true) { AutoFlush = true };
    }

    public static void Info(string message) => Log("INFO", message);
    public static void Error(string message) => Log("ERROR", message);

    private static void Log(string level, string message)
    {
        var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
        lock (_lock)
        {
            Console.WriteLine(entry);
            _fileWriter.WriteLine(entry);
        }
    }
}
