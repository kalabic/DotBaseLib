using DotBase.Buffers.Integral.Internal;
using System.Diagnostics;

namespace DotBase.Buffers.Integral;


/// <summary>Provides factory methods for creating byte-addressed integral ring buffers with a fixed byte order.</summary>
public static class IntegralRingBuffer
{
    public static ByteOrder GetNativeByteOrder()
    {
        return BitConverter.IsLittleEndian
            ? ByteOrder.LittleEndian
            : ByteOrder.BigEndian;
    }

    public static IIntegralRingBuffer Create(
        int capacity,
        ByteOrder byteOrder)
    {
        return Resolve(byteOrder) switch
        {
            ByteOrder.LittleEndian => new ByteRingLE(capacity),
            ByteOrder.BigEndian => new ByteRingBE(capacity),
            _ => throw new UnreachableException(),
        };
    }

    public static IIntegralRingBuffer CreateLocked(
        int capacity,
        ByteOrder byteOrder)
    {
        return Resolve(byteOrder) switch
        {
            ByteOrder.LittleEndian => new LockedRingBufferLE(capacity),
            ByteOrder.BigEndian => new LockedRingBufferBE(capacity),
            _ => throw new UnreachableException(),
        };
    }

    public static IWaitableRingBuffer CreateWaitable(
        int capacity,
        ByteOrder byteOrder)
    {
        return Resolve(byteOrder) switch
        {
            ByteOrder.LittleEndian => new WaitableRingBufferLE(capacity),
            ByteOrder.BigEndian => new WaitableRingBufferBE(capacity),
            _ => throw new UnreachableException(),
        };
    }

    private static ByteOrder Resolve(ByteOrder byteOrder)
    {
        return byteOrder switch
        {
            ByteOrder.Native => GetNativeByteOrder(),
            ByteOrder.LittleEndian => ByteOrder.LittleEndian,
            ByteOrder.BigEndian => ByteOrder.BigEndian,
            _ => throw new ArgumentOutOfRangeException(nameof(byteOrder)),
        };
    }
}
