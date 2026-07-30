using DotBase.Buffers;
using DotBase.Integral.Internal;

namespace DotBase.Integral;


/// <summary>
///
/// Represents a contiguous view into integral value memory, with explicit knowledge
/// of layout and format.
///
/// <para>
/// <see cref="Ptr"/> and <see cref="BytePtr"/> retain the original base pointer.
/// <see cref="Offset"/> is applied only when calculating <see cref="DataPtr"/> or
/// the address of an individual value.
/// </para>
///
/// </summary>
public readonly unsafe struct IntegralSpan
{
    public static readonly IntegralSpan Empty = new();


    /// <summary> Integral pointer holds information about format of values it is referencing. </summary>
    public readonly IntegralPtr Ptr;

    /// <summary> Measured in BYTES. </summary>
    public readonly long Offset;

    /// <summary> Measured in BYTES. </summary>
    public readonly long Length;

    /// <summary> Count of bytes, integral values, and blocks in the region. </summary>
    public readonly IntegralCapacity CountOf;

    /// <summary> Gets the original, unadjusted base pointer. </summary>
    public byte* BytePtr { get { return Ptr.BytePtr; } }

    /// <summary> Gets the pointer to the first byte in this span. </summary>
    public byte* DataPtr { get { return Ptr.BytePtr + Offset; } }

    public IntegralFormat Format { get { return Ptr.Fmt; } }

    public IntegralType IntegralValueType { get { return Ptr.Fmt.ValueType; } }

    public long IntegralOffset
    {
        get
        {
            return CountOf.ValueByteCount == 0
                ? 0
                : Offset / CountOf.ValueByteCount;
        }
    }

    public long IntegralLength { get { return CountOf.TotalValueCount; } }

    public long BlockLength { get { return CountOf.BlockCount; } }

    public int TrailingValueCount { get { return CountOf.TrailingValueCount; } }


    public IntegralSpan()
    {
        Ptr = IntegralPtr.NULL;
        Length = 0;
        Offset = 0;
        CountOf = IntegralCapacity.Zero;
    }

    public IntegralSpan(byte* ptr, long offset, long length, IntegralType valueType, int blockValueCount)
        : this(ptr, offset, length, new IntegralFormat(valueType, blockValueCount))
    { }

    public IntegralSpan(byte* ptr, long offset, long length, IntegralFormat fmt)
        : this(new IntegralPtr(ptr, fmt), offset, length)
    { }

    public IntegralSpan(in IntegralPtr ptr, long offset, long length)
    {
        ptr.Fmt.Validate();

        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        if (ptr.IsNull && (offset != 0 || length != 0))
        {
            throw new ArgumentException(
                "A null integral pointer can describe only the empty span.",
                nameof(ptr));
        }

        Ptr = ptr;
        Length = length;
        Offset = offset;
        CountOf = new IntegralCapacity(length, ptr.Fmt.ValueType, ptr.Fmt.BlockCapacity);
        CountOf.ThrowIfArgumentOutOfRange();

        if (CountOf.ValueByteCount > 0 &&
            (offset % CountOf.ValueByteCount) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(offset),
                offset,
                "The byte offset must be aligned to the scalar value size.");
        }
    }

    /// <summary> Parameter <paramref name="index"/> is the linear index of an integral value. </summary>
    public T AtIndex<T>(long index)
        where T : unmanaged
    {
        ValidateTypeCompatibility<T>();
        byte* valuePtr = GetValueBytePtr(index);
        return ReadScalar<T>(valuePtr);
    }

    /// <summary> Parameter <paramref name="blockValueIndex"/> is index of a value inside certain block. </summary>
    public T AtBlockIndex<T>(long blockIndex, int blockValueIndex)
        where T : unmanaged
    {
        ValidateTypeCompatibility<T>();
        byte* valuePtr = GetBlockValueBytePtr(blockIndex, blockValueIndex);
        return ReadScalar<T>(valuePtr);
    }

    /// <summary> Writes a value at the supplied linear integral-value index. </summary>
    public void SetAtIndex<T>(long index, T value)
        where T : unmanaged
    {
        ValidateTypeCompatibility<T>();
        byte* valuePtr = GetValueBytePtr(index);
        WriteScalar(valuePtr, value);
    }

    /// <summary> Writes a value at the supplied block and within-block indices. </summary>
    public void SetAtBlockIndex<T>(
        long blockIndex,
        int blockValueIndex,
        T value)
        where T : unmanaged
    {
        ValidateTypeCompatibility<T>();
        byte* valuePtr = GetBlockValueBytePtr(blockIndex, blockValueIndex);
        WriteScalar(valuePtr, value);
    }

    /// <summary> Gets the byte address of a value at a linear integral-value index. </summary>
    public byte* GetValueBytePtr(long index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(
            index,
            IntegralLength);

        return DataPtr + index * CountOf.ValueByteCount;
    }

    /// <summary> Gets the byte address of a value at the supplied block and within-block indices. </summary>
    public byte* GetBlockValueBytePtr(
        long blockIndex,
        int blockValueIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(blockIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(blockValueIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(
            blockIndex,
            BlockLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(
            blockValueIndex,
            CountOf.BlockCapacity);

        long valueIndex =
            blockIndex * CountOf.BlockCapacity +
            blockValueIndex;
        return DataPtr + valueIndex * CountOf.ValueByteCount;
    }

    /// <summary>
    /// Gets a typed pointer to a value at a linear integral-value index.
    /// The returned pointer may be unaligned and must not be directly dereferenced
    /// by portable code.
    /// </summary>
    public T* GetValuePtr<T>(long index)
        where T : unmanaged
    {
        ValidateTypeCompatibility<T>();
        return (T*)GetValueBytePtr(index);
    }

    /// <summary>
    /// Gets a typed pointer to a value at the supplied block and within-block indices.
    /// The returned pointer may be unaligned and must not be directly dereferenced
    /// by portable code.
    /// </summary>
    public T* GetBlockValuePtr<T>(long blockIndex, int blockValueIndex)
        where T : unmanaged
    {
        ValidateTypeCompatibility<T>();
        return (T*)GetBlockValueBytePtr(blockIndex, blockValueIndex);
    }

    public IntegralSpan GetBlockSpan(long blockOffset, long blockCount)
    {
        ValidateRange(
            blockOffset,
            blockCount,
            BlockLength,
            nameof(blockOffset),
            nameof(blockCount));

        return GetSubSpan(
            checked(blockOffset * CountOf.BlockByteCount),
            checked(blockCount * CountOf.BlockByteCount));
    }

    public IntegralSpan GetValueSpan(long valueOffset, long valueCount)
    {
        ValidateRange(
            valueOffset,
            valueCount,
            IntegralLength,
            nameof(valueOffset),
            nameof(valueCount));

        return GetSubSpan(
            checked(valueOffset * CountOf.ValueByteCount),
            checked(valueCount * CountOf.ValueByteCount));
    }

    public IntegralSpan GetSubSpan(long byteOffset, long byteLength)
    {
        ValidateRange(
            byteOffset,
            byteLength,
            Length,
            nameof(byteOffset),
            nameof(byteLength));

        if (CountOf.ValueByteCount > 0 &&
            (byteOffset % CountOf.ValueByteCount) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteOffset),
                "The subspan must begin on a scalar value boundary.");
        }

        if (CountOf.ValueByteCount > 0 &&
            (byteLength % CountOf.ValueByteCount) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteLength),
                "The subspan must end on a scalar value boundary.");
        }

        return new IntegralSpan(
            Ptr,
            checked(Offset + byteOffset),
            byteLength);
    }

    private T ReadScalar<T>(byte* source)
        where T : unmanaged
    {
        return Format.ByteOrder switch
        {
            ByteOrder.Native => IntegralCodec<T, NativeEndianCodec>.Read(source),
            ByteOrder.LittleEndian => IntegralCodec<T, LittleEndianCodec>.Read(source),
            ByteOrder.BigEndian => IntegralCodec<T, BigEndianCodec>.Read(source),
            _ => throw new ArgumentOutOfRangeException(
                nameof(IntegralFormat.ByteOrder),
                Format.ByteOrder,
                "Undefined byte order."),
        };
    }

    private void WriteScalar<T>(byte* destination, T value)
        where T : unmanaged
    {
        switch (Format.ByteOrder)
        {
            case ByteOrder.Native:
                IntegralCodec<T, NativeEndianCodec>.Write(destination, value);
                break;

            case ByteOrder.LittleEndian:
                IntegralCodec<T, LittleEndianCodec>.Write(destination, value);
                break;

            case ByteOrder.BigEndian:
                IntegralCodec<T, BigEndianCodec>.Write(destination, value);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(IntegralFormat.ByteOrder),
                    Format.ByteOrder,
                    "Undefined byte order.");
        }
    }

    private void ValidateTypeCompatibility<T>()
        where T : unmanaged
    {
        if (!Ptr.IsCompatible<T>())
        {
            throw new ArgumentException(
                $"Type '{typeof(T)}' is not compatible with integral value type " +
                $"'{IntegralValueType}'.",
                nameof(T));
        }
    }

    private static void ValidateRange(
        long offset,
        long count,
        long availableCount,
        string offsetName,
        string countName)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset, offsetName);
        ArgumentOutOfRangeException.ThrowIfNegative(count, countName);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            offset,
            availableCount,
            offsetName);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            count,
            availableCount - offset,
            countName);
    }
}
