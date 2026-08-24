using DotBase.Buffers;
using DotBase.Buffers.Integral;
using DotBase.Integral;

namespace DotBaseLib.Tests;


public class RingBufferNonThrowingContractTests
{
    public enum RingKind
    {
        Unlocked,
        Locked,
        Waitable,
    }

    public static TheoryData<RingKind, ByteOrder> AllVariants()
    {
        TheoryData<RingKind, ByteOrder> data = [];
        foreach (RingKind kind in Enum.GetValues<RingKind>())
        {
            data.Add(kind, ByteOrder.LittleEndian);
            data.Add(kind, ByteOrder.BigEndian);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(AllVariants))]
    public void ByteCapacityBoundariesArePartialForAllVariants(
        RingKind kind,
        ByteOrder byteOrder)
    {
        byte[] oversized = [1, 2, 3, 4, 5];
        using (IIntegralRingBuffer ring = Create(kind, 4, byteOrder))
        {
            long writtenBefore = GetCounter(ring, "TotalWritten");
            const int expected = 4;
            Assert.Equal(expected, ring.Write(oversized, 0, oversized.Length));
            Assert.Equal(expected, ring.StoredBytes);
            Assert.Equal(writtenBefore + expected, GetCounter(ring, "TotalWritten"));

            long readBefore = GetCounter(ring, "TotalRead");
            Assert.Equal(expected, ring.Read(new byte[oversized.Length], 0, oversized.Length));
            Assert.Equal(0, ring.StoredBytes);
            Assert.Equal(readBefore + expected, GetCounter(ring, "TotalRead"));
        }

        using (IIntegralRingBuffer ring = Create(kind, 4, byteOrder))
        {
            byte[] exact = [1, 2, 3, 4];
            Assert.Equal(exact.Length, ring.Write(exact, 0, exact.Length));
            Assert.Equal(exact.Length, ring.Read(new byte[exact.Length], 0, exact.Length));
        }
    }

    [Theory]
    [MemberData(nameof(AllVariants))]
    public void ScalarsReturnFalseAndDefaultForImpossibleOrClosedOperations(
        RingKind kind,
        ByteOrder byteOrder)
    {
        using (IIntegralRingBuffer ring = Create(kind, 1, byteOrder))
        {
            int value = 42;
            Assert.False(ring.Read(out value));
            Assert.Equal(default, value);
            Assert.False(ring.Write(42));
            Assert.Equal(0, ring.StoredBytes);
            Assert.Equal(0, GetCounter(ring, "TotalRead"));
            Assert.Equal(0, GetCounter(ring, "TotalWritten"));
        }

        using (IIntegralRingBuffer ring = Create(kind, sizeof(int), byteOrder))
        {
            ring.Close();
            int value = 42;
            Assert.False(ring.Read(out value));
            Assert.Equal(default, value);
            Assert.False(ring.Write(42));
            Assert.Equal(0, ring.Read(new byte[1], 0, 1));
            Assert.Equal(0, ring.Write(new byte[1], 0, 1));
        }
    }

    [Theory]
    [MemberData(nameof(AllVariants))]
    public void AtomicFailurePreservesDataAndCounters(
        RingKind kind,
        ByteOrder byteOrder)
    {
        using IIntegralRingBuffer ring = Create(kind, sizeof(int), byteOrder);
        int[] twoValues = [7, 8];

        Assert.False(ring.TryWrite((ReadOnlySpan<int>)twoValues));
        Assert.Equal(0, ring.StoredBytes);
        Assert.Equal(0, GetCounter(ring, "TotalWritten"));

        Assert.True(ring.Write(123));
        long readBefore = GetCounter(ring, "TotalRead");
        Assert.False(ring.TryRead(twoValues.AsSpan()));
        Assert.Equal(sizeof(int), ring.StoredBytes);
        Assert.Equal(readBefore, GetCounter(ring, "TotalRead"));

        long writtenBefore = GetCounter(ring, "TotalWritten");
        Assert.False(ring.TryWrite(456));
        Assert.Equal(writtenBefore, GetCounter(ring, "TotalWritten"));
        Assert.True(ring.Read(out int value));
        Assert.Equal(123, value);
    }

