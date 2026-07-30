using DotBase.Buffers;
using DotBase.Integral;
using System;

namespace DotBaseLib.Tests;


public unsafe class IntegralMemoryCopyTests
{
    [Fact]
    public void IdentityCopyCoversCompleteTypeEndianAndAlignmentCrossProduct()
    {
        RunCrossProduct(
            IntegralConversion.Identity,
            1);
    }

    [Fact]
    public void AffineCopyCoversCompleteTypeEndianAndAlignmentCrossProduct()
    {
        RunCrossProduct(
            new IntegralConversion(
                2,
                1),
            3);
    }

    [Fact]
    public void SameTypeOppositeEndianIdentityCopyReversesLanes()
    {
        // Exercises the memcpy + lane-reverse fast path (no numeric convert).
        int[] hostValues = [0x01020304, unchecked((int)0xAABBCCDD), 0x11223344];
        byte[] leStorage = new byte[hostValues.Length * sizeof(int)];
        byte[] beStorage = new byte[hostValues.Length * sizeof(int)];

        fixed (byte* lePtr = leStorage)
        fixed (byte* bePtr = beStorage)
        {
            IntegralSpan le = IntegralTestData.CreateSpan(
                lePtr,
                hostValues.Length,
                IntegralType.Int32,
                ByteOrder.LittleEndian);
            IntegralSpan be = IntegralTestData.CreateSpan(
                bePtr,
                hostValues.Length,
                IntegralType.Int32,
                ByteOrder.BigEndian);

            for (int i = 0; i < hostValues.Length; ++i)
            {
                le.SetAtIndex(i, hostValues[i]);
            }

            IntegralMemory.Copy(le, be);

            for (int i = 0; i < hostValues.Length; ++i)
            {
                Assert.Equal(hostValues[i], be.AtIndex<int>(i));
            }

            // Raw lane bytes must be opposite endian.
            for (int i = 0; i < hostValues.Length; ++i)
            {
                int offset = i * sizeof(int);
                Assert.Equal(leStorage[offset], beStorage[offset + 3]);
                Assert.Equal(leStorage[offset + 1], beStorage[offset + 2]);
                Assert.Equal(leStorage[offset + 2], beStorage[offset + 1]);
                Assert.Equal(leStorage[offset + 3], beStorage[offset]);
            }
        }
    }

    [Fact]
    public void SameRepresentationIdentityCopyPreservesExactBytes()
    {
        foreach (IntegralType type in IntegralTestData.Types)
        {
            int size = IntegralTestData.SizeOf(type);
            foreach (ByteOrder byteOrder in IntegralTestData.ByteOrders)
            {
                const int ValueCount = 3;
                int byteCount = size * ValueCount;
                byte* sourcePtr = IntegralTestData.AlignedAlloc(byteCount);
                byte* destinationPtr = IntegralTestData.AlignedAlloc(byteCount);
                try
                {
                    for (int index = 0; index < byteCount; ++index)
                    {
                        sourcePtr[index] = unchecked(
                            (byte)(index * 73 + (int)type * 19));
                    }

                    IntegralSpan source =
                        IntegralTestData.CreateSpan(
                            sourcePtr,
                            ValueCount,
                            type,
                            byteOrder);
                    IntegralSpan destination =
                        IntegralTestData.CreateSpan(
                            destinationPtr,
                            ValueCount,
                            type,
                            byteOrder);

                    IntegralMemory.Copy(
                        source,
                        destination);

                    Assert.Equal(
                        new ReadOnlySpan<byte>(sourcePtr, byteCount).ToArray(),
                        new ReadOnlySpan<byte>(destinationPtr, byteCount).ToArray());
                }
                finally
                {
                    IntegralTestData.AlignedFree(sourcePtr);
                    IntegralTestData.AlignedFree(destinationPtr);
                }
            }
        }
    }

    [Fact]
    public void EmptyCopyIsANoOp()
    {
        IntegralMemory.Copy(
            IntegralSpan.Empty,
            IntegralSpan.Empty);

        byte value = 0x5A;
        IntegralSpan source = IntegralTestData.CreateSpan(
            &value,
            0,
            IntegralType.UInt8);
        IntegralSpan destination = IntegralTestData.CreateSpan(
            &value,
            0,
            IntegralType.UInt8);

        IntegralMemory.Copy(
            source,
            destination,
            0);
        Assert.Equal(0x5A, value);
    }

