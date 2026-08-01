using DotBase.Buffers;

namespace DotBase.Integral;


/// <summary>
///
/// Details of integral values, blocks with values, and a byte order for certain buffer.
/// Construction does not fully validate; call <see cref="Validate"/> or
/// <see cref="IsValid"/> when the format is used.
///
/// </summary>
public readonly struct IntegralFormat
{
    public static readonly IntegralFormat NONE = new IntegralFormat(0, 0);

    /// <summary> Number of bytes occupied by a complete block of integral values. </summary>
    public long BytesPerBlock { get { return (long)ValueSize * BlockCapacity; } }

    /// <summary> Byte order used to store integral values inside certain buffer. </summary>
    public readonly ByteOrder ByteOrder;

    /// <summary> Size in bytes of a single value. </summary>
    public readonly int ValueSize;

    /// <summary> Type of integral values stored inside certain buffer. </summary>
    public readonly IntegralType ValueType;

    public readonly int BlockCapacity;

    /// <summary>
    /// Optional (estimated) bytes per second rate passing through certain buffer.
    /// A value of 0 indicates that the rate is unknown or inapplicable.
    /// </summary>
    public readonly long ByteRate;

    /// <summary>
    /// Known integral type. Sets <see cref="ValueSize"/> from <see cref="IntegralTypeExtensions.Size"/>.
    /// Prefer the size constructor for <see cref="IntegralType.NONE"/> or size-only formats.
    /// Does not validate; call <see cref="Validate"/> when the format is used.
    /// </summary>
    public IntegralFormat(
        IntegralType valueType,
        int blockCapacity,
        ByteOrder byteOrder = ByteOrder.Native,
        long byteRate = 0)
    {
        if (valueType == IntegralType.NONE)
        {
            throw new ArgumentException(
                $"'{IntegralType.NONE}' is not a supported by format. Value size must be provided.",  nameof(valueType));
        }

        ValueSize = valueType.Size();
        ValueType = valueType;
        BlockCapacity = blockCapacity;
        ByteOrder = byteOrder;
        ByteRate = byteRate;
    }

    /// <summary>
    /// Size-only format (<see cref="IntegralType.NONE"/>) or empty sentinel (0, 0).
    /// Does not validate; call <see cref="Validate"/> when the format is used.
    /// </summary>
    public IntegralFormat(
        int valueSize,
        int blockCapacity,
        ByteOrder byteOrder = ByteOrder.Native,
        long byteRate = 0)
    {
        ValueSize = valueSize;
        ValueType = IntegralType.NONE;
        BlockCapacity = blockCapacity;
        ByteOrder = byteOrder;
        ByteRate = byteRate;
    }

    /// <summary>
    /// Format for CLR scalar <typeparamref name="T"/> (via
    /// <see cref="IntegralTypeExtensions.DefaultForType{T}"/>).
    /// </summary>
    public static IntegralFormat For<T>(
        int blockCapacity = 1,
        ByteOrder byteOrder = ByteOrder.Native,
        long byteRate = 0)
        where T : unmanaged
    {
        IntegralType valueType = IntegralType.NONE.DefaultForType<T>();
        if (valueType == IntegralType.NONE)
        {
            throw new ArgumentException(
                $"Type '{typeof(T)}' is not a supported integral scalar type.",
                nameof(T));
        }

        return new IntegralFormat(valueType, blockCapacity, byteOrder, byteRate);
    }

    public bool IsCompatible<T>()
        where T : unmanaged
    {
        return ValueType.IsCompatible<T>();
    }

    public bool IsEmptyType()
    {
        return IsEmptyType(ValueType, ValueSize, BlockCapacity);
    }

    /// <summary>True for empty sentinel or a fully consistent non-empty format.</summary>
    public bool IsValid()
    {
        return IsValid(ValueType, ValueSize, BlockCapacity, ByteOrder);
    }

    /// <summary>
    /// Throws if this format is not a valid empty sentinel or a consistent non-empty description.
    /// Call sites decide when to run this; constructors do not.
    /// </summary>
    public void Validate()
    {
        Validate(ValueType, ValueSize, BlockCapacity, ByteOrder);
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

        if (valueType != IntegralType.NONE &&
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

        if (valueType != IntegralType.NONE && valueType.Size() != valueSize)
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
        return valueType == IntegralType.NONE &&
               valueSize == 0 &&
               blockCapacity == 0;
    }
}
