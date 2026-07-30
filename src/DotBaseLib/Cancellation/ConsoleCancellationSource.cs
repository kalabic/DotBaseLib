using DotBase.Core;
using DotBase.Event;
using DotBase.Log;

namespace DotBase.Cancellation;


/// <summary>
/// Triggers cancellation only once until object state is reset using <see cref="Reset(bool)"/>.
/// </summary>
public class ConsoleCancellationSource
    : DisposableBase
{
    public bool IsCancellationRequested => _cancellation.IsCancellationRequested;

    public CancellationToken Token => _cancellation.Token;

    public IEventProducer<CancellationEvent> CancellationEvent => _cancelledEvent;

    private readonly ConsoleCancelEventHandler _cancelKeyPressHandler;

    private readonly CancellationEventProducer _cancelledEvent = new();

    private readonly ResettableCancellationTokenSource _cancellation = new();

    public ConsoleCancellationSource()
    {
        _cancelKeyPressHandler = CreateWeakCancelKeyPressHandler(this);
        Console.CancelKeyPress += _cancelKeyPressHandler;
    }

    ~ConsoleCancellationSource()
    {
        Console.CancelKeyPress -= _cancelKeyPressHandler;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Unregister();
            _cancellation.DisposeAfter(_cancelledEvent);
            _cancellation.Dispose();
        }

        base.Dispose(disposing);
    }

    public void Reset(bool continueExec = false)
    {
        _cancellation.TryReset();
    }

    public void Unregister()
    {
        Console.CancelKeyPress -= _cancelKeyPressHandler;
    }

    public void Cancel()
    {
        Cancel(false);
    }

    public void Cancel(bool throwOnFirstException)
    {
        using ResettableCancellationTokenSource.CancellationOperation? operation =
            _cancellation.TryBeginCancellation();

        if (operation is not null)
        {
            operation.Source.Cancel(throwOnFirstException);
            _cancelledEvent.Invoke();
        }
    }

    public Task CancelAsync()
    {
        ResettableCancellationTokenSource.CancellationOperation? operation =
            _cancellation.TryBeginCancellation();

        return operation is null
            ? Task.CompletedTask
            : CancelAsync(operation);
    }

    public bool WaitOne(int miliseconds = -1)
    {
        try
        {
            return Token.WaitHandle.WaitOne(miliseconds);
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
    }

    private async Task CancelAsync(ResettableCancellationTokenSource.CancellationOperation operation)
    {
        using (operation)
        {
            Task? cancellation = null;
            try
            {
                cancellation = operation.Source.CancelAsync();
                _cancelledEvent.Invoke();
            }
            finally
            {
                if (cancellation is not null)
                {
                    await cancellation.ConfigureAwait(false);
                }
            }
        }
    }

    private static ConsoleCancelEventHandler CreateWeakCancelKeyPressHandler(ConsoleCancellationSource source)
    {
        var weakSource = new WeakReference<ConsoleCancellationSource>(source);
        return (_, ev) =>
        {
            if (weakSource.TryGetTarget(out ConsoleCancellationSource? target))
            {
                LiteLog.Log.Info("Cancellation event by a key press (Ctrl-C)");
                ev.Cancel = true;
                _ = target.CancelAsync();
            }
        };
    }
}
