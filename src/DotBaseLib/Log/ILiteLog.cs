using System.Diagnostics.Tracing;

namespace DotBase.Log;


public interface ILiteLog
{
    public enum DebugE
    {
        ObjectDisposed,
        TaskFinished,
    }

    public enum SystemE
    {
        Cancellation,
    }


    /// <summary> Assigned event level: <see cref="EventLevel.Critical"/> </summary>
    void ExceptionOccurred(string message, Exception ex);


    /// <summary> Assigned event level: <see cref="EventLevel.Error"/> </summary>
    void Error(string message);


    /// <summary> Assigned event level: <see cref="EventLevel.Warning"/> </summary>
    void Warning(string message);


    /// <summary> Assigned event level: <see cref="EventLevel.Informational"/> </summary>
    void Info(string message);


    /// <summary> Assigned event level: <see cref="EventLevel.LogAlways"/> </summary>
    void Notice(string message);


    /// <summary> Assigned event level: <see cref="EventLevel.Verbose"/> </summary>
    void Event(string eventType, string message);


    /// <summary> Assigned event level: <see cref="EventLevel.Verbose"/> </summary>
    void DebugEvent(DebugE eventType, string message);


    /// <summary> Assigned event level: <see cref="EventLevel.Verbose"/> </summary>
    void DebugEvent(string eventType, string message);


    /// <summary> Assigned event level: <see cref="EventLevel.Informational"/> </summary>
    void SystemEvent(SystemE eventType, string message);


    /// <summary> Assigned event level: <see cref="EventLevel.Informational"/> </summary>
    void SystemEvent(string eventType, string message);
}
