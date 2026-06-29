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

    void ExceptionOccurred(string message, Exception ex);

    void Error(string message);

    void Warning(string message);

    void Info(string message);

    void Event(string eventType, string message);

    void DebugEvent(DebugE eventType, string message);

    void DebugEvent(string eventType, string message);

    void SystemEvent(SystemE eventType, string message);

    void SystemEvent(string eventType, string message);
}
