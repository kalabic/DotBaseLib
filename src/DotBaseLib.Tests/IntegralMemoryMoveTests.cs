using DotBase.Buffers;
using DotBase.Integral;

namespace DotBaseLib.Tests;


public unsafe class IntegralMemoryMoveTests
{
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
        byte[] storage =
        [
            0xFF, 0xC1, 0x23, 0x45,
            0x80, 0x00, 0x00, 0x00,
        ];
        byte[] expected = storage.ToArray();

        fixed (byte* pointer = storage)
        {
            IntegralSpan span = IntegralTestData.CreateSpan(
                pointer,
                2,
                IntegralType.Float,
                ByteOrder.BigEndian);
            IntegralMemory.Move(
                span,
                span);
        }

        Assert.Equal(expected, storage);
    }

    [Fact]
    public void ExactSamePointerCanReverseEndianInPlace()
    {
        int[] values =
        [
            unchecked((int)0x01020304),
            unchecked((int)0x89ABCDEF),
        ];
        byte[] storage = new byte[values.Length * sizeof(int)];

        fixed (byte* pointer = storage)
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
                    storage.AsSpan(
                        index * sizeof(int),
                        sizeof(int)).ToArray());
            }
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
        byte[] storage = new byte[
            checked(valueCount * sizeof(ushort) + 1)];

        for (int index = 0; index < valueCount; ++index)
        {
            storage[index] = unchecked(
                (byte)(index % 251));
        }

        fixed (byte* pointer = storage)
        {
            IntegralSpan source = IntegralTestData.CreateSpan(
                pointer,
                valueCount,
                IntegralType.UInt8);
            IntegralSpan destination = IntegralTestData.CreateSpan(
                pointer + 1,
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
    }
}
