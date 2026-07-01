namespace DotBase.Log;


/// <summary>
/// 
/// The interface has real value because it abstracts multiple output targets, not only <see cref="LiteConsole"/>.
/// 
/// </summary>
public interface ITextConsole
{
    public void SetCursorLeft(int value);

    public void Write(string? message);

    public void WriteLine();

    public void WriteLine(string? message);

    public void WriteNotification(string message);
}
