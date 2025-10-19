using DotBase.Log;

namespace DotBase.Tools;


public class ConsoleCancellationSource
{
    public CancellationToken Token { get { return _cts.Token; } }

    public CancellationTokenSource TokenSource { get { return _cts; } }


    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    private readonly ConsoleCancelEventHandler _sessionCanceler;


    public ConsoleCancellationSource()
    {
        _sessionCanceler = (sender, e) =>
        {
            LiteLog.Log.SystemEvent("Console event (Ctrl-C)");

            _cts.CancelAsync();
            e.Cancel = true; // Execution continues after the delegate.
        };

        Console.CancelKeyPress += _sessionCanceler;
    }

    public void Unregister()
    {
        Console.CancelKeyPress -= _sessionCanceler;
    }
}
