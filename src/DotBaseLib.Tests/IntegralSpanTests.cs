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
                1).Validate());
        Assert.Throws<ArgumentException>(
            () => new IntegralSpan(
                null,
                sizeof(int),
                0,
                IntegralType.Int32,
                1).Validate());
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
                1).Validate());
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new IntegralSpan(
                (byte*)address,
                0,
                -1,
                IntegralType.Int32,
                1).Validate());
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new IntegralSpan(
                (byte*)address,
                1,
                sizeof(int),
                IntegralType.Int32,
                1).Validate());
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new IntegralSpan(
                (byte*)address,
                0,
                sizeof(int) + 1,
                IntegralType.Int32,
                1).Validate());
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
                1).Validate());
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

            Assert.Equal(11, span.ValueCount);
            Assert.Equal(2, span.BlockCount);
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
            Assert.Equal(2, blocks.BlockCount);
            Assert.Equal(ByteOrder.BigEndian, blocks.Format.ByteOrder);

            IntegralSpan valuesSlice = span.GetValueSpan(2, 5);
            Assert.Equal(2 * sizeof(int), valuesSlice.Offset);
            Assert.Equal(5 * sizeof(int), valuesSlice.Length);
            Assert.Equal(5, valuesSlice.ValueCount);
            Assert.Equal(1, valuesSlice.BlockCount);
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
            Assert.Equal(3, middle.ValueCount);
            Assert.Equal(33, middle.AtIndex<int>(0));
            Assert.Equal(44, middle.AtIndex<int>(1));
            Assert.Equal(55, middle.AtIndex<int>(2));
            Assert.Equal(IntegralType.Int32, middle.IntegralValueType);
            Assert.Equal(3, middle.Capacity.BlockCapacity);
        }
    }

    [Fact]
    public void GetBlockSpanRangeMatchesOffsetCountAndRetypeKeepsParentUnits()
    {
        byte[] storage = new byte[16];
        for (int i = 0; i < storage.Length; ++i)
        {
            storage[i] = unchecked((byte)(i + 1));
        }

        fixed (byte* pointer = storage)
        {
            IntegralSpan slab = new(
                pointer,
                0,
                storage.Length,
                IntegralType.UInt8,
                1);

            // On UInt8 BC=1 parent: BlockOffset/BlockCount numerically equal byte offsets.
            IntegralRange range = new(
                blockOffset: 4,
                blockCount: 8,
                blockByteSize: 1);
            Assert.Equal(4, range.BlockOffset);
            Assert.Equal(8, range.BlockCount);
            Assert.Equal(1, range.BlockByteSize);
            Assert.Equal(4, range.ByteOffset);
            Assert.Equal(8, range.ByteLength);

            IntegralSpan viaRange = slab.GetBlockSpan(range);
            IntegralSpan viaArgs = slab.GetBlockSpan(4, 8);
            Assert.Equal(viaArgs.Offset, viaRange.Offset);
            Assert.Equal(viaArgs.Length, viaRange.Length);
            Assert.Equal(viaArgs.ValueCount, viaRange.ValueCount);

            // 8 parent blocks (bytes here); retype to Int32 does not treat 8 as int32 values.
            IntegralSpan asInt32 = slab.GetBlockSpan(
                range,
                IntegralType.Int32,
                blockCapacity: 1);
            Assert.Equal(8, asInt32.Length);
            Assert.Equal(2, asInt32.ValueCount);
            Assert.Equal(slab.Format.ByteOrder, asInt32.Format.ByteOrder);
        }
    }

    [Fact]
    public void ChangeFormatRelabelsRegionWithoutMovingMemory()
    {
        int[] values = [0x01020304, 0x05060708];
        fixed (int* pointer = values)
        {
            IntegralSpan bytes = new(
                (byte*)pointer,
                0,
                values.Length * sizeof(int),
                IntegralType.UInt8,
                1);

            IntegralSpan beBytes = new(
                bytes.BytePtr,
                bytes.Offset,
                bytes.Length,
                new IntegralFormat(IntegralType.UInt8, 1, ByteOrder.BigEndian));

            IntegralSpan asInt32 = beBytes.ChangeFormat(
                IntegralType.Int32,
                blockCapacity: 2);

            Assert.Equal((nint)beBytes.BytePtr, (nint)asInt32.BytePtr);
            Assert.Equal(beBytes.Offset, asInt32.Offset);
            Assert.Equal(beBytes.Length, asInt32.Length);
            Assert.Equal(IntegralType.Int32, asInt32.IntegralValueType);
            Assert.Equal(2, asInt32.Capacity.BlockCapacity);
            // Byte order is preserved from the source span, not an argument.
            Assert.Equal(ByteOrder.BigEndian, asInt32.Format.ByteOrder);
            Assert.Equal(2, asInt32.ValueCount);
            Assert.Equal(1, asInt32.BlockCount);

            IntegralSpan empty = IntegralSpan.Empty.ChangeFormat(IntegralType.Int16);
            Assert.Equal(0, empty.Length);
            Assert.Equal(0, (nint)empty.BytePtr);
        }
    }

    [Fact]
    public void ChangeFormatCheckedRejectsMisalignedGeometry()
    {
        byte[] storage = new byte[6];
        fixed (byte* pointer = storage)
        {
            IntegralSpan bytes = new(
                pointer,
                0,
                storage.Length,
                IntegralType.UInt8,
                1);

            Assert.ThrowsAny<ArgumentException>(
                () => bytes.ChangeFormatChecked(IntegralType.Int32));
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
        // ValueSize × BlockCapacity is a long product — fits for any two ints.
        IntegralFormat largeBlockFormat = new(
            IntegralType.UInt64,
            int.MaxValue);
        largeBlockFormat.Validate();
        Assert.Equal(
            (long)sizeof(ulong) * int.MaxValue,
            largeBlockFormat.BytesPerBlock);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new IntegralCapacity(
                long.MaxValue,
                IntegralType.UInt64,
                1).Validate());

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
                1).Validate());
    }

    private static void AssertEmpty(in IntegralSpan span)
    {
        Assert.Equal(0, span.Offset);
        Assert.Equal(0, span.Length);
        Assert.Equal(0, span.ValueCount);
        Assert.Equal(0, span.BlockCount);
        Assert.Equal(0, span.TrailingValueCount);
        Assert.Equal(0, (nint)span.BytePtr);
        Assert.Equal(0, (nint)span.DataPtr);
        Assert.True(span.Capacity.IsValueAligned());
    }
}
