using DotBase.Log;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Runtime.Versioning;
using System.Security;
using Microsoft.Win32;

namespace DotBaseWin.Log;


/// <summary>
/// Forwards selected DotBase <see cref="EventSource"/> events to Windows Event Log.
/// </summary>
public static class WindowsEventLogBridge
{
    public static void CreateEventSource(string eventSourceName)
    {
        ValidateWindowsEventLogSourceName(eventSourceName, nameof(eventSourceName));

        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException($"{nameof(CreateEventSource)} is only supported on Windows.");
        }

        WindowsEventLogBridgeListener.CreateEventSource(eventSourceName);
    }


    public static void DeleteEventSource(string eventSourceName)
    {
        ValidateWindowsEventLogSourceName(eventSourceName, nameof(eventSourceName));

        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException($"{nameof(DeleteEventSource)} is only supported on Windows.");
        }

        WindowsEventLogBridgeListener.DeleteEventSource(eventSourceName);
    }


    public static IDisposable Enable(
        string eventSourceName,
        IEnumerable<EventLevel>? allowedLevels = null)
    {
        ValidateWindowsEventLogSourceName(eventSourceName, nameof(eventSourceName));

        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException($"{nameof(Enable)} is only supported on Windows.");
        }

        return new WindowsEventLogBridgeListener(
            windowsEventLogSourceName: eventSourceName,
            allowedLevels: allowedLevels);
    }


    private static void ValidateWindowsEventLogSourceName(string eventSourceName, string parameterName)
    {
        if (String.IsNullOrWhiteSpace(eventSourceName))
        {
            throw new ArgumentException("Windows Event Log source name is required.", parameterName);
        }
    }
}


