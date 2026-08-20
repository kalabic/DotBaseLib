using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotBase.AsyncValue;


/// <summary>Defines an inclusive range of possible <see cref="long"/> values.</summary>
/// <remarks>
/// <see langword="new"/> <see cref="LongValueRange()"/> creates the full <see cref="long"/> range.
/// Because this is a value type, <see langword="default"/>(<see cref="LongValueRange"/>) represents the collapsed range [0, 0].
/// </remarks>
public readonly struct LongValueRange
{
    /// <summary>Gets the inclusive upper bound.</summary>
    public readonly long Maximum;

    /// <summary>Gets the inclusive lower bound.</summary>
    public readonly long Minimum;

    /// <summary>Creates the full <see cref="long"/> range.</summary>
    public LongValueRange()
    {
        Minimum = long.MinValue; Maximum = long.MaxValue;
    }

    /// <summary>Creates an inclusive range with the specified bounds.</summary>
    /// <param name="low">The inclusive lower bound.</param>
    /// <param name="high">The inclusive upper bound.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="low"/> is greater than <paramref name="high"/>.</exception>
    public LongValueRange(long low, long high)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(low, high, nameof(low));
        Minimum = low; Maximum = high;
    }

    /// <summary>Determines whether a value is below, inside, or above the range.</summary>
    /// <returns><c>-1</c> below the range, <c>0</c> inside it, or <c>1</c> above it.</returns>
    public int Compare(long value)
    {
        if (value > Maximum)
        {
            return 1;
        }
        
        if (value < Minimum)
        {
            return -1;
        }

        return 0;
    }

    /// <summary>
    /// This method assumes call site has validated that <paramref name="target"/> is
    /// inside current range.
    /// <para>
    /// Target admission inside range is foremost, but admission alone is insufficient:<br/>
    /// - GreaterThan(range.Maximum) is impossible.<br/>
    /// - LessThan(range.Minimum) is impossible.<br/>
    /// - NotEqualTo(target) is impossible when the range is collapsed at target.<br/>
    /// </para>
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    internal bool IsValidComparison(AsyncComparison operation, long target)
    {
        if (operation == AsyncComparison.NotEqualTo)
        {
            return CanBeNotEqualTo(target);
        }

        if (operation == AsyncComparison.GreaterThan)
        {
            return CanBeGreaterThan(target);
        }

        if (operation == AsyncComparison.LessThan)
        {
            return CanBeLessThan(target);
        }

        return true;
    }

    /// <summary>
    /// NotEqualTo(target) is impossible when the range is collapsed at target.
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool CanBeNotEqualTo(long target)
    {
        return (Maximum != target) || (Minimum != target);
    }

    /// <summary>
    /// GreaterThan(range.Maximum) is impossible.
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool CanBeGreaterThan(long target)
    {
        return Maximum != target;
    }

    /// <summary>
    /// LessThan(range.Minimum) is impossible.
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool CanBeLessThan(long target)
    {
        return Minimum != target;
    }

    /// <summary>
    /// Evaluates relationship between this range and parameters <paramref name="operation"/>, <paramref name="value"/>
    /// and <paramref name="target"/>:<br/>
    /// Is <paramref name="target"/> in range?
    /// Is <paramref name="value"/> meeting the goal?
    /// Is <paramref name="operation"/> even allowed in this range and with provided <paramref name="target"/>?
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="value"></param>
    /// <param name="target"></param>
    /// <returns>
    /// <list type="bullet">
    /// <item> <see langword="1"/> - <paramref name="target"/> is in range, but <paramref name="value"/> is not meeting
    /// the goal. <paramref name="operation"/> is valid in current range. </item>
    /// <item> <see langword="0"/> - <paramref name="target"/> is in range and <paramref name="value"/> is meeting the goal.</item>
    /// <item> <see langword="-1"/> - <paramref name="target"/> is not in range or <paramref name="operation"/> is invalid in current range. </item>
    /// </list>
    /// </returns>
    internal int EvaluateOperation(AsyncComparison operation, long value, long target)
    {
        Debug.Assert(value <= Maximum && value >= Minimum);

        if (target > Maximum || target < Minimum)
        {
            return -1;
        }

        if (operation.Compare(value, target))
        {
            return 0;
        }

        if (!IsValidComparison(operation, target))
        {
            return -1;
        }

        return 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int EvaluateEqualTo(long value, long target)
    {
        Debug.Assert(value <= Maximum && value >= Minimum);

        if (target > Maximum || target < Minimum)
        {
            return -1;
        }

        if (value == target)
        {
            return 0;
        }

        return 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int EvaluateNotEqualTo(long value, long target)
    {
        Debug.Assert(value <= Maximum && value >= Minimum);

        if (target > Maximum || target < Minimum)
        {
            return -1;
        }

        if (value != target)
        {
            return 0;
        }

        if (!CanBeNotEqualTo(target))
        {
            return -1;
        }

        return 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int EvaluateGreaterThan(long value, long target)
    {
        Debug.Assert(value <= Maximum && value >= Minimum);

        if (target > Maximum || target < Minimum)
        {
            return -1;
        }

        if (value > target)
        {
            return 0;
        }

        if (!CanBeGreaterThan(target))
        {
            return -1;
        }

        return 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int EvaluateGreaterOrEqualTo(long value, long target)
    {
        Debug.Assert(value <= Maximum && value >= Minimum);

        if (target > Maximum || target < Minimum)
        {
            return -1;
        }

        if (value >= target)
        {
            return 0;
        }

        return 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int EvaluateLessThan(long value, long target)
    {
        Debug.Assert(value <= Maximum && value >= Minimum);

        if (target > Maximum || target < Minimum)
        {
            return -1;
        }

        if (value < target)
        {
            return 0;
        }

        if (!CanBeLessThan(target))
        {
            return -1;
        }

        return 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int EvaluateLessOrEqualTo(long value, long target)
    {
        Debug.Assert(value <= Maximum && value >= Minimum);

        if (target > Maximum || target < Minimum)
        {
            return -1;
        }

        if (value <= target)
        {
            return 0;
        }

        return 1;
    }
}
