using DotBase.Buffers;

namespace DotBase.Integral;


/// <summary>
///
/// Details of integral values, blocks with values, and a byte order for certain buffer.
///
/// </summary>
public readonly struct IntegralFormat
{
    public static readonly IntegralFormat NONE = new IntegralFormat(IntegralType.NONE, 0);

    /// <summary> Number of bytes occupied by a complete block of integral values. </summary>
    public int BytesPerBlock
    {
        get
        {
            return checked(ValueType.Size() * BlockCapacity);
        }
    }

    /// <summary> Byte order used to store integral values inside certain buffer. </summary>
    public readonly ByteOrder ByteOrder;

    /// <summary> Type of integral values stored inside certain buffer. </summary>
    public readonly IntegralType ValueType;

    public readonly int BlockCapacity;

    /// <summary>
    /// Optional (estimated) bytes per second rate passing through certain buffer.
    /// A value of -1 indicates that the rate is unknown or inapplicable.
    /// </summary>
    public readonly int ByteRate;

    public IntegralFormat(IntegralType valueType, int blockCapacity, ByteOrder byteOrder = ByteOrder.Native, int byteRate = -1)
    {
        Validate(
            valueType,
            blockCapacity,
            byteOrder,
            byteRate);

        ValueType = valueType;
        BlockCapacity = blockCapacity;
        ByteOrder = byteOrder;
        ByteRate = byteRate;
    }

    public bool IsCompatible<T>()
        where T : unmanaged
    {
        return ValueType.IsCompatible<T>();
    }

    internal void Validate()
    {
        Validate(
            ValueType,
            BlockCapacity,
            ByteOrder,
            ByteRate);
    }

    private static void Validate(
        IntegralType valueType,
        int blockCapacity,
        ByteOrder byteOrder,
        int byteRate)
    {
        _ = byteOrder switch
        {
            ByteOrder.Native => byteOrder,
            ByteOrder.LittleEndian => byteOrder,
            ByteOrder.BigEndian => byteOrder,
            _ => throw new ArgumentOutOfRangeException(nameof(byteOrder)),
        };

        if (!Enum.IsDefined(valueType))
        {
            throw new ArgumentOutOfRangeException(nameof(valueType));
        }

        if (valueType == IntegralType.NONE)
        {
            ArgumentOutOfRangeException.ThrowIfNotEqual(
                blockCapacity,
                0);
        }
        else
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
                blockCapacity,
                0);
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(
            byteRate,
            -1);

        _ = checked(valueType.Size() * blockCapacity);
    }
}
