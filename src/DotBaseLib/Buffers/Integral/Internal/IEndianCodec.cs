namespace DotBase.Buffers.Integral.Internal;


internal interface IEndianCodec
{
    static abstract ByteOrder ByteOrder { get; }

    static abstract short ReadInt16(ReadOnlySpan<byte> source);
    static abstract ushort ReadUInt16(ReadOnlySpan<byte> source);
    static abstract int ReadInt32(ReadOnlySpan<byte> source);
    static abstract uint ReadUInt32(ReadOnlySpan<byte> source);
    static abstract long ReadInt64(ReadOnlySpan<byte> source);
    static abstract ulong ReadUInt64(ReadOnlySpan<byte> source);
    static abstract nint ReadIntPtr(ReadOnlySpan<byte> source);
    static abstract nuint ReadUIntPtr(ReadOnlySpan<byte> source);

    static abstract void WriteInt16(Span<byte> destination, short value);
    static abstract void WriteUInt16(Span<byte> destination, ushort value);
    static abstract void WriteInt32(Span<byte> destination, int value);
    static abstract void WriteUInt32(Span<byte> destination, uint value);
    static abstract void WriteInt64(Span<byte> destination, long value);
    static abstract void WriteUInt64(Span<byte> destination, ulong value);
    static abstract void WriteIntPtr(Span<byte> destination, nint value);
    static abstract void WriteUIntPtr(Span<byte> destination, nuint value);
}
