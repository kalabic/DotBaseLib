using System.Diagnostics.Tracing;

namespace DotBase.Log;


public interface InfoLog
{
    /// <summary> Assigned event level: <see cref="EventLevel.LogAlways"/>. </summary>
    void Notice(string message);


    /// <summary> Assigned event level: <see cref="EventLevel.LogAlways"/>. </summary>
    void Notice(string message, Exception ex);


    /// <summary> Assigned event level: <see cref="EventLevel.Critical"/>. </summary>
    void Critical(string message);


    /// <summary> Assigned event level: <see cref="EventLevel.Critical"/>. </summary>
    void Critical(string message, Exception ex);


    /// <summary> Assigned event level: <see cref="EventLevel.Error"/>. </summary>
    void Error(string message);


    /// <summary> Assigned event level: <see cref="EventLevel.Error"/>. </summary>
    void Error(string message, Exception ex);


    /// <summary> Assigned event level: <see cref="EventLevel.Warning"/>. </summary>
    void Warning(string message);


    /// <summary> Assigned event level: <see cref="EventLevel.Warning"/>. </summary>
    void Warning(string message, Exception ex);


    /// <summary> Assigned event level: <see cref="EventLevel.Informational"/>. </summary>
    void Info(string message);


    /// <summary> Assigned event level: <see cref="EventLevel.Informational"/>. </summary>
    void Info(string message, Exception ex);


    /// <summary> Assigned event level: <see cref="EventLevel.Verbose"/>. </summary>
    void Event(string eventType, string message);


    /// <summary> Assigned event level: <see cref="EventLevel.Verbose"/>. </summary>
    void Event(string eventType, string message, object obj);
}
