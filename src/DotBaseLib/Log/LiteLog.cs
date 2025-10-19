using System.Diagnostics.Tracing;

namespace DotBase.Log;


[EventSource(Name = "DotBase.Log.LiteLog")]
public class LiteLog
    : EventSource
    , ILiteLog
{
    public static readonly string NAME = "DotBase.Log.LiteLog";

    public static ILiteLog Log { get; } = new LiteLog();

    private LiteLog() : base(LiteLog.NAME) { }

    [Event(1, Message = "_exception occured: {0}", Level = EventLevel.Critical)]
    public void ExceptionOccured(string message, Exception ex)
    {
        if (String.IsNullOrEmpty(message))
        {
            WriteEvent(1, ex);
        }
        else
        {
            WriteEvent(1, message + " - " + ex);
        }
    }

    [Event(2, Message = "Error: {0}", Level = EventLevel.Error)]
    public void Error(string message)
    {
        WriteEvent(2, message);
    }

    [Event(3, Message = "Warning: {0}", Level = EventLevel.Warning)]
    public void Warning(string message)
    {
        WriteEvent(3, message);
    }

    [Event(4, Message = "Info: {0}", Level = EventLevel.Informational)]
    public void Info(string message)
    {
        WriteEvent(4, message);
    }

    [Event(5, Message = "SystemEvent: {0}", Level = EventLevel.Informational)]
    public void SystemEvent(string message)
    {
        WriteEvent(6, message);
    }
}
