using DotBase.Buffers;
using DotBase.Integral;

namespace DotBaseLib.Tests;


public class IntegralFormatTests
{
    [Fact]
    public void SizeMatchesScalarWidths()
    {
        Assert.Equal(0, IntegralType.NONE.Size());
        Assert.Equal(1, IntegralType.UInt8.Size());
        Assert.Equal(1, IntegralType.Int8.Size());
        Assert.Equal(2, IntegralType.Int16.Size());
        Assert.Equal(4, IntegralType.Int32.Size());
        Assert.Equal(4, IntegralType.Float.Size());
        Assert.Equal(8, IntegralType.Int64.Size());
        Assert.Equal(8, IntegralType.Double.Size());
    }

    [Fact]
    public void DefaultForTypeMapsClrScalars()
    {
        Assert.Equal(IntegralType.Int64, IntegralType.NONE.DefaultForType<long>());
        Assert.Equal(IntegralType.UInt16, IntegralType.NONE.DefaultForType<ushort>());
        Assert.Equal(IntegralType.Float, IntegralType.NONE.DefaultForType<float>());
        Assert.Equal(IntegralType.NONE, IntegralType.NONE.DefaultForType<decimal>());
    }

    [Fact]
    public void ForBuildsFormatFromClrType()
    {
        IntegralFormat stereoBe = IntegralFormat.For<short>(
            blockCapacity: 2,
            ByteOrder.BigEndian,
            byteRate: 48000 * 4);

        Assert.Equal(IntegralType.Int16, stereoBe.ValueType);
        Assert.Equal(2, stereoBe.BlockCapacity);
        Assert.Equal(ByteOrder.BigEndian, stereoBe.ByteOrder);
        Assert.Equal(4, stereoBe.BytesPerBlock);
        Assert.Equal(48000 * 4, stereoBe.ByteRate);
        Assert.True(stereoBe.IsCompatible<short>());
        Assert.False(stereoBe.IsCompatible<int>());
    }

    [Fact]
    public void ForRejectsUnsupportedType()
    {
        Assert.Throws<ArgumentException>(
            () => IntegralFormat.For<decimal>());
    }

    [Fact]
    public unsafe void SpanIsEqualComparesResolvedEndian()
    {
        byte* p = IntegralTestData.AlignedAlloc(sizeof(int));
        try
        {
            IntegralSpan native = IntegralTestData.CreateSpan(
                p, 1, IntegralType.Int32, ByteOrder.Native);
            IntegralSpan little = IntegralTestData.CreateSpan(
                p, 1, IntegralType.Int32, ByteOrder.LittleEndian);
            IntegralSpan big = IntegralTestData.CreateSpan(
                p, 1, IntegralType.Int32, ByteOrder.BigEndian);

            ByteOrder host = BitConverter.IsLittleEndian
                ? ByteOrder.LittleEndian
                : ByteOrder.BigEndian;

            Assert.True(native.IsEqual(host));
            Assert.True(native.IsEqual(ByteOrder.Native));
            Assert.True(native.IsEqual(host == ByteOrder.LittleEndian ? little : big));
            Assert.False(native.IsEqual(host == ByteOrder.LittleEndian ? big : little));
            Assert.False(little.IsEqual(big));
        }
        finally
        {
            IntegralTestData.AlignedFree(p);
        }
    }
}
