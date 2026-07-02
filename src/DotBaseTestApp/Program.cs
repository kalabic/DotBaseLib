
using DotBase.Cancellation;
using DotBase.Log;
using DotBaseWin.Log;
using System.Diagnostics.Tracing;

namespace DotBaseTestApp;

public partial class Program
{
    /// <summary>
    /// Connected to <see cref="Console.CancelKeyPress"/> inside <see cref="InitializeEnvironment"/> method.
    /// </summary>
    static private readonly ConsoleCancellationSource ExitSource = new();

    public static void Main(string[] args)
    {
        // Assumes 'DotBaseTestSetup' was previously used to create a Windows Event Log source 'DotBaseTestApp'.
        using var eventLogBridge = WindowsEventLogBridge.Enable("DotBaseTestApp");

        // Enables informational messages to be written to both the console and the event log.
        var info = new ConsoleInfoLog(EventLevel.Informational);

        info.Info("Running. Ctrl-C to stop.");

        ExitSource.WaitOne();

        TestNullException(info, "Testing null exception.", null);

        info.Info("Finished");
    }

    private static void TestNullException(InfoLog log, string message, Exception? ex)
    {
#pragma warning disable CS8604 // Possible null reference argument.
        log.Warning(message, ex);
#pragma warning restore CS8604 // Possible null reference argument.
    }
}