    [Fact]
    public void SamePointerAndOverlappingRangesAreRejected()
    {
        byte[] storage = new byte[16];
        fixed (byte* pointer = storage)
        {
            IntegralSpan same = IntegralTestData.CreateSpan(
                pointer,
                2,
                IntegralType.Int32);
            Assert.Throws<ArgumentException>(
                () => IntegralMemory.Copy(
                    same,
                    same));

            IntegralSpan source = IntegralTestData.CreateSpan(
                pointer,
                2,
                IntegralType.Int32);
            IntegralSpan destination = IntegralTestData.CreateSpan(
                pointer + sizeof(int),
                2,
                IntegralType.Int32);
            Assert.Throws<ArgumentException>(
                () => IntegralMemory.Copy(
                    source,
                    destination));
        }
    }

    [Fact]
    public void AdjacentRangesAreAccepted()
    {
        byte[] storage = new byte[4 * sizeof(int)];
        fixed (byte* pointer = storage)
        {
            IntegralSpan source = IntegralTestData.CreateSpan(
                pointer,
                2,
                IntegralType.Int32);
            IntegralSpan destination = IntegralTestData.CreateSpan(
                pointer + 2 * sizeof(int),
                2,
                IntegralType.Int32);

            source.SetAtIndex(0, 17);
            source.SetAtIndex(1, 29);
            IntegralMemory.Copy(
                source,
                destination);

            Assert.Equal(17, destination.AtIndex<int>(0));
            Assert.Equal(29, destination.AtIndex<int>(1));
        }
    }

    [Fact]
    public void SizeMismatchesAndIncompleteScalarLengthsAreRejected()
    {
        int sourceValue = 1;
        long destinationValue = 0;
        IntegralSpan source = IntegralTestData.CreateSpan(
            (byte*)&sourceValue,
            1,
            IntegralType.Int32);
        IntegralSpan destination = IntegralTestData.CreateSpan(
            (byte*)&destinationValue,
            1,
            IntegralType.Int64);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => IntegralMemory.Copy(
                source,
                destination,
                2));

        nint address = 1;
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new IntegralSpan(
                (byte*)address,
                0,
                sizeof(int) + 1,
                IntegralType.Int32,
                1));
    }

    private static void RunCrossProduct(
        in IntegralConversion conversion,
        double expectedValue)
    {
        foreach (IntegralType sourceType in IntegralTestData.Types)
        {
            int sourceSize = IntegralTestData.SizeOf(
                sourceType);
            foreach (IntegralType destinationType in
                     IntegralTestData.Types)
            {
                int destinationSize = IntegralTestData.SizeOf(
                    destinationType);
                foreach (ByteOrder sourceByteOrder in
                         IntegralTestData.ByteOrders)
                {
                    foreach (ByteOrder destinationByteOrder in
                             IntegralTestData.ByteOrders)
                    {
                        byte* sourcePtr =
                            IntegralTestData.AlignedAlloc(sourceSize);
                        byte* destinationPtr =
                            IntegralTestData.AlignedAlloc(destinationSize);
                        try
                        {
                            IntegralSpan source =
                                IntegralTestData.CreateSpan(
                                    sourcePtr,
                                    1,
                                    sourceType,
                                    sourceByteOrder);
                            IntegralSpan destination =
                                IntegralTestData.CreateSpan(
                                    destinationPtr,
                                    1,
                                    destinationType,
                                    destinationByteOrder);

                            IntegralTestData.SetNumber(
                                source,
                                0,
                                1);
                            IntegralMemory.Copy(
                                source,
                                destination,
                                1,
                                conversion);

                            Assert.Equal(
                                expectedValue,
                                IntegralTestData.GetNumber(
                                    destination,
                                    0));
                        }
                        finally
                        {
                            IntegralTestData.AlignedFree(sourcePtr);
                            IntegralTestData.AlignedFree(destinationPtr);
                        }
                    }
                }
            }
        }
    }
}
