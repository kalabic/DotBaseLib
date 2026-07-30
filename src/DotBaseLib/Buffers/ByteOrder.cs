namespace DotBase.Buffers;


/// <summary>Specifies the byte order used to encode a value.</summary>
public enum ByteOrder
{
    Native,
    LittleEndian,
    BigEndian,
}

internal static class ByteOrderMethods
{
    internal static bool IsNativeCompatible(this ByteOrder value)
    {
        return value switch
        {
            ByteOrder.Native => true,
            ByteOrder.LittleEndian => BitConverter.IsLittleEndian,
            ByteOrder.BigEndian => !BitConverter.IsLittleEndian,
            _ => throw new ArgumentOutOfRangeException(nameof(value)),
        };
    }
}
