using DotBase.Buffers.Integral.Internal.Endian;

namespace DotBase.Buffers.Integral;


public static class IntegralRingBuffer
{
    public static IIntegralRingBuffer CreateUnlocked(int capacity, ByteOrder byteOrder)
    {
        return byteOrder.Resolve() switch
        {
            ByteOrder.LittleEndian => new UnlockedRingBufferLE(capacity),
            ByteOrder.BigEndian => new UnlockedRingBufferBE(capacity),
            _ => throw new ArgumentOutOfRangeException(nameof(byteOrder)),
        };
    }

    public static IIntegralRingBuffer CreateLocked(int capacity, ByteOrder byteOrder)
    {
        return byteOrder.Resolve() switch
        {
            ByteOrder.LittleEndian => new LockedRingBufferLE(capacity),
            ByteOrder.BigEndian => new LockedRingBufferBE(capacity),
            _ => throw new ArgumentOutOfRangeException(nameof(byteOrder)),
        };
    }

    public static IWaitableRingBuffer CreateWaitable(int capacity, ByteOrder byteOrder)
    {
        return byteOrder.Resolve() switch
        {
            ByteOrder.LittleEndian => new WaitableRingBufferLE(capacity),
            ByteOrder.BigEndian => new WaitableRingBufferBE(capacity),
            _ => throw new ArgumentOutOfRangeException(nameof(byteOrder)),
        };
    }
}
