using System.Buffers.Binary;

namespace DotBase.Buffers.Integral.Internal;


internal readonly struct LittleEndianCodec : IEndianCodec
{
    public static ByteOrder ByteOrder => ByteOrder.LittleEndian;

    public static short ReadInt16(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt16LittleEndian(source);
    public static ushort ReadUInt16(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt16LittleEndian(source);
    public static int ReadInt32(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt32LittleEndian(source);
    public static uint ReadUInt32(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt32LittleEndian(source);
    public static long ReadInt64(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt64LittleEndian(source);
    public static ulong ReadUInt64(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt64LittleEndian(source);
    public static nint ReadIntPtr(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadIntPtrLittleEndian(source);
    public static nuint ReadUIntPtr(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUIntPtrLittleEndian(source);

    public static void WriteInt16(Span<byte> destination, short value) => BinaryPrimitives.WriteInt16LittleEndian(destination, value);
    public static void WriteUInt16(Span<byte> destination, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(destination, value);
    public static void WriteInt32(Span<byte> destination, int value) => BinaryPrimitives.WriteInt32LittleEndian(destination, value);
    public static void WriteUInt32(Span<byte> destination, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(destination, value);
    public static void WriteInt64(Span<byte> destination, long value) => BinaryPrimitives.WriteInt64LittleEndian(destination, value);
    public static void WriteUInt64(Span<byte> destination, ulong value) => BinaryPrimitives.WriteUInt64LittleEndian(destination, value);
    public static void WriteIntPtr(Span<byte> destination, nint value) => BinaryPrimitives.WriteIntPtrLittleEndian(destination, value);
    public static void WriteUIntPtr(Span<byte> destination, nuint value) => BinaryPrimitives.WriteUIntPtrLittleEndian(destination, value);
}


internal readonly struct BigEndianCodec : IEndianCodec
{
    public static ByteOrder ByteOrder => ByteOrder.BigEndian;

    public static short ReadInt16(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt16BigEndian(source);
    public static ushort ReadUInt16(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt16BigEndian(source);
    public static int ReadInt32(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt32BigEndian(source);
    public static uint ReadUInt32(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt32BigEndian(source);
    public static long ReadInt64(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt64BigEndian(source);
    public static ulong ReadUInt64(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt64BigEndian(source);
    public static nint ReadIntPtr(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadIntPtrBigEndian(source);
    public static nuint ReadUIntPtr(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUIntPtrBigEndian(source);

    public static void WriteInt16(Span<byte> destination, short value) => BinaryPrimitives.WriteInt16BigEndian(destination, value);
    public static void WriteUInt16(Span<byte> destination, ushort value) => BinaryPrimitives.WriteUInt16BigEndian(destination, value);
    public static void WriteInt32(Span<byte> destination, int value) => BinaryPrimitives.WriteInt32BigEndian(destination, value);
    public static void WriteUInt32(Span<byte> destination, uint value) => BinaryPrimitives.WriteUInt32BigEndian(destination, value);
    public static void WriteInt64(Span<byte> destination, long value) => BinaryPrimitives.WriteInt64BigEndian(destination, value);
    public static void WriteUInt64(Span<byte> destination, ulong value) => BinaryPrimitives.WriteUInt64BigEndian(destination, value);
    public static void WriteIntPtr(Span<byte> destination, nint value) => BinaryPrimitives.WriteIntPtrBigEndian(destination, value);
    public static void WriteUIntPtr(Span<byte> destination, nuint value) => BinaryPrimitives.WriteUIntPtrBigEndian(destination, value);
}
