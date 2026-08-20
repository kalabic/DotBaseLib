using DotBase.AsyncValue.Internal;
using DotBase.Core;

namespace DotBase.AsyncValue;


/// <summary>
/// Maintains a <see cref="long"/> value and lets callers asynchronously wait for comparison targets.
/// </summary>
/// <remarks>
/// The current range describes values that remain possible. The current value and every admitted target
/// must be inside that range. Narrowing the range completes pending waits that are no longer reachable.
/// </remarks>
public class AsyncWaitableValue
    : DisposableBase
{
    // Public properties >>

    public bool IsOpen { get { lock (_lock) { return _state.IsOpen; } } }

    public long Value { get { lock (_lock) { return _state.Value; } } }


    // Private members >>

    /// <summary> Shared with <see cref="_simpleValue"/>. </summary>
    private readonly LockedWaitableValueState _state;

    private readonly object _lock;

    private readonly SimpleWaitableLongValue _simpleValue;

    private readonly AsyncValueAwaiterList _awaiterList;


    // Implementation >>

    public AsyncWaitableValue()
    {
        _state = new LockedWaitableValueState();
        _lock = _state.Lock;
        _simpleValue = new SimpleWaitableLongValue(_state);
        _awaiterList = new(this);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            lock (_lock)
            {
                _awaiterList.CompleteAll(LongResult.Disposed(_state.Value));
            }

            _simpleValue.Dispose();
            _state.Dispose(); // << Monitor.PulseAll() inside
        }
        base.Dispose(disposing);
    }

    private bool IsDisposedAny()
    {
        return IsDisposed || _state.IsDisposed;
    }

    public void Close()
    {
        lock (_lock)
        {
            if (IsDisposedAny() || !_state.IsOpen)
            {
                return;
            }

            _state.CloseLocked();
            _awaiterList.CompleteAll(LongResult.Closed(_state.Value));
            Monitor.PulseAll(_lock);
        }
    }

    public void Open()
    {
        lock (_lock)
        {
            if (IsDisposedAny() || _state.IsOpen)
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
            if (IsDisposedAny() || _state.IsOpen)
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

    internal void RemoveFinishedTargets()
    {
        lock (_lock)
        {
            if (IsDisposedAny())
            {
                return;
            }

            _awaiterList.RemoveFinishedTargets();
        }
    }

    /// <summary>Sets the possible-value range and reevaluates all pending waits.</summary>
    /// <param name="range">The new range. It must contain the current value.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="range"/> does not contain the current value.</exception>
    public void SetRange(LongValueRange range)
    {
        lock (_lock)
        {
            if (IsDisposedAny())
            {
                throw new ObjectDisposedException(nameof(AsyncWaitableValue));
            }

            if (range.Compare(_state.Value) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(range));
            }

            _state.Range = range;

            if (_state.IsOpen)
            {
                _awaiterList.CheckValueTargets(_state.Value, _state.Range);
            }

            Monitor.PulseAll(_lock);
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

    /// <summary>Sets the current value and reevaluates all pending waits.</summary>
    /// <param name="value">The new value. It must be inside the current range.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is outside the current range.</exception>
    private long SetValueLocked(long value)
    {
        if (IsDisposedAny())
        {
            throw new ObjectDisposedException(nameof(AsyncWaitableValue));
        }

        if (_state.Range.Compare(value) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        _state.Value = value;

        if (_state.IsOpen)
        {
            _awaiterList.CheckValueTargets(_state.Value, _state.Range);
        }

        Monitor.PulseAll(_lock);
        return _state.Value;
    }

    /// <summary>Atomically sets the current value and possible-value range, then reevaluates all pending waits.</summary>
    /// <param name="value">The new current value.</param>
    /// <param name="range">The new range. It must contain <paramref name="value"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="range"/> does not contain <paramref name="value"/>.</exception>
    public void SetValueAndRange(long value, LongValueRange range)
    {
        lock (_lock)
        {
            if (IsDisposedAny())
            {
                throw new ObjectDisposedException(nameof(AsyncWaitableValue));
            }

            if (range.Compare(value) != 0)
            {
                throw new ArgumentOutOfRangeException("SetValueAndRange", $"{nameof(value)} not in {nameof(range)}");
            }

            _state.Range = range;
            _state.Value = value;

            if (_state.IsOpen)
            {
                _awaiterList.CheckValueTargets(_state.Value, _state.Range);
            }

            Monitor.PulseAll(_lock);
        }
    }

    //
    // All APIs throw only in case of critical unhandled exceptions. Every normal
    // error, cancelation or a timeout is returned as `LongResult` status.
    //

    public LongResult WaitEqualTo(long target) => _simpleValue.WaitEqualTo(target);
    public LongResult WaitNotEqualTo(long target) => _simpleValue.WaitNotEqualTo(target);
    public LongResult WaitGreaterThan(long target) => _simpleValue.WaitGreaterThan(target);
    public LongResult WaitGreaterOrEqualTo(long target) => _simpleValue.WaitGreaterOrEqualTo(target);
    public LongResult WaitLessThan(long target) => _simpleValue.WaitLessThan(target);
    public LongResult WaitLessOrEqualTo(long target) => _simpleValue.WaitLessOrEqualTo(target);

    public LongResult WaitEqualTo(long target, int timeout, CancellationToken cancellationToken) => _simpleValue.WaitEqualTo(target, timeout, cancellationToken);
    public LongResult WaitNotEqualTo(long target, int timeout, CancellationToken cancellationToken) => _simpleValue.WaitNotEqualTo(target, timeout, cancellationToken);
    public LongResult WaitGreaterThan(long target, int timeout, CancellationToken cancellationToken) => _simpleValue.WaitGreaterThan(target, timeout, cancellationToken);
    public LongResult WaitGreaterOrEqualTo(long target, int timeout, CancellationToken cancellationToken) => _simpleValue.WaitGreaterOrEqualTo(target, timeout, cancellationToken);
    public LongResult WaitLessThan(long target, int timeout, CancellationToken cancellationToken) => _simpleValue.WaitLessThan(target, timeout, cancellationToken);
    public LongResult WaitLessOrEqualTo(long target, int timeout, CancellationToken cancellationToken) => _simpleValue.WaitLessOrEqualTo(target, timeout, cancellationToken);


    /// <summary>Waits until the current value equals <paramref name="target"/>, subject to timeout and cancellation.</summary>
    /// <param name="target">The comparison target. It must be inside the current range.</param>
    /// <param name="timeout">
    /// A positive timeout in milliseconds. A value of <c>0</c> or <see cref="Timeout.Infinite"/> disables the timer.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token. When <see cref="CancellationToken.CanBeCanceled"/> is <see langword="false"/>,
    /// cancellation is disabled. If both timeout and cancellation are disabled, the wait has no time-based termination.
    /// </param>
    /// <returns>
    /// A result with status <see cref="ResultStatus.SUCCESS"/> when reached, or
    /// <see cref="ResultStatus.OUT_OF_RANGE"/> when the target is inadmissible or unreachable.
    /// </returns>
    /// <remarks>
    /// Immediate outcomes are evaluated in this order: timeout validation, pre-cancellation, target admission,
    /// current-value satisfaction, and reachability. A waiter is registered only when none applies.
    /// </remarks>
    public ValueTask<LongResult> WaitEqualToAsync(long target, int timeout, CancellationToken cancellationToken) => WaitAsync(AsyncComparison.EqualTo, target, timeout, cancellationToken);
    public ValueTask<LongResult> WaitNotEqualToAsync(long target, int timeout, CancellationToken cancellationToken) => WaitAsync(AsyncComparison.NotEqualTo, target, timeout, cancellationToken);
    public ValueTask<LongResult> WaitGreaterThanAsync(long target, int timeout, CancellationToken cancellationToken) => WaitAsync(AsyncComparison.GreaterThan, target, timeout, cancellationToken);
    public ValueTask<LongResult> WaitGreaterOrEqualToAsync(long target, int timeout, CancellationToken cancellationToken) => WaitAsync(AsyncComparison.GreaterOrEqualTo, target, timeout, cancellationToken);
    public ValueTask<LongResult> WaitLessThanAsync(long target, int timeout, CancellationToken cancellationToken) => WaitAsync(AsyncComparison.LessThan, target, timeout, cancellationToken);
    public ValueTask<LongResult> WaitLessOrEqualToAsync(long target, int timeout, CancellationToken cancellationToken) => WaitAsync(AsyncComparison.LessOrEqualTo, target, timeout, cancellationToken);



    /// <summary>Waits until the current value equals <paramref name="target"/>.</summary>
    /// <param name="target">The comparison target. It must be inside the current range.</param>
    /// <returns>
    /// A result with status <see cref="ResultStatus.SUCCESS"/> when reached, or
    /// <see cref="ResultStatus.OUT_OF_RANGE"/> when the target is inadmissible or unreachable.
    /// </returns>
    public ValueTask<LongResult> WaitEqualToAsync(long target)
    {
        lock (_lock)
        {
            var awaitStatus = CurrentAwaiterStatus();
            if (awaitStatus != ResultStatus.SUCCESS)
            {
                return ValueTaskResult.FromStatus(awaitStatus, _state.Value);
            }

            int result = _state.Range.EvaluateEqualTo(_state.Value, target);
            if (result == 0)
            {
                return ValueTaskResult.Success(_state.Value);
            }
            if (result < 0)
            {
                return ValueTaskResult.OutOfRange(_state.Value);
            }

            return AddTargetToList(AsyncComparison.EqualTo, target);
        }
    }

    /// <summary>Waits until the current value differs from <paramref name="target"/>.</summary>
    /// <inheritdoc cref="WaitEqualToAsync(long)"/>
    public ValueTask<LongResult> WaitNotEqualToAsync(long target)
    {
        lock (_lock)
        {
            var awaitStatus = CurrentAwaiterStatus();
            if (awaitStatus != ResultStatus.SUCCESS)
            {
                return ValueTaskResult.FromStatus(awaitStatus, _state.Value);
            }

            int result = _state.Range.EvaluateNotEqualTo(_state.Value, target);
            if (result == 0)
            {
                return ValueTaskResult.Success(_state.Value);
            }
            if (result < 0)
            {
                return ValueTaskResult.OutOfRange(_state.Value);
            }

            return AddTargetToList(AsyncComparison.NotEqualTo, target);
        }
    }

    /// <summary>Waits until the current value is greater than <paramref name="target"/>.</summary>
    /// <inheritdoc cref="WaitEqualToAsync(long)"/>
    public ValueTask<LongResult> WaitGreaterThanAsync(long target)
    {
        lock (_lock)
        {
            var awaitStatus = CurrentAwaiterStatus();
            if (awaitStatus != ResultStatus.SUCCESS)
            {
                return ValueTaskResult.FromStatus(awaitStatus, _state.Value);
            }

            int result = _state.Range.EvaluateGreaterThan(_state.Value, target);
            if (result == 0)
            {
                return ValueTaskResult.Success(_state.Value);
            }
            if (result < 0)
            {
                return ValueTaskResult.OutOfRange(_state.Value);
            }

            return AddTargetToList(AsyncComparison.GreaterThan, target);
        }
    }

    /// <summary>Waits until the current value is greater than or equal to <paramref name="target"/>.</summary>
    /// <inheritdoc cref="WaitEqualToAsync(long)"/>
    public ValueTask<LongResult> WaitGreaterOrEqualToAsync(long target)
    {
        lock (_lock)
        {
            var awaitStatus = CurrentAwaiterStatus();
            if (awaitStatus != ResultStatus.SUCCESS)
            {
                return ValueTaskResult.FromStatus(awaitStatus, _state.Value);
            }

            int result = _state.Range.EvaluateGreaterOrEqualTo(_state.Value, target);
            if (result == 0)
            {
                return ValueTaskResult.Success(_state.Value);
            }
            if (result < 0)
            {
                return ValueTaskResult.OutOfRange(_state.Value);
            }

            return AddTargetToList(AsyncComparison.GreaterOrEqualTo, target);
        }
    }

    /// <summary>Waits until the current value is less than <paramref name="target"/>.</summary>
    /// <inheritdoc cref="WaitEqualToAsync(long)"/>
    public ValueTask<LongResult> WaitLessThanAsync(long target)
    {
        lock (_lock)
        {
            var awaitStatus = CurrentAwaiterStatus();
            if (awaitStatus != ResultStatus.SUCCESS)
            {
                return ValueTaskResult.FromStatus(awaitStatus, _state.Value);
            }

            int result = _state.Range.EvaluateLessThan(_state.Value, target);
            if (result == 0)
            {
                return ValueTaskResult.Success(_state.Value);
            }
            if (result < 0)
            {
                return ValueTaskResult.OutOfRange(_state.Value);
            }

            return AddTargetToList(AsyncComparison.LessThan, target);
        }
    }


    /// <summary>Waits until the current value is less than or equal to <paramref name="target"/>.</summary>
    /// <inheritdoc cref="WaitEqualToAsync(long)"/>
    public ValueTask<LongResult> WaitLessOrEqualToAsync(long target)
    {
        lock (_lock)
        {
            var awaitStatus = CurrentAwaiterStatus();
            if (awaitStatus != ResultStatus.SUCCESS)
            {
                return ValueTaskResult.FromStatus(awaitStatus, _state.Value);
            }

            int result = _state.Range.EvaluateLessOrEqualTo(_state.Value, target);
            if (result == 0)
            {
                return ValueTaskResult.Success(_state.Value);
            }
            if (result < 0)
            {
                return ValueTaskResult.OutOfRange(_state.Value);
            }

            return AddTargetToList(AsyncComparison.LessOrEqualTo, target);
        }
    }

    /// <summary>Implements the timeout-and-cancellation overloads.</summary>
    /// <param name="operation"></param>
    /// <param name="target"></param>
    /// <param name="timeout"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private ValueTask<LongResult> WaitAsync(
        AsyncComparison operation,
        long target,
        int timeout,
        CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            if (timeout < Timeout.Infinite)
            {
                return ValueTaskResult.InvalidArgument();
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return ValueTaskResult.Canceled();
            }

            var awaitStatus = CurrentAwaiterStatus();
            if (awaitStatus != ResultStatus.SUCCESS)
            {
                return ValueTaskResult.FromStatus(awaitStatus, _state.Value);
            }

            int result = _state.Range.EvaluateOperation(operation, _state.Value, target);
            if (result == 0)
            {
                return ValueTaskResult.Success(_state.Value);
            }
            if (result < 0)
            {
                return ValueTaskResult.OutOfRange(_state.Value);
            }

            ValueTaskSource<LongResult> vts = _awaiterList.AddTarget(operation, target, timeout, cancellationToken);
            return new ValueTask<LongResult>(vts, vts.Version);
        }
    }

    /// <summary>
    /// Default error status if in closed or disposed state, value cannot be awaited.
    /// </summary>
    /// <returns></returns>
    private ResultStatus CurrentAwaiterStatus()
    {
        if (IsDisposedAny())
        {
            return ResultStatus.DISPOSED;
        }

        return _state.IsOpen ? ResultStatus.SUCCESS : ResultStatus.CLOSED;
    }

    private ValueTask<LongResult> AddTargetToList(AsyncComparison operation, long target)
    {
        ValueTaskSource<LongResult> vts = _awaiterList.AddTarget(operation, target);
        return new ValueTask<LongResult>(vts, vts.Version);
    }

    // Private static members >>

    private static LongResult GetValueTaskResult(ValueTask<LongResult> valueTask)
    {
        return valueTask.IsCompletedSuccessfully
            ? valueTask.Result
            : valueTask.AsTask().GetAwaiter().GetResult();
    }
}
