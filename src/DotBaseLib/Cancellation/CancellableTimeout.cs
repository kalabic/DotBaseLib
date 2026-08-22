using DotBase.AsyncEvent;
using DotBase.Core;
using System.Runtime.ExceptionServices;

namespace DotBase.Cancellation;


internal class CancellableTimeout
    : DisposableBase
{
    public enum Result
    {
        Idle = 0,
        Waiting,
        Cancelled,
        Timeout,
        Disposed,
    }

    // Public event types >>

    public readonly struct CancelledMsg
    {
        public readonly CancellationToken Token;

        public CancelledMsg() { Token = CancellationToken.None; }

        public CancelledMsg(CancellationToken token) { Token = token; }
    }

    public readonly struct TimeoutMsg
    {
        public readonly int Timeout;

        public TimeoutMsg() { Timeout = 0; }

        public TimeoutMsg(int timeout) { Timeout = timeout; }
    }

    public readonly struct DisposedMsg { }

    public class CancellableTimeoutContext
    {
        public readonly object? Context;

        public readonly CancellableTimeout Sender;

        public CancellableTimeoutContext(object? context, CancellableTimeout sender)
        {
            Context = context;
            Sender = sender;
        }
    }

    // Public event sources >>

    public event AsyncEventHandler<CancellableTimeout.CancelledMsg>? CancelledEvent;

    public event AsyncEventHandler<CancellableTimeout.TimeoutMsg>? TimeoutEvent;

    public event AsyncEventHandler<CancellableTimeout.DisposedMsg>? DisposedEvent;


    // Private members >>

    private readonly object _lock = new object();

    private readonly ResettableCancellationSignal _cancellationSignal = new();

    private readonly ResettableTimeoutSignal _timeoutSignal = new();

    private bool _clear = true;

    private bool _setting;

    private Result _result = Result.Idle;


    public CancellableTimeout()
    {
        _cancellationSignal.Triggered += CancellationTriggered;
        _timeoutSignal.Triggered += TimeoutTriggered;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Result notification = Result.Idle;
            lock (_lock)
            {
                if (_result == Result.Waiting)
                {
                    _result = Result.Disposed;
                    notification = Result.Disposed;
                }

                _clear = true;
                _setting = false;
            }

            _cancellationSignal.Triggered -= CancellationTriggered;
            _timeoutSignal.Triggered -= TimeoutTriggered;
            _cancellationSignal.Dispose();
            _timeoutSignal.Dispose();

            PublishResult(notification, 0, CancellationToken.None, null);
        }
        base.Dispose(disposing);
    }

    public void Reset()
    {
        bool reset;
        lock (_lock)
        {
            reset = !_clear;
            if (reset)
            {
                _result = Result.Idle;
                _setting = false;
            }
        }

        if (!reset)
        {
            return;
        }

        _cancellationSignal.Reset();
        _timeoutSignal.Reset();

        lock (_lock)
        {
            _clear = true;
        }
    }

    private void LongResult(Result result, int timeout, CancellationToken cancellation, object? context)
    {
        bool publish;
        lock (_lock)
        {
            if (_result != Result.Waiting)
            {
                // Ok, just ignore invalid transitions and race conditions.
                return;
            }

            _result = result;
            publish = !_setting;
        }

        if (publish)
        {
            PublishResult(result, timeout, cancellation, context);
        }
    }

    /// <summary>
    /// Value of <see langword="0"/> for <paramref name="timeout"/> will disable a timer, only cancellation token will
    /// be configured.<br/>
    /// <br/>
    /// If value of <see cref="CancellationToken.CanBeCanceled"/> is <see langword="false"/>, only timer is configured.<br/>
    /// <br/>
    /// <see cref="Timeout.Infinite"/> also disables the timer. If both timer and cancellation are disabled,
    /// the operation remains pending until it is completed or disposed by its owner.<br/>
    /// <br/>
    /// Non-null parameter <paramref name="context"/> changes the event handler semantics:<br/>
    /// - It becomes 'sender' parameter in event callback.<br/>
    /// - If <see langword="null"/>, 'this' object becomes a sender.
    /// </summary>
    /// <param name="timeout"></param>
    /// <param name="token"></param>
    /// <param name="context"></param>
    public void SetCancellableTimeout(int timeout, CancellationToken token, object? context)
    {
        Exception? setupException = null;
        Result notification = Result.Idle;

        lock (_lock)
        {
            ThrowIfDisposed();
            if (!_clear)
            {
                throw new InvalidOperationException("Reset the cancellable timeout before setting it again.");
            }

            _clear = false;
            _setting = true;
            _result = Result.Waiting;

            try
            {
                if (token.CanBeCanceled)
                {
                    _cancellationSignal.Set(token, context);
                }

                if (_result == Result.Waiting && timeout > 0)
                {
                    _timeoutSignal.Set(timeout, context);
                }
            }
            catch (Exception ex)
            {
                setupException = ex;
                _result = Result.Idle;
                _setting = false;
            }

            _setting = false;
            notification = _result == Result.Waiting ? Result.Idle : _result;
        }

        if (setupException is not null)
        {
            _cancellationSignal.Reset();
            _timeoutSignal.Reset();
            lock (_lock)
            {
                _clear = true;
            }

            ExceptionDispatchInfo.Capture(setupException).Throw();
        }

        if (notification != Result.Idle)
        {
            PublishResult(notification, timeout, token, context);
        }
    }

    private void CancellationTriggered(object? sender, ResettableCancellationSignal.CancellationSignal ev)
    {
        LongResult(Result.Cancelled, 0, ev.Token, sender);
    }

    private void TimeoutTriggered(object? sender, ResettableTimeoutSignal.TimeoutSignal ev)
    {
        LongResult(Result.Timeout, ev.Timeout, CancellationToken.None, sender);
    }

    private void PublishResult(Result result, int timeout, CancellationToken cancellation, object? context)
    {
        Task? notification = result switch
        {
            Result.Cancelled => CancelledEvent?.InvokeAsync(new CancellableTimeoutContext(context, this), new CancelledMsg(cancellation)),
            Result.Timeout => TimeoutEvent?.InvokeAsync(new CancellableTimeoutContext(context, this), new TimeoutMsg(timeout)),
            Result.Disposed => DisposedEvent?.InvokeAsync(new CancellableTimeoutContext(context, this), new DisposedMsg()),
            _ => null,
        };

        if (notification is not null)
        {
            _ = notification.ContinueWith(
                static task => { _ = task.Exception; },
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }
    }
}
