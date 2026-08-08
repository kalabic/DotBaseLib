using System.Diagnostics;
using System.Runtime.CompilerServices;
using DotBase.Buffers;

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

    public long ValueCount { get { return Capacity.TotalValueCount; } }

    public long BlockCount { get { return Capacity.BlockCount; } }

    public int TrailingValueCount { get { return Capacity.TrailingValueCount; } }

    /// <summary>
    /// <see cref="Format"/> byte order with <see cref="ByteOrder.Native"/> folded to
    /// host little/big endian.
    /// </summary>
    public ByteOrder ResolvedByteOrder => Format.ByteOrder.Resolve();

    /// <summary>
    /// Whether this span's resolved byte order matches <paramref name="otherEndian"/>
    /// (after resolving <see cref="ByteOrder.Native"/> on both sides).
    /// </summary>
    public bool IsEqual(ByteOrder otherEndian)
    {
        return ResolvedByteOrder == otherEndian.Resolve();
    }

    /// <summary>
    /// Whether this span and <paramref name="other"/> share the same resolved byte order.
    /// </summary>
    public bool IsEqual(in IntegralSpan other)
    {
        return IsEqual(other.Format.ByteOrder);
    }


    public IntegralSpan()
    {
        Ptr = IntegralPtr.NULL;
        Length = 0;
        Offset = 0;
        Capacity = IntegralCapacity.Zero;
    }

    /// <summary>
    /// Builds a view over <paramref name="valueCount"/> contiguous values at <paramref name="ptr"/>.
    /// Does not allocate or pin; the pointer must remain valid for the span's use.
    /// </summary>
    public static IntegralSpan FromValues<T>(
        T* ptr,
        long valueCount,
        int blockCapacity = 1,
        ByteOrder byteOrder = ByteOrder.Native,
        IntegralType valueType = IntegralType.None)
        where T : unmanaged
    {
        ArgumentOutOfRangeException.ThrowIfNegative(valueCount);

        IntegralType resolvedType = valueType == IntegralType.None
            ? IntegralType.None.DefaultForType<T>()
            : valueType;
        if (resolvedType == IntegralType.None)
        {
            throw new ArgumentException(
                $"Type '{typeof(T)}' is not a supported integral scalar type.",
                nameof(T));
        }

        if (!resolvedType.IsCompatible<T>())
        {
            throw new ArgumentException(
                $"Type '{typeof(T)}' is not compatible with integral type '{resolvedType}'.",
                nameof(valueType));
        }

        long byteLength = checked(valueCount * Unsafe.SizeOf<T>());
        return new IntegralSpan(
            (byte*)ptr,
            0,
            byteLength,
            new IntegralFormat(resolvedType, blockCapacity, byteOrder));
    }

    public IntegralSpan(byte* ptr, long offset, long length, IntegralType valueType, int blockValueCount)
        : this(ptr, offset, length, new IntegralFormat(valueType, blockValueCount))
    { }

    public IntegralSpan(byte* ptr, long offset, long length, IntegralFormat fmt)
        : this(new IntegralPtr(ptr, fmt), offset, length)
    { }

    /// <summary>
    /// Builds a span descriptor without validating format, ranges, or alignment.
    /// Call <see cref="Validate"/> when the span will be used with checked APIs.
    /// </summary>
    public IntegralSpan(in IntegralPtr ptr, long offset, long length)
    {
        IntegralCapacity capacity = new(length, ptr.Fmt);
        this = new IntegralSpan(ptr, offset, length, capacity);
    }

    /// <summary>
    /// Slice constructor. Does not re-validate; used after a parent was validated
    /// or when the caller accepts an unvalidated descriptor.
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

    /// <summary>
    /// Whether format, capacity, ranges, null rules, and scalar alignment are consistent.
    /// </summary>
    public bool IsValid()
    {
        if (!Format.IsValid())
        {
            return false;
        }

        if (Offset < 0 || Length < 0)
        {
            return false;
        }

        if (Ptr.IsNull && (Offset != 0 || Length != 0))
        {
            return false;
        }

        if (!Capacity.IsValid() || Capacity.ByteCount != Length)
        {
            return false;
        }

        if (Capacity.ValueByteCount > 0 &&
            (Offset % Capacity.ValueByteCount) != 0)
        {
            return false;
        }

        if (!Ptr.IsNull &&
            Capacity.ValueByteCount > 1 &&
            (((nuint)Ptr.BytePtr + (nuint)Offset) %
             (nuint)Capacity.ValueByteCount) != 0)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Throws if format, capacity, ranges, null rules, or scalar alignment are invalid.
    /// Constructors do not call this; call sites decide.
    /// </summary>
    public void Validate()
    {
        Format.Validate();

        ArgumentOutOfRangeException.ThrowIfNegative(Offset);
        ArgumentOutOfRangeException.ThrowIfNegative(Length);

        if (Ptr.IsNull && (Offset != 0 || Length != 0))
        {
            throw new ArgumentException(
                "A null integral pointer can describe only the empty span.",
                nameof(Ptr));
        }

        if (Capacity.ByteCount != Length)
        {
            throw new ArgumentException(
                "Capacity byte count must match span length.",
                nameof(Capacity));
        }

        Capacity.Validate();

        if (Capacity.ValueByteCount > 0 &&
            (Offset % Capacity.ValueByteCount) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Offset),
                Offset,
                "The byte offset must be aligned to the scalar value size.");
        }

        if (!Ptr.IsNull &&
            Capacity.ValueByteCount > 1 &&
            (((nuint)Ptr.BytePtr + (nuint)Offset) %
             (nuint)Capacity.ValueByteCount) != 0)
        {
            throw new ArgumentException(
                "The integral span base address plus offset must be " +
                "natural-aligned to the scalar value size.",
                nameof(Ptr));
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
            ValueCount);

        return DataPtr + index * Capacity.ValueByteCount;
    }

    /// <summary> Gets the byte address of a value at the supplied block and within-block indices. </summary>
    public byte* GetBlockValueBytePtr(long blockIndex, int blockValueIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(blockIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(blockValueIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(
            blockIndex,
            BlockCount);
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
            BlockCount,
            nameof(blockOffset),
            nameof(blockCount));

        return GetSubSpan(
            checked(blockOffset * Capacity.BlockByteCount),
            checked(blockCount * Capacity.BlockByteCount));
    }

    /// <summary>
    /// Slice this span to <paramref name="range"/> using
    /// <see cref="IntegralRange.BlockOffset"/> / <see cref="IntegralRange.BlockCount"/>
    /// (parent block units only - not bytes or scalar values).
    /// </summary>
    public IntegralSpan GetBlockSpan(in IntegralRange range)
    {
        return GetBlockSpan(range.BlockOffset, range.BlockCount);
    }

    /// <summary>
    /// Parent-block slice, then re-label with type and block capacity
    /// (preserves this span's byte order and converter via <see cref="ChangeFormat"/>).
    /// <paramref name="range"/> stays in <b>parent</b> block units; it is not
    /// rescaled to the new type's blocks or values.
    /// </summary>
    public IntegralSpan GetBlockSpan(
        in IntegralRange range,
        IntegralType valueType,
        int blockCapacity = 1)
    {
        return GetBlockSpan(range).ChangeFormat(valueType, blockCapacity);
    }

    /// <summary>
    /// Parent-block slice, then re-label with a full format (trusted).
    /// <paramref name="range"/> stays in <b>parent</b> block units; it is not
    /// rescaled to the new format's block geometry.
    /// </summary>
    public IntegralSpan GetBlockSpan(
        in IntegralRange range,
        in IntegralFormat format)
    {
        IntegralSpan slice = GetBlockSpan(range);
        if (slice.Length == 0)
        {
            return Empty;
        }

        return new IntegralSpan(
            slice.BytePtr,
            slice.Offset,
            slice.Length,
            format);
    }

    public IntegralSpan GetValueSpan(long valueOffset, long valueCount)
    {
        ValidateRange(
            valueOffset,
            valueCount,
            ValueCount,
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

    /// <summary>
    /// Same memory region with a different value type / block layout. Does not
    /// convert data. Preserves <see cref="IntegralFormat.ByteOrder"/> and
    /// <see cref="IntegralFormat.Converter"/> from this span.
    /// Use <see cref="IntegralMemory.Convert"/> / <see cref="IntegralMemory.ReverseCopy"/>
    /// for content transforms.
    /// <para>
    /// <b>Trusted:</b> does not validate format or geometry under the new value size.
    /// Call <see cref="Validate"/> or use <see cref="ChangeFormatChecked"/> when the
    /// descriptor may be hostile.
    /// </para>
    /// </summary>
    public IntegralSpan ChangeFormat(
        IntegralType valueType,
        int blockCapacity = 1)
    {
        return ChangeFormatCore(
            new IntegralFormat(
                valueType,
                blockCapacity,
                Format.ByteOrder,
                Format.Converter));
    }

    /// <summary>
    /// Size-only format (<see cref="IntegralType.None"/>) variant of
    /// <see cref="ChangeFormat(IntegralType, int)"/>.
    /// </summary>
    public IntegralSpan ChangeFormat(
        int valueSize,
        int blockCapacity = 1)
    {
        return ChangeFormatCore(
            new IntegralFormat(
                valueSize,
                blockCapacity,
                Format.ByteOrder,
                Format.Converter));
    }

    /// <summary>
    /// Same as <see cref="ChangeFormat(IntegralType, int)"/>, then validates
    /// the resulting span.
    /// </summary>
    public IntegralSpan ChangeFormatChecked(
        IntegralType valueType,
        int blockCapacity = 1)
    {
        return ChangeFormatCheckedCore(
            new IntegralFormat(
                valueType,
                blockCapacity,
                Format.ByteOrder,
                Format.Converter));
    }

    /// <summary>
    /// Size-only format variant of <see cref="ChangeFormatChecked(IntegralType, int)"/>.
    /// </summary>
    public IntegralSpan ChangeFormatChecked(
        int valueSize,
        int blockCapacity = 1)
    {
        return ChangeFormatCheckedCore(
            new IntegralFormat(
                valueSize,
                blockCapacity,
                Format.ByteOrder,
                Format.Converter));
    }

    private IntegralSpan ChangeFormatCore(in IntegralFormat format)
    {
        if (Length == 0)
        {
            return Empty;
        }

        return new IntegralSpan(BytePtr, Offset, Length, format);
    }

    private IntegralSpan ChangeFormatCheckedCore(in IntegralFormat format)
    {
        IntegralSpan changed = ChangeFormatCore(format);
        if (changed.Length != 0)
        {
            changed.Validate();
        }

        return changed;
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
                IntegralPrimitives.Swap2(hostPtr, source);
                break;
            case 4:
                IntegralPrimitives.Swap4(hostPtr, source);
                break;
            case 8:
                IntegralPrimitives.Swap8(hostPtr, source);
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
                IntegralPrimitives.Swap2(destination, hostPtr);
                return;
            case 4:
                IntegralPrimitives.Swap4(destination, hostPtr);
                return;
            case 8:
                IntegralPrimitives.Swap8(destination, hostPtr);
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
