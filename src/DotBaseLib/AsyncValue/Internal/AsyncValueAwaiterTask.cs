using DotBase.Cancellation;
using static DotBase.Cancellation.CancellableTimeout;

namespace DotBase.AsyncValue.Internal;


/// <summary>
/// No reuse. New instance every time waitable value is awaited.
/// </summary>
internal sealed class AsyncValueAwaiterTask
    : IDisposable
{
    public bool IsPending
    {
        get { return _vts.IsPending; }
    }

    public ValueTaskSource<LongResult> VTS
    {
        get { return _vts; }
    }

    private AsyncComparison _comparison = AsyncComparison.Undefined;

    private long _target;

    private readonly ValueTaskSource<LongResult> _vts = new();

    private CancellableTimeout? _cancellableTimeout = null;


    // Implementation >>

    internal AsyncValueAwaiterTask()
    { }

    public void Dispose()
    {
        DisposeCancellableTimeout();
    }

    internal bool TryComplete(LongResult result)
    {
        bool completed = _vts.TrySetResult(result);
        DisposeCancellableTimeout();
        return completed;
    }

    public void DisposeCancellableTimeout()
    {
        CancellableTimeout? existing = Interlocked.Exchange(ref _cancellableTimeout, null);
        if (existing is not null)
        {
            existing.CancelledEvent -= CancellationTriggered;
            existing.TimeoutEvent -= TimeoutTriggered;
            existing.Dispose();
        }
    }

    public void DisposeCancellableTimeout(CancellableTimeout cancellableTimeout)
    {
        _ = Interlocked.CompareExchange(ref _cancellableTimeout, null, cancellableTimeout);
        cancellableTimeout.CancelledEvent -= CancellationTriggered;
        cancellableTimeout.TimeoutEvent -= TimeoutTriggered;
        cancellableTimeout.Dispose();
    }

    internal void SetComparison(AsyncComparison comparison, long target)
    {
        _comparison = comparison;
        _target = target;
    }

    internal void SetCancellableTimeout(WeakReference<AsyncWaitableValue>? owner, int timeout, CancellationToken cancellationToken)
    {
        var cancellableTimeout = new CancellableTimeout();
        _cancellableTimeout = cancellableTimeout;
        cancellableTimeout.CancelledEvent += CancellationTriggered;
        cancellableTimeout.TimeoutEvent += TimeoutTriggered;
        cancellableTimeout.SetCancellableTimeout(timeout, cancellationToken, owner);
    }

    /// <summary>
    /// Invoked from <see cref="AsyncValueAwaiterList"/>, so that class is responsible
    /// for cleanup and not this method.
    /// </summary>
    /// <param name="value"></param>
    /// <returns> <see langword="true"/> if target is in <paramref name="range"/> and
    /// comparison with <paramref name="value"/> is satisfied.<br/>
    /// Additionally, <see cref="ValueTaskSource{T}.TrySetResult"/> must not fail.
    /// </returns>
    internal bool CheckValueTarget(long value, LongValueRange range)
    {
        bool isInRange = range.Compare(_target) == 0;
        if (isInRange)
        {
            isInRange = range.IsValidComparison(_comparison, _target);
        }

        if (isInRange)
        {
            bool targetReached = _comparison.Compare(value, _target);
            if (!targetReached || !_vts.TrySetResult(LongResult.Success(value)))
            {
                return false;
            }

            DisposeCancellableTimeout();
            return true;
        }
        else
        {
            if (_vts.TrySetResult(LongResult.OutOfRange(value)))
            {
                DisposeCancellableTimeout();
            }
            return false;
        }
    }

    /// <summary>
    /// System callback, explicit cleanup is needed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="ev"></param>
    /// <returns></returns>
    private Task CancellationTriggered(object? sender, CancellableTimeout.CancelledMsg ev)
    {
        if (!_vts.TrySetResult(LongResult.CANCELED))
        {
            return Task.CompletedTask;
        }

        if (sender is CancellableTimeoutContext cancellableSender)
        {
            StartCleanup(cancellableSender);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// System callback, explicit cleanup is needed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="ev"></param>
    /// <returns></returns>
    private Task TimeoutTriggered(object? sender, CancellableTimeout.TimeoutMsg ev)
    {
        if (!_vts.TrySetResult(LongResult.TIMEOUT))
        {
            return Task.CompletedTask;
        }

        if (sender is CancellableTimeoutContext cancellableSender)
        {
            StartCleanup(cancellableSender);
        }

        return Task.CompletedTask;
    }

    private void StartCleanup(CancellableTimeoutContext cancellableSender)
    {
        _ = Interlocked.CompareExchange(ref _cancellableTimeout, null, cancellableSender.Sender);
        cancellableSender.Sender.CancelledEvent -= CancellationTriggered;
        cancellableSender.Sender.TimeoutEvent -= TimeoutTriggered;

        ThreadPool.UnsafeQueueUserWorkItem(static (CancellableTimeoutContext sender) =>
        {
            try
            {
                var cancellableTimeout = sender.Sender;
                cancellableTimeout.Dispose();

                if (sender.Context is WeakReference<AsyncWaitableValue> owner)
                {
                    if (owner is not null && owner.TryGetTarget(out var strongOwner))
                    {
                        strongOwner.RemoveFinishedTargets();
                    }
                }
            }
            catch { }

        }, cancellableSender, false);
    }
}
