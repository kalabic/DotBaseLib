using DotBase.Buffers;
using DotBase.Integral;

namespace DotBaseLib.Tests;


public unsafe class ScalarCodecTests
{
    [Fact]
    public void EveryIntegralTypeRoundTripsBoundsAcrossEndianAndAlignmentMatrix()
    {
        foreach (ByteOrder byteOrder in IntegralTestData.ByteOrders)
        {
            foreach (int alignmentOffset in new[] { 0, 1 })
            {
                RunBoundsCase<byte>(
                    IntegralType.UInt8,
                    byte.MinValue,
                    byte.MaxValue,
                    byteOrder,
                    alignmentOffset);
                RunBoundsCase<sbyte>(
                    IntegralType.Int8,
                    sbyte.MinValue,
                    sbyte.MaxValue,
                    byteOrder,
                    alignmentOffset);
                RunBoundsCase<ushort>(
                    IntegralType.UInt16,
                    ushort.MinValue,
                    ushort.MaxValue,
                    byteOrder,
                    alignmentOffset);
                RunBoundsCase<short>(
                    IntegralType.Int16,
                    short.MinValue,
                    short.MaxValue,
                    byteOrder,
                    alignmentOffset);
                RunBoundsCase<uint>(
                    IntegralType.UInt32,
                    uint.MinValue,
                    uint.MaxValue,
                    byteOrder,
                    alignmentOffset);
                RunBoundsCase<int>(
                    IntegralType.Int32,
                    int.MinValue,
                    int.MaxValue,
                    byteOrder,
                    alignmentOffset);
                RunBoundsCase<ulong>(
                    IntegralType.UInt64,
                    ulong.MinValue,
                    ulong.MaxValue,
                    byteOrder,
                    alignmentOffset);
                RunBoundsCase<long>(
                    IntegralType.Int64,
                    long.MinValue,
                    long.MaxValue,
                    byteOrder,
                    alignmentOffset);
                RunBoundsCase<float>(
                    IntegralType.Float,
                    float.MinValue,
                    float.MaxValue,
                    byteOrder,
                    alignmentOffset);
                RunBoundsCase<double>(
                    IntegralType.Double,
                    double.MinValue,
                    double.MaxValue,
                    byteOrder,
                    alignmentOffset);
            }
        }
    }

    [Fact]
    public void FloatingPointSpecialRepresentationsArePreserved()
    {
        float[] singles =
        [
            BitConverter.Int32BitsToSingle(unchecked((int)0x80000000)),
            float.PositiveInfinity,
            float.NegativeInfinity,
            BitConverter.Int32BitsToSingle(unchecked((int)0x7FC00001)),
            BitConverter.Int32BitsToSingle(unchecked((int)0xFFC12345)),
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

        foreach (ByteOrder byteOrder in IntegralTestData.ByteOrders)
        {
            foreach (int alignmentOffset in new[] { 0, 1 })
            {
                RunSpecialCase(
                    IntegralType.Float,
                    singles,
                    byteOrder,
                    alignmentOffset);
                RunSpecialCase(
                    IntegralType.Double,
                    doubles,
                    byteOrder,
                    alignmentOffset);
            }
        }
    }

    [Fact]
    public void ScalarIndicesRejectValuesOutsideTheSpan()
    {
        int value = 0;
        IntegralSpan span = new(
            (byte*)&value,
            0,
            sizeof(int),
            IntegralType.Int32,
            1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => span.AtIndex<int>(-1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => span.AtIndex<int>(1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => span.SetAtIndex(1, 1));
    }

    [Fact]
    public void UndefinedByteOrderIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new IntegralFormat(
                IntegralType.Int32,
                1,
                (ByteOrder)int.MaxValue));
    }

    private static void RunBoundsCase<T>(
        IntegralType type,
        T minimum,
        T maximum,
        ByteOrder byteOrder,
        int alignmentOffset)
        where T : unmanaged
    {
        int size = sizeof(T);
        byte[] storage = new byte[
            checked(alignmentOffset + size * 2)];

        fixed (byte* storagePtr = storage)
        {
            byte* data = storagePtr + alignmentOffset;
            IntegralSpan span = IntegralTestData.CreateSpan(
                data,
                2,
                type,
                byteOrder);

            span.SetAtIndex(0, minimum);
            span.SetAtIndex(1, maximum);

            Assert.Equal(
                IntegralTestData.EncodedBytes(
                    minimum,
                    byteOrder),
                new ReadOnlySpan<byte>(data, size).ToArray());
            Assert.Equal(
                IntegralTestData.EncodedBytes(
                    maximum,
                    byteOrder),
                new ReadOnlySpan<byte>(
                    data + size,
                    size).ToArray());

            AssertBitwiseEqual(
                minimum,
                span.AtIndex<T>(0));
            AssertBitwiseEqual(
                maximum,
                span.AtIndex<T>(1));
        }
    }

    private static void RunSpecialCase<T>(
        IntegralType type,
        T[] values,
        ByteOrder byteOrder,
        int alignmentOffset)
        where T : unmanaged
    {
        int size = sizeof(T);
        byte[] storage = new byte[
            checked(alignmentOffset + size * values.Length)];

        fixed (byte* storagePtr = storage)
        {
            byte* data = storagePtr + alignmentOffset;
            IntegralSpan span = IntegralTestData.CreateSpan(
                data,
                values.Length,
                type,
                byteOrder);

            for (int index = 0; index < values.Length; ++index)
            {
                span.SetAtIndex(index, values[index]);
            }

            for (int index = 0; index < values.Length; ++index)
            {
                AssertBitwiseEqual(
                    values[index],
                    span.AtIndex<T>(index));
                Assert.Equal(
                    IntegralTestData.EncodedBytes(
                        values[index],
                        byteOrder),
                    new ReadOnlySpan<byte>(
                        data + index * size,
                        size).ToArray());
            }
        }
    }

    private static void AssertBitwiseEqual<T>(
        T expected,
        T actual)
        where T : unmanaged
    {
        Assert.Equal(
            IntegralTestData.NativeBytes(expected),
            IntegralTestData.NativeBytes(actual));
    }
}
