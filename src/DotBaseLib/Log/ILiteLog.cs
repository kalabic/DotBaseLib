namespace DotBase.Log;


public interface ILiteLog
{
    void ExceptionOccured(string message, Exception ex);

    void Error(string message);

    void Warning(string message);

    void Info(string message);

    void SystemEvent(string message);
}
