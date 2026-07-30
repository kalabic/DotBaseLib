using DotBase.Integral;

namespace DotBaseLib.Tests;


public unsafe class IntegralMemoryStridedTests
{
    [Fact]
    public void ExtractsAndInsertsInterleavedChannelsWithOffsets()
    {
        int[] interleaved =
        [
            10, 11, 12,
            20, 21, 22,
            30, 31, 32,
            40, 41, 42,
        ];
        int[] extracted = new int[4];

        fixed (int* sourcePtr = interleaved)
        fixed (int* destinationPtr = extracted)
        {
            IntegralSpan source = IntegralTestData.CreateSpan(
                (byte*)sourcePtr,
                interleaved.Length,
                IntegralType.Int32);
            IntegralSpan destination = IntegralTestData.CreateSpan(
                (byte*)destinationPtr,
                extracted.Length,
                IntegralType.Int32);

            IntegralMemory.CopyStrided(
                source,
                1,
                3,
                destination,
                0,
                1,
                4);
        }

        Assert.Equal(
            new[] { 11, 21, 31, 41 },
            extracted);

        int[] inserted = Enumerable.Repeat(-1, 12).ToArray();
        int[] channel = [7, 8, 9, 10];
        fixed (int* sourcePtr = channel)
        fixed (int* destinationPtr = inserted)
        {
            IntegralSpan source = IntegralTestData.CreateSpan(
                (byte*)sourcePtr,
                channel.Length,
                IntegralType.Int32);
            IntegralSpan destination = IntegralTestData.CreateSpan(
                (byte*)destinationPtr,
                inserted.Length,
                IntegralType.Int32);

            IntegralMemory.CopyStrided(
                source,
                0,
                1,
                destination,
                2,
                3,
                4);
        }

        Assert.Equal(7, inserted[2]);
        Assert.Equal(8, inserted[5]);
        Assert.Equal(9, inserted[8]);
        Assert.Equal(10, inserted[11]);
        Assert.Equal(-1, inserted[10]);
    }

    [Fact]
    public void SupportsOneValueAndTheLastValidTouchedValue()
    {
        short[] sourceValues = [1, 2, 3, 4, 5, 6, 7, 8];
        short[] destinationValues = new short[8];

        fixed (short* sourcePtr = sourceValues)
        fixed (short* destinationPtr = destinationValues)
        {
            IntegralSpan source = IntegralTestData.CreateSpan(
                (byte*)sourcePtr,
                sourceValues.Length,
                IntegralType.Int16);
            IntegralSpan destination = IntegralTestData.CreateSpan(
                (byte*)destinationPtr,
                destinationValues.Length,
                IntegralType.Int16);

            IntegralMemory.CopyStrided(
                source,
                7,
                long.MaxValue,
                destination,
                7,
                long.MaxValue,
                1);
            Assert.Equal(8, destinationValues[7]);

            IntegralMemory.CopyStrided(
                source,
                1,
                3,
                destination,
                0,
                2,
                3);
            Assert.Equal(2, destinationValues[0]);
            Assert.Equal(5, destinationValues[2]);
            Assert.Equal(8, destinationValues[4]);
        }
    }

    [Fact]
    public void AllowsDisjointTouchedRangesInTheSameAllocation()
    {
        byte[] values = [1, 0, 2, 0, 3, 0];

        fixed (byte* pointer = values)
        {
            IntegralSpan span = IntegralTestData.CreateSpan(
                pointer,
                values.Length,
                IntegralType.UInt8);

            IntegralMemory.CopyStrided(
                span,
                0,
                2,
                span,
                1,
                2,
                3);
        }

        Assert.Equal(
            new byte[] { 1, 1, 2, 2, 3, 3 },
            values);
    }

    [Fact]
    public void TouchedRangeOverlapIsRejected()
    {
        byte[] values = [1, 2, 3, 4, 5, 6];

        fixed (byte* pointer = values)
        {
            IntegralSpan span = IntegralTestData.CreateSpan(
                pointer,
                values.Length,
                IntegralType.UInt8);

            Assert.Throws<ArgumentException>(
                () => IntegralMemory.CopyStrided(
                    span,
                    0,
                    2,
                    span,
                    1,
                    1,
                    3));
        }

        Assert.Equal(
            new byte[] { 1, 2, 3, 4, 5, 6 },
            values);
    }

    [Fact]
    public void InvalidStrideCountAndBoundsAreRejected()
    {
        int[] sourceValues = new int[4];
        int[] destinationValues = new int[4];

        fixed (int* sourcePtr = sourceValues)
        fixed (int* destinationPtr = destinationValues)
        {
            IntegralSpan source = IntegralTestData.CreateSpan(
                (byte*)sourcePtr,
                sourceValues.Length,
                IntegralType.Int32);
            IntegralSpan destination = IntegralTestData.CreateSpan(
                (byte*)destinationPtr,
                destinationValues.Length,
                IntegralType.Int32);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => IntegralMemory.CopyStrided(
                    source,
                    0,
                    0,
                    destination,
                    0,
                    1,
                    1));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => IntegralMemory.CopyStrided(
                    source,
                    0,
                    1,
                    destination,
                    0,
                    1,
                    -1));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => IntegralMemory.CopyStrided(
                    source,
                    1,
                    2,
                    destination,
                    0,
                    1,
                    3));

            IntegralMemory.CopyStrided(
                source,
                source.IntegralLength,
                0,
                destination,
                destination.IntegralLength,
                0,
                0);
        }
    }

    [Fact]
    public void StridedIndexArithmeticOverflowIsRejected()
    {
        IntegralSpan source = IntegralTestData.CreateSpan(
            (byte*)1,
            long.MaxValue,
            IntegralType.UInt8);
        IntegralSpan destination = IntegralTestData.CreateSpan(
            (byte*)2,
            long.MaxValue,
            IntegralType.UInt8);

        Assert.Throws<OverflowException>(
            () => IntegralMemory.CopyStrided(
                source,
                1,
                long.MaxValue,
                destination,
                0,
                1,
                2));
    }
}
