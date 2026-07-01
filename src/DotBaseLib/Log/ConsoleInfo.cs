using System.Diagnostics.Tracing;

namespace DotBase.Log;


public class ConsoleInfo : InfoLog
{
    internal const string STR_CRITICAL = "Error: [critical] ";
    internal const string STR_ERROR = "Error: ";
    internal const string STR_EVENT = "Info: [event] ";
    internal const string STR_INFO = "Info: ";
    internal const string STR_NOTICE = " >>>> ";
    internal const string STR_WARNING = "Warning: ";

    internal static string SafeJoin(string first, string? second)
    {
        if (!String.IsNullOrEmpty(second))
        {
            return first + second;
        }
        else
        {
            return first + "<empty string>";
        }
    }

    internal static string SafeJoin(string first, Exception? ex)
    {
        if (ex is not null)
        {
            return first + ex.ToString();
        }
        else
        {
            return first + "<null exception>";
        }
    }

    internal static string SafeJoin(string first, object? obj)
    {
        if (obj is not null)
        {
            return first + obj.GetType().ToString();
        }
        else
        {
            return first + "<null object>";
        }
    }

    internal static string SafeJoin(string first, string? second, Exception? ex)
    {
        var text = SafeJoin(first, second) + "; [exception occurred] ";
        return SafeJoin(text, ex);
    }


    public EventLevel EventLevel { get { return _level; } set { _level = value; } }

    private readonly ITextConsole _console;

    private EventLevel _level;

    public ConsoleInfo(EventLevel eventLevel = EventLevel.Error)
    {
        _console = new LiteConsole();
        _level = eventLevel;
    }

    public ConsoleInfo(ITextConsole console, EventLevel eventLevel = EventLevel.Error)
    {
        ArgumentNullException.ThrowIfNull(console, nameof(console));
        _console = console;
        _level = eventLevel;
    }


    public void Critical(string message)
    {
        var text = SafeJoin(STR_CRITICAL, message);
        _console.WriteLine(text);
    }

    public void Critical(string message, Exception ex)
    {
        var text = SafeJoin(STR_CRITICAL, message, ex);
        _console.WriteLine(text);
    }

    public void Error(string message)
    {
        if (_level < EventLevel.Error)
        {
            return;
        }

        var text = SafeJoin(STR_ERROR, message);
        _console.WriteLine(text);
    }

    public void Error(string message, Exception ex)
    {
        if (_level < EventLevel.Error)
        {
            return;
        }

        var text = SafeJoin(STR_ERROR, message, ex);
        _console.WriteLine(text);
    }

    public void Event(string eventType, string message)
    {
        if (_level < EventLevel.Verbose)
        {
            return;
        }

        var text = SafeJoin(STR_EVENT, eventType) + "; ";
        text = SafeJoin(text, message);
        _console.WriteLine(text);
    }

    public void Event(string eventType, string message, object obj)
    {
        if (_level < EventLevel.Verbose)
        {
            return;
        }

        var text = SafeJoin(STR_EVENT, eventType) + "; ";
        text = SafeJoin(text, message) + "; ";
        text = SafeJoin(text, obj);
        _console.WriteLine(text);
    }

    public void Info(string message)
    {
        if (_level < EventLevel.Informational)
        {
            return;
        }

        var text = SafeJoin(STR_INFO, message);
        _console.WriteLine(text);
    }

    public void Info(string message, Exception ex)
    {
        if (_level < EventLevel.Informational)
        {
            return;
        }

        var text = SafeJoin(STR_INFO, message, ex);
        _console.WriteLine(text);
    }

    public void Notice(string message)
    {
        var text = SafeJoin(STR_NOTICE, message);
        _console.WriteLine(text);
    }

    public void Notice(string message, Exception ex)
    {
        var text = SafeJoin(STR_NOTICE, message, ex);
        _console.WriteLine(text);
    }

    public void Warning(string message)
    {
        if (_level < EventLevel.Warning)
        {
            return;
        }

        var text = SafeJoin(STR_WARNING, message);
        _console.WriteLine(text);
    }

    public void Warning(string message, Exception ex)
    {
        if (_level < EventLevel.Warning)
        {
            return;
        }

        var text = SafeJoin(STR_WARNING, message, ex);
        _console.WriteLine(text);
    }
}
