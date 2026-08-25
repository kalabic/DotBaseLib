using DotBase.Buffers;
using DotBase.Integral.Conversion;
using DotBase.Integral.Internal;

namespace DotBase.Integral;


/// <summary>
/// Describes integral value type, block layout, and byte order for a buffer.
/// Construction does not fully validate; call <see cref="Validate"/> or
/// <see cref="IsValid"/> when the format is used.
/// <para>
/// This type is fully unmanaged. Its conversion policy stores one process-local
/// registry index covering contiguous, interleaved, and planar paths.
/// </para>
/// </summary>
public readonly struct IntegralFormat
{
    /// <summary>Empty / no-buffer sentinel.</summary>
    public static readonly IntegralFormat Empty = new IntegralFormat(0, 0);

    public static readonly IntegralFormat ByteStream = new IntegralFormat(IntegralType.UInt8, 1);

    public static readonly IntegralFormat BigEndianStream = new IntegralFormat(IntegralType.UInt8, 1, ByteOrder.BigEndian);

    public static readonly IntegralFormat LittleEndianStream = new IntegralFormat(IntegralType.UInt8, 1, ByteOrder.LittleEndian);

    public ByteOrder ByteOrder { get { return _valueFormat.ByteOrder; } }

    /// <summary> Number of bytes occupied by a complete block of integral values. </summary>
    public long BytesPerBlock { get { return (long)_valueFormat.ValueSize * BlockCapacity; } }

    public int ValueSize { get { return _valueFormat.ValueSize; } }

    public IntegralType ValueType { get { return _valueFormat.ValueType; } }

    public IntegralConversionPolicy ConversionPolicy { get { return _conversionPolicy; } }


    /// <summary> Internal container for basic value properties. </summary>
    private readonly IntegralValueFormat _valueFormat;

    public readonly int BlockCapacity;

    private readonly IntegralConversionPolicy _conversionPolicy;

    /// <summary>
    /// Known integral type. Sets <see cref="ValueSize"/> from <see cref="IntegralTypeExtensions.Size"/>.
    /// Prefer the size constructor for <see cref="IntegralType.None"/> or size-only formats.
    /// Does not validate; call <see cref="Validate"/> when the format is used.
    /// </summary>
    public IntegralFormat(
        IntegralType valueType,
        int blockCapacity,
        ByteOrder byteOrder = ByteOrder.Native,
        IntegralConversionPolicy conversionPolicy = default)
    {
        if (valueType == IntegralType.None)
        {
            throw new ArgumentException(
                $"'{IntegralType.None}' is not supported by this constructor. Value size must be provided.",
                nameof(valueType));
        }

        _valueFormat = new IntegralValueFormat(byteOrder, valueType, valueType.Size());
        BlockCapacity = blockCapacity;
        _conversionPolicy = conversionPolicy;
    }

    /// <summary>
    /// Size-only format (<see cref="IntegralType.None"/>) or empty sentinel (0, 0).
    /// Does not validate; call <see cref="Validate"/> when the format is used.
    /// </summary>
    public IntegralFormat(
        int valueSize,
        int blockCapacity,
        ByteOrder byteOrder = ByteOrder.Native,
        IntegralConversionPolicy conversionPolicy = default)
    {
        _valueFormat = new IntegralValueFormat(byteOrder, IntegralType.None, valueSize);
        BlockCapacity = blockCapacity;
        _conversionPolicy = conversionPolicy;
    }

    /// <summary>
    /// Format for CLR scalar <typeparamref name="T"/> (via
    /// <see cref="IntegralTypeExtensions.DefaultForType{T}"/>).
    /// </summary>
    public static IntegralFormat For<T>(
        int blockCapacity = 1,
        ByteOrder byteOrder = ByteOrder.Native,
        IntegralConversionPolicy conversionPolicy = default)
        where T : unmanaged
    {
        IntegralType valueType = IntegralType.None.DefaultForType<T>();
        if (valueType == IntegralType.None)
        {
            throw new ArgumentException(
                $"Type '{typeof(T)}' is not a supported integral scalar type.",
                nameof(T));
        }

        return new IntegralFormat(valueType, blockCapacity, byteOrder, conversionPolicy);
    }

    public bool IsCompatible<T>()
        where T : unmanaged
    {
        return _valueFormat.IsCompatible<T>();
    }

    public bool IsEmptyType()
    {
        return IsEmptyType(_valueFormat.ValueType, _valueFormat.ValueSize, BlockCapacity);
    }

    /// <summary>True for empty sentinel or a fully consistent non-empty format.</summary>
    public bool IsValid()
    {
        return IsValid(_valueFormat.ValueType, _valueFormat.ValueSize, BlockCapacity, _valueFormat.ByteOrder);
    }

    /// <summary>
    /// Throws if this format is not a valid empty sentinel or a consistent non-empty description.
    /// Call sites decide when to run this; constructors do not.
    /// </summary>
    public void Validate()
    {
        Validate(_valueFormat.ValueType, _valueFormat.ValueSize, BlockCapacity, _valueFormat.ByteOrder);
    }

    private static void Validate(
        IntegralType valueType,
        int valueSize,
        int blockCapacity,
        ByteOrder byteOrder)
    {
        if (IsValid(valueType, valueSize, blockCapacity, byteOrder))
        {
            return;
        }

        if (byteOrder.Resolve() == ByteOrder.Undefined)
        {
            throw new ArgumentOutOfRangeException(nameof(byteOrder));
        }

        if (!Enum.IsDefined(valueType))
        {
            throw new ArgumentOutOfRangeException(nameof(valueType));
        }

        if (IsEmptyType(valueType, valueSize, blockCapacity))
        {
            return;
        }

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(valueSize, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(blockCapacity, 0);

        if (valueType != IntegralType.None &&
            valueType.Size() != valueSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(valueSize),
                valueSize,
                "Value size does not match the integral type width.");
        }
    }

    public static bool IsValid(
        IntegralType valueType,
        int valueSize,
        int blockCapacity,
        ByteOrder byteOrder)
    {
        if (byteOrder.Resolve() == ByteOrder.Undefined)
        {
            return false;
        }

        if (!Enum.IsDefined(valueType))
        {
            return false;
        }

        if (IsEmptyType(valueType, valueSize, blockCapacity))
        {
            return true;
        }

        if (valueSize <= 0 || blockCapacity <= 0)
        {
            return false;
        }

        if (valueType != IntegralType.None && valueType.Size() != valueSize)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Empty / no-buffer sentinel: unknown type, zero size, zero block capacity.
    /// </summary>
    public static bool IsEmptyType(
        IntegralType valueType,
        int valueSize,
        int blockCapacity)
    {
        return valueType == IntegralType.None &&
               valueSize == 0 &&
               blockCapacity == 0;
    }
}
