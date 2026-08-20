namespace DotBase.Cancellation;


internal sealed class ResettableTimeoutSignal
    : ResettableSignal<ResettableTimeoutSignal.TimeoutSignal>
{
    public readonly struct TimeoutSignal { public readonly int Timeout; public TimeoutSignal(int timeout) { Timeout = timeout; } }


    private sealed class CallbackState
    {
        private readonly ResettableTimeoutSignal _owner;

        private readonly long _generation;

        private readonly int _timeout;

        private readonly object? _context;

        internal CallbackState(ResettableTimeoutSignal owner, long generation, int timeout, object? context)
        {
            _owner = owner;
            _generation = generation;
            _timeout = timeout;
            _context = context;
        }

        internal void InvokeTriggered()
        {
            _owner.InvokeTriggered(_generation, new TimeoutSignal(_timeout), _context);
        }
    }

    public void Set(int timeout, object? context)
    {
        if (timeout < System.Threading.Timeout.Infinite)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout));
        }

        long generation = BeginSet();

        if (timeout == System.Threading.Timeout.Infinite)
        {
            return;
        }

        var callbackState = new CallbackState(this, generation, timeout, context);
        var timer = new Timer(
            static state =>
            {
                var callback = (CallbackState)state!;
                callback.InvokeTriggered();
            },
            callbackState,
            timeout,
            System.Threading.Timeout.Infinite);

        AttachResource(generation, timer);
    }
}
