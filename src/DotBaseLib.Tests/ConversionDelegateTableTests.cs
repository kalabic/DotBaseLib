using DotBase.Buffers;
using DotBase.Integral;
using DotBase.Integral.Conversion;
using DotBase.Integral.Conversion.Numeric;
using DotBase.Integral.Conversion.Numeric.Defaults;

namespace DotBaseLib.Tests;


public unsafe class ConversionDelegateTableTests
{
    [Fact]
    public void UInt8ToUInt8_L2L_CopiesBytesAndClampsCount()
    {
        const int capacity = 8;
        byte* srcMem = IntegralTestData.AlignedAlloc(capacity);
        byte* dstMem = IntegralTestData.AlignedAlloc(capacity);
        try
        {
            for (int i = 0; i < capacity; i++)
            {
                srcMem[i] = (byte)(10 + i);
                dstMem[i] = 0xFF;
            }

            IntegralSpan src = IntegralTestData.CreateSpan(
                srcMem, capacity, IntegralType.UInt8, ByteOrder.LittleEndian);
            IntegralSpan dst = IntegralTestData.CreateSpan(
                dstMem, capacity, IntegralType.UInt8, ByteOrder.LittleEndian);

            IntegralConversionHandle handle =
                ConversionDelegateTable.Instance.GetConversionHandle(src, dst);

            Assert.False(handle.IsNull);

            long converted = handle.Convert(src, dst, count: 100);
            Assert.Equal(capacity, converted);

            for (int i = 0; i < capacity; i++)
            {
                Assert.Equal((byte)(10 + i), dstMem[i]);
            }
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Fact]
    public void UInt8ToFloat_L2L_ConvertsValuesToLittleEndianWire()
    {
        const int count = 4;
        const ByteOrder wire = ByteOrder.LittleEndian;
        byte* srcMem = IntegralTestData.AlignedAlloc(count);
        byte* dstMem = IntegralTestData.AlignedAlloc(count * sizeof(float));
        try
        {
            srcMem[0] = 0;
            srcMem[1] = 1;
            srcMem[2] = 128;
            srcMem[3] = 255;

            IntegralSpan src = IntegralTestData.CreateSpan(
                srcMem, count, IntegralType.UInt8, wire);
            IntegralSpan dst = IntegralTestData.CreateSpan(
                dstMem, count, IntegralType.Float, wire);

            IntegralConversionHandle handle =
                ConversionDelegateTable.Instance.GetConversionHandle(src, dst);

            long converted = handle.Convert(src, dst, count);
            Assert.Equal(count, converted);

            IntegralTestData.AssertEncodedEqual(0f, dstMem, 0, wire);
            IntegralTestData.AssertEncodedEqual(1f, dstMem, 1, wire);
            IntegralTestData.AssertEncodedEqual(128f, dstMem, 2, wire);
            IntegralTestData.AssertEncodedEqual(255f, dstMem, 3, wire);
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Fact]
    public void Int16ToInt16_OppositeEndian_ReversesLanes()
    {
        const int count = 3;
        const ByteOrder sourceWire = ByteOrder.LittleEndian;
        const ByteOrder destinationWire = ByteOrder.BigEndian;
        byte* srcMem = IntegralTestData.AlignedAlloc(count * sizeof(short));
        byte* dstMem = IntegralTestData.AlignedAlloc(count * sizeof(short));
        try
        {
            // Independent LE wire construction (not host short stores).
            IntegralTestData.WriteEncoded(
                srcMem, 0, unchecked((short)0x1234), sourceWire);
            IntegralTestData.WriteEncoded(
                srcMem, 1, unchecked((short)0xABCD), sourceWire);
            IntegralTestData.WriteEncoded(
                srcMem, 2, unchecked((short)0x00FF), sourceWire);

            IntegralSpan input = IntegralTestData.CreateSpan(
                srcMem, count, IntegralType.Int16, sourceWire);
            IntegralSpan output = IntegralTestData.CreateSpan(
                dstMem, count, IntegralType.Int16, destinationWire);

            IntegralConversionHandle handle =
                ConversionDelegateTable.Instance.GetConversionHandle(input, output);

            long converted = handle.Convert(input, output, count);
            Assert.Equal(count, converted);

            // LE 0x1234 bytes 34 12 -> BE wire same numeric is 12 34
            IntegralTestData.AssertEncodedEqual(
                unchecked((short)0x1234), dstMem, 0, destinationWire);
            IntegralTestData.AssertEncodedEqual(
                unchecked((short)0xABCD), dstMem, 1, destinationWire);
            IntegralTestData.AssertEncodedEqual(
                unchecked((short)0x00FF), dstMem, 2, destinationWire);

            Assert.Equal(0x12, dstMem[0]);
            Assert.Equal(0x34, dstMem[1]);
            Assert.Equal(0xAB, dstMem[2]);
            Assert.Equal(0xCD, dstMem[3]);
            Assert.Equal(0x00, dstMem[4]);
            Assert.Equal(0xFF, dstMem[5]);
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Fact]
    public void NativeByteOrder_ResolvesToSameSlotAsHostEndian()
    {
        const int count = 2;
        byte* srcMem = IntegralTestData.AlignedAlloc(count);
        byte* dstMem = IntegralTestData.AlignedAlloc(count * sizeof(float));
        try
        {
            srcMem[0] = 7;
            srcMem[1] = 9;

            ByteOrder host = IntegralTestData.ResolveByteOrder(ByteOrder.Native);

            IntegralSpan srcNative = IntegralTestData.CreateSpan(
                srcMem, count, IntegralType.UInt8, ByteOrder.Native);
            IntegralSpan dstNative = IntegralTestData.CreateSpan(
                dstMem, count, IntegralType.Float, ByteOrder.Native);

            IntegralSpan srcExplicit = IntegralTestData.CreateSpan(
                srcMem, count, IntegralType.UInt8, host);
            IntegralSpan dstExplicit = IntegralTestData.CreateSpan(
                dstMem, count, IntegralType.Float, host);

            var handleNative =
                ConversionDelegateTable.Instance.GetConversionHandle(srcNative, dstNative);
            var handleExplicit =
                ConversionDelegateTable.Instance.GetConversionHandle(srcExplicit, dstExplicit);

            Assert.False(handleNative.IsNull);
            Assert.False(handleExplicit.IsNull);

            // Both should convert the same way; inspect host-wire float bytes.
            long n1 = handleNative.Convert(srcNative, dstNative, count);
            float a0 = IntegralTestData.ReadEncoded<float>(dstMem, 0, host);
            float a1 = IntegralTestData.ReadEncoded<float>(dstMem, 1, host);

            new Span<byte>(dstMem, count * sizeof(float)).Clear();

            long n2 = handleExplicit.Convert(srcExplicit, dstExplicit, count);

            Assert.Equal(n1, n2);
            IntegralTestData.AssertEncodedEqual(a0, dstMem, 0, host);
            IntegralTestData.AssertEncodedEqual(a1, dstMem, 1, host);
            IntegralTestData.AssertEncodedEqual(7f, dstMem, 0, host);
            IntegralTestData.AssertEncodedEqual(9f, dstMem, 1, host);
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Fact]
    public void FloatToUInt8_SaturatesLikeIntegralNumericConversion()
    {
        const int count = 4;
        const ByteOrder wire = ByteOrder.LittleEndian;
        byte* srcMem = IntegralTestData.AlignedAlloc(count * sizeof(float));
        byte* dstMem = IntegralTestData.AlignedAlloc(count);
        try
        {
            // Build LE float wire independently of host endianness.
            IntegralTestData.WriteEncoded(srcMem, 0, -10f, wire);
            IntegralTestData.WriteEncoded(srcMem, 1, 42.9f, wire);
            IntegralTestData.WriteEncoded(srcMem, 2, 255.1f, wire);
            IntegralTestData.WriteEncoded(srcMem, 3, 100f, wire);

            IntegralSpan input = IntegralTestData.CreateSpan(
                srcMem, count, IntegralType.Float, wire);
            IntegralSpan output = IntegralTestData.CreateSpan(
                dstMem, count, IntegralType.UInt8, wire);

            long converted = ConversionDelegateTable.Instance
                .GetConversionHandle(input, output)
                .Convert(input, output, count);

            Assert.Equal(count, converted);
            Assert.Equal(0, dstMem[0]);      // saturated
            Assert.Equal(42, dstMem[1]);     // truncated toward zero via saturate path
            Assert.Equal(255, dstMem[2]);    // saturated
            Assert.Equal(100, dstMem[3]);
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Fact]
    public void ZeroCount_ReturnsZeroWithoutTouchingDestination()
    {
        byte* srcMem = IntegralTestData.AlignedAlloc(4);
        byte* dstMem = IntegralTestData.AlignedAlloc(4);
        try
        {
            srcMem[0] = 1;
            dstMem[0] = 99;

            IntegralSpan src = IntegralTestData.CreateSpan(
                srcMem, 4, IntegralType.UInt8, ByteOrder.LittleEndian);
            IntegralSpan dst = IntegralTestData.CreateSpan(
                dstMem, 4, IntegralType.UInt8, ByteOrder.LittleEndian);

            long converted = ConversionDelegateTable.Instance
                .GetConversionHandle(src, dst)
                .Convert(src, dst, 0);

            Assert.Equal(0, converted);
            Assert.Equal(99, dstMem[0]);
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Fact]
    public void DefaultPath_Int8ToUInt8_SaturatesNegativesToZero()
    {
        // Default handle uses default table (no format Converter).
        const int count = 4;
        byte* srcMem = IntegralTestData.AlignedAlloc(count);
        byte* dstMem = IntegralTestData.AlignedAlloc(count);
        try
        {
            sbyte* src = (sbyte*)srcMem;
            src[0] = -1;
            src[1] = 0;
            src[2] = 1;
            src[3] = 127;

            IntegralSpan input = IntegralTestData.CreateSpan(
                srcMem, count, IntegralType.Int8, ByteOrder.LittleEndian);
            IntegralSpan output = IntegralTestData.CreateSpan(
                dstMem, count, IntegralType.UInt8, ByteOrder.LittleEndian);

            long n = ConversionDelegateTable.Instance
                .GetConversionHandle(input, output)
                .Convert(input, output, count);

            Assert.Equal(count, n);
            Assert.Equal(0, dstMem[0]);   // not 255 from unchecked cast
            Assert.Equal(0, dstMem[1]);
            Assert.Equal(1, dstMem[2]);
            Assert.Equal(127, dstMem[3]);
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Fact]
    public void DefaultPath_DoubleToFloat_SaturatesInfinityAndNaN()
    {
        const int count = 4;
        const ByteOrder wire = ByteOrder.LittleEndian;
        byte* srcMem = IntegralTestData.AlignedAlloc(count * sizeof(double));
        byte* dstMem = IntegralTestData.AlignedAlloc(count * sizeof(float));
        try
        {
            // Build LE double wire independently of host endianness.
            IntegralTestData.WriteEncoded(srcMem, 0, double.NaN, wire);
            IntegralTestData.WriteEncoded(srcMem, 1, double.PositiveInfinity, wire);
            IntegralTestData.WriteEncoded(srcMem, 2, double.NegativeInfinity, wire);
            IntegralTestData.WriteEncoded(srcMem, 3, 1.5d, wire);

            IntegralSpan input = IntegralTestData.CreateSpan(
                srcMem, count, IntegralType.Double, wire);
            IntegralSpan output = IntegralTestData.CreateSpan(
                dstMem, count, IntegralType.Float, wire);

            long n = ConversionDelegateTable.Instance
                .GetConversionHandle(input, output)
                .Convert(input, output, count);

            Assert.Equal(count, n);
            IntegralTestData.AssertEncodedEqual(0f, dstMem, 0, wire);
            IntegralTestData.AssertEncodedEqual(float.MaxValue, dstMem, 1, wire);
            IntegralTestData.AssertEncodedEqual(-float.MaxValue, dstMem, 2, wire);
            IntegralTestData.AssertEncodedEqual(1.5f, dstMem, 3, wire);
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Fact]
    public void DefaultHandle_IsNullAndConvertReturnsZeroWithoutThrowing()
    {
        IntegralConversionHandle defaultHandle = default;
        IntegralConversionHandle viaNew = new();
        IntegralConversionHandle explicitNull = new IntegralConversionHandle(
            func: null,
            context: new NumericConverters(DefaultConvertersFactory.Instance));

        Assert.True(defaultHandle.IsNull);
        Assert.True(viaNew.IsNull);
        Assert.True(explicitNull.IsNull);

        byte* mem = IntegralTestData.AlignedAlloc(4);
        try
        {
            IntegralSpan span = IntegralTestData.CreateSpan(
                mem, 4, IntegralType.UInt8, ByteOrder.LittleEndian);

            Assert.Equal(0, defaultHandle.Convert(span, span, 4));
            Assert.Equal(0, viaNew.Convert(span, span, 4));
            Assert.Equal(0, explicitNull.Convert(span, span, 4));
        }
        finally
        {
            IntegralTestData.AlignedFree(mem);
        }
    }

    [Fact]
    public void CustomSameType_CustomConverter_IsInvokedPerValue()
    {
        const int count = 4;
        const ByteOrder wire = ByteOrder.LittleEndian;
        byte* srcMem = IntegralTestData.AlignedAlloc(count * sizeof(int));
        byte* dstMem = IntegralTestData.AlignedAlloc(count * sizeof(int));
        try
        {
            for (int i = 0; i < count; ++i)
            {
                IntegralTestData.WriteEncoded(srcMem, i, 10 + i, wire);
            }

            IIntegralValueConverter converter =
                new ConvertersOnlyValueConverter(
                    new NumericConverters(new ScaleInt32IdentityFactory(3)));

            IntegralSpan input = IntegralTestData.CreateSpan(
                srcMem, count, IntegralType.Int32, wire);
            IntegralSpan output = new(
                dstMem,
                0,
                count * sizeof(int),
                new IntegralFormat(
                    IntegralType.Int32,
                    1,
                    wire,
                    converter));

            long n = ConversionDelegateTable.Instance
                .GetConversionHandle(input, output)
                .Convert(input, output, count);

            Assert.Equal(count, n);
            for (int i = 0; i < count; ++i)
            {
                IntegralTestData.AssertEncodedEqual(
                    (10 + i) * 3,
                    dstMem,
                    i,
                    wire);
            }
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Fact]
    public void CustomSameType_CustomConverter_OppositeEndian_IsInvokedPerValue()
    {
        const int count = 3;
        const ByteOrder sourceWire = ByteOrder.LittleEndian;
        const ByteOrder destinationWire = ByteOrder.BigEndian;
        byte* srcMem = IntegralTestData.AlignedAlloc(count * sizeof(int));
        byte* dstMem = IntegralTestData.AlignedAlloc(count * sizeof(int));
        try
        {
            int[] values = [7, -5, 100];
            for (int i = 0; i < count; ++i)
            {
                IntegralTestData.WriteEncoded(srcMem, i, values[i], sourceWire);
            }

            IIntegralValueConverter converter =
                new ConvertersOnlyValueConverter(
                    new NumericConverters(new ScaleInt32IdentityFactory(2)));

            IntegralSpan input = IntegralTestData.CreateSpan(
                srcMem, count, IntegralType.Int32, sourceWire);
            IntegralSpan output = new(
                dstMem,
                0,
                count * sizeof(int),
                new IntegralFormat(
                    IntegralType.Int32,
                    1,
                    destinationWire,
                    converter));

            long n = ConversionDelegateTable.Instance
                .GetConversionHandle(input, output)
                .Convert(input, output, count);

            Assert.Equal(count, n);
            for (int i = 0; i < count; ++i)
            {
                IntegralTestData.AssertEncodedEqual(
                    values[i] * 2,
                    dstMem,
                    i,
                    destinationWire);
            }
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Fact]
    public void DefaultSameType_WithoutCustomConverter_StillCopiesBytes()
    {
        const int count = 3;
        const ByteOrder wire = ByteOrder.LittleEndian;
        byte* srcMem = IntegralTestData.AlignedAlloc(count * sizeof(int));
        byte* dstMem = IntegralTestData.AlignedAlloc(count * sizeof(int));
        try
        {
            for (int i = 0; i < count; ++i)
            {
                IntegralTestData.WriteEncoded(srcMem, i, 100 + i, wire);
            }

            IntegralSpan input = IntegralTestData.CreateSpan(
                srcMem, count, IntegralType.Int32, wire);
            IntegralSpan output = IntegralTestData.CreateSpan(
                dstMem, count, IntegralType.Int32, wire);

            long n = ConversionDelegateTable.Instance
                .GetConversionHandle(input, output)
                .Convert(input, output, count);

            Assert.Equal(count, n);
            for (int i = 0; i < count; ++i)
            {
                IntegralTestData.AssertEncodedEqual(100 + i, dstMem, i, wire);
            }
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    public void DefaultUInt16_OppositeEndian_BulkSwapPreservesValues(int count)
    {
        RunDefault16BitOppositeEndianIdentity(
            count,
            IntegralType.UInt16,
            i => (ushort)(0xA100 + i));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    public void DefaultInt16_OppositeEndian_BulkSwapPreservesValues(int count)
    {
        RunDefault16BitOppositeEndianIdentity(
            count,
            IntegralType.Int16,
            i => (short)(-100 + i));
    }

    [Fact]
    public void CustomUInt16_OppositeEndian_StillInvokesPerValueConverter()
    {
        const int count = 5;
        const ByteOrder sourceWire = ByteOrder.LittleEndian;
        const ByteOrder destinationWire = ByteOrder.BigEndian;
        byte* srcMem = IntegralTestData.AlignedAlloc(count * sizeof(ushort));
        byte* dstMem = IntegralTestData.AlignedAlloc(count * sizeof(ushort));
        try
        {
            ushort[] values = [1, 2, 3, 4, 5];
            for (int i = 0; i < count; ++i)
            {
                IntegralTestData.WriteEncoded(srcMem, i, values[i], sourceWire);
            }

            IIntegralValueConverter converter =
                new ConvertersOnlyValueConverter(
                    new NumericConverters(new ScaleUInt16IdentityFactory(10)));

            IntegralSpan input = IntegralTestData.CreateSpan(
                srcMem, count, IntegralType.UInt16, sourceWire);
            IntegralSpan output = new(
                dstMem,
                0,
                count * sizeof(ushort),
                new IntegralFormat(
                    IntegralType.UInt16,
                    1,
                    destinationWire,
                    converter));

            long n = ConversionDelegateTable.Instance
                .GetConversionHandle(input, output)
                .Convert(input, output, count);

            Assert.Equal(count, n);
            for (int i = 0; i < count; ++i)
            {
                IntegralTestData.AssertEncodedEqual(
                    (ushort)(values[i] * 10),
                    dstMem,
                    i,
                    destinationWire);
            }
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    private static void RunDefault16BitOppositeEndianIdentity<T>(
        int count,
        IntegralType type,
        Func<int, T> valueAt)
        where T : unmanaged
    {
        const ByteOrder sourceWire = ByteOrder.LittleEndian;
        const ByteOrder destinationWire = ByteOrder.BigEndian;
        int size = sizeof(T);
        byte* srcMem = IntegralTestData.AlignedAlloc(Math.Max(1, count * size));
        byte* dstMem = IntegralTestData.AlignedAlloc(Math.Max(1, count * size));
        try
        {
            T[] values = new T[count];
            for (int i = 0; i < count; ++i)
            {
                values[i] = valueAt(i);
                IntegralTestData.WriteEncoded(srcMem, i, values[i], sourceWire);
            }

            IntegralSpan input = IntegralTestData.CreateSpan(
                srcMem, count, type, sourceWire);
            IntegralSpan output = IntegralTestData.CreateSpan(
                dstMem, count, type, destinationWire);

            long n = ConversionDelegateTable.Instance
                .GetConversionHandle(input, output)
                .Convert(input, output, count);

            Assert.Equal(count, n);
            for (int i = 0; i < count; ++i)
            {
                IntegralTestData.AssertEncodedEqual(
                    values[i],
                    dstMem,
                    i,
                    destinationWire);
            }

            // Also exercise reverse direction (B2L) using the BE output as source.
            if (count > 0)
            {
                byte* roundTrip = IntegralTestData.AlignedAlloc(count * size);
                try
                {
                    IntegralSpan beSource = IntegralTestData.CreateSpan(
                        dstMem, count, type, destinationWire);
                    IntegralSpan leDest = IntegralTestData.CreateSpan(
                        roundTrip, count, type, sourceWire);

                    long n2 = ConversionDelegateTable.Instance
                        .GetConversionHandle(beSource, leDest)
                        .Convert(beSource, leDest, count);

                    Assert.Equal(count, n2);
                    for (int i = 0; i < count; ++i)
                    {
                        IntegralTestData.AssertEncodedEqual(
                            values[i],
                            roundTrip,
                            i,
                            sourceWire);
                    }
                }
                finally
                {
                    IntegralTestData.AlignedFree(roundTrip);
                }
            }
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    /// <summary>Output converter that only supplies <see cref="IIntegralValueConverter.Converters"/>.</summary>
    private sealed class ConvertersOnlyValueConverter : IIntegralValueConverter
    {
        public ConvertersOnlyValueConverter(NumericConverters converters)
        {
            Converters = converters;
        }

        public IntegralSpanConversionFunc? Func => null;

        public NumericConverters? Converters { get; }
    }

    /// <summary>
    /// Default numeric converters with a non-identity <c>ConvertInt32ToInt32</c> scale.
    /// </summary>
    private sealed class ScaleInt32IdentityFactory : INumericConvertersFactory
    {
        private readonly int _scale;
        private readonly INumericConvertersFactory _defaults =
            DefaultConvertersFactory.Instance;

        public ScaleInt32IdentityFactory(int scale)
        {
            _scale = scale;
        }

        public NumericConversionToUInt8 UInt8Conversion() => _defaults.UInt8Conversion();
        public NumericConversionToInt8 Int8Conversion() => _defaults.Int8Conversion();
        public NumericConversionToUInt16 UInt16Conversion() => _defaults.UInt16Conversion();
        public NumericConversionToInt16 Int16Conversion() => _defaults.Int16Conversion();
        public NumericConversionToUInt32 UInt32Conversion() => _defaults.UInt32Conversion();
        public NumericConversionToUInt64 UInt64Conversion() => _defaults.UInt64Conversion();
        public NumericConversionToInt64 Int64Conversion() => _defaults.Int64Conversion();
        public NumericConversionToFloat FloatConversion() => _defaults.FloatConversion();
        public NumericConversionToDouble DoubleConversion() => _defaults.DoubleConversion();

        public NumericConversionToInt32 Int32Conversion()
        {
            ConversionToInt32Delegates d = new();
            d.ResetToDefaults();
            int scale = _scale;
            d.ConvertInt32ToInt32 = value => value * scale;
            return new NumericConversionToInt32(d);
        }
    }

    /// <summary>
    /// Default numeric converters with a non-identity <c>ConvertUInt16ToUInt16</c> scale.
    /// </summary>
    private sealed class ScaleUInt16IdentityFactory : INumericConvertersFactory
    {
        private readonly int _scale;
        private readonly INumericConvertersFactory _defaults =
            DefaultConvertersFactory.Instance;

        public ScaleUInt16IdentityFactory(int scale)
        {
            _scale = scale;
        }

        public NumericConversionToUInt8 UInt8Conversion() => _defaults.UInt8Conversion();
        public NumericConversionToInt8 Int8Conversion() => _defaults.Int8Conversion();
        public NumericConversionToInt16 Int16Conversion() => _defaults.Int16Conversion();
        public NumericConversionToUInt32 UInt32Conversion() => _defaults.UInt32Conversion();
        public NumericConversionToInt32 Int32Conversion() => _defaults.Int32Conversion();
        public NumericConversionToUInt64 UInt64Conversion() => _defaults.UInt64Conversion();
        public NumericConversionToInt64 Int64Conversion() => _defaults.Int64Conversion();
        public NumericConversionToFloat FloatConversion() => _defaults.FloatConversion();
        public NumericConversionToDouble DoubleConversion() => _defaults.DoubleConversion();

        public NumericConversionToUInt16 UInt16Conversion()
        {
            ConversionToUInt16Delegates d = new();
            d.ResetToDefaults();
            int scale = _scale;
            d.ConvertUInt16ToUInt16 = value => (ushort)(value * scale);
            return new NumericConversionToUInt16(d);
        }
    }
}
