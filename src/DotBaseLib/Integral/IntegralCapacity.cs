using System.Diagnostics;

namespace DotBase.Integral;


/// <summary>
///
/// Calculates integral value and block capacity for a region of a given byte length.
/// Construction does not validate; call <see cref="Validate"/> or
/// <see cref="ThrowIfArgumentOutOfRange"/> when needed.
///
/// </summary>
public readonly struct IntegralCapacity
{
    public static readonly IntegralCapacity Zero = new IntegralCapacity();

    /// <summary> Must be a multiple of the scalar value byte size. </summary>
    public readonly long ByteCount;

    /// <summary> Bytes occupied by single integral value. </summary>
    public readonly int ValueByteCount;

    /// <summary> Count of integral values inside a single block. </summary>
    public readonly int BlockCapacity;

    /// <summary> Bytes occupied by a single full block. </summary>
    public long BlockByteCount { get { return (long)ValueByteCount * BlockCapacity; } }

    /// <summary> Count of complete blocks of integral values for a given byte length. </summary>
    public long BlockCount
    {
        get
        {
            return BlockCapacity == 0
                ? 0
                : TotalValueCount / BlockCapacity;
        }
    }

    /// <summary> Count of scalar values not belonging to a complete trailing block. </summary>
    public int TrailingValueCount
    {
        get
        {
            return BlockCapacity == 0
                ? 0
                : (int)(TotalValueCount % BlockCapacity);
        }
    }

    /// <summary> Count of all complete scalar values.</summary>
    public long TotalValueCount
    {
        get
        {
            return ValueByteCount == 0
                ? 0
                : ByteCount / ValueByteCount;
        }
    }

    public IntegralCapacity()
    {
        ByteCount = 0;
        ValueByteCount = 0;
        BlockCapacity = 0;
    }

    /// <summary>
    /// Capacity for a named integral type. Does not validate; call <see cref="Validate"/>.
    /// For <see cref="IntegralType.NONE"/> prefer <see cref="IntegralCapacity(long, in IntegralFormat)"/>.
    /// </summary>
    public IntegralCapacity(long byteCount, IntegralType valueType, int blockCapacity)
    {
        ByteCount = byteCount;
        ValueByteCount = valueType.Size();
        BlockCapacity = blockCapacity;
    }

    /// <summary>
    /// Capacity from a format (known type, size-only, or empty sentinel).
    /// Does not validate the format or capacity; call <see cref="Validate"/> /
    /// <see cref="IntegralFormat.Validate"/> as needed.
    /// </summary>
    public IntegralCapacity(long byteCount, in IntegralFormat format)
    {
        ByteCount = byteCount;
        ValueByteCount = format.ValueSize;
        BlockCapacity = format.BlockCapacity;
    }

    /// <summary>
    /// Construction for slices of an already-validated span. Does not re-validate.
    /// </summary>
    internal IntegralCapacity(long byteCount, int valueByteCount, int blockCapacity)
    {
        ByteCount = byteCount;
        ValueByteCount = valueByteCount;
        BlockCapacity = blockCapacity;
    }

    public bool IsBlockAligned()
    {
        if (IsZero)
        {
            return true;
        }

        return
            ByteCount >= 0 &&
            ValueByteCount > 0 &&
            BlockCapacity > 0 &&
            (ByteCount % BlockByteCount) == 0;
    }

    public bool IsValueAligned()
    {
        if (IsZero)
        {
            return true;
        }

        return
            ByteCount >= 0 &&
            ValueByteCount > 0 &&
            BlockCapacity > 0 &&
            (ByteCount % ValueByteCount) == 0;
    }

    /// <summary>Empty / zero capacity (no values).</summary>
    public bool IsZero =>
        ByteCount == 0 && ValueByteCount == 0 && BlockCapacity == 0;

    public bool IsValid()
    {
        if (IsZero)
        {
            return true;
        }

        return
            ByteCount >= 0 &&
            ValueByteCount > 0 &&
            BlockCapacity > 0 &&
            (ByteCount % ValueByteCount) == 0;
    }

    /// <summary>
    /// Throws if capacity metadata is not zero and not a consistent value-aligned description.
    /// </summary>
    public void Validate()
    {
        ThrowIfArgumentOutOfRange();
    }

    public void DebugAssertBlockAligned()
    {
        Debug.Assert(IsBlockAligned(), "Capacity metadata must be zero or block-aligned.");
    }

    public void DebugAssertValueAligned()
    {
        Debug.Assert(IsValueAligned(), "Capacity metadata must be zero or value-aligned.");
    }

    public void ThrowIfArgumentOutOfRange()
    {
        if (IsZero)
        {
            return;
        }

        ArgumentOutOfRangeException.ThrowIfNegative(ByteCount);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(ValueByteCount, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(BlockCapacity, 0);
        ArgumentOutOfRangeException.ThrowIfNotEqual(ByteCount % ValueByteCount, 0);
    }
}
