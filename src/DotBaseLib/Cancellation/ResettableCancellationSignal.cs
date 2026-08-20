namespace DotBase.Cancellation;


internal sealed class ResettableCancellationSignal
    : ResettableSignal<ResettableCancellationSignal.CancellationSignal>
{
    public readonly struct CancellationSignal
    {
        public readonly CancellationToken Token;

        public CancellationSignal() { Token = CancellationToken.None; }

        public CancellationSignal(CancellationToken token) { Token = token; }
    }


    private sealed class CallbackState
    {
        private readonly ResettableCancellationSignal _owner;

        private readonly long _generation;

        private readonly CancellationToken _token;

        private readonly object? _context;

        internal CallbackState(ResettableCancellationSignal owner, long generation, CancellationToken token, object? context)
        {
            _owner = owner;
            _generation = generation;
            _token = token;
            _context = context;
        }

        internal void InvokeTriggered()
        {
            _owner.InvokeTriggered(_generation, new CancellationSignal(_token), _context);
        }
    }

    public void Set(CancellationToken token, object? context)
    {
        long generation = BeginSet();

        if (!token.CanBeCanceled)
        {
            return;
        }

        var callbackState = new CallbackState(this, generation, token, context);
        CancellationTokenRegistration registration = token.UnsafeRegister(
            static state =>
            {
                var callback = (CallbackState)state!;
                callback.InvokeTriggered();
            },
            callbackState);

        AttachResource(generation, registration);
    }
}
