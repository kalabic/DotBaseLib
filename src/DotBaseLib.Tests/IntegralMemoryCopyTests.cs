using DotBase.Buffers;
using DotBase.Integral;
using System;

namespace DotBaseLib.Tests;


public unsafe class IntegralMemoryCopyTests
{
    [Fact]
    public void IdentityConvertCoversCompleteTypeEndianCrossProduct()
    {
        RunConvertCrossProduct(
            IntegralConversion.Identity,
            [0, 1, 7, 31, 63]);
    }

    [Fact]
    public void AffineConvertCoversCompleteTypeEndianCrossProduct()
    {
        RunConvertCrossProduct(
            new IntegralConversion(
                2,
                1),
            [0, 1, 7, 31, 63]);
    }

    [Fact]
    public void SameTypeOppositeEndianReverseCopyReversesLanes()
    {
        const int ValueCount = 5;
        const int BlockCapacity = 2;
        const byte DestinationSentinel = 0xA5;

        foreach (IntegralType type in IntegralTestData.Types)
        {
            int valueSize = IntegralTestData.SizeOf(type);
            int byteCount = ValueCount * valueSize;
            byte[] leStorage = new byte[byteCount];
            byte[] beStorage = Enumerable.Repeat(
                DestinationSentinel,
                byteCount).ToArray();

            for (int valueIndex = 0; valueIndex < ValueCount; ++valueIndex)
            {
                for (int byteIndex = 0; byteIndex < valueSize; ++byteIndex)
                {
                    leStorage[valueIndex * valueSize + byteIndex] =
                        unchecked((byte)(
                            17 +
                            valueIndex * 37 +
                            byteIndex * 11 +
                            (int)type * 5));
                }
            }

            byte[] originalLeStorage = leStorage.ToArray();
            fixed (byte* lePtr = leStorage)
            fixed (byte* bePtr = beStorage)
            {
                IntegralSpan le = IntegralTestData.CreateSpan(
                    lePtr,
                    ValueCount,
                    type,
                    ByteOrder.LittleEndian,
                    BlockCapacity);
                IntegralSpan be = IntegralTestData.CreateSpan(
                    bePtr,
                    ValueCount,
                    type,
                    ByteOrder.BigEndian,
                    BlockCapacity);

                IntegralMemory.ReverseCopyChecked(le, be);

                int copiedValueCount =
                    ValueCount - ValueCount % BlockCapacity;
                for (int valueIndex = 0;
                     valueIndex < copiedValueCount;
                     ++valueIndex)
                {
                    for (int byteIndex = 0;
                         byteIndex < valueSize;
                         ++byteIndex)
                    {
                        Assert.Equal(
                            leStorage[
                                valueIndex * valueSize +
                                valueSize - byteIndex - 1],
                            beStorage[valueIndex * valueSize + byteIndex]);
                    }
                }

                for (int byteIndex = copiedValueCount * valueSize;
                     byteIndex < byteCount;
                     ++byteIndex)
                {
                    Assert.Equal(DestinationSentinel, beStorage[byteIndex]);
                }

                Assert.Equal(originalLeStorage, leStorage);
            }
        }
    }

    [Fact]
    public void CopyRejectsOppositeEndianAndTypeMismatch()
    {
        int leValue = 1;
        int beValue = 0;
        long wide = 0;
        int differentBlockValue = 0;
        IntegralSpan le = IntegralTestData.CreateSpan(
            (byte*)&leValue,
            1,
            IntegralType.Int32,
            ByteOrder.LittleEndian);
        IntegralSpan be = IntegralTestData.CreateSpan(
            (byte*)&beValue,
            1,
            IntegralType.Int32,
            ByteOrder.BigEndian);
        IntegralSpan asLong = IntegralTestData.CreateSpan(
            (byte*)&wide,
            1,
            IntegralType.Int64,
            ByteOrder.LittleEndian);
        IntegralSpan differentBlock = IntegralTestData.CreateSpan(
            (byte*)&differentBlockValue,
            1,
            IntegralType.Int32,
            ByteOrder.LittleEndian,
            blockCapacity: 2);

        Assert.Throws<ArgumentException>(() => IntegralMemory.CopyChecked(le, be));
        Assert.Throws<ArgumentException>(() => IntegralMemory.CopyChecked(le, asLong));
        Assert.Throws<ArgumentException>(() => IntegralMemory.CopyChecked(le, differentBlock));
        Assert.Throws<ArgumentException>(() => IntegralMemory.ReverseCopyChecked(le, le));
        Assert.Equal(0, beValue);
        Assert.Equal(0, wide);
        Assert.Equal(0, differentBlockValue);
    }

