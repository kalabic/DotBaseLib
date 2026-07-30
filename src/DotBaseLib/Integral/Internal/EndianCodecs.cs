using DotBase.Buffers;

namespace DotBase.Integral.Internal;


internal readonly struct NativeEndianCodec : IEndianCodec
{
    public static ByteOrder ByteOrder => ByteOrder.Native;
}


internal readonly struct LittleEndianCodec : IEndianCodec
{
    public static ByteOrder ByteOrder => ByteOrder.LittleEndian;
}


internal readonly struct BigEndianCodec : IEndianCodec
{
    public static ByteOrder ByteOrder => ByteOrder.BigEndian;
}
