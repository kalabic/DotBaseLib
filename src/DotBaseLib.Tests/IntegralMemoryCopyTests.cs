using DotBase.Buffers;
using DotBase.Integral;

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
    public void SameRepresentationIdentityCopyPreservesExactBytes()
    {
        foreach (IntegralType type in IntegralTestData.Types)
        {
            int size = IntegralTestData.SizeOf(type);
            foreach (ByteOrder byteOrder in IntegralTestData.ByteOrders)
            {
                foreach (int alignmentOffset in new[] { 0, 1 })
                {
                    const int ValueCount = 3;
                    byte[] sourceStorage = new byte[
                        alignmentOffset + size * ValueCount];
                    byte[] destinationStorage = new byte[
                        alignmentOffset + size * ValueCount];

                    for (int index = alignmentOffset;
                         index < sourceStorage.Length;
                         ++index)
                    {
                        sourceStorage[index] = unchecked(
                            (byte)(index * 73 + (int)type * 19));
                    }

                    fixed (byte* sourcePtr = sourceStorage)
                    fixed (byte* destinationPtr = destinationStorage)
                    {
                        IntegralSpan source =
                            IntegralTestData.CreateSpan(
                                sourcePtr + alignmentOffset,
                                ValueCount,
                                type,
                                byteOrder);
                        IntegralSpan destination =
                            IntegralTestData.CreateSpan(
                                destinationPtr + alignmentOffset,
                                ValueCount,
                                type,
                                byteOrder);

                        IntegralMemory.Copy(
                            source,
                            destination);
                    }

                    Assert.Equal(
                        sourceStorage.AsSpan(
                            alignmentOffset).ToArray(),
                        destinationStorage.AsSpan(
                            alignmentOffset).ToArray());
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
                        foreach (int alignmentOffset in
                                 new[] { 0, 1 })
                        {
                            byte[] sourceStorage = new byte[
                                alignmentOffset + sourceSize];
                            byte[] destinationStorage = new byte[
                                alignmentOffset + destinationSize];

                            fixed (byte* sourcePtr = sourceStorage)
                            fixed (byte* destinationPtr =
                                   destinationStorage)
                            {
                                IntegralSpan source =
                                    IntegralTestData.CreateSpan(
                                        sourcePtr + alignmentOffset,
                                        1,
                                        sourceType,
                                        sourceByteOrder);
                                IntegralSpan destination =
                                    IntegralTestData.CreateSpan(
                                        destinationPtr + alignmentOffset,
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
                        }
                    }
                }
            }
        }
    }
}
