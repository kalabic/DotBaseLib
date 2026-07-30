using DotBase.Buffers;
using DotBase.Integral;

namespace DotBaseLib.Tests;


public unsafe class IntegralSpanTests
{
    [Fact]
    public void EmptySpansAreNormalized()
    {
        AssertEmpty(default);
        AssertEmpty(new IntegralSpan());
        AssertEmpty(IntegralSpan.Empty);

        IntegralSpan explicitEmpty = new(
            null,
            0,
            0,
            IntegralFormat.NONE);
        AssertEmpty(explicitEmpty);
    }

    [Fact]
    public void NullPointerAcceptsOnlyZeroOffsetAndLength()
    {
        IntegralSpan typedEmpty = new(
            null,
            0,
            0,
            new IntegralFormat(
                IntegralType.Int32,
                1));

        Assert.Equal(0, typedEmpty.Length);
        Assert.Equal(IntegralType.Int32, typedEmpty.IntegralValueType);

        Assert.Throws<ArgumentException>(
            () => new IntegralSpan(
                null,
                0,
                sizeof(int),
                IntegralType.Int32,
                1));
        Assert.Throws<ArgumentException>(
            () => new IntegralSpan(
                null,
                sizeof(int),
                0,
                IntegralType.Int32,
                1));
    }

