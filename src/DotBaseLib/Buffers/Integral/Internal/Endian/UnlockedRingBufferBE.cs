namespace DotBase.Buffers.Integral.Internal.Endian;


internal sealed class UnlockedRingBufferBE 
    : UnlockedRingBuffer
{
    public override ByteOrder ByteOrder => ByteOrder.BigEndian;

    internal UnlockedRingBufferBE(int capacity)
        : base(capacity)
    {
    }
}
