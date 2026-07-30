using System.Diagnostics;

namespace DotBase.Cancellation;



/// <summary>
/// 
/// <see cref="CancellableEventSlim.Wait"/> is cancellable both by <see cref="CancellationToken"/>
/// provided in constructor and its own unrelated method <see cref="CancellableEventSlim.Cancel"/>
/// 
/// </summary>
public class CancellableEventSlim : IDisposable
{
    public const int EVENT_NOT_CANCELLED = 0;

    public const int EVENT_DISPOSED = 1;

    public const int EVENT_CANCELLED = 2;

    public const int EVENT_TOKEN_CANCELLED = 3;



    public event EventHandler<Exception>? ExceptionOccured;

    public int CancellationEvent { get { return _cancellationEvent; } }

    public bool IsDisposed { get { return _isDisposed != 0; } }

    public bool IsEventCancelled { get {  return _cancellationEvent != EVENT_NOT_CANCELLED; } }

    public bool IsWaitable { get { return (_cancellationEvent == EVENT_NOT_CANCELLED) && (_isDisposed == 0); } }



    private int _isDisposed;

    private readonly Object _lock = new();

    private readonly CancellationToken _cancellation;

    private readonly CancellationTokenRegistration _registration;

    private readonly ManualResetEventSlim _event;

    private int _cancellationEvent = EVENT_NOT_CANCELLED;


    public CancellableEventSlim(bool initialState = false, int spinCount = 0)
    {
        _cancellation = CancellationToken.None;
        _event = new ManualResetEventSlim(initialState, spinCount);
        _registration = default;
    }

    public CancellableEventSlim(CancellationToken cancellation, bool initialState = false, int spinCount = 0)
    {
        Debug.Assert(!cancellation.IsCancellationRequested);
        _cancellation = cancellation;
        _event = new ManualResetEventSlim(initialState, spinCount);
        _registration = cancellation.Register(() => { Cancel(EVENT_TOKEN_CANCELLED); });
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        lock (_lock)
        {
            if (IsDisposed)
            {
                return;
            }

            Cancel(EVENT_DISPOSED);
            _isDisposed = 1;
        }

        _registration.Dispose();
        _event.Dispose();
    }

    public void Cancel(bool disposing)
    {
        Cancel(disposing ? EVENT_DISPOSED : EVENT_CANCELLED);
    }

    public void Cancel(int cevent = EVENT_CANCELLED)
    {
        Debug.Assert(cevent != EVENT_NOT_CANCELLED);

        lock (_lock)
        {
            if (IsDisposed)
            {
                return;
            }

            if (IsEventCancelled)
            {
                Debug.Assert(_event.IsSet);
                return;
            }

            _cancellationEvent = cevent;
            _event.Set();
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            if (IsDisposed)
            {
                return;
            }

            if (IsEventCancelled)
            {
                Debug.Assert(_event.IsSet);
                return;
            }

            _event.Reset();
            return;
        }
    }

    public void Set()
    {
        lock (_lock)
        {
            if (IsDisposed)
            {
                return;
            }

            if (IsEventCancelled)
            {
                Debug.Assert(_event.IsSet);
                return;
            }

            _event.Set();
            return;
        }
    }

    /// <summary>
    /// 
    /// Is cancellable both by <see cref="CancellationToken"/> provided in constructor and by <see cref="CancellableEventSlim.Cancel"/>.
    /// 
    /// </summary>
    /// <param name="noExceptions"></param>
    /// <returns></returns>
    public bool Wait(bool noExceptions = true)
    {
        if (IsDisposed)
        {
            return false;
        }

        if (IsEventCancelled)
        {
            Debug.Assert(_event.IsSet);
            return false;
        }

        if (noExceptions)
        {
            try
            {
                _event.Wait(_cancellation);
            }
            catch (OperationCanceledException ex)
            {
                ExceptionOccured?.Invoke(this, ex);
                return false;
            }
            catch (ObjectDisposedException ex)
            {
                ExceptionOccured?.Invoke(this, ex);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                ExceptionOccured?.Invoke(this, ex);
                return false;
            }
        }
        else
        {
            _event.Wait(_cancellation);
        }

        return IsWaitable;
    }
}