    [Theory]
    [MemberData(nameof(AllVariants))]
    public unsafe void CheckedSpanAboveIntMaxIsOperationallyImpossible(
        RingKind kind,
        ByteOrder byteOrder)
    {
        using IIntegralRingBuffer ring = Create(kind, 4, byteOrder);
        byte value = 0;
        IntegralSpan huge = new(
            &value,
            0,
            (long)int.MaxValue + 1,
            new IntegralFormat(
                IntegralType.UInt8,
                1,
                ByteOrder.Native));

        Assert.False(ring.TryWriteChecked(huge));
        Assert.False(ring.TryReadChecked(huge));
        Assert.Equal(0, ring.StoredBytes);
        Assert.Equal(0, GetCounter(ring, "TotalRead"));
        Assert.Equal(0, GetCounter(ring, "TotalWritten"));
    }

    [Theory]
    [MemberData(nameof(AllVariants))]
    public unsafe void IntegralSpanTrailingValuesDoNotIncreaseRequiredCapacity(
        RingKind kind,
        ByteOrder byteOrder)
    {
        int[] source = [11, 22, 33];
        int[] destination = [-1, -1, -1];
        using IIntegralRingBuffer ring = Create(
            kind,
            2 * sizeof(int),
            byteOrder);

        fixed (int* sourcePtr = source)
        fixed (int* destinationPtr = destination)
        {
            IntegralSpan sourceSpan = IntegralTestData.CreateSpan(
                (byte*)sourcePtr,
                source.Length,
                IntegralType.Int32,
                ByteOrder.Native,
                blockCapacity: 2);
            IntegralSpan destinationSpan = IntegralTestData.CreateSpan(
                (byte*)destinationPtr,
                destination.Length,
                IntegralType.Int32,
                ByteOrder.Native,
                blockCapacity: 2);

            Assert.Equal(2, ring.WriteChecked(sourceSpan));
            Assert.Equal(2 * sizeof(int), ring.StoredBytes);
            Assert.Equal(2, ring.ReadChecked(destinationSpan));
        }

        Assert.Equal([11, 22, -1], destination);
    }

    [Theory]
    [MemberData(nameof(AllVariants))]
    public unsafe void MalformedArgumentsStillThrow(
        RingKind kind,
        ByteOrder byteOrder)
    {
        using IIntegralRingBuffer ring = Create(kind, 4, byteOrder);
        byte[] bytes = new byte[4];

        Assert.Throws<ArgumentOutOfRangeException>(
            () => ring.Write(bytes, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ring.Read(bytes, 0, -1));
        Assert.Throws<ArgumentNullException>(
            () => ring.Write((byte*)null, 0, 1));

        int value = 0;
        IntegralSpan invalidFormat = new(
            (byte*)&value,
            0,
            sizeof(int),
            new IntegralFormat(
                IntegralType.Int32,
                1,
                ByteOrder.Undefined));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ring.TryWriteChecked(invalidFormat));
        Assert.Equal(0, ring.StoredBytes);
    }

    private static IIntegralRingBuffer Create(
        RingKind kind,
        int capacity,
        ByteOrder byteOrder)
    {
        return kind switch
        {
            RingKind.Unlocked => IntegralRingBuffer.CreateUnlocked(capacity, byteOrder),
            RingKind.Locked => IntegralRingBuffer.CreateLocked(capacity, byteOrder),
            RingKind.Waitable => IntegralRingBuffer.CreateWaitable(capacity, byteOrder),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }

    private static long GetCounter(IIntegralRingBuffer ring, string propertyName)
    {
        object? value = ring.GetType().GetProperty(propertyName)?.GetValue(ring);
        return Assert.IsType<long>(value);
    }
}
