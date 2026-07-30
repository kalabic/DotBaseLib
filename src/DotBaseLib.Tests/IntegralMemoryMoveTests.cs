using DotBase.Buffers;
using DotBase.Integral;

namespace DotBaseLib.Tests;


public unsafe class IntegralMemoryMoveTests
{
    [Fact]
    public void MoveHandlesLargeOverlappingRegionViaHeapTemp()
    {
        // Force NativeMemory temp path (>512 bytes) for overlapping Move.
        const int Length = 1024;
        byte[] storage = new byte[Length + 64];
        for (int i = 0; i < storage.Length; ++i)
        {
            storage[i] = unchecked((byte)i);
        }

        fixed (byte* pointer = storage)
        {
            IntegralSpan source = IntegralTestData.CreateSpan(
                pointer,
                Length,
                IntegralType.UInt8);
            IntegralSpan destination = IntegralTestData.CreateSpan(
                pointer + 32,
                Length,
                IntegralType.UInt8);
            IntegralMemory.Move(source, destination);
        }

        for (int i = 0; i < Length; ++i)
        {
            Assert.Equal(unchecked((byte)i), storage[i + 32]);
        }
    }

    [Fact]
    public void MoveHandlesForwardAndBackwardOverlap()
    {
        byte[] forward = [1, 2, 3, 4, 5, 6, 7, 8];
        fixed (byte* pointer = forward)
        {
            IntegralSpan source = IntegralTestData.CreateSpan(
                pointer,
                6,
                IntegralType.UInt8);
            IntegralSpan destination = IntegralTestData.CreateSpan(
                pointer + 2,
                6,
                IntegralType.UInt8);
            IntegralMemory.Move(
                source,
                destination);
        }
        Assert.Equal(
            new byte[] { 1, 2, 1, 2, 3, 4, 5, 6 },
            forward);

        byte[] backward = [1, 2, 3, 4, 5, 6, 7, 8];
        fixed (byte* pointer = backward)
        {
            IntegralSpan source = IntegralTestData.CreateSpan(
                pointer + 2,
                6,
                IntegralType.UInt8);
            IntegralSpan destination = IntegralTestData.CreateSpan(
                pointer,
                6,
                IntegralType.UInt8);
            IntegralMemory.Move(
                source,
                destination);
        }
        Assert.Equal(
            new byte[] { 3, 4, 5, 6, 7, 8, 7, 8 },
            backward);
    }

    [Fact]
    public void ExactSameSourceAndDestinationPreservesBytes()
    {
        byte* pointer = IntegralTestData.AlignedAlloc(8);
        try
        {
            pointer[0] = 0xFF;
            pointer[1] = 0xC1;
            pointer[2] = 0x23;
            pointer[3] = 0x45;
            pointer[4] = 0x80;
            pointer[5] = 0x00;
            pointer[6] = 0x00;
            pointer[7] = 0x00;
            byte[] expected =
            [
                0xFF, 0xC1, 0x23, 0x45,
                0x80, 0x00, 0x00, 0x00,
            ];

            IntegralSpan span = IntegralTestData.CreateSpan(
                pointer,
                2,
                IntegralType.Float,
                ByteOrder.BigEndian);
            IntegralMemory.Move(
                span,
                span);

            Assert.Equal(
                expected,
                new ReadOnlySpan<byte>(pointer, 8).ToArray());
        }
        finally
        {
            IntegralTestData.AlignedFree(pointer);
        }
    }

    [Fact]
    public void ExactSamePointerCanReverseEndianInPlace()
    {
        int[] values =
        [
            unchecked((int)0x01020304),
            unchecked((int)0x89ABCDEF),
        ];
        byte* pointer = IntegralTestData.AlignedAlloc(values.Length * sizeof(int));
        try
        {
            IntegralSpan little = IntegralTestData.CreateSpan(
                pointer,
                values.Length,
                IntegralType.Int32,
                ByteOrder.LittleEndian);
            foreach ((int value, int index) in
                     values.Select((value, index) =>
                         (value, index)))
            {
                little.SetAtIndex(index, value);
            }

            IntegralSpan big = IntegralTestData.CreateSpan(
                pointer,
                values.Length,
                IntegralType.Int32,
                ByteOrder.BigEndian);
            IntegralMemory.Move(
                little,
                big);

            for (int index = 0; index < values.Length; ++index)
            {
                Assert.Equal(
                    values[index],
                    big.AtIndex<int>(index));
                Assert.Equal(
                    IntegralTestData.EncodedBytes(
                        values[index],
                        ByteOrder.BigEndian),
                    new ReadOnlySpan<byte>(
                        pointer + index * sizeof(int),
                        sizeof(int)).ToArray());
            }
        }
        finally
        {
            IntegralTestData.AlignedFree(pointer);
        }
    }

    [Fact]
    public void OverlappingTypeAndAffineConversionsPreserveLogicalSource()
    {
        RunOverlappingExpansion(
            4,
            IntegralConversion.Identity,
            index => index + 1);
        RunOverlappingExpansion(
            4,
            new IntegralConversion(
                2,
                1),
            index => (index + 1) * 2 + 1);
    }

    [Fact]
    public void OverlappingConversionUsesStackAndNativeScratchPaths()
    {
        RunOverlappingExpansion(
            64,
            IntegralConversion.Identity,
            index => index % 251);
        RunOverlappingExpansion(
            600,
            IntegralConversion.Identity,
            index => index % 251);
    }

    private static void RunOverlappingExpansion(
        int valueCount,
        in IntegralConversion conversion,
        Func<int, int> expected)
    {
        // Dest at +2 keeps UInt16 natural alignment while still overlapping.
        int byteCount = checked(2 + valueCount * sizeof(ushort));
        byte* pointer = IntegralTestData.AlignedAlloc(byteCount);
        try
        {
            for (int index = 0; index < valueCount; ++index)
            {
                pointer[index] = unchecked(
                    (byte)(index % 251));
            }

            IntegralSpan source = IntegralTestData.CreateSpan(
                pointer,
                valueCount,
                IntegralType.UInt8);
            IntegralSpan destination = IntegralTestData.CreateSpan(
                pointer + 2,
                valueCount,
                IntegralType.UInt16);

            if (valueCount == 4)
            {
                for (int index = 0; index < valueCount; ++index)
                {
                    source.SetAtIndex(
                        index,
                        (byte)(index + 1));
                }
            }

            IntegralMemory.Move(
                source,
                destination,
                valueCount,
                conversion);

            for (int index = 0; index < valueCount; ++index)
            {
                Assert.Equal(
                    expected(index),
                    destination.AtIndex<ushort>(index));
            }
        }
        finally
        {
            IntegralTestData.AlignedFree(pointer);
        }
    }
}