    [Fact]
    public void NegativeAndMisalignedRangesAreRejected()
    {
        // Aligned fake base; exercise offset/length rules only.
        nint address = 8;

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new IntegralSpan(
                (byte*)address,
                -1,
                sizeof(int),
                IntegralType.Int32,
                1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new IntegralSpan(
                (byte*)address,
                0,
                -1,
                IntegralType.Int32,
                1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new IntegralSpan(
                (byte*)address,
                1,
                sizeof(int),
                IntegralType.Int32,
                1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new IntegralSpan(
                (byte*)address,
                0,
                sizeof(int) + 1,
                IntegralType.Int32,
                1));
    }

    [Fact]
    public void UnalignedBaseAddressIsRejectedForMultiByteScalars()
    {
        nint unaligned = 1;
        Assert.Throws<ArgumentException>(
            () => new IntegralSpan(
                (byte*)unaligned,
                0,
                sizeof(int),
                IntegralType.Int32,
                1));
    }

    [Fact]
    public void CapacityReportsCompleteBlocksAndTrailingValues()
    {
        int[] values = new int[11];
        fixed (int* pointer = values)
        {
            IntegralSpan span = new(
                (byte*)pointer,
                0,
                values.Length * sizeof(int),
                IntegralType.Int32,
                4);

            Assert.Equal(11, span.IntegralLength);
            Assert.Equal(2, span.BlockLength);
            Assert.Equal(3, span.TrailingValueCount);
            Assert.Equal(4, span.Capacity.BlockCapacity);
            Assert.Equal(16, span.Capacity.BlockByteCount);
        }
    }

    [Fact]
    public void BlockValueAndByteSlicesRetainFormatAndBasePointer()
    {
        int[] values = new int[12];
        fixed (int* pointer = values)
        {
            IntegralSpan span = new(
                (byte*)pointer,
                0,
                values.Length * sizeof(int),
                new IntegralFormat(
                    IntegralType.Int32,
                    3,
                    ByteOrder.BigEndian));

            IntegralSpan blocks = span.GetBlockSpan(1, 2);
            Assert.Equal((nint)span.BytePtr, (nint)blocks.BytePtr);
            Assert.Equal(3 * sizeof(int), blocks.Offset);
            Assert.Equal(6 * sizeof(int), blocks.Length);
            Assert.Equal(2, blocks.BlockLength);
            Assert.Equal(ByteOrder.BigEndian, blocks.Format.ByteOrder);

            IntegralSpan valuesSlice = span.GetValueSpan(2, 5);
            Assert.Equal(2 * sizeof(int), valuesSlice.Offset);
            Assert.Equal(5 * sizeof(int), valuesSlice.Length);
            Assert.Equal(5, valuesSlice.IntegralLength);
            Assert.Equal(1, valuesSlice.BlockLength);
            Assert.Equal(2, valuesSlice.TrailingValueCount);

            IntegralSpan bytes = span.GetSubSpan(
                4 * sizeof(int),
                3 * sizeof(int));
            Assert.Equal(4 * sizeof(int), bytes.Offset);
            Assert.Equal(3 * sizeof(int), bytes.Length);
            Assert.Equal((nint)(pointer + 4), (nint)bytes.DataPtr);
        }
    }

    [Fact]
    public void ZeroLengthSlicesAreAllowedAtTheEnd()
    {
        long[] values = new long[4];
        fixed (long* pointer = values)
        {
            IntegralSpan span = new(
                (byte*)pointer,
                0,
                values.Length * sizeof(long),
                IntegralType.Int64,
                2);

            IntegralSpan blockEnd = span.GetBlockSpan(2, 0);
            IntegralSpan valueEnd = span.GetValueSpan(4, 0);
            IntegralSpan byteEnd = span.GetSubSpan(span.Length, 0);

            Assert.Equal(span.Length, blockEnd.Offset);
            Assert.Equal(span.Length, valueEnd.Offset);
            Assert.Equal(span.Length, byteEnd.Offset);
            Assert.Equal(0, blockEnd.Length);
            Assert.Equal(0, valueEnd.Length);
            Assert.Equal(0, byteEnd.Length);
        }
    }

    [Fact]
    public void GetSubSpanPreservesScalarValuesWithoutRevalidatingFormat()
    {
        int[] values = [11, 22, 33, 44, 55, 66];
        fixed (int* pointer = values)
        {
            IntegralSpan span = new(
                (byte*)pointer,
                0,
                values.Length * sizeof(int),
                IntegralType.Int32,
                3);

            IntegralSpan middle = span.GetValueSpan(2, 3);
            Assert.Equal(3, middle.IntegralLength);
            Assert.Equal(33, middle.AtIndex<int>(0));
            Assert.Equal(44, middle.AtIndex<int>(1));
            Assert.Equal(55, middle.AtIndex<int>(2));
            Assert.Equal(IntegralType.Int32, middle.IntegralValueType);
            Assert.Equal(3, middle.Capacity.BlockCapacity);
        }
    }

    [Fact]
    public void LittleAndBigEndianScalarRoundTripThroughSpan()
    {
        int host = unchecked((int)0xA1B2C3D4);
        byte[] leBytes = new byte[sizeof(int)];
        byte[] beBytes = new byte[sizeof(int)];

        fixed (byte* lePtr = leBytes)
        fixed (byte* bePtr = beBytes)
        {
            IntegralSpan le = IntegralTestData.CreateSpan(
                lePtr,
                1,
                IntegralType.Int32,
                ByteOrder.LittleEndian);
            IntegralSpan be = IntegralTestData.CreateSpan(
                bePtr,
                1,
                IntegralType.Int32,
                ByteOrder.BigEndian);

            le.SetAtIndex(0, host);
            be.SetAtIndex(0, host);

            Assert.Equal(host, le.AtIndex<int>(0));
            Assert.Equal(host, be.AtIndex<int>(0));
            Assert.Equal(leBytes[0], beBytes[3]);
            Assert.Equal(leBytes[1], beBytes[2]);
            Assert.Equal(leBytes[2], beBytes[1]);
            Assert.Equal(leBytes[3], beBytes[0]);
        }
    }

    [Fact]
    public void SliceBoundsAndAlignmentAreValidated()
    {
        nint address = 8;
        IntegralSpan span = new(
            (byte*)address,
            0,
            4 * sizeof(int),
            IntegralType.Int32,
            2);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => span.GetBlockSpan(2, 1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => span.GetValueSpan(4, 1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => span.GetSubSpan(1, sizeof(int)));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => span.GetSubSpan(0, sizeof(int) - 1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => span.GetSubSpan(span.Length + 1, 0));
    }

    [Fact]
    public void ArithmeticAndCapacityOverflowAreRejected()
    {
        Assert.Throws<OverflowException>(
            () => new IntegralFormat(
                IntegralType.UInt64,
                int.MaxValue));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new IntegralCapacity(
                long.MaxValue,
                IntegralType.UInt64,
                1).ThrowIfArgumentOutOfRange());

        nint address = 8;
        IntegralSpan highOffset = new(
            (byte*)address,
            long.MaxValue - 1,
            sizeof(ushort),
            IntegralType.UInt16,
            1);
        Assert.Throws<OverflowException>(
            () => highOffset.GetSubSpan(
                sizeof(ushort),
                0));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new IntegralSpan(
                (byte*)address,
                0,
                long.MaxValue,
                IntegralType.UInt64,
                1));
    }

    private static void AssertEmpty(in IntegralSpan span)
    {
        Assert.Equal(0, span.Offset);
        Assert.Equal(0, span.Length);
        Assert.Equal(0, span.IntegralLength);
        Assert.Equal(0, span.BlockLength);
        Assert.Equal(0, span.TrailingValueCount);
        Assert.Equal(IntegralType.NONE, span.IntegralValueType);
        Assert.Equal(0, (nint)span.BytePtr);
        Assert.Equal(0, (nint)span.DataPtr);
        Assert.True(span.Capacity.IsValid());
    }
}
