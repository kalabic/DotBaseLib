
using DotBaseWin.Log;
using System.Security;

namespace DotBaseTestSetup;

public partial class Program
{
    private const string EVENT_SOURCE_NAME = "DotBaseTestApp";

    public static int Main(string[] args)
    {
        if (!OperatingSystem.IsWindows())
        {
            Console.WriteLine("Windows Event Log setup is only supported on Windows.");
            return 1;
        }

        try
        {
            if (args.Length == 1 && String.Equals(args[0], "-uninstall", StringComparison.OrdinalIgnoreCase))
            {
                WindowsEventLogBridge.DeleteEventSource(EVENT_SOURCE_NAME);
                Console.WriteLine($"Windows Event Log source '{EVENT_SOURCE_NAME}' was removed.");
            }
            else if (args.Length == 0)
            {
                WindowsEventLogBridge.CreateEventSource(EVENT_SOURCE_NAME);
                Console.WriteLine($"Windows Event Log source '{EVENT_SOURCE_NAME}' is ready.");
            }
            else
            {
                Console.Error.WriteLine("Bad arguments.");
                return 3;
            }

            return 0;
        }
        catch (SecurityException ex)
        {
            Console.Error.WriteLine("Could not update the Windows Event Log source. Run this setup app as Administrator.");
            Console.Error.WriteLine(ex.Message);
            return 2;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.Error.WriteLine("Could not update the Windows Event Log source. Run this setup app as Administrator.");
            Console.Error.WriteLine(ex.Message);
            return 2;
        }
    }
}
