namespace DotBase.AsyncValue.Internal;


/// <summary>
/// This class assumes it is kept under lock by <see cref="AsyncWaitableValue"/>.
/// </summary>
internal class AsyncValueAwaiterList
{
    private readonly WeakReference<AsyncWaitableValue>? _owner;

    private readonly List<AsyncValueAwaiterTask> _list = new();

    internal int ActiveCount
    {
        get { return _list.Count; }
    }

    internal AsyncValueAwaiterList(AsyncWaitableValue? owner = null)
    {
        _owner = (owner is not null) ? new(owner) : null;
    }

    internal int CompleteAll(LongResult result)
    {
        int completed = 0;

        foreach (AsyncValueAwaiterTask item in _list)
        {
            if (item.TryComplete(result))
            {
                completed++;
            }
        }

        _list.Clear();
        return completed;
    }

    internal ValueTaskSource<LongResult> AddTarget(
        AsyncComparison comparison,
        long target)
    {
        var item = new AsyncValueAwaiterTask();
        item.SetComparison(comparison, target);
        _list.Add(item);
        return item.VTS;
    }

    internal ValueTaskSource<LongResult> AddTarget(
        AsyncComparison comparison,
        long target,
        int timeout,
        CancellationToken cancellationToken)
    {
        if (timeout < System.Threading.Timeout.Infinite)
        {
            return ValueTaskSource<LongResult>.FromResult(LongResult.INVALID_ARGUMENT);
        }

        var item = new AsyncValueAwaiterTask();
        item.SetComparison(comparison, target);
        _list.Add(item);

        if (timeout > 0 || cancellationToken.CanBeCanceled)
        {
            try
            {
                item.SetCancellableTimeout(_owner, timeout, cancellationToken);
            }
            catch (Exception)
            {
                _list.Remove(item);
                item.Dispose();
                return ValueTaskSource<LongResult>.FromResult(LongResult.FAILED);
            }
        }

        return item.VTS;
    }

    /// <summary>
    /// Execute value vs target comparison on all list members in 'pending' state.
    /// </summary>
    /// <param name="value"></param>
    /// <returns> Count of items that reached their target. </returns>
    internal int CheckValueTargets(long value, LongValueRange range)
    {
        int count = 0;

        for (int i=0; i<_list.Count; i++)
        {
            var item = _list[i];
            if (item.IsPending && item.CheckValueTarget(value, range))
            {
                count++;
            }
        }

        for (int i = _list.Count - 1; i >= 0; i--)
        {
            if (!_list[i].IsPending)
            {
                _list.RemoveAt(i);
            }
        }

        return count;
    }

    internal int RemoveFinishedTargets()
    {
        int count = 0;

        for (int i = _list.Count - 1; i >= 0; i--)
        {
            if (!_list[i].IsPending)
            {
                _list.RemoveAt(i);
                count++;
            }
        }

        return count;
    }
}
