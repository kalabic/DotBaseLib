using System.Diagnostics;

namespace DotBase.Integral;


/// <summary>
///
/// Calculates and validates integral value and block capacity for a buffer allocated with a certain count of bytes.
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
    /// Use these methods to validate arguments: <see cref="IsValid"/> or  <see cref="DebugAssert"/> or <see cref="ThrowIfArgumentOutOfRange"/>
    /// </summary>
    /// <param name="byteCount">Must be a multiple of the scalar value byte size.</param>
    /// <param name="valueType"></param>
    /// <param name="blockCapacity">Count of integral values inside a single block.</param>
    public IntegralCapacity(long byteCount, IntegralType valueType, int blockCapacity)
    {
        IntegralFormat format = new(
            valueType,
            blockCapacity);

        ByteCount = byteCount;
        ValueByteCount = format.ValueType.Size();
        BlockCapacity = format.BlockCapacity;
    }

    public bool IsValid()
    {
        if (ByteCount == 0 &&
            ValueByteCount == 0 &&
            BlockCapacity == 0)
        {
            return true;
        }

        return
            ByteCount >= 0 &&
            ValueByteCount > 0 &&
            BlockCapacity > 0 &&
            (ByteCount % ValueByteCount) == 0;
    }

    public void DebugAssert()
    {
        Debug.Assert(
            IsValid(),
            "Capacity metadata must be normalized empty metadata or describe complete scalar values.");
    }

    public void ThrowIfArgumentOutOfRange()
    {
        if (ByteCount == 0 &&
            ValueByteCount == 0 &&
            BlockCapacity == 0)
        {
            return;
        }

        ArgumentOutOfRangeException.ThrowIfNegative(ByteCount);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(ValueByteCount, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(BlockCapacity, 0);

        ArgumentOutOfRangeException.ThrowIfNotEqual(ByteCount % ValueByteCount, 0);
    }
}
