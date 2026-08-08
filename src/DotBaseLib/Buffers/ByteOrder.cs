namespace DotBase.Buffers;


/// <summary>Specifies the byte order used to encode a value.</summary>
public enum ByteOrder
{
    Native,
    LittleEndian,
    BigEndian,

    /// <summary>
    /// Fallback for all invalid values. TODO: Very likely to be used to identify
    /// order-ignorant types that are to be treated like a stream of bytes.
    /// </summary>
    Undefined,
}

public static class ByteOrderExtensions
{
    /// <summary>
    /// Folds <see cref="ByteOrder.Native"/> to host little/big endian; leaves
    /// explicit orders unchanged.
    /// </summary>
    public static ByteOrder Resolve(this ByteOrder byteOrder)
    {
        return byteOrder switch
        {
            ByteOrder.Native => BitConverter.IsLittleEndian
                ? ByteOrder.LittleEndian
                : ByteOrder.BigEndian,
            ByteOrder.LittleEndian => ByteOrder.LittleEndian,
            ByteOrder.BigEndian => ByteOrder.BigEndian,
            _ => ByteOrder.Undefined,
        };
    }

    public static ByteOrder TrimUndefined(this ByteOrder byteOrder)
    {
        return byteOrder switch
        {
            ByteOrder.Native => ByteOrder.Native,
            ByteOrder.LittleEndian => ByteOrder.LittleEndian,
            ByteOrder.BigEndian => ByteOrder.BigEndian,
            _ => ByteOrder.Undefined,
        };
    }

    internal static bool IsNativeCompatible(this ByteOrder value)
    {
        return value switch
        {
            ByteOrder.Native => true,
            ByteOrder.LittleEndian => BitConverter.IsLittleEndian,
            ByteOrder.BigEndian => !BitConverter.IsLittleEndian,
            _ => false,
        };
    }
}
