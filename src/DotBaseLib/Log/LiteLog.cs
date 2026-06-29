using System.Diagnostics.Tracing;
using static DotBase.Log.ILiteLog;

namespace DotBase.Log;


[EventSource(
    Name = "DotBase.Log.LiteLog",
    Guid = "6E2F1248-AAB0-473C-8719-321BC64D19DD"
)]
public class LiteLog
    : EventSource
    , ILiteLog
{
    public static readonly string NAME = "DotBase.Log.LiteLog";

    public static ILiteLog Log { get; } = new LiteLog();

    private LiteLog() : base(LiteLog.NAME) { }

    [Event(1, Message = "_exception occurred: {0}", Level = EventLevel.Critical)]
    public void ExceptionOccurred(string message, Exception ex)
    {
        if (String.IsNullOrEmpty(message))
        {
            WriteEvent(1, ex.ToString());
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

    [Event(5, Message = "Event: {0} - {1}", Level = EventLevel.Verbose)]
    public void Event(string eventType, string message)
    {
        WriteEvent(5, eventType, message);
    }

    [NonEvent]
    public void DebugEvent(DebugE eventType, string message)
    {
        DebugEvent(eventType.ToString(), message);
    }

    [Event(6, Message = "DebugEvent: {0} - {1}", Level = EventLevel.Verbose)]
    public void DebugEvent(string eventType, string message)
    {
        WriteEvent(6, eventType, message);
    }

    [NonEvent]
    public void SystemEvent(SystemE eventType, string message)
    {
        SystemEvent(eventType.ToString(), message);
    }

    [Event(7, Message = "SystemEvent: {0} - {1}", Level = EventLevel.Informational)]
    public void SystemEvent(string eventType, string message)
    {
        WriteEvent(7, eventType, message);
    }
}
