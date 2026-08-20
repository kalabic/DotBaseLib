using DotBase.Buffers;
using DotBase.Buffers.Integral;
using DotBase.Integral;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DotBaseLib.Tests;


public class IntegralRingBufferTests
{
    [Fact]
    public void EveryDeclaredRepresentationHasCompletePartialAtomicAndWraparoundSemantics()
    {
        RunRingCase<byte>([1, 2, 3, 4, 5, 6]);
        RunRingCase<sbyte>([1, 2, 3, 4, 5, 6]);
        RunRingCase<ushort>([1, 2, 3, 4, 5, 6]);
        RunRingCase<short>([1, 2, 3, 4, 5, 6]);
        RunRingCase<uint>([1, 2, 3, 4, 5, 6]);
        RunRingCase<int>([1, 2, 3, 4, 5, 6]);
        RunRingCase<ulong>([1, 2, 3, 4, 5, 6]);
        RunRingCase<long>([1, 2, 3, 4, 5, 6]);
        RunRingCase<float>([1, 2, 3, 4, 5, 6]);
        RunRingCase<double>([1, 2, 3, 4, 5, 6]);
    }

    [Fact]
    public void FloatingPointRingTransfersPreserveExactRepresentations()
    {
        float[] singles =
        [
            BitConverter.Int32BitsToSingle(
                unchecked((int)0x80000000)),
            float.PositiveInfinity,
            float.NegativeInfinity,
            BitConverter.Int32BitsToSingle(
                unchecked((int)0x7FC00001)),
            BitConverter.Int32BitsToSingle(
                unchecked((int)0xFFC12345)),
        ];
        double[] doubles =
        [
            BitConverter.Int64BitsToDouble(
                unchecked((long)0x8000000000000000)),
            double.PositiveInfinity,
            double.NegativeInfinity,
            BitConverter.Int64BitsToDouble(
                unchecked((long)0x7FF8000000000001)),
            BitConverter.Int64BitsToDouble(
                unchecked((long)0xFFF8123456789ABC)),
        ];

        RunExactRepresentationCase(singles);
        RunExactRepresentationCase(doubles);
    }

    [Fact]
    public async Task WaitableReadsRequireTheCompleteRequestForEveryRepresentation()
    {
        await RunWaitableCase<byte>(1, 2);
        await RunWaitableCase<sbyte>(1, 2);
        await RunWaitableCase<ushort>(1, 2);
        await RunWaitableCase<short>(1, 2);
        await RunWaitableCase<uint>(1, 2);
        await RunWaitableCase<int>(1, 2);
        await RunWaitableCase<ulong>(1, 2);
        await RunWaitableCase<long>(1, 2);
        await RunWaitableCase<float>(1, 2);
        await RunWaitableCase<double>(1, 2);
    }

