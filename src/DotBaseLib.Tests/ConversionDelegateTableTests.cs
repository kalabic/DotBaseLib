using DotBase.Buffers;
using DotBase.Integral;
using DotBase.Integral.Conversion;
using DotBase.Integral.Conversion.Internal.Interleaved;
using DotBase.Integral.Conversion.Internal.Standard;
using DotBase.Integral.Conversion.Numeric;
using DotBase.Integral.Conversion.Numeric.Defaults;
using System.Reflection;

namespace DotBaseLib.Tests;


public unsafe class ConversionDelegateTableTests
{
    [Fact]
    public void NumericTable_LazilyPublishesOneDefaultDelegatePerSlot()
    {
        DefaultNumericValueDelegateTable table = CreateDefaultNumericTable();
        const IntegralType inputType = IntegralType.Int16;
        const IntegralType outputType = IntegralType.Double;
        int tableIndex = NumericTableIndex(inputType, outputType);

        Assert.Equal(0, CountResolvedNumericSlots(table));

        Delegate[] converters = new Delegate[128];
        Parallel.For(0, converters.Length, index =>
        {
            converters[index] = table.GetConverter(
                tableIndex,
                inputType,
                outputType);
        });

        Assert.Equal(1, CountResolvedNumericSlots(table));
        Delegate expected = converters[0];
        foreach (Delegate converter in converters)
        {
            Assert.Same(expected, converter);
        }
    }

    [Fact]
    public void NumericTable_ResolvesEveryTypeSlot()
    {
        DefaultNumericValueDelegateTable table = CreateDefaultNumericTable();
        IntegralType[] valueTypes =
            Enum.GetValues<IntegralType>()
                .Where(type => type != IntegralType.None)
                .ToArray();

        foreach (IntegralType inputType in valueTypes)
        foreach (IntegralType outputType in valueTypes)
        {
            Assert.NotNull(table.GetConverter(
                NumericTableIndex(inputType, outputType),
                inputType,
                outputType));
        }

        Assert.Equal(
            NumericValueConverters.TableSize,
            CountResolvedNumericSlots(table));
    }

    [Fact]
    public void NumericValueConverters_StoresOnlyConfiguredOverrides()
    {
        ConvertInt16ToDouble_Delegate custom = value => value + 0.5;
        NumericValueConverters table = NumericValueConverters.Create(registration =>
            registration.SetConverter(
                custom,
                IntegralType.Int16,
                IntegralType.Double));

        Assert.Equal(1, CountNumericOverrides(table));
        Assert.Same(
            custom,
            table.GetConverter(IntegralType.Int16, IntegralType.Double));
        Assert.Same(
            NumericValueConverters.Default.GetConverter(
                IntegralType.UInt32,
                IntegralType.Float),
            table.GetConverter(IntegralType.UInt32, IntegralType.Float));
        Assert.Equal(1, CountNumericOverrides(table));
    }

    [Fact]
    public void StandardTable_LazilyPublishesOneDelegatePairPerSlot()
    {
        StandardDelegateTable table = CreateStandardTable();
        IntegralFormat input = new(
            IntegralType.Int16,
            1,
            ByteOrder.BigEndian);
        IntegralFormat output = new(
            IntegralType.Double,
            1,
            ByteOrder.LittleEndian);

        Assert.Equal(0, CountResolvedStandardSlots(table));

        IntegralConversionHandle[] handles =
            new IntegralConversionHandle[128];
        Parallel.For(0, handles.Length, index =>
        {
            handles[index] = (index & 1) == 0
                ? table.GetDefaultHandle(input, output)
                : table.GetCustomHandle(
                    input,
                    output,
                    NumericValueConverters.Default);
        });

        Assert.Equal(1, CountResolvedStandardSlots(table));
        nint expectedDefault =
            table.GetDefaultHandle(input, output)._func;
        nint expectedCustom =
            table.GetCustomHandle(
                input,
                output,
                NumericValueConverters.Default)._func;
        for (int i = 0; i < handles.Length; ++i)
        {
            Assert.Equal(
                (i & 1) == 0 ? expectedDefault : expectedCustom,
                handles[i]._func);
        }
    }

