using DotBase.Cancellation;
using DotBase.Core;
using System.Numerics;

namespace DotBase.Tools;


/// <summary> 
///
/// Supports a single high mark and a single low mark waiter.
/// High and low mark values have meaning only while they have
/// a waiter. They reset to default values as soon as waiter
/// stops waiting.
///
/// </summary>
internal class SimpleWaitableValue<T>
    : DisposableBase
    where T : INumber<T>, IMinMaxValue<T>
{
    public bool IsOpen { get { return _isOpen; } }

    /// <summary> High and low mark values have meaning only while they have a waiter. </summary>
    public T WaitingHighMark { get { return _highMark; } }

    /// <summary> High and low mark values have meaning only while they have a waiter. </summary>
    public T WaitingLowMark { get { return _lowMark; } }

    private readonly object _lock = new object();

    private readonly CancellableEventSlim _highMarkEvent = new CancellableEventSlim();

    private readonly CancellableEventSlim _lowMarkEvent = new CancellableEventSlim();

    private T _highMark = T.MinValue;

    private T _lowMark = T.MaxValue;

    private T _value = T.Zero;

    private bool _isOpen = true;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            lock (_lock)
            {
                // 'IsDisposed' flag is 'true' already here, so no race condition possible.
                Close();

                _highMarkEvent.Dispose();
                _lowMarkEvent.Dispose();
            }
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// See remarks about reopening: <see cref="Open"/>.
    /// </summary>
    public void Close()
    {
        lock (_lock)
        {
            if (_isOpen)
            {
                _isOpen = false;

                // Set waiters free.
                _highMarkEvent.Set();
                _lowMarkEvent.Set();
            }
        }
    }

    /// <summary>
    /// Reopening immediately after closing may cause waiters released by a preceding
    /// Close() to return true. Such waiters must recheck their condition.
    /// </summary>
    public void Open()
    {
        lock (_lock)
        {
            if (!IsDisposed)
            {
                _isOpen = true;
            }
        }
    }

    public T Decrease(T decrement)
    {
        lock (_lock)
        {
            return SetValue(checked(_value - decrement));
        }
    }

    public T Increase(T increment)
    {
        lock (_lock)
        {
            return SetValue(checked(_value + increment));
        }
    }

    public T SetValue(T value)
    {
        lock (_lock)
        {
            _value = value;

            if (IsOpen)
            {
                if (!_highMarkEvent.IsSet)
                {
                    if (_value >= _highMark)
                    {
                        _highMarkEvent.Set();
                    }
                }

                if (!_lowMarkEvent.IsSet)
                {
                    if (_value <= _lowMark)
                    {
                        _lowMarkEvent.Set();
                    }
                }
            }

            return _value;
        }
    }


    /// <summary>
    /// Waits until the high-mark event is signaled. The value still may change after the signal.
    /// </summary>
    /// <returns> 
    ///   <list type="bullet">
    ///      <item> <see langword="true"/> - High mark value is reached. In case of reopening <see langword="true"/> result may be a stale or spurious wake-up. </item>
    ///      <item> <see langword="false"/> - Closure or cancellation was observed. </item>
    /// </list>
    /// </returns>
    public bool WaitHighMarkValue(T highMark)
    {
        if (ResetStoredHighMarkEvent(highMark))
        {
            bool result = _highMarkEvent.Wait();
            lock (_lock) { _highMark = T.MinValue; }
            if (!result)
            {
                return false;
            }
        }

        return _isOpen;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="highMark"></param>
    /// <returns> 
    ///   <list type="bullet">
    ///     <item> <c>true</c> if high mark value is not reached. Caller needs to wait. </item>
    ///     <item> <c>false</c> if high mark value is reached, or if object was disposed. Caller should not wait. </item>
    /// </list>
    /// </returns>
    private bool ResetStoredHighMarkEvent(T highMark)
    {
        lock (_lock)
        {
            if (!_isOpen || (_value >= highMark))
            {
                _highMarkEvent.Set();
                _highMark = T.MinValue;
                return false;
            }
            else
            {
                _highMarkEvent.Reset();
                _highMark = highMark;
                return true;
            }
        }
    }


    /// <summary>
    /// Waits until the low-mark event is signaled. The value still may change after the signal.
    /// </summary>
    /// <returns> 
    ///   <list type="bullet">
    ///      <item> <see langword="true"/> - Low mark value is reached. In case of reopening <see langword="true"/> result may be a stale or spurious wake-up. </item>
    ///      <item> <see langword="false"/> - Closure or cancellation was observed. </item>
    /// </list>
    /// </returns>
    public bool WaitLowMarkValue(T lowMark)
    {
        if (ResetStoredLowMarkEvent(lowMark))
        {
            bool result = _lowMarkEvent.Wait();
            lock (_lock) { _lowMark = T.MaxValue; }
            if (!result)
            {
                return false;
            }
        }

        return _isOpen;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="lowMark"></param>
    /// <returns> 
    ///   <list type="bullet">
    ///     <item> <c>true</c> if low mark value is not reached. Caller needs to wait. </item>
    ///     <item> <c>false</c> if low mark value is reached, or if object was disposed. Caller should not wait. </item>
    /// </list>
    /// </returns>
    private bool ResetStoredLowMarkEvent(T lowMark)
    {
        lock (_lock)
        {
            if (!_isOpen || (_value <= lowMark))
            {
                _lowMarkEvent.Set();
                _lowMark = T.MaxValue;
                return false;
            }
            else
            {
                _lowMarkEvent.Reset();
                _lowMark = lowMark;
                return true;
            }
        }
    }
}