    [Fact]
    public void SameRepresentationIdentityCopyPreservesExactBytes()
    {
        const int GuardByteCount = 8;
        const int ValueCount = 5;
        const byte DestinationSentinel = 0xA5;

        foreach (IntegralType type in IntegralTestData.Types)
        {
            int size = IntegralTestData.SizeOf(type);
            foreach (ByteOrder byteOrder in IntegralTestData.ByteOrders)
            {
                int byteCount = size * ValueCount;
                int allocationByteCount =
                    GuardByteCount + byteCount + GuardByteCount;
                byte* sourcePtr = IntegralTestData.AlignedAlloc(allocationByteCount);
                byte* destinationPtr = IntegralTestData.AlignedAlloc(allocationByteCount);
                try
                {
                    new Span<byte>(sourcePtr, allocationByteCount).Fill(0x3C);
                    new Span<byte>(destinationPtr, allocationByteCount).Fill(
                        DestinationSentinel);
                    for (int index = 0; index < byteCount; ++index)
                    {
                        sourcePtr[GuardByteCount + index] = unchecked(
                            (byte)(index * 73 + (int)type * 19));
                    }
                    byte[] originalSource = new ReadOnlySpan<byte>(
                        sourcePtr,
                        allocationByteCount).ToArray();

                    IntegralSpan source =
                        IntegralTestData.CreateSpan(
                            sourcePtr,
                            ValueCount,
                            type,
                            byteOrder,
                            byteOffset: GuardByteCount);
                    IntegralSpan destination =
                        IntegralTestData.CreateSpan(
                            destinationPtr,
                            ValueCount,
                            type,
                            byteOrder,
                            byteOffset: GuardByteCount);

                    IntegralMemory.CopyChecked(
                        source,
                        destination);

                    Assert.Equal(
                        new ReadOnlySpan<byte>(
                            sourcePtr + GuardByteCount,
                            byteCount).ToArray(),
                        new ReadOnlySpan<byte>(
                            destinationPtr + GuardByteCount,
                            byteCount).ToArray());
                    Assert.Equal(
                        originalSource,
                        new ReadOnlySpan<byte>(
                            sourcePtr,
                            allocationByteCount).ToArray());
                    for (int index = 0; index < GuardByteCount; ++index)
                    {
                        Assert.Equal(DestinationSentinel, destinationPtr[index]);
                        Assert.Equal(
                            DestinationSentinel,
                            destinationPtr[
                                GuardByteCount + byteCount + index]);
                    }
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
    public void EmptyAndExplicitZeroCountCopiesDoNotMutate()
    {
        IntegralMemory.Copy(
            IntegralSpan.Empty,
            IntegralSpan.Empty);
        IntegralMemory.CopyChecked(
            IntegralSpan.Empty,
            IntegralSpan.Empty);

        byte sourceValue = 0x3C;
        byte destinationValue = 0x5A;
        IntegralSpan source = IntegralTestData.CreateSpan(
            &sourceValue,
            1,
            IntegralType.UInt8);
        IntegralSpan destination = IntegralTestData.CreateSpan(
            &destinationValue,
            1,
            IntegralType.UInt8);

        IntegralMemory.Copy(
            source,
            destination,
            0);
        Assert.Equal(0x3C, sourceValue);
        Assert.Equal(0x5A, destinationValue);
    }

    [Theory]
    [InlineData(5, 2, 5, 2, 4)]
    [InlineData(6, 2, 8, 4, 4)]
    [InlineData(12, 2, 16, 8, 8)]
    [InlineData(8, 4, 6, 2, 4)]
    [InlineData(16, 8, 12, 2, 8)]
    public void DefaultCopyStopsAtBoundarySharedByBothAndExcludesTrailingValues(
        int sourceValueCount,
        int sourceBlockCapacity,
        int destinationValueCount,
        int destinationBlockCapacity,
        int expectedValueCount)
    {
        const int DestinationSentinel = -1;
        int[] sourceValues = Enumerable.Range(
            10,
            sourceValueCount).ToArray();
        int[] destinationValues = Enumerable.Repeat(
            DestinationSentinel,
            destinationValueCount).ToArray();

        fixed (int* sourcePtr = sourceValues)
        fixed (int* destinationPtr = destinationValues)
        {
            IntegralSpan source = IntegralTestData.CreateSpan(
                (byte*)sourcePtr,
                sourceValues.Length,
                IntegralType.Int32,
                ByteOrder.Native,
                sourceBlockCapacity);
            IntegralSpan destination = IntegralTestData.CreateSpan(
                (byte*)destinationPtr,
                destinationValues.Length,
                IntegralType.Int32,
                ByteOrder.Native,
                destinationBlockCapacity);

            Assert.Equal(
                expectedValueCount,
                IntegralMemory.CountBlockCompleteValues(source, destination));

            IntegralMemory.Copy(source, destination);

            for (int index = 0; index < expectedValueCount; ++index)
            {
                Assert.Equal(sourceValues[index], destinationValues[index]);
            }
            for (int index = expectedValueCount;
                 index < destinationValues.Length;
                 ++index)
            {
                Assert.Equal(DestinationSentinel, destinationValues[index]);
            }
        }
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
                () => IntegralMemory.CopyChecked(
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
                () => IntegralMemory.CopyChecked(
                    source,
                    destination));

            IntegralSpan reverseSource = IntegralTestData.CreateSpan(
                pointer + sizeof(int),
                2,
                IntegralType.Int32);
            IntegralSpan reverseDestination = IntegralTestData.CreateSpan(
                pointer,
                2,
                IntegralType.Int32);
            Assert.Throws<ArgumentException>(
                () => IntegralMemory.CopyChecked(
                    reverseSource,
                    reverseDestination));
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
            IntegralMemory.CopyChecked(
                source,
                destination);

            Assert.Equal(17, destination.AtIndex<int>(0));
            Assert.Equal(29, destination.AtIndex<int>(1));

            destination.SetAtIndex(0, 31);
            destination.SetAtIndex(1, 43);
            IntegralMemory.CopyChecked(
                destination,
                source);

            Assert.Equal(31, source.AtIndex<int>(0));
            Assert.Equal(43, source.AtIndex<int>(1));
        }
    }

    [Fact]
    public void ExplicitCountBoundsTypeMismatchAndIncompleteLengthsAreRejected()
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

        Assert.Throws<ArgumentException>(
            () => IntegralMemory.CopyChecked(
                source,
                destination,
                1));
        Assert.Equal(0, destinationValue);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => IntegralMemory.CopyChecked(
                source,
                source,
                2));

        nint address = 1;
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new IntegralSpan(
                (byte*)address,
                0,
                sizeof(int) + 1,
                IntegralType.Int32,
                1).Validate());
    }

