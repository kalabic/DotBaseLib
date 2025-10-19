using System.Diagnostics;

namespace DotBase.Core;


public abstract class DisposableBase 
    : IDisposableInfo
    , IDisposable
{
    // Public properties >>

    public bool IsDisposed => Volatile.Read(ref _isDisposed) != 0;

    public IDisposableInfo.STATE DisposeState { get { return _status; } }


    // Private properties >>

    private int _isDisposed = 0;

    private IDisposableInfo.STATE _status = IDisposableInfo.STATE.NOT_DISPOSED;


    // Implementation >>

    protected void ThrowIfDisposed()
    {
        if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
    }


    internal bool TryBeginDispose()
    {
        return Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0;
    }


    public void Dispose()
    {
        if (TryBeginDispose())
        {
            Dispose(true);
            Debug.Assert(_status == IDisposableInfo.STATE.DISPOSED, "Overridden dispose method not invoked. Is this what you wanted?");
            GC.SuppressFinalize(this);
        }
    }


    /// <summary> Derived types MUST call base.Dispose(bool) </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Still safe to invoke Dispose(), CloseBuffer(), etc. on managed objects.
            _status = IDisposableInfo.STATE.DISPOSED;
        }
        else
        {
            // Danger zone for managed objects.
            _status = IDisposableInfo.STATE.FINALIZED;
        }
    }
}
