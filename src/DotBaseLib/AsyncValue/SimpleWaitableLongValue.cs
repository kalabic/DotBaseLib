using DotBase.AsyncValue.Internal;
using DotBase.Cancellation;
using DotBase.Core;
using static DotBase.Cancellation.CancellableTimeout;

namespace DotBase.AsyncValue;


internal class SimpleWaitableLongValue
    : DisposableBase
{
    // Public properties >>

    public bool IsOpen { get { lock (_lock) { return _state.IsOpen; } } }

    public long Value { get { lock (_lock) { return _state.Value; } } }

    public LongValueRange Range { get { lock (_lock) { return _state.Range; } } }


    // Private members >>

    private readonly object _lock;

    private LockedWaitableValueState _state;

    internal SimpleWaitableLongValue()
    {
        _state = new();
        _lock = _state.Lock;
    }

    internal SimpleWaitableLongValue(LongValueRange range, long value)
    {
        if (range.Compare(value) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                $"{nameof(value)} not in {nameof(range)}");
        }

        _state = new();
        _lock = _state.Lock;
        _state.Range = range;
        _state.Value = value;
    }

    internal SimpleWaitableLongValue(LockedWaitableValueState sharedState)
    {
        _state = sharedState;
        _lock = _state.Lock;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _state.Dispose(); // << Monitor.PulseAll() inside
        }
        base.Dispose(disposing);
    }

    public void Close()
    {
        lock (_lock)
        {
            if (_state.IsDisposed || !_state.IsOpen)
            {
                return;
            }

            _state.CloseLocked();
            Monitor.PulseAll(_lock);
        }
    }

    public void Open()
    {
        lock (_lock)
        {
            if (_state.IsDisposed || _state.IsOpen)
            {
                return;
            }

            _state.OpenLocked();
        }
    }

    public void Open(LongValueRange range, long value)
    {
        lock (_lock)
        {
            if (_state.IsDisposed || _state.IsOpen)
            {
                return;
            }

            if (range.Compare(value) != 0)
            {
                throw new ArgumentOutOfRangeException("Open", $"{nameof(value)} not in {nameof(range)}");
            }

            _state.Range = range;
            _state.Value = value;

            _state.OpenLocked();
        }
    }

    public long IncreaseValue(long increment)
    {
        lock (_lock)
        {
            return SetValueLocked(checked(_state.Value + increment));
        }
    }

    public long DecreaseValue(long decrement)
    {
        lock (_lock)
        {
            return SetValueLocked(checked(_state.Value - decrement));
        }
    }

    public long SetValue(long value)
    {
        lock (_lock)
        {
            return SetValueLocked(value);
        }
    }

    /// <summary>Sets the possible-value range and reevaluates all pending waits.</summary>
    /// <param name="range">The new range. It must contain the current value.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="range"/> does not contain the current value.</exception>
    public void SetRange(LongValueRange range)
    {
        lock (_lock)
        {
            if (_state.IsDisposed)
            {
                throw new ObjectDisposedException(nameof(SimpleWaitableLongValue));
            }

            if (range.Compare(_state.Value) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(range));
            }

            _state.Range = range;

            if (_state.IsOpen)
            {
                Monitor.PulseAll(_lock);
            }
        }
    }

    /// <summary>Atomically sets the current value and possible-value range, then reevaluates all pending waits.</summary>
    /// <param name="value">The new current value.</param>
    /// <param name="range">The new range. It must contain <paramref name="value"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="range"/> does not contain <paramref name="value"/>.</exception>
    public long SetValueAndRange(long value, LongValueRange range)
    {
        lock (_lock)
        {
            if (_state.IsDisposed)
            {
                throw new ObjectDisposedException(nameof(SimpleWaitableLongValue));
            }

            if (range.Compare(value) != 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(SetValueAndRange),
                    $"{nameof(value)} not in {nameof(range)}");
            }

            _state.Range = range;
            _state.Value = value;

            if (_state.IsOpen)
            {
                Monitor.PulseAll(_lock);
            }

            return _state.Value;
        }
    }

    private long SetValueLocked(long value)
    {
        if (_state.IsDisposed)
        {
            throw new ObjectDisposedException(nameof(SimpleWaitableLongValue));
        }

        if (_state.Range.Compare(value) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        _state.Value = value;

        if (_state.IsOpen)
        {
            Monitor.PulseAll(_lock);
        }

        return _state.Value;
    }

    public LongResult WaitEqualTo(long target)
    {
        lock (_lock)
        {
            int generation = _state.Generation;

            while (true)
            {
                var awaitStatus = CurrentAwaiterStatus(generation);
                if (awaitStatus != ResultStatus.SUCCESS)
                {
                    return LongResult.FromStatus(awaitStatus, _state.Value);
                }

                int result = _state.Range.EvaluateEqualTo(_state.Value, target);
                if (result == 0)
                {
                    return LongResult.Success(_state.Value);
                }
                if (result < 0)
                {
                    return LongResult.OutOfRange(_state.Value);
                }

                Monitor.Wait(_lock);
            }
        }
    }

    public LongResult WaitNotEqualTo(long target)
    {
        lock (_lock)
        {
            int generation = _state.Generation;

            while (true)
            {
                var awaitStatus = CurrentAwaiterStatus(generation);
                if (awaitStatus != ResultStatus.SUCCESS)
                {
                    return LongResult.FromStatus(awaitStatus, _state.Value);
                }

                int result = _state.Range.EvaluateNotEqualTo(_state.Value, target);
                if (result == 0)
                {
                    return LongResult.Success(_state.Value);
                }
                if (result < 0)
                {
                    return LongResult.OutOfRange(_state.Value);
                }

                Monitor.Wait(_lock);
            }
        }
    }

    public LongResult WaitGreaterThan(long target)
    {
        lock (_lock)
        {
            int generation = _state.Generation;

            while (true)
            {
                var awaitStatus = CurrentAwaiterStatus(generation);
                if (awaitStatus != ResultStatus.SUCCESS)
                {
                    return LongResult.FromStatus(awaitStatus, _state.Value);
                }

                int result = _state.Range.EvaluateGreaterThan(_state.Value, target);
                if (result == 0)
                {
                    return LongResult.Success(_state.Value);
                }
                if (result < 0)
                {
                    return LongResult.OutOfRange(_state.Value);
                }

                Monitor.Wait(_lock);
            }
        }
    }

    public LongResult WaitGreaterOrEqualTo(long target)
    {
        lock (_lock)
        {
            int generation = _state.Generation;

            while (true)
            {
                var awaitStatus = CurrentAwaiterStatus(generation);
                if (awaitStatus != ResultStatus.SUCCESS)
                {
                    return LongResult.FromStatus(awaitStatus, _state.Value);
                }

                int result = _state.Range.EvaluateGreaterOrEqualTo(_state.Value, target);
                if (result == 0)
                {
                    return LongResult.Success(_state.Value);
                }
                if (result < 0)
                {
                    return LongResult.OutOfRange(_state.Value);
                }

                Monitor.Wait(_lock);
            }
        }
    }

    public LongResult WaitLessThan(long target)
    {
        lock (_lock)
        {
            int generation = _state.Generation;

            while (true)
            {
                var awaitStatus = CurrentAwaiterStatus(generation);
                if (awaitStatus != ResultStatus.SUCCESS)
                {
                    return LongResult.FromStatus(awaitStatus, _state.Value);
                }

                int result = _state.Range.EvaluateLessThan(_state.Value, target);
                if (result == 0)
                {
                    return LongResult.Success(_state.Value);
                }
                if (result < 0)
                {
                    return LongResult.OutOfRange(_state.Value);
                }

                Monitor.Wait(_lock);
            }
        }
    }

    public LongResult WaitLessOrEqualTo(long target)
    {
        lock (_lock)
        {
            int generation = _state.Generation;

            while (true)
            {
                var awaitStatus = CurrentAwaiterStatus(generation);
                if (awaitStatus != ResultStatus.SUCCESS)
                {
                    return LongResult.FromStatus(awaitStatus, _state.Value);
                }

                int result = _state.Range.EvaluateLessOrEqualTo(_state.Value, target);
                if (result == 0)
                {
                    return LongResult.Success(_state.Value);
                }
                if (result < 0)
                {
                    return LongResult.OutOfRange(_state.Value);
                }

                Monitor.Wait(_lock);
            }
        }
    }

    public LongResult WaitEqualTo(long target, int timeout, CancellationToken cancellationToken)
    {
        if (timeout < Timeout.Infinite)
        {
            return LongResult.InvalidArgument();
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return LongResult.Canceled();
        }

        using CancelableTimeoutObserver cto = new(_state);

        lock (_lock)
        {
            int generation = _state.Generation;

            cto.SetCancellableTimeout(timeout, cancellationToken);

            while (true)
            {
                var awaitStatus = CurrentAwaiterStatus(generation, cto);
                if (awaitStatus != ResultStatus.SUCCESS)
                {
                    return LongResult.FromStatus(awaitStatus, _state.Value);
                }

                int result = _state.Range.EvaluateEqualTo(_state.Value, target);
                if (result == 0)
                {
                    return LongResult.Success(_state.Value);
                }
                if (result < 0)
                {
                    return LongResult.OutOfRange(_state.Value);
                }

                Monitor.Wait(_lock);
            }
        }
    }

    public LongResult WaitNotEqualTo(long target, int timeout, CancellationToken cancellationToken)
    {
        if (timeout < Timeout.Infinite)
        {
            return LongResult.InvalidArgument();
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return LongResult.Canceled();
        }

        using CancelableTimeoutObserver cto = new(_state);

        lock (_lock)
        {
            int generation = _state.Generation;

            cto.SetCancellableTimeout(timeout, cancellationToken);

            while (true)
            {
                var awaitStatus = CurrentAwaiterStatus(generation, cto);
                if (awaitStatus != ResultStatus.SUCCESS)
                {
                    return LongResult.FromStatus(awaitStatus, _state.Value);
                }

                int result = _state.Range.EvaluateNotEqualTo(_state.Value, target);
                if (result == 0)
                {
                    return LongResult.Success(_state.Value);
                }
                if (result < 0)
                {
                    return LongResult.OutOfRange(_state.Value);
                }

                Monitor.Wait(_lock);
            }
        }
    }

    public LongResult WaitGreaterThan(long target, int timeout, CancellationToken cancellationToken)
    {
        if (timeout < Timeout.Infinite)
        {
            return LongResult.InvalidArgument();
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return LongResult.Canceled();
        }

        using CancelableTimeoutObserver cto = new(_state);

        lock (_lock)
        {
            int generation = _state.Generation;

            cto.SetCancellableTimeout(timeout, cancellationToken);

            while (true)
            {
                var awaitStatus = CurrentAwaiterStatus(generation, cto);
                if (awaitStatus != ResultStatus.SUCCESS)
                {
                    return LongResult.FromStatus(awaitStatus, _state.Value);
                }

                int result = _state.Range.EvaluateGreaterThan(_state.Value, target);
                if (result == 0)
                {
                    return LongResult.Success(_state.Value);
                }
                if (result < 0)
                {
                    return LongResult.OutOfRange(_state.Value);
                }

                Monitor.Wait(_lock);
            }
        }
    }

    public LongResult WaitGreaterOrEqualTo(long target, int timeout, CancellationToken cancellationToken)
    {
        if (timeout < Timeout.Infinite)
        {
            return LongResult.InvalidArgument();
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return LongResult.Canceled();
        }

        using CancelableTimeoutObserver cto = new(_state);

        lock (_lock)
        {
            int generation = _state.Generation;

            cto.SetCancellableTimeout(timeout, cancellationToken);

            while (true)
            {
                var awaitStatus = CurrentAwaiterStatus(generation, cto);
                if (awaitStatus != ResultStatus.SUCCESS)
                {
                    return LongResult.FromStatus(awaitStatus, _state.Value);
                }

                int result = _state.Range.EvaluateGreaterOrEqualTo(_state.Value, target);
                if (result == 0)
                {
                    return LongResult.Success(_state.Value);
                }
                if (result < 0)
                {
                    return LongResult.OutOfRange(_state.Value);
                }

                Monitor.Wait(_lock);
            }
        }
    }

    public LongResult WaitLessThan(long target, int timeout, CancellationToken cancellationToken)
    {
        if (timeout < Timeout.Infinite)
        {
            return LongResult.InvalidArgument();
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return LongResult.Canceled();
        }

        using CancelableTimeoutObserver cto = new(_state);

        lock (_lock)
        {
            int generation = _state.Generation;

            cto.SetCancellableTimeout(timeout, cancellationToken);

            while (true)
            {
                var awaitStatus = CurrentAwaiterStatus(generation, cto);
                if (awaitStatus != ResultStatus.SUCCESS)
                {
                    return LongResult.FromStatus(awaitStatus, _state.Value);
                }

                int result = _state.Range.EvaluateLessThan(_state.Value, target);
                if (result == 0)
                {
                    return LongResult.Success(_state.Value);
                }
                if (result < 0)
                {
                    return LongResult.OutOfRange(_state.Value);
                }

                Monitor.Wait(_lock);
            }
        }
    }

    public LongResult WaitLessOrEqualTo(long target, int timeout, CancellationToken cancellationToken)
    {
        if (timeout < Timeout.Infinite)
        {
            return LongResult.InvalidArgument();
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return LongResult.Canceled();
        }

        using CancelableTimeoutObserver cto = new(_state);

        lock (_lock)
        {
            int generation = _state.Generation;

            cto.SetCancellableTimeout(timeout, cancellationToken);

            while (true)
            {
                var awaitStatus = CurrentAwaiterStatus(generation, cto);
                if (awaitStatus != ResultStatus.SUCCESS)
                {
                    return LongResult.FromStatus(awaitStatus, _state.Value);
                }

                int result = _state.Range.EvaluateLessOrEqualTo(_state.Value, target);
                if (result == 0)
                {
                    return LongResult.Success(_state.Value);
                }
                if (result < 0)
                {
                    return LongResult.OutOfRange(_state.Value);
                }

                Monitor.Wait(_lock);
            }
        }
    }

    /// <summary>
    /// Default error status if in closed or disposed state, value cannot be awaited.
    /// </summary>
    /// <returns></returns>
    private ResultStatus CurrentAwaiterStatus(int generation)
    {
        if (_state.IsDisposed)
        {
            return ResultStatus.DISPOSED;
        }

        return (_state.IsOpen && (_state.Generation == generation)) ? ResultStatus.SUCCESS : ResultStatus.CLOSED;
    }

    private ResultStatus CurrentAwaiterStatus(int generation, CancelableTimeoutObserver cto)
    {
        if (cto.IsCancelled)
        {
            return ResultStatus.CANCELED;
        }

        if (cto.IsTimeout)
        {
            return ResultStatus.TIMEOUT;
        }

        if (_state.IsDisposed)
        {
            return ResultStatus.DISPOSED;
        }

        return (_state.IsOpen && (_state.Generation == generation)) ? ResultStatus.SUCCESS : ResultStatus.CLOSED;
    }

    private class CancelableTimeoutObserver
        : DisposableBase
    {
        public bool IsCancelled { get { return _isCancelled; } }

        public bool IsTimeout { get { return _isTimeout; } }

        private readonly object _lock;

        private CancellableTimeout? _ct = null;

        private bool _isCancelled;

        private bool _isTimeout;

        internal CancelableTimeoutObserver(LockedWaitableValueState state)
        {
            _lock = state.Lock;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_ct is not null)
                {
                    _ct.CancelledEvent -= CancellationTriggered;
                    _ct.TimeoutEvent -= TimeoutTriggered;
                    _ct.Dispose();
                    _ct = null;
                }
            }
            base.Dispose(disposing);
        }

        internal void SetCancellableTimeout(int timeout, CancellationToken cancellationToken)
        {
            _ct = new CancellableTimeout();
            _ct.CancelledEvent += CancellationTriggered;
            _ct.TimeoutEvent += TimeoutTriggered;
            _ct.SetCancellableTimeout(timeout, cancellationToken, null);
        }

        private Task CancellationTriggered(object? sender, CancellableTimeout.CancelledMsg ev)
        {
            lock (_lock)
            {
                _isCancelled = true;
                Monitor.PulseAll(_lock);
            }

            if (sender is CancellableTimeoutContext cancellableSender)
            {
                cancellableSender.Sender.CancelledEvent -= CancellationTriggered;
                cancellableSender.Sender.TimeoutEvent -= TimeoutTriggered;
                cancellableSender.Sender.Dispose();
            }

            return Task.CompletedTask;
        }

        private Task TimeoutTriggered(object? sender, CancellableTimeout.TimeoutMsg ev)
        {
            lock (_lock)
            {
                _isTimeout = true;
                Monitor.PulseAll(_lock);
            }

            if (sender is CancellableTimeoutContext cancellableSender)
            {
                cancellableSender.Sender.CancelledEvent -= CancellationTriggered;
                cancellableSender.Sender.TimeoutEvent -= TimeoutTriggered;
                cancellableSender.Sender.Dispose();
            }

            return Task.CompletedTask;
        }
    }
}
