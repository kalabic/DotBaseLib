using DotBase.Buffers.Integral.Internal.Endian;
using DotBase.Integral;

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

    public static IIntegralRingBuffer CreateUnlocked(int capacity, IntegralFormat format)
    {
        return format.ByteOrder.Resolve() switch
        {
            ByteOrder.LittleEndian => new UnlockedRingBufferLE(capacity, format),
            ByteOrder.BigEndian => new UnlockedRingBufferBE(capacity, format),
            _ => throw new ArgumentOutOfRangeException(nameof(format.ByteOrder)),
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

    public static IIntegralRingBuffer CreateLocked(int capacity, IntegralFormat format)
    {
        return format.ByteOrder.Resolve() switch
        {
            ByteOrder.LittleEndian => new LockedRingBufferLE(capacity, format),
            ByteOrder.BigEndian => new LockedRingBufferBE(capacity, format),
            _ => throw new ArgumentOutOfRangeException(nameof(format.ByteOrder)),
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

    public static IWaitableRingBuffer CreateWaitable(int capacity, IntegralFormat format)
    {
        return format.ByteOrder.Resolve() switch
        {
            ByteOrder.LittleEndian => new WaitableRingBufferLE(capacity, format),
            ByteOrder.BigEndian => new WaitableRingBufferBE(capacity, format),
            _ => throw new ArgumentOutOfRangeException(nameof(format.ByteOrder)),
        };
    }
}
