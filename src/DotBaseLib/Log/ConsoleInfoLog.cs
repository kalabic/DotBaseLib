using System.Diagnostics.Tracing;

namespace DotBase.Log;


public class ConsoleInfoLog : InfoLog
{
    public EventLevel EventLevel { get { return _level; } set { _level = value; } }

    private readonly ITextConsole _console;

    private readonly InfoLog _info;

    private EventLevel _level;

    public ConsoleInfoLog(EventLevel eventLevel = EventLevel.Error)
    {
        _console = new LiteConsole();
        _info = LiteLog.Log;
        _level = eventLevel;
    }

    public ConsoleInfoLog(ITextConsole console, EventLevel eventLevel = EventLevel.Error)
    {
        ArgumentNullException.ThrowIfNull(console, nameof(console));
        _console = console;
        _info = LiteLog.Log;
        _level = eventLevel;
    }

    public ConsoleInfoLog(InfoLog info, EventLevel eventLevel = EventLevel.Error)
    {
        ArgumentNullException.ThrowIfNull(info, nameof(info));
        _console = new LiteConsole();
        _info = info;
        _level = eventLevel;
    }

    public ConsoleInfoLog(ITextConsole console, InfoLog info, EventLevel eventLevel = EventLevel.Error)
    {
        ArgumentNullException.ThrowIfNull(console, nameof(console));
        ArgumentNullException.ThrowIfNull(info, nameof(info));
        _console = console;
        _info = info;
        _level = eventLevel;
    }

    public void Critical(string message)
    {
        var text = ConsoleInfo.SafeJoin(ConsoleInfo.STR_CRITICAL, message);
        _console.WriteLine(text);
        _info.Critical(message);
    }

    public void Critical(string message, Exception ex)
    {
        var text = ConsoleInfo.SafeJoin(ConsoleInfo.STR_CRITICAL, message, ex);
        _console.WriteLine(text);
        _info.Critical(message, ex);
    }

    public void Error(string message)
    {
        if (_level >= EventLevel.Error)
        {
            var text = ConsoleInfo.SafeJoin(ConsoleInfo.STR_ERROR, message);
            _console.WriteLine(text);
        }

        _info.Error(message);
    }

    public void Error(string message, Exception ex)
    {
        if (_level >= EventLevel.Error)
        {
            var text = ConsoleInfo.SafeJoin(ConsoleInfo.STR_ERROR, message, ex);
            _console.WriteLine(text);
        }

        _info.Error(message, ex);
    }

    public void Event(string eventType, string message)
    {
        if (_level >= EventLevel.Verbose)
        {
            var text = ConsoleInfo.SafeJoin(ConsoleInfo.STR_EVENT, eventType) + "; ";
            text = ConsoleInfo.SafeJoin(text, message);
            _console.WriteLine(text);
        }

        _info.Event(eventType, message);
    }

    public void Event(string eventType, string message, object obj)
    {
        if (_level >= EventLevel.Verbose)
        {
            var text = ConsoleInfo.SafeJoin(ConsoleInfo.STR_EVENT, eventType) + "; ";
            text = ConsoleInfo.SafeJoin(text, message) + "; ";
            text = ConsoleInfo.SafeJoin(text, obj);
            _console.WriteLine(text);
        }

        _info.Event(eventType, message, obj);
    }

    public void Info(string message)
    {
        if (_level >= EventLevel.Informational)
        {
            var text = ConsoleInfo.SafeJoin(ConsoleInfo.STR_INFO, message);
            _console.WriteLine(text);
        }

        _info.Info(message);
    }

    public void Info(string message, Exception ex)
    {
        if (_level >= EventLevel.Informational)
        {
            var text = ConsoleInfo.SafeJoin(ConsoleInfo.STR_INFO, message, ex);
            _console.WriteLine(text);
        }

        _info.Info(message, ex);
    }

    public void Notice(string message)
    {
        var text = ConsoleInfo.SafeJoin(ConsoleInfo.STR_NOTICE, message);
        _console.WriteLine(text);
        _info.Notice(message);
    }

    public void Notice(string message, Exception ex)
    {
        var text = ConsoleInfo.SafeJoin(ConsoleInfo.STR_NOTICE, message, ex);
        _console.WriteLine(text);
        _info.Notice(message, ex);
    }

    public void Warning(string message)
    {
        if (_level >= EventLevel.Warning)
        {
            var text = ConsoleInfo.SafeJoin(ConsoleInfo.STR_WARNING, message);
            _console.WriteLine(text);
        }

        _info.Warning(message);
    }

    public void Warning(string message, Exception ex)
    {
        if (_level >= EventLevel.Warning)
        {
            var text = ConsoleInfo.SafeJoin(ConsoleInfo.STR_WARNING, message, ex);
            _console.WriteLine(text);
        }

        _info.Warning(message, ex);
    }
}
