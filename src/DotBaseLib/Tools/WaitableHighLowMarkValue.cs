using DotBase.AsyncValue;
using DotBase.Core;

namespace DotBase.Tools;


/// <summary>
/// Maintains a <see cref="long"/> value and provides synchronous high- and low-mark waits.
/// </summary>
/// <remarks>
/// Multiple callers may wait independently. Closing or disposing the value completes all
/// pending waits with the corresponding terminal <see cref="ResultStatus"/>.
/// Callers must switch on <see cref="LongResult.Status"/>: implicit conversion to
/// <see langword="bool"/> is <see cref="ResultStatus.SUCCESS"/> only and collapses
/// <see cref="ResultStatus.CLOSED"/> vs <see cref="ResultStatus.OUT_OF_RANGE"/>.
/// A wait target outside the current range returns <see cref="ResultStatus.OUT_OF_RANGE"/>
/// immediately. <see cref="Close"/> completes waiters with <see cref="ResultStatus.CLOSED"/>
/// rather than collapsing the range.
/// </remarks>
internal sealed class WaitableHighLowMarkValue
    : DisposableBase
{
    public bool IsOpen => _value.IsOpen;

    public long Value => _value.Value;

    private readonly SimpleWaitableLongValue _value;

    public WaitableHighLowMarkValue()
    {
        _value = new SimpleWaitableLongValue();
    }

    public WaitableHighLowMarkValue(LongValueRange range, long value = 0)
    {
        _value = new SimpleWaitableLongValue(range, value);
    }

    public WaitableHighLowMarkValue(long minimum, long maximum, long value = 0)
        : this(new LongValueRange(minimum, maximum), value)
    {
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _value.Dispose();
        }

        base.Dispose(disposing);
    }

    public void Close()
    {
        _value.Close();
    }

    public void Open()
    {
        _value.Open();
    }

    public long Decrease(long decrement)
    {
        return _value.DecreaseValue(decrement);
    }

    public long Increase(long increment)
    {
        return _value.IncreaseValue(increment);
    }

    public long SetValue(long value)
    {
        return _value.SetValue(value);
    }

    /// <summary>Sets the possible-value range and reevaluates all pending waits.</summary>
    /// <param name="range">The new range. It must contain the current value.</param>
    public void SetRange(LongValueRange range)
    {
        _value.SetRange(range);
    }

    /// <summary>Atomically sets the current value and possible-value range, then reevaluates all pending waits.</summary>
    public long SetValueAndRange(long value, LongValueRange range)
    {
        return _value.SetValueAndRange(value, range);
    }

    /// <summary>Waits until the current value is greater than or equal to <paramref name="highMark"/>.</summary>
    public LongResult WaitHighMarkValue(long highMark)
    {
        return _value.WaitGreaterOrEqualTo(highMark);
    }

    /// <summary>Waits until the current value is less than or equal to <paramref name="lowMark"/>.</summary>
    public LongResult WaitLowMarkValue(long lowMark)
    {
        return _value.WaitLessOrEqualTo(lowMark);
    }
}
