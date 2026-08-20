using System.Threading.Tasks.Sources;

namespace DotBase.AsyncValue;


/// <summary>
///
/// This version:
/// <list type="bullet">
/// <item> Provides <see cref="IsResultSet"/> property.</item>
/// <item> NOT reusable. Invoking <see cref="Reset"/> will throw. </item>
/// <item> Will NOT throw if <see cref="SetResult(T)"/> is invoked multiple times. Result is set only the first time it is invoked. </item>
/// </list>
///
/// </summary>
internal class ValueTaskSource<T> : IValueTaskSource<T>
{
    public static ValueTaskSource<T> FromException(Exception ex)
    {
        ValueTaskSource<T> vts = new();
        vts.SetException(ex);
        return vts;
    }

    public static ValueTaskSource<T> FromResult(T value)
    {
        ValueTaskSource<T> vts = new();
        vts.SetResult(value);
        return vts;
    }

    // Public properties >>

    /// <summary>
    /// Set to true after <see cref="SetResult(T)"/> or <see cref="SetException(Exception)"/> was invoked.
    /// </summary>
    public bool IsResultSet { get { return _resultSet != 0; } }

    public bool IsCanceled { get { return _core.GetStatus(_fixedToken) == ValueTaskSourceStatus.Canceled; } }

    public bool IsFaulted { get { return _core.GetStatus(_fixedToken) == ValueTaskSourceStatus.Faulted; } }

    public bool IsPending { get { return _core.GetStatus(_fixedToken) == ValueTaskSourceStatus.Pending; } }

    public bool IsSucceeded { get { return _core.GetStatus(_fixedToken) == ValueTaskSourceStatus.Succeeded; } }

    public bool RunContinuationsAsynchronously { get { return _core.RunContinuationsAsynchronously; } }

    public short Version { get { return _fixedToken; } }


    // Private data >>

    private ManualResetValueTaskSourceCore<T> _core;

    private short _fixedToken;

    private int _resultSet = 0;


    // Implementation >>

    public ValueTaskSource()
    {
        _core = new()
        {
            RunContinuationsAsynchronously = true
        };
        _fixedToken = _core.Version;
    }

    /// <summary>
    /// Not ready for reuse.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void Reset()
    {
        throw new NotImplementedException();
    }

    public void SetException(Exception exception)
    {
        _ = TrySetException(exception);
    }

    public void SetResult(T result)
    {
        _ = TrySetResult(result);
    }

    public bool TrySetException(Exception exception)
    {
        if (Interlocked.CompareExchange(ref _resultSet, 1, 0) == 0)
        {
            _core.SetException(exception);
            return true;
        }

        return false;
    }

    public bool TrySetResult(T result)
    {
        if (Interlocked.CompareExchange(ref _resultSet, 1, 0) == 0)
        {
            _core.SetResult(result);
            return true;
        }

        return false;
    }

    public ValueTaskSourceStatus GetStatus(short token)
    {
        return _core.GetStatus(token);
    }

    public T GetResult(short token)
    {
        return _core.GetResult(token);
    }

    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
    {
        _core.OnCompleted(continuation, state, token, flags);
    }
}
