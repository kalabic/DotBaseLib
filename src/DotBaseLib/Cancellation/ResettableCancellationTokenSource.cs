namespace DotBase.Cancellation;


/// <summary>
/// Owns replaceable cancellation-token sources and defers their disposal while
/// cancellation operations are active.
/// </summary>
internal sealed class ResettableCancellationTokenSource
    : IDisposable
{
    private readonly object _lock = new();

    private readonly List<IDisposable> _disposeAfter = new();

    private readonly List<CancellationTokenSource> _retiredSources = new();

    private CancellationTokenSource _source = new();

    private int _activeOperations;

    private bool _cancellationStarted;

    private bool _isDisposed;

    public bool IsCancellationRequested
    {
        get
        {
            lock (_lock)
            {
                return _source.IsCancellationRequested;
            }
        }
    }

    public CancellationToken Token
    {
        get
        {
            lock (_lock)
            {
                ObjectDisposedException.ThrowIf(_isDisposed, this);
                return _source.Token;
            }
        }
    }

    public CancellationOperation? TryBeginCancellation()
    {
        lock (_lock)
        {
            if (_isDisposed || _cancellationStarted)
            {
                return null;
            }

            _cancellationStarted = true;
            _activeOperations++;
            return new CancellationOperation(this, _source);
        }
    }

    public bool TryReset()
    {
        lock (_lock)
        {
            if (_isDisposed)
            {
                return false;
            }

            _cancellationStarted = false;
            _retiredSources.Add(_source);
            _source = new CancellationTokenSource();
        }

        DisposeResourcesIfIdle();
        return true;
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _retiredSources.Add(_source);
        }

        DisposeResourcesIfIdle();
    }

    public void DisposeAfter(IDisposable resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        lock (_lock)
        {
            if (!_isDisposed || (_activeOperations != 0))
            {
                _disposeAfter.Add(resource);
                return;
            }
        }

        resource.Dispose();
    }

    private void ReleaseOperation()
    {
        lock (_lock)
        {
            _activeOperations--;
        }

        DisposeResourcesIfIdle();
    }

    private void DisposeResourcesIfIdle()
    {
        IDisposable[] disposeAfter;
        CancellationTokenSource[] sources;
        lock (_lock)
        {
            if (_activeOperations != 0)
            {
                return;
            }

            sources = _retiredSources.ToArray();
            _retiredSources.Clear();

            disposeAfter = _isDisposed
                ? _disposeAfter.ToArray()
                : Array.Empty<IDisposable>();

            if (_isDisposed)
            {
                _disposeAfter.Clear();
            }
        }

        foreach (CancellationTokenSource source in sources)
        {
            source.Dispose();
        }

        foreach (IDisposable resource in disposeAfter)
        {
            resource.Dispose();
        }
    }

    internal sealed class CancellationOperation
        : IDisposable
    {
        private ResettableCancellationTokenSource? _owner;

        public CancellationTokenSource Source { get; }

        internal CancellationOperation(
            ResettableCancellationTokenSource owner,
            CancellationTokenSource source)
        {
            _owner = owner;
            Source = source;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _owner, null)?.ReleaseOperation();
        }
    }
}
