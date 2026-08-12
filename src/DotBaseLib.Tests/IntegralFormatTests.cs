using DotBase.Buffers;
using DotBase.Integral;
using DotBase.Integral.Conversion;
using DotBase.Integral.Conversion.Internal;
using DotBase.Integral.Conversion.Numeric;
using System.Runtime.CompilerServices;
using System.Reflection;

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
        Assert.True(stereoBe.ConversionPolicy.RegistryIndex > 0);
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
    public void IntegralFormatPolicyAndHandleAreUnmanaged()
    {
        MustBeUnmanaged<IntegralFormat>();
        MustBeUnmanaged<IntegralConversionPolicy>();
        MustBeUnmanaged<IntegralConversionHandle>();
        Assert.Equal(sizeof(int), Unsafe.SizeOf<IntegralConversionPolicy>());
    }

    [Fact]
    public void NegativeValueSizeIsPreservedAndRejectedAsInvalid()
    {
        IntegralConversionPolicy policy =
            IntegralConversionPolicy.RefuseAll();
        IntegralFormat format = new(
            valueSize: -7,
            blockCapacity: 0,
            ByteOrder.BigEndian,
            policy);

        Assert.Equal(IntegralType.None, format.ValueType);
        Assert.Equal(-7, format.ValueSize);
        Assert.Equal(0, format.BlockCapacity);
        Assert.Equal(ByteOrder.BigEndian, format.ByteOrder);
        Assert.Equal(policy, format.ConversionPolicy);
        Assert.False(format.IsEmptyType());
        Assert.False(format.IsValid());

        ArgumentOutOfRangeException error =
            Assert.Throws<ArgumentOutOfRangeException>(format.Validate);
        Assert.Equal("valueSize", error.ParamName);
    }

    [Theory]
    [InlineData(0, 0, true, true)]
    [InlineData(0, 1, false, false)]
    [InlineData(1, 0, false, false)]
    [InlineData(-1, 0, false, false)]
    [InlineData(1, 1, false, true)]
    public void EmptyFormatRequiresExactNoneZeroZeroTuple(
        int valueSize,
        int blockCapacity,
        bool expectedEmpty,
        bool expectedValid)
    {
        IntegralFormat format = new(valueSize, blockCapacity);

        Assert.Equal(expectedEmpty, format.IsEmptyType());
        Assert.Equal(expectedValid, format.IsValid());
    }

    [Fact]
    public void FromValueConverters_SharesRegistryIndex()
    {
        NumericValueConverters table = NumericValueConverters.Default;
        IntegralConversionPolicy a = IntegralConversionPolicy.FromValueConverters(table);
        IntegralConversionPolicy b = IntegralConversionPolicy.FromValueConverters(table);
        Assert.Equal(a, b);
        Assert.True(a.RegistryIndex > 0);
    }

    [Fact]
    public void FromValueConverters_ConcurrentCallsObserveOneRegistryIndex()
    {
        NumericValueConverters table = NumericValueConverters.Create(_ => { });
        IntegralConversionPolicy[] policies = new IntegralConversionPolicy[256];

        Parallel.For(
            0,
            policies.Length,
            i => policies[i] = IntegralConversionPolicy.FromValueConverters(table));

        IntegralConversionPolicy expected = policies[0];
        Assert.All(policies, policy => Assert.Equal(expected, policy));
        Assert.True(expected.RegistryIndex > 0);
    }

    [Fact]
    public void FromValueConverters_SelectsManagedConverterOnEveryPath()
    {
        ConvertInt32ToInt32_Delegate converter = value => value + 1;
        NumericValueConverters table = NumericValueConverters.Create(registration =>
            registration.SetConverter(
                converter,
                IntegralType.Int32,
                IntegralType.Int32));
        IntegralConversionPolicy policy =
            IntegralConversionPolicy.FromValueConverters(table);
        IntegralFormat input = IntegralFormat.For<int>();
        IntegralFormat output = IntegralFormat.For<int>(conversionPolicy: policy);

        IntegralConversionHandle span = ConversionHandles.GetHandle(input, output);
        ConversionContext planar = ConversionHandles.GetPlanarReaderContext(
            input,
            output,
            planeCapacity: 1,
            blockCapacity: 1,
            inputPlaneIndex: 0)!;
        ConversionContext interleaved = ConversionHandles.GetInterleavedReaderContext(
            input,
            output,
            inputBlockCapacity: 2,
            index: 0)!;

        Assert.Same(converter, span.ResolveNumericConverter());
        Assert.Same(
            converter,
            ConversionHandles.GetContext(span)!.NumericFunc);
        Assert.Same(converter, planar.NumericFunc);
        Assert.Same(converter, interleaved.NumericFunc);
    }

    [Fact]
    public void FromFunc_DeduplicatesByStaticDelegateAndTableIdentity()
    {
        NumericValueConverters table = NumericValueConverters.Create(_ => { });
        NumericValueConverters equalContentTable = NumericValueConverters.Create(_ => { });

        IntegralConversionPolicy first = IntegralConversionPolicy.FromFunc(
            StaticConversion,
            table);
        IntegralConversionPolicy duplicate = IntegralConversionPolicy.FromFunc(
            StaticConversion,
            table);
        IntegralConversionPolicy differentFunction = IntegralConversionPolicy.FromFunc(
            OtherStaticConversion,
            table);
        IntegralConversionPolicy differentTable = IntegralConversionPolicy.FromFunc(
            StaticConversion,
            equalContentTable);
        IntegralConversionPolicy withoutTable =
            IntegralConversionPolicy.FromFunc(StaticConversion);
        IntegralConversionPolicy duplicateWithoutTable =
            IntegralConversionPolicy.FromFunc(StaticConversion);

        Assert.Equal(first, duplicate);
        Assert.NotEqual(first, differentFunction);
        Assert.NotEqual(first, differentTable);
        Assert.Equal(withoutTable, duplicateWithoutTable);
        Assert.NotEqual(first, withoutTable);
    }

    [Fact]
    public void FromFunc_ConcurrentCallsObserveOneRegistryIndex()
    {
        NumericValueConverters table = NumericValueConverters.Create(_ => { });
        IntegralConversionPolicy[] policies = new IntegralConversionPolicy[256];

        Parallel.For(
            0,
            policies.Length,
            i => policies[i] = IntegralConversionPolicy.FromFunc(
                StaticConversion,
                table));

        IntegralConversionPolicy expected = policies[0];
        Assert.All(policies, policy => Assert.Equal(expected, policy));
    }

    [Fact]
    public void FromValueConverters_DeduplicatesByTableIdentityOnly()
    {
        NumericValueConverters firstTable = NumericValueConverters.Create(_ => { });
        NumericValueConverters secondTable = NumericValueConverters.Create(_ => { });

        IntegralConversionPolicy first =
            IntegralConversionPolicy.FromValueConverters(firstTable);
        IntegralConversionPolicy duplicate =
            IntegralConversionPolicy.FromValueConverters(firstTable);
        IntegralConversionPolicy equalContent =
            IntegralConversionPolicy.FromValueConverters(secondTable);

        Assert.Equal(first, duplicate);
        Assert.NotEqual(first, equalContent);
    }

    [Fact]
    public void FromFunc_RejectsNonStaticAndMulticastDelegates()
    {
        InstanceConversion instance = new();
        IntegralSpanConversionFunc instanceFunc = instance.Convert;

        int captured = 1;
        IntegralSpanConversionFunc capturingFunc =
            (in IntegralSpan input,
             in IntegralSpan output,
             long count,
             ConversionContext? context) => captured;

        IntegralSpanConversionFunc multicast = StaticConversion;
        multicast += OtherStaticConversion;

        Assert.Equal(
            "func",
            Assert.Throws<ArgumentException>(() =>
                IntegralConversionPolicy.FromFunc(instanceFunc)).ParamName);
        Assert.Equal(
            "func",
            Assert.Throws<ArgumentException>(() =>
                IntegralConversionPolicy.FromFunc(capturingFunc)).ParamName);
        Assert.Equal(
            "func",
            Assert.Throws<ArgumentException>(() =>
                IntegralConversionPolicy.FromFunc(multicast)).ParamName);
    }

    [Fact]
    public void FromFunc_OverridesOnlySpanPathAndCarriesManagedConverter()
    {
        NumericValueConverters table = NumericValueConverters.Create(_ => { });
        IntegralConversionPolicy policy = IntegralConversionPolicy.FromFunc(
            StaticConversion,
            table);
        IntegralFormat input = IntegralFormat.For<int>();
        IntegralFormat output = IntegralFormat.For<int>(conversionPolicy: policy);

        IntegralConversionHandle span = ConversionHandles.GetHandle(input, output);
        IntegralConversionHandle planar = ConversionHandles.GetPlanarHandle(input, output);
        IntegralConversionHandle interleaved =
            ConversionHandles.GetInterleavedHandle(input, output);

        Assert.Equal(
            new IntegralSpanConversionFunc(StaticConversion),
            span.ResolveFunc());
        Assert.Same(
            table.GetConverter(IntegralType.Int32, IntegralType.Int32),
            span.ResolveNumericConverter());
        Assert.NotEqual(span._func, planar._func);
        Assert.NotEqual(span._func, interleaved._func);
        Assert.Equal(0, planar._numericConverter);
        Assert.Equal(0, interleaved._numericConverter);

        ConversionContext context =
            ConversionHandles.GetContext(span)!;
        Assert.Equal(
            17,
            context.Convert(default, default, count: 1));
    }

    [Fact]
    public void FromFunc_WithoutNumericConverter_ResolvesStructuralHandleInContext()
    {
        IntegralConversionPolicy policy =
            IntegralConversionPolicy.FromFunc(StaticConversion);
        IntegralFormat input = IntegralFormat.For<int>();
        IntegralFormat output = IntegralFormat.For<int>(conversionPolicy: policy);

        IntegralConversionHandle handle =
            ConversionHandles.GetHandle(input, output);
        ConversionContext context =
            ConversionHandles.GetContext(handle)!;

        Assert.True(handle.NeedsContext);
        Assert.NotEqual(0, handle._func);
        Assert.Null(context.NumericFunc);
        Assert.Equal(17, context.Convert(default, default, count: 1));
    }

    [Fact]
    public void ManagedRegistryRootsConverterTableAndDelegateTarget()
    {
        (IntegralConversionPolicy policy,
         WeakReference tableReference,
         WeakReference targetReference) = CreateRootedPolicy();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.True(tableReference.IsAlive);
        Assert.True(targetReference.IsAlive);

        IntegralFormat input = IntegralFormat.For<int>();
        IntegralFormat output = IntegralFormat.For<int>(conversionPolicy: policy);
        IntegralConversionHandle handle = ConversionHandles.GetHandle(input, output);

        Assert.NotEqual(0, handle._numericConverter);
        Assert.Same(targetReference.Target, handle.ResolveNumericConverter()!.Target);
        ConvertInt32ToInt32_Delegate converter =
            Assert.IsType<ConvertInt32ToInt32_Delegate>(
                handle.ResolveNumericConverter());
        Assert.Equal(21, converter(7));
    }

    [Fact]
    public void DelegateHandlesAreInternalAndDisposalIsNotPublicContract()
    {
        Type converters = typeof(NumericValueConverters);
        Assert.False(typeof(IDisposable).IsAssignableFrom(converters));
        Assert.Null(converters.GetProperty("SelfHandle"));
        Assert.Null(converters.GetMethod("GetConverterFunctionPointer"));
        Assert.DoesNotContain(
            typeof(IntegralConversionPolicy).GetMethods(),
            method => method.Name == "Create");

        BindingFlags fields = BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic;
        FieldInfo policyField = Assert.Single(
            typeof(IntegralConversionPolicy).GetFields(fields));
        Assert.Equal(typeof(int), policyField.FieldType);
        Assert.Equal(
            2,
            typeof(IntegralConversionHandle).GetFields(fields)
                .Count(field => field.FieldType == typeof(nint)));
        Assert.Contains(
            converters.GetFields(fields),
            field => field.FieldType == typeof(nint[]));
        Assert.DoesNotContain(
            typeof(IntegralConversionHandle).GetFields(BindingFlags.Instance | BindingFlags.Public),
            field => field.FieldType == typeof(nint));

        string[] removedFactoryTypes =
        [
            "IntegralSpanConversionHandleFunc",
            "InterleavedConversionHandleFunc",
            "PlanarConversionHandleFunc",
            "IntegralSpanConversionContextFunc",
            "InterleavedReaderConversionContextFunc",
            "InterleavedWriterConversionContextFunc",
            "InterleavedTransferConversionContextFunc",
            "PlanarReaderConversionContextFunc",
            "PlanarWriterConversionContextFunc",
            "PlanarTransferConversionContextFunc",
        ];
        foreach (string typeName in removedFactoryTypes)
        {
            Assert.Null(
                converters.Assembly.GetType(
                    $"DotBase.Integral.Conversion.{typeName}"));
        }
    }

    [Fact]
    public void RefuseAll_ProducesNullHandlesAndDirectContexts()
    {
        IntegralFormat input = IntegralFormat.For<byte>();
        IntegralFormat output = new(
            IntegralType.UInt8,
            1,
            ByteOrder.Native,
            IntegralConversionPolicy.RefuseAll());

        Assert.True(ConversionHandles.GetHandle(input, output).IsNull);
        Assert.True(ConversionHandles.GetInterleavedHandle(input, output).IsNull);
        Assert.True(ConversionHandles.GetPlanarHandle(input, output).IsNull);
        Assert.Null(ConversionHandles.GetPlanarReaderContext(
            input, output, planeCapacity: 1, blockCapacity: 1, inputPlaneIndex: 0));
        Assert.Null(ConversionHandles.GetPlanarWriterContext(
            input, output, planeCapacity: 1, blockCapacity: 1, outputPlaneIndex: 0));
        Assert.Null(ConversionHandles.GetPlanarTransferContext(
            input,
            output,
            planeCapacity: 1,
            blockCapacity: 1,
            inputPlaneIndex: 0,
            outputPlaneIndex: 0));
        Assert.Null(ConversionHandles.GetInterleavedReaderContext(
            input, output, inputBlockCapacity: 2, index: 0));
        Assert.Null(ConversionHandles.GetInterleavedWriterContext(
            input, output, outputBlockCapacity: 2, index: 0));
        Assert.Null(ConversionHandles.GetInterleavedTransferContext(
            input,
            output,
            inputBlockCapacity: 2,
            inputValueIndex: 0,
            outputBlockCapacity: 2,
            outputValueIndex: 0));
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

    private static long StaticConversion(
        in IntegralSpan input,
        in IntegralSpan output,
        long count,
        ConversionContext? context)
    {
        _ = input;
        _ = output;
        _ = count;
        _ = context;
        return 17;
    }

    private static long OtherStaticConversion(
        in IntegralSpan input,
        in IntegralSpan output,
        long count,
        ConversionContext? context)
    {
        _ = input;
        _ = output;
        _ = count;
        _ = context;
        return 23;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (
        IntegralConversionPolicy Policy,
        WeakReference TableReference,
        WeakReference TargetReference) CreateRootedPolicy()
    {
        ScaleState state = new();
        NumericValueConverters table = NumericValueConverters.Create(registration =>
        {
            ConvertInt32ToInt32_Delegate converter = state.Convert;
            registration.SetConverter(
                converter,
                IntegralType.Int32,
                IntegralType.Int32);
        });
        IntegralConversionPolicy policy =
            IntegralConversionPolicy.FromValueConverters(table);
        return (policy, new WeakReference(table), new WeakReference(state));
    }

    private sealed class InstanceConversion
    {
        internal long Convert(
            in IntegralSpan input,
            in IntegralSpan output,
            long count,
            ConversionContext? context)
        {
            _ = input;
            _ = output;
            _ = count;
            _ = context;
            return 0;
        }
    }

    private sealed class ScaleState
    {
        internal int Convert(int value) => value * 3;
    }
}