    [Fact]
    public unsafe void CheckedRawDescriptorsSeparateCapacityFromValidation()
    {
        using IIntegralRingBuffer ring =
            IntegralRingBuffer.CreateUnlocked(
                2 * sizeof(int),
                ForeignByteOrder);
        int[] sourceValues = [1, 2, 3];

        fixed (int* sourcePtr = sourceValues)
        {
            IntegralSpan oversized = IntegralTestData.CreateSpan(
                (byte*)sourcePtr,
                sourceValues.Length,
                IntegralType.Int32);
            Assert.False(ring.TryWriteChecked(oversized));
            Assert.Equal(0, ring.StoredBytes);

            IntegralSpan capacityMismatch =
                IntegralTestData.CreateSpan(
                    (byte*)sourcePtr,
                    2,
                    IntegralType.Int32,
                    blockCapacity: 1);
            ref IntegralSpanLayout mismatchLayout =
                ref Unsafe.As<
                    IntegralSpan,
                    IntegralSpanLayout>(
                        ref capacityMismatch);
            mismatchLayout.Capacity = new IntegralCapacity(
                2 * sizeof(int),
                IntegralType.Int32,
                2);

            Assert.Throws<ArgumentException>(
                () => ring.TryWriteChecked(
                    capacityMismatch));
            Assert.Equal(0, ring.StoredBytes);

            // Explicitly invalid format (no layout overlay): Undefined byte order.
            IntegralSpan invalidFormat = new(
                (byte*)sourcePtr,
                0,
                2 * sizeof(int),
                new IntegralFormat(
                    IntegralType.Int32,
                    1,
                    ByteOrder.Undefined));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => ring.TryWriteChecked(
                    invalidFormat));
            Assert.Equal(0, ring.StoredBytes);
        }
    }

    [Fact]
    public unsafe void FailedRawTryOperationsLeaveBothSidesUnchanged()
    {
        using IIntegralRingBuffer ring =
            IntegralRingBuffer.CreateUnlocked(
                2 * sizeof(int),
                ForeignByteOrder);

        int existing = 17;
        Assert.True(ring.TryWrite(existing));

        byte[] destinationBytes =
            Enumerable.Repeat((byte)0xA5, 2 * sizeof(int))
                .ToArray();
        byte[] expectedDestination =
            destinationBytes.ToArray();

        fixed (byte* destinationPtr = destinationBytes)
        {
            IntegralSpan destination =
                IntegralTestData.CreateSpan(
                    destinationPtr,
                    2,
                    IntegralType.Int32);
            Assert.False(ring.TryReadChecked(
                destination));
        }

        Assert.Equal(
            expectedDestination,
            destinationBytes);
        Assert.Equal(sizeof(int), ring.StoredBytes);

        int[] tooMany = [1, 2];
        fixed (int* sourcePtr = tooMany)
        {
            IntegralSpan source = IntegralTestData.CreateSpan(
                (byte*)sourcePtr,
                2,
                IntegralType.Int32);
            Assert.False(ring.TryWriteChecked(
                source));
        }

        Assert.Equal(sizeof(int), ring.StoredBytes);
        Assert.True(ring.TryRead<int>(out int actual));
        Assert.Equal(existing, actual);
    }

    private static ByteOrder ForeignByteOrder =>
        BitConverter.IsLittleEndian
            ? ByteOrder.BigEndian
            : ByteOrder.LittleEndian;

    private static void RunRingCase<T>(T[] values)
        where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();

        using (IIntegralRingBuffer ring =
               IntegralRingBuffer.CreateUnlocked(
                   3 * size,
                   ForeignByteOrder))
        {
            Assert.Equal(
                3,
                ring.Write<T>(
                    values,
                    0,
                    values.Length));

            T[] destination = new T[values.Length];
            Assert.Equal(
                3,
                ring.Read<T>(
                    destination,
                    0,
                    destination.Length));
            AssertValuesEqual<T>(
                values.AsSpan(0, 3),
                destination.AsSpan(0, 3));

            if (size > 1)
            {
                byte[] incomplete =
                    Enumerable.Repeat(
                        (byte)0x5A,
                        size - 1).ToArray();
                Assert.Equal(
                    incomplete.Length,
                    ring.Write(
                        incomplete,
                        0,
                        incomplete.Length));
                T[] oneValue = new T[1];
                Assert.Equal(
                    0,
                    ring.Read<T>(
                        oneValue,
                        0,
                        1));
                Assert.Equal(
                    incomplete.Length,
                    ring.StoredBytes);
                ring.ClearBuffer();
            }
        }

        using (IIntegralRingBuffer ring =
               IntegralRingBuffer.CreateUnlocked(
                   2 * size,
                   ForeignByteOrder))
        {
            Assert.False(
                ring.TryWrite<T>(
                    values,
                    0,
                    3));
            Assert.Equal(0, ring.StoredBytes);

            Assert.True(ring.TryWrite(values[0]));
            T[] unchanged =
                Enumerable.Repeat(
                    values[5],
                    2).ToArray();
            T[] expectedUnchanged = unchanged.ToArray();
            Assert.False(
                ring.TryRead<T>(
                    unchanged,
                    0,
                    unchanged.Length));
            AssertValuesEqual<T>(
                expectedUnchanged,
                unchanged);
            Assert.Equal(size, ring.StoredBytes);

            Assert.False(
                ring.TryWrite<T>(
                    values,
                    1,
                    2));
            Assert.Equal(size, ring.StoredBytes);
            Assert.True(ring.TryRead<T>(out T existing));
            AssertValueEqual(
                values[0],
                existing);
        }

        using (IIntegralRingBuffer ring =
               IntegralRingBuffer.CreateUnlocked(
                   4 * size,
                   ForeignByteOrder))
        {
            Assert.Equal(
                3,
                ring.Write<T>(
                    values,
                    0,
                    3));

            T[] discarded = new T[2];
            Assert.Equal(
                2,
                ring.Read<T>(
                    discarded,
                    0,
                    2));
            AssertValuesEqual<T>(
                values.AsSpan(0, 2),
                discarded);

            Assert.Equal(
                3,
                ring.Write<T>(
                    values,
                    3,
                    3));

            T[] wrapped = new T[4];
            Assert.Equal(
                4,
                ring.Read<T>(
                    wrapped,
                    0,
                    wrapped.Length));

            T[] expected =
            [
                values[2],
                values[3],
                values[4],
                values[5],
            ];
            AssertValuesEqual<T>(
                expected,
                wrapped);
        }
    }

    private static void RunExactRepresentationCase<T>(
        T[] values)
        where T : unmanaged
    {
        using IIntegralRingBuffer ring =
            IntegralRingBuffer.CreateUnlocked(
                checked(values.Length * Unsafe.SizeOf<T>()),
                ForeignByteOrder);

        Assert.Equal(
            values.Length,
            ring.Write<T>(
                values,
                0,
                values.Length));

        T[] destination = new T[values.Length];
        Assert.Equal(
            values.Length,
            ring.Read<T>(
                destination,
                0,
                destination.Length));
        AssertValuesEqual<T>(
            values,
            destination);
    }

    private static async Task RunWaitableCase<T>(
        T first,
        T second)
        where T : unmanaged
    {
        using IWaitableRingBuffer ring =
            IntegralRingBuffer.CreateWaitable(
                4 * Unsafe.SizeOf<T>(),
                ForeignByteOrder);

        Assert.True(ring.TryWrite(first));
        T[] destination = new T[2];
        using ManualResetEventSlim started = new();

        Task<int> readTask = Task.Run(
            () =>
            {
                started.Set();
                return ring.Read<T>(
                    destination,
                    0,
                    destination.Length);
            });

        Assert.True(
            started.Wait(
                TimeSpan.FromSeconds(2)));
        await Task.Delay(50);
        Assert.False(readTask.IsCompleted);

        Assert.True(ring.TryWrite(second));
        int readCount = await readTask.WaitAsync(
            TimeSpan.FromSeconds(5));

        Assert.Equal(2, readCount);
        AssertValueEqual(first, destination[0]);
        AssertValueEqual(second, destination[1]);
    }

    private static void AssertValuesEqual<T>(
        ReadOnlySpan<T> expected,
        ReadOnlySpan<T> actual)
        where T : unmanaged
    {
        Assert.Equal(expected.Length, actual.Length);
        for (int index = 0; index < expected.Length; ++index)
        {
            AssertValueEqual(
                expected[index],
                actual[index]);
        }
    }

    private static void AssertValueEqual<T>(
        T expected,
        T actual)
        where T : unmanaged
    {
        Assert.Equal(
            IntegralTestData.NativeBytes(expected),
            IntegralTestData.NativeBytes(actual));
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IntegralSpanLayout
    {
        internal IntegralPtr Ptr;
        internal long Offset;
        internal long Length;
        internal IntegralCapacity Capacity;
    }
}
