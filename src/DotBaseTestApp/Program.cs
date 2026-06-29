
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

        Console.WriteLine("DONE");
        log.Info("DONE");
    }
}