    [Fact]
    public void StandardTable_ResolvesEveryEndianAndTypeSlot()
    {
        StandardDelegateTable table = CreateStandardTable();
        ByteOrder[] byteOrders =
        [
            ByteOrder.LittleEndian,
            ByteOrder.BigEndian,
        ];
        IntegralType[] valueTypes =
            Enum.GetValues<IntegralType>()
                .Where(type => type != IntegralType.None)
                .ToArray();

        foreach (ByteOrder inputOrder in byteOrders)
        foreach (ByteOrder outputOrder in byteOrders)
        foreach (IntegralType inputType in valueTypes)
        foreach (IntegralType outputType in valueTypes)
        {
            IntegralFormat input = new(inputType, 1, inputOrder);
            IntegralFormat output = new(outputType, 1, outputOrder);

            Assert.False(table.GetDefaultHandle(input, output).IsNull);
            Assert.False(table.GetCustomHandle(
                input,
                output,
                NumericValueConverters.Default).IsNull);
        }

        Assert.Equal(
            StandardDelegateTable.TableSize,
            CountResolvedStandardSlots(table));
    }

    [Fact]
    public void InterleavedTable_LazilyPublishesOneDelegatePairPerSlot()
    {
        InterleavedDelegateTable table = CreateInterleavedTable();
        IntegralFormat input = new(
            IntegralType.Int16,
            2,
            ByteOrder.BigEndian);
        IntegralFormat output = new(
            IntegralType.Double,
            1,
            ByteOrder.LittleEndian);

        Assert.Equal(0, CountResolvedInterleavedSlots(table));

        IntegralConversionHandle[] handles =
            new IntegralConversionHandle[128];
        Parallel.For(0, handles.Length, index =>
        {
            handles[index] = (index & 1) == 0
                ? table.GetDefaultHandle(input, output)
                : table.GetCustomHandle(
                    input,
                    output,
                    NumericValueConverters.Default);
        });

        Assert.Equal(1, CountResolvedInterleavedSlots(table));
        nint expectedDefault =
            table.GetDefaultHandle(input, output)._func;
        nint expectedCustom =
            table.GetCustomHandle(
                input,
                output,
                NumericValueConverters.Default)._func;
        for (int i = 0; i < handles.Length; ++i)
        {
            Assert.Equal(
                (i & 1) == 0 ? expectedDefault : expectedCustom,
                handles[i]._func);
        }
    }

    [Fact]
    public void InterleavedTable_ResolvesEveryEndianAndTypeSlot()
    {
        InterleavedDelegateTable table = CreateInterleavedTable();
        ByteOrder[] byteOrders =
        [
            ByteOrder.LittleEndian,
            ByteOrder.BigEndian,
        ];
        IntegralType[] valueTypes =
            Enum.GetValues<IntegralType>()
                .Where(type => type != IntegralType.None)
                .ToArray();

        foreach (ByteOrder inputOrder in byteOrders)
        foreach (ByteOrder outputOrder in byteOrders)
        foreach (IntegralType inputType in valueTypes)
        foreach (IntegralType outputType in valueTypes)
        {
            IntegralFormat input = new(inputType, 2, inputOrder);
            IntegralFormat output = new(outputType, 1, outputOrder);

            Assert.False(table.GetDefaultHandle(input, output).IsNull);
            Assert.False(table.GetCustomHandle(
                input,
                output,
                NumericValueConverters.Default).IsNull);
        }

        Assert.Equal(
            InterleavedDelegateTable.TableSize,
            CountResolvedInterleavedSlots(table));
    }

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
                ConversionHandles.GetHandle(src.Format, dst.Format);

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
                ConversionHandles.GetHandle(src.Format, dst.Format);

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
                ConversionHandles.GetHandle(input.Format, output.Format);

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
                ConversionHandles.GetHandle(srcNative.Format, dstNative.Format);
            var handleExplicit =
                ConversionHandles.GetHandle(srcExplicit.Format, dstExplicit.Format);

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

            long converted = ConversionHandles.GetHandle(input.Format, output.Format)
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

            long converted = ConversionHandles.GetHandle(src.Format, dst.Format)
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

            long n = ConversionHandles.GetHandle(input.Format, output.Format)
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

