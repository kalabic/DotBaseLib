using DotBase.Core;

namespace DotBase.AsyncValue.Internal;


internal class LockedWaitableValueState
    : DisposableBase
{
    // Public properties >>

    internal object Lock { get { return _lock; } }

    internal bool IsOpen {  get { return _isOpen; } }

    internal int Generation {  get { return _generation; } }

    public long Value;

    public LongValueRange Range;


    // Private members >>

    private readonly object _lock = new object();

    private bool _isOpen;

    private int _generation;

    public LockedWaitableValueState()
    {
        _isOpen = true;
        Value = 0;
        Range = new LongValueRange();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            lock (_lock)
            {
                _isOpen = false;
                Monitor.PulseAll(_lock);
            }
        }
        base.Dispose(disposing);
    }

    internal void OpenLocked()
    {
        _isOpen = true;
    }

    internal void CloseLocked()
    {
        _generation++;
        _isOpen = false;
    }
}
