using DotBase.Integral;

namespace DotBase.Buffers.Integral.Internal.Endian;


internal sealed class UnlockedRingBufferLE 
    : UnlockedRingBuffer
{
    public override ByteOrder ByteOrder => ByteOrder.LittleEndian;

    internal UnlockedRingBufferLE(int capacity)
        : base(capacity, IntegralFormat.LittleEndianStream)
    {
    }
    internal UnlockedRingBufferLE(int capacity, IntegralFormat format)
        : base(capacity, format)
    {
    }
}
