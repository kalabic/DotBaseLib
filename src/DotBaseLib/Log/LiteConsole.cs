namespace DotBase.Log;


public class LiteConsole : ITextConsole
{
    /// <summary>
    /// 
    /// Exists mainly because <see cref="WriteNotification(string)"/> is likely to be invoked from concurrent task or a thread.
    /// 
    /// </summary>
    private readonly object _lock = new object();

    private bool _lineFinished = true;

    public void SetCursorLeft(int value)
    {
        lock (_lock)
        {
            Console.CursorLeft = value;
            _lineFinished = false;
        }
    }

    public void Write(string? message)
    {
        lock (_lock)
        {
            Console.Write(message);
            _lineFinished = false;
        }
    }

    public void WriteLine()
    {
        lock (_lock)
        {
            Console.WriteLine();
            _lineFinished = true;
        }
    }

    public void WriteLine(string? message)
    {
        lock (_lock)
        {
            Console.WriteLine(message);
            _lineFinished = true;
        }
    }

    public void WriteNotification(string message)
    {
        lock (_lock)
        {
            if (!_lineFinished)
            {
                Console.WriteLine();
            }

            Console.WriteLine(message);
            _lineFinished = true;
        }
    }
}