    private static void RunConvertCrossProduct(
        in IntegralConversion conversion,
        double[] sourceValues)
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
                        int valueCount = sourceValues.Length;
                        byte* sourcePtr = IntegralTestData.AlignedAlloc(
                            checked(sourceSize * valueCount));
                        byte* destinationPtr = IntegralTestData.AlignedAlloc(
                            checked(destinationSize * valueCount));
                        try
                        {
                            IntegralSpan source =
                                IntegralTestData.CreateSpan(
                                    sourcePtr,
                                    valueCount,
                                    sourceType,
                                    sourceByteOrder);
                            IntegralSpan destination =
                                IntegralTestData.CreateSpan(
                                    destinationPtr,
                                    valueCount,
                                    destinationType,
                                    destinationByteOrder);

                            for (int index = 0;
                                 index < valueCount;
                                 ++index)
                            {
                                IntegralTestData.SetNumber(
                                    source,
                                    index,
                                    sourceValues[index]);
                            }
                            IntegralMemory.ConvertChecked(
                                source,
                                destination,
                                valueCount,
                                conversion);

                            for (int index = 0;
                                 index < valueCount;
                                 ++index)
                            {
                                double expectedValue =
                                    sourceValues[index] * conversion.Scale +
                                    conversion.Bias;
                                Assert.Equal(
                                    expectedValue,
                                    IntegralTestData.GetNumber(
                                        destination,
                                        index));
                            }
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
