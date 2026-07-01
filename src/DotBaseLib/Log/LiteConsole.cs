namespace DotBase.Log;


public class LiteConsole
{
    public void SetCursorLeft(int value)
    {
        Console.CursorLeft = value;
    }

    public void Write(string? message)
    {
        Console.Write(message);
    }

    public void WriteLine()
    {
        Console.WriteLine();
    }

    public void WriteLine(string? message)
    {
        Console.WriteLine(message);
    }

    public void WriteNotification(string message)
    {
        Console.WriteLine(message);
    }
}
