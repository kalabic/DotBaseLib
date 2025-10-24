using DotBase.Core;
using DotBase.Event;
using DotBase.Log;

namespace DotBase.Cancellation;


/// <summary>
/// 
/// Triggers cancellation only once until object state is reset again using <see cref="Reset(bool)"/>.
/// 
/// </summary>
public class ConsoleCancellationSource 
    : DisposableBase
{
    // Public properties >>

    public bool IsCancellationRequested { get { return _cts.IsCancellationRequested; } }

    public CancellationToken Token { get { return _cts.Token; } }

    public IEventProducer<CancellationEvent> CancellationEvent { get { return _cancelledEvent; } }

    // Private data >>


    /// <summary>
    /// 
    /// For now this lock only applies to:
    /// <list type="bullet">
    /// <item> <see cref="Dispose(bool)"/> </item>
    /// <item> <see cref="Reset(bool)"/> </item>
    /// <item> <see cref="HandleCancelKeyPress(object?, ConsoleCancelEventArgs)"/> </item>
    /// </list>
    /// 
    /// </summary>
    private readonly object _disposeLock = new object();

    private CancellationEventProducer _cancelledEvent;

    private CancellationTokenSource _cts;

    private volatile int _isCancelled = 0;

    // Implementation >>

    public ConsoleCancellationSource()
    {
        _cancelledEvent = new CancellationEventProducer();
        _cts = new CancellationTokenSource();
        Console.CancelKeyPress += HandleCancelKeyPress;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            lock(_disposeLock)
            {
                Unregister();
                _cts.Dispose();
                _cancelledEvent.Dispose();
                base.Dispose(disposing);
                return;
            }
        }
        base.Dispose(disposing);
    }

    public void Reset(bool continueExec = false)
    {
        lock(_disposeLock)
        {
            if (!IsDisposed)
            {
                _isCancelled = 0;
                _cts.Dispose();
                _cts = new CancellationTokenSource();
            }
        }
    }

    public void Unregister()
    {
        Console.CancelKeyPress -= HandleCancelKeyPress;
    }

    private void HandleCancelKeyPress(object? s, ConsoleCancelEventArgs ev)
    {
        LiteLog.Log.SystemEvent("Cancellation event (Ctrl-C)");
        lock (_disposeLock)
        {
            if (TryBeginCancel())
            {
                _cts.CancelAsync();
                _cancelledEvent.Invoke();
            }

            // Setting this to FALSE creates a DEADLOCK when Visual Studio 2022 debugger is attached??
            // (If true it means current process should continue when signal handler returns)
            ev.Cancel = true;
        }
    }

    public void Cancel()
    {
        if (TryBeginCancel())
        {
            _cts.Cancel();
            _cancelledEvent.Invoke();
        }
    }

    public void Cancel(bool throwOnFirstException) 
    {
        if (TryBeginCancel())
        {
            _cts.Cancel(throwOnFirstException);
            _cancelledEvent.Invoke();
        }
    }

    public Task CancelAsync()
    {
        if (TryBeginCancel())
        {
            var task = _cts.CancelAsync();
            _cancelledEvent.Invoke();
            return task;
        }
        else
        {
            return Task.CompletedTask;
        }
    }

    public bool WaitOne(int miliseconds = -1)
    {
        return _cts.Token.WaitHandle.WaitOne(miliseconds);
    }


    /// <summary>
    /// <list type="bullet">
    /// <item> Returns true only once until object state is reset again using <see cref="Reset(bool)"/>. </item>
    /// <item> Once disposed, it returns false always. </item>
    /// </list>
    /// </summary>
    /// <returns></returns>
    private bool TryBeginCancel()
    {
        return !IsDisposed && Interlocked.CompareExchange(ref _isCancelled, 1, 0) == 0;
    }
}
