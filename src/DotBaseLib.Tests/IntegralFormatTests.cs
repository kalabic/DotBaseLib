using DotBase.Buffers;
using DotBase.Integral;
using DotBase.Integral.Conversion;
using DotBase.Integral.Conversion.Internal;

namespace DotBaseLib.Tests;


public class IntegralFormatTests
{
    [Fact]
    public void SizeMatchesScalarWidths()
    {
        Assert.Equal(0, IntegralType.None.Size());
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
        Assert.Equal(IntegralType.Int64, IntegralType.None.DefaultForType<long>());
        Assert.Equal(IntegralType.UInt16, IntegralType.None.DefaultForType<ushort>());
        Assert.Equal(IntegralType.Float, IntegralType.None.DefaultForType<float>());
        Assert.Equal(IntegralType.None, IntegralType.None.DefaultForType<decimal>());
    }

    [Fact]
    public void ForBuildsFormatFromClrType()
    {
        IntegralConversionPolicy policy =
            IntegralConversionPolicy.FromValueConverters(NumericValueConverters.Default);
        IntegralFormat stereoBe = IntegralFormat.For<short>(
            blockCapacity: 2,
            ByteOrder.BigEndian,
            policy);

        Assert.Equal(IntegralType.Int16, stereoBe.ValueType);
        Assert.Equal(2, stereoBe.BlockCapacity);
        Assert.Equal(ByteOrder.BigEndian, stereoBe.ByteOrder);
        Assert.Equal(policy, stereoBe.ConversionPolicy);
        Assert.False(ConversionPolicySlot.IsDefault(stereoBe.ConversionPolicy.SpanHandleFactorySlot));
        Assert.Equal(4, stereoBe.BytesPerBlock);
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
    public void IntegralFormatAndPolicyAreUnmanaged()
    {
        MustBeUnmanaged<IntegralFormat>();
        MustBeUnmanaged<IntegralConversionPolicy>();
    }

    [Fact]
    public void FromValueConverters_SharesFactorySlots()
    {
        NumericValueConverters table = NumericValueConverters.Default;
        IntegralConversionPolicy a = IntegralConversionPolicy.FromValueConverters(table);
        IntegralConversionPolicy b = IntegralConversionPolicy.FromValueConverters(table);
        Assert.Equal(a, b);
        Assert.True(ConversionPolicySlot.IsFactory(a.SpanHandleFactorySlot));
        Assert.True(ConversionPolicySlot.IsFactory(a.ReaderHandleFactorySlot));
        Assert.True(ConversionPolicySlot.IsFactory(a.WriterHandleFactorySlot));
    }

    [Fact]
    public void RefuseAll_ProducesNullHandles()
    {
        IntegralFormat input = IntegralFormat.For<byte>();
        IntegralFormat output = new(
            IntegralType.UInt8,
            1,
            ByteOrder.Native,
            IntegralConversionPolicy.RefuseAll());

        Assert.True(ConversionHandles.GetHandle(input, output).IsNull);
        Assert.True(ConversionHandles.GetInterleavedReaderHandle(input, output).IsNull);
        Assert.True(ConversionHandles.GetInterleavedWriterHandle(input, output).IsNull);
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

    private static void MustBeUnmanaged<T>()
        where T : unmanaged
    {
    }
}