[SupportedOSPlatform("windows")]
internal sealed class WindowsEventLogBridgeListener
    : EventListener
{
    private const string EventLogRegistryPath = @"SYSTEM\CurrentControlSet\Services\EventLog";
    private const int MaxEventLogMessageLength = 30_000;

    private static readonly HashSet<string> DefaultDotNetEventSourceNames =
    [
        LiteLog.NAME,
    ];

    private readonly string _windowsEventLogSourceName;
    private readonly string _windowsEventLogName;
    private readonly EventLevel _minimumLevel;
    private readonly HashSet<string>? _dotNetEventSourceNames;
    private readonly HashSet<EventLevel>? _allowedLevels;
    private bool _writeFailed;


    internal WindowsEventLogBridgeListener(
        string windowsEventLogSourceName,
        string windowsEventLogName = "Application",
        IEnumerable<string>? dotNetEventSourceNames = null,
        IEnumerable<EventLevel>? allowedLevels = null)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException($"{nameof(WindowsEventLogBridgeListener)} is only supported on Windows.");
        }

        _windowsEventLogSourceName = windowsEventLogSourceName;
        _windowsEventLogName = windowsEventLogName;
        _dotNetEventSourceNames = dotNetEventSourceNames is null
            ? new HashSet<string>(DefaultDotNetEventSourceNames, StringComparer.Ordinal)
            : new HashSet<string>(dotNetEventSourceNames, StringComparer.Ordinal);
        _allowedLevels = allowedLevels is null
            ? null
            : new HashSet<EventLevel>(allowedLevels);
        _minimumLevel = GetSubscriptionLevel(_allowedLevels);

        EnsureEventSourceExists();

        // The EventListener base constructor can report existing EventSources before
        // this constructor body assigns fields, so enable them again after setup.
        foreach (var dotNetEventSource in EventSource.GetSources())
        {
            EnableDotNetEventSource(dotNetEventSource);
        }
    }


    protected override void OnEventSourceCreated(EventSource dotNetEventSource)
        => EnableDotNetEventSource(dotNetEventSource);


    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        if (_writeFailed
            || _dotNetEventSourceNames is null
            || !_dotNetEventSourceNames.Contains(eventData.EventSource.Name)
            || (_allowedLevels is not null && !_allowedLevels.Contains(eventData.Level)))
        {
            return;
        }

        try
        {
            using var eventLog = new EventLog(_windowsEventLogName)
            {
                Source = _windowsEventLogSourceName,
            };

            eventLog.WriteEntry(
                FormatMessage(eventData),
                GetEntryType(eventData.Level),
                eventData.EventId);
        }
        catch (Exception ex) when (IsEventLogAccessFailure(ex))
        {
            _writeFailed = true;
        }
    }


    private void EnableDotNetEventSource(EventSource dotNetEventSource)
    {
        if (_dotNetEventSourceNames is null || !_dotNetEventSourceNames.Contains(dotNetEventSource.Name))
        {
            return;
        }

        EnableEvents(dotNetEventSource, _minimumLevel, EventKeywords.All);
    }


    internal static void CreateEventSource(
        string windowsEventLogSourceName,
        string windowsEventLogName = "Application")
    {
        if (EventSourceRegistryKeyExists(windowsEventLogSourceName, windowsEventLogName))
        {
            return;
        }

        var eventSourceData = new EventSourceCreationData(windowsEventLogSourceName, windowsEventLogName);
        EventLog.CreateEventSource(eventSourceData);

        if (!EventSourceRegistryKeyExists(windowsEventLogSourceName, windowsEventLogName))
        {
            throw new InvalidOperationException(
                $"Windows Event Log source '{windowsEventLogSourceName}' was not created in '{windowsEventLogName}'.");
        }
    }


    internal static void DeleteEventSource(
        string windowsEventLogSourceName,
        string windowsEventLogName = "Application")
    {
        if (!EventSourceRegistryKeyExists(windowsEventLogSourceName, windowsEventLogName))
        {
            return;
        }

        EventLog.DeleteEventSource(windowsEventLogSourceName);

        if (EventSourceRegistryKeyExists(windowsEventLogSourceName, windowsEventLogName))
        {
            throw new InvalidOperationException(
                $"Windows Event Log source '{windowsEventLogSourceName}' was not deleted from '{windowsEventLogName}'.");
        }
    }


    private void EnsureEventSourceExists()
    {
        if (EventSourceRegistryKeyExists())
        {
            return;
        }

        throw new InvalidOperationException(
            $"Windows Event Log source '{_windowsEventLogSourceName}' is not registered in '{_windowsEventLogName}'.");
    }


    private bool EventSourceRegistryKeyExists()
        => EventSourceRegistryKeyExists(_windowsEventLogSourceName, _windowsEventLogName);


    private static bool EventSourceRegistryKeyExists(string windowsEventLogSourceName, string windowsEventLogName)
    {
        var sourceRegistryPath = $@"{EventLogRegistryPath}\{windowsEventLogName}\{windowsEventLogSourceName}";
        using var eventSourceKey = Registry.LocalMachine.OpenSubKey(sourceRegistryPath);

        return eventSourceKey is not null;
    }


    private static EventLevel GetSubscriptionLevel(HashSet<EventLevel>? allowedLevels)
        => allowedLevels is null || allowedLevels.Count == 0
            ? EventLevel.Informational
            : allowedLevels.Max();


    private static EventLogEntryType GetEntryType(EventLevel level)
        => level switch
        {
            EventLevel.Critical => EventLogEntryType.Error,
            EventLevel.Error => EventLogEntryType.Error,
            EventLevel.Warning => EventLogEntryType.Warning,
            EventLevel.Informational => EventLogEntryType.Information,
            EventLevel.Verbose => EventLogEntryType.Information,
            EventLevel.LogAlways => EventLogEntryType.Information,
            _ => EventLogEntryType.Information,
        };


    private static string FormatMessage(EventWrittenEventArgs eventData)
    {
        var message = $"{eventData.EventSource.Name}/{eventData.EventName ?? eventData.EventId.ToString()} [{eventData.Level}]";

        if (eventData.Payload is null || eventData.Payload.Count == 0)
        {
            return message;
        }

        for (int i = 0; i < eventData.Payload.Count; i++)
        {
            var name = eventData.PayloadNames?[i] ?? $"Payload{i}";
            message += $"{Environment.NewLine}{name}: {eventData.Payload[i]}";
        }

        return message.Length <= MaxEventLogMessageLength
            ? message
            : message[..MaxEventLogMessageLength];
    }


    private static bool IsEventLogAccessFailure(Exception ex)
        => ex is SecurityException
            or UnauthorizedAccessException
            or InvalidOperationException;
}
