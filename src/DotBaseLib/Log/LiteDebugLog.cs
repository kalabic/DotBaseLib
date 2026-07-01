using System.Diagnostics.Tracing;

namespace DotBase.Log;


[EventSource(
    Name = NAME,
    Guid = "0E21492C-6E9A-4869-B9AF-BAF5B9875350"
)]
public class LiteDebugLog
    : EventSource
    , InfoLog
{
    public const string NAME = "DotBase.Logging.Debug";

    public static InfoLog Log { get; } = new LiteDebugLog();

    private LiteDebugLog() : base(LiteDebugLog.NAME) { }


    // Notice >>>

    [Event(1010, Message = "Notice: {0}", Level = EventLevel.LogAlways)]
    public void Notice(string message)
    {
        WriteEvent(1010, message);
    }


    [Event(1011, Message = "Notice: {0} - {1}", Level = EventLevel.LogAlways)]
    public void NoticeException(string message, string exception)
    {
        WriteEvent(1011, message, exception);
    }


    [NonEvent]
    public void Notice(string message, Exception ex)
    {
        NoticeException(message, ex?.ToString() ?? "<null exception>");
    }


    // Critical >>>

    [Event(1020, Message = "Critical: {0}", Level = EventLevel.Critical)]
    public void Critical(string message)
    {
        WriteEvent(1020, message);
    }


    [Event(1021, Message = "Critical: {0} - {1}", Level = EventLevel.Critical)]
    public void CriticalException(string message, string exception)
    {
        WriteEvent(1021, message, exception);
    }


    [NonEvent]
    public void Critical(string message, Exception ex)
    {
        CriticalException(message, ex?.ToString() ?? "<null exception>");
    }


    // Error >>>

    [Event(1030, Message = "Error: {0}", Level = EventLevel.Error)]
    public void Error(string message)
    {
        WriteEvent(1030, message);
    }


    [Event(1031, Message = "Error: {0} - {1}", Level = EventLevel.Error)]
    public void ErrorException(string message, string exception)
    {
        WriteEvent(1031, message, exception);
    }


    [NonEvent]
    public void Error(string message, Exception ex)
    {
        ErrorException(message, ex?.ToString() ?? "<null exception>");
    }


    // Warning >>>

    [Event(1040, Message = "Warning: {0}", Level = EventLevel.Warning)]
    public void Warning(string message)
    {
        WriteEvent(1040, message);
    }


    [Event(1041, Message = "Warning: {0} - {1}", Level = EventLevel.Warning)]
    public void WarningException(string message, string exception)
    {
        WriteEvent(1041, message, exception);
    }


    [NonEvent]
    public void Warning(string message, Exception ex)
    {
        WarningException(message, ex?.ToString() ?? "<null exception>");
    }


    // Info >>>

    [Event(1050, Message = "Info: {0}", Level = EventLevel.Informational)]
    public void Info(string message)
    {
        WriteEvent(1050, message);
    }


    [Event(1051, Message = "Info: {0} - {1}", Level = EventLevel.Informational)]
    public void InfoException(string message, string exception)
    {
        WriteEvent(1051, message, exception);
    }


    [NonEvent]
    public void Info(string message, Exception ex)
    {
        InfoException(message, ex?.ToString() ?? "<null exception>");
    }


    // Event >>>

    [Event(1060, Message = "Event: {0} - {1}", Level = EventLevel.Verbose)]
    public void Event(string eventType, string message)
    {
        WriteEvent(1060, eventType, message);
    }


    [Event(1061, Message = "Event: {0} - {1} - {2}", Level = EventLevel.Verbose)]
    public void EventObject(string eventType, string message, string objectType)
    {
        WriteEvent(1061, eventType, message, objectType);
    }


    [NonEvent]
    public void Event(string eventType, string message, object obj)
    {
        if (obj is not null)
        {
            EventObject(eventType, message, obj.GetType().ToString());
        }
        else
        {
            EventObject(eventType, message, "<null object>");
        }
    }
}
