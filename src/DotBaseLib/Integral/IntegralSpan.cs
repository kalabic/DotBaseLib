using System.Diagnostics;
using System.Runtime.CompilerServices;
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
/// <para>
/// <b>Alignment:</b> when <see cref="BytePtr"/> is natural-aligned to
/// <see cref="IntegralCapacity.ValueByteCount"/> and <see cref="Offset"/> is a
/// multiple of that size, every value address is scalar-aligned. Wire helpers and
/// compatible host/wire scalar access require that contract.
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
    public readonly IntegralCapacity Capacity;

    /// <summary> Gets the original, unadjusted base pointer. </summary>
    public byte* BytePtr { get { return Ptr.BytePtr; } }

    /// <summary> Gets the pointer to the first byte in this span. </summary>
    public byte* DataPtr { get { return Ptr.BytePtr + Offset; } }

    public IntegralFormat Format { get { return Ptr.Fmt; } }

    public IntegralType IntegralValueType { get { return Ptr.Fmt.ValueType; } }

    public long IntegralLength { get { return Capacity.TotalValueCount; } }

    public long BlockLength { get { return Capacity.BlockCount; } }

    public int TrailingValueCount { get { return Capacity.TrailingValueCount; } }


    public IntegralSpan()
    {
        Ptr = IntegralPtr.NULL;
        Length = 0;
        Offset = 0;
        Capacity = IntegralCapacity.Zero;
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

        IntegralCapacity capacity = new(
            length,
            ptr.Fmt.ValueType,
            ptr.Fmt.BlockCapacity);
        capacity.ThrowIfArgumentOutOfRange();

        if (capacity.ValueByteCount > 0 &&
            (offset % capacity.ValueByteCount) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(offset),
                offset,
                "The byte offset must be aligned to the scalar value size.");
        }

        // Value addresses must be natural-aligned for wire load/store.
        if (!ptr.IsNull &&
            capacity.ValueByteCount > 1 &&
            (((nuint)ptr.BytePtr + (nuint)offset) %
             (nuint)capacity.ValueByteCount) != 0)
        {
            throw new ArgumentException(
                "The integral span base address plus offset must be " +
                "natural-aligned to the scalar value size.",
                nameof(ptr));
        }

        this = new IntegralSpan(ptr, offset, length, capacity);
    }

    /// <summary>
    /// Trusted slice constructor. Caller must ensure format validity, non-negative
    /// ranges, null-pointer rules, and value-size alignment.
    /// </summary>
    private IntegralSpan(
        in IntegralPtr ptr,
        long offset,
        long length,
        IntegralCapacity capacity)
    {
        Ptr = ptr;
        Offset = offset;
        Length = length;
        Capacity = capacity;
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
    public void SetAtBlockIndex<T>(long blockIndex, int blockValueIndex, T value)
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

        return DataPtr + index * Capacity.ValueByteCount;
    }

    /// <summary> Gets the byte address of a value at the supplied block and within-block indices. </summary>
    public byte* GetBlockValueBytePtr(long blockIndex, int blockValueIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(blockIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(blockValueIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(
            blockIndex,
            BlockLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(
            blockValueIndex,
            Capacity.BlockCapacity);

        long valueIndex =
            blockIndex * Capacity.BlockCapacity +
            blockValueIndex;
        return DataPtr + valueIndex * Capacity.ValueByteCount;
    }

    /// <summary>
    /// Gets a typed pointer to a value at a linear integral-value index.
    /// When the span base is natural-aligned to the scalar size and
    /// <see cref="Offset"/> is a multiple of that size, the result is
    /// scalar-aligned and may be dereferenced for compatible host/wire endian.
    /// </summary>
    public T* GetValuePtr<T>(long index)
        where T : unmanaged
    {
        ValidateTypeCompatibility<T>();
        return (T*)GetValueBytePtr(index);
    }

    /// <summary>
    /// Gets a typed pointer to a value at the supplied block and within-block indices.
    /// Same alignment contract as <see cref="GetValuePtr{T}"/>.
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
            checked(blockOffset * Capacity.BlockByteCount),
            checked(blockCount * Capacity.BlockByteCount));
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
            checked(valueOffset * Capacity.ValueByteCount),
            checked(valueCount * Capacity.ValueByteCount));
    }

    public IntegralSpan GetSubSpan(long byteOffset, long byteLength)
    {
        ValidateRange(
            byteOffset,
            byteLength,
            Length,
            nameof(byteOffset),
            nameof(byteLength));

        if (Capacity.ValueByteCount > 0 &&
            (byteOffset % Capacity.ValueByteCount) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteOffset),
                "The subspan must begin on a scalar value boundary.");
        }

        if (Capacity.ValueByteCount > 0 &&
            (byteLength % Capacity.ValueByteCount) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteLength),
                "The subspan must end on a scalar value boundary.");
        }

        // Parent already validated format; build capacity without re-running Format.Validate.
        long newOffset = checked(Offset + byteOffset);
        IntegralCapacity capacity = new(
            byteLength,
            Capacity.ValueByteCount,
            Capacity.BlockCapacity);
        return new IntegralSpan(Ptr, newOffset, byteLength, capacity);
    }

    private T ReadScalar<T>(byte* source)
        where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        Debug.Assert(
            size <= 1 || ((nuint)source % (nuint)size) == 0,
            "IntegralSpan scalar address must be natural-aligned to the value size.");

        // Compatible host/wire (or 1-byte): single aligned load.
        if (IsHostWireCompatible() || size == 1)
        {
            return *(T*)source;
        }

        // Opposite endian: size-switch into Swap* → host stack slot.
        T host = default;
        byte* hostPtr = (byte*)&host;
        switch (size)
        {
            case 2:
                IntegralWire.Swap2(hostPtr, source);
                break;
            case 4:
                IntegralWire.Swap4(hostPtr, source);
                break;
            case 8:
                IntegralWire.Swap8(hostPtr, source);
                break;
            default:
                throw new NotSupportedException(
                    $"Scalar size {size} is not supported.");
        }

        return host;
    }

    private void WriteScalar<T>(byte* destination, T value)
        where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        Debug.Assert(
            size <= 1 || ((nuint)destination % (nuint)size) == 0,
            "IntegralSpan scalar address must be natural-aligned to the value size.");

        // Compatible host/wire (or 1-byte): single aligned store.
        if (IsHostWireCompatible() || size == 1)
        {
            *(T*)destination = value;
            return;
        }

        // Opposite endian: size-switch Swap* from host value bits.
        byte* hostPtr = (byte*)&value;
        switch (size)
        {
            case 2:
                IntegralWire.Swap2(destination, hostPtr);
                return;
            case 4:
                IntegralWire.Swap4(destination, hostPtr);
                return;
            case 8:
                IntegralWire.Swap8(destination, hostPtr);
                return;
            default:
                throw new NotSupportedException(
                    $"Scalar size {size} is not supported.");
        }
    }

    /// <summary>
    /// True when wire byte order matches host (Native always matches).
    /// </summary>
    private bool IsHostWireCompatible()
    {
        ByteOrder order = Format.ByteOrder;
        if (order == ByteOrder.Native)
        {
            return true;
        }

        return order == ByteOrder.LittleEndian
            ? BitConverter.IsLittleEndian
            : !BitConverter.IsLittleEndian;
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