            long n = ConversionHandles.GetHandle(input.Format, output.Format)
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
            func: 0,
            numericConverter: 0);

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

            IntegralConversionPolicy conversionPolicy =
                IntegralConversionPolicy.FromValueConverters(
                    NumericValueConverters.Create(table =>
                    {
                        int scale = 3;
                        ConvertInt32ToInt32_Delegate d = value => value * scale;
                        table.SetConverter(d, IntegralType.Int32, IntegralType.Int32);
                    }));

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
                    conversionPolicy));

            IntegralConversionHandle handle =
                ConversionHandles.GetHandle(input.Format, output.Format);
            ConversionContext? ctx =
                ConversionHandles.GetContext(handle);
            Assert.NotNull(ctx);
            long n = ctx.Convert(input, output, count);

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

            IntegralConversionPolicy converterPolicy =
                IntegralConversionPolicy.FromValueConverters(
                    NumericValueConverters.Create(table =>
                    {
                        int scale = 2;
                        ConvertInt32ToInt32_Delegate d = value => value * scale;
                        table.SetConverter(d, IntegralType.Int32, IntegralType.Int32);
                    }));

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
                    converterPolicy));

            IntegralConversionHandle handle =
                ConversionHandles.GetHandle(input.Format, output.Format);
            ConversionContext? ctx =
                ConversionHandles.GetContext(handle);
            Assert.NotNull(ctx);
            long n = ctx.Convert(input, output, count);

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

            long n = ConversionHandles.GetHandle(input.Format, output.Format)
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

            IntegralConversionPolicy converterPolicy =
                IntegralConversionPolicy.FromValueConverters(
                    NumericValueConverters.Create(table =>
                    {
                        int scale = 10;
                        ConvertUInt16ToUInt16_Delegate d = value => (ushort)(value * scale);
                        table.SetConverter(d, IntegralType.UInt16, IntegralType.UInt16);
                    }));

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
                    converterPolicy));

            IntegralConversionHandle handle =
                ConversionHandles.GetHandle(input.Format, output.Format);
            ConversionContext? ctx =
                ConversionHandles.GetContext(handle);
            Assert.NotNull(ctx);
            long n = ctx.Convert(input, output, count);

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

            long n = ConversionHandles.GetHandle(input.Format, output.Format)
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

                    long n2 = ConversionHandles.GetHandle(beSource.Format, leDest.Format)
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

    private static StandardDelegateTable CreateStandardTable()
    {
        return (StandardDelegateTable)Activator.CreateInstance(
            typeof(StandardDelegateTable),
            nonPublic: true)!;
    }

    private static int CountResolvedStandardSlots(
        StandardDelegateTable table)
    {
        FieldInfo field = typeof(StandardDelegateTable).GetField(
            "_funcTable",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        Array slots = (Array)field.GetValue(table)!;
        int count = 0;
        foreach (object? slot in slots)
        {
            if (slot is not null)
            {
                ++count;
            }
        }

        return count;
    }

    private static DefaultNumericValueDelegateTable CreateDefaultNumericTable()
    {
        return (DefaultNumericValueDelegateTable)Activator.CreateInstance(
            typeof(DefaultNumericValueDelegateTable),
            nonPublic: true)!;
    }

    private static int CountResolvedNumericSlots(
        DefaultNumericValueDelegateTable table)
    {
        FieldInfo field = typeof(DefaultNumericValueDelegateTable).GetField(
            "_converters",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        return CountNonNullSlots((Array)field.GetValue(table)!);
    }

    private static int CountNumericOverrides(NumericValueConverters table)
    {
        FieldInfo field = typeof(NumericValueConverters).GetField(
            "_overrides",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        return CountNonNullSlots((Array)field.GetValue(table)!);
    }

    private static int CountNonNullSlots(Array slots)
    {
        int count = 0;
        foreach (object? slot in slots)
        {
            if (slot is not null)
            {
                ++count;
            }
        }

        return count;
    }

    private static int NumericTableIndex(
        IntegralType inputType,
        IntegralType outputType)
    {
        return (int)inputType - 1 + 10 * ((int)outputType - 1);
    }

    private static InterleavedDelegateTable CreateInterleavedTable()
    {
        return (InterleavedDelegateTable)Activator.CreateInstance(
            typeof(InterleavedDelegateTable),
            nonPublic: true)!;
    }

    private static int CountResolvedInterleavedSlots(
        InterleavedDelegateTable table)
    {
        FieldInfo field = typeof(InterleavedDelegateTable).GetField(
            "_funcTable",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        Array slots = (Array)field.GetValue(table)!;
        int count = 0;
        foreach (object? slot in slots)
        {
            if (slot is not null)
            {
                ++count;
            }
        }

        return count;
    }
}
