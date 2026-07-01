
using DotBase.Cancellation;
using DotBase.Log;
using DotBaseWin.Log;

namespace DotBaseTestApp;

public partial class Program
{
    /// <summary>
    /// Connected to <see cref="Console.CancelKeyPress"/> inside <see cref="InitializeEnvironment"/> mehod.
    /// </summary>
    static private readonly ConsoleCancellationSource ExitSource = new();

    public static void Main(string[] args)
    {
        var log = LiteLog.Log;
        using var eventLogBridge = WindowsEventLogBridge.Enable("DotBaseTestApp");

        Console.WriteLine("Running. Ctrl-C to stop.");
        log.Info("Running. Ctrl-C to stop.");

        ExitSource.WaitOne();

        TestNullException(log, "Testing null exception.", null);

        Console.WriteLine("Finished");
        log.Info("Finished");
    }

    private static void TestNullException(InfoLog log, string message, Exception? ex)
    {
#pragma warning disable CS8604 // Possible null reference argument.
        log.Warning(message, ex);
#pragma warning restore CS8604 // Possible null reference argument.
    }
}
