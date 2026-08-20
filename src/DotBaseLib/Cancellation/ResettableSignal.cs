namespace DotBase.Cancellation;


internal abstract class ResettableSignal<TEvent>
    : IDisposable
{
    public enum State
    {
        Idle = 0,
        Waiting,
        Triggered,
        Disposed,
    }

    public event EventHandler<TEvent>? Triggered;

    public State CurrentState
    {
        get
        {
            lock (_lock)
            {
                return _state;
            }
        }
    }


    private readonly object _lock = new object();

    private IDisposable? _resource;

    private long _generation;

    private State _state = State.Idle;


    public void Reset()
    {
        IDisposable? resource;
        lock (_lock)
        {
            if (_state == State.Disposed)
            {
                return;
            }

            _generation++;
            _state = State.Idle;
            resource = _resource;
            _resource = null;
        }

        resource?.Dispose();
    }

    public void Dispose()
    {
        IDisposable? resource;
        lock (_lock)
        {
            if (_state == State.Disposed)
            {
                return;
            }

            _generation++;
            _state = State.Disposed;
            resource = _resource;
            _resource = null;
            Triggered = null;
        }

        resource?.Dispose();
        GC.SuppressFinalize(this);
    }

    protected long BeginSet()
    {
        lock (_lock)
        {
            ObjectDisposedException.ThrowIf(_state == State.Disposed, this);
            if (_state != State.Idle)
            {
                throw new InvalidOperationException("Reset the signal before setting it again.");
            }

            _state = State.Waiting;
            return ++_generation;
        }
    }

    protected void AttachResource(long generation, IDisposable resource)
    {
        bool keepResource;
        lock (_lock)
        {
            keepResource = (_state != State.Disposed)
                && (_generation == generation)
                && (_resource is null);

            if (keepResource)
            {
                _resource = resource;
            }
        }

        if (!keepResource)
        {
            resource.Dispose();
        }
    }

    /// <summary>
    /// Non-null parameter <paramref name="context"/> changes the event handler semantics:<br/>
    /// - It becomes 'sender' parameter in event callback.<br/>
    /// - If <see langword="null"/>, 'this' object becomes a sender.
    /// </summary>
    /// <param name="generation"></param>
    /// <param name="ev"></param>
    /// <param name="context"></param>
    protected void InvokeTriggered(long generation, TEvent ev, object? context = null)
    {
        EventHandler<TEvent>? triggered;
        lock (_lock)
        {
            if ((_state != State.Waiting) || (_generation != generation))
            {
                return;
            }

            _state = State.Triggered;
            triggered = Triggered;
        }

        if (context is not null)
        {
            triggered?.Invoke(context, ev);
        }
        else
        {
            triggered?.Invoke(this, ev);
        }
    }
}
