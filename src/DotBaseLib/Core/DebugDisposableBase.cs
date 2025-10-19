using DotBase.Log;

namespace DotBase.Core;


public abstract class DebugDisposableBase 
    : FinalizableBase
{
    // Private properties >>

    private DebugDisposalTracker? _debugTracker;


    // Implementation >>

    public DebugDisposableBase()
    {
        _debugTracker = DebugDisposalTracker.Attach(this);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _debugTracker?.MarkDisposed();
            _debugTracker = null;
        } 
        // else.. - DebugDisposalTracker will handle disposing=false case automatically (disposed by a finalizer).

        base.Dispose(disposing);
    }
}
