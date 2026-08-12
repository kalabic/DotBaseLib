using System.Diagnostics;

namespace DotBase.Integral.Conversion.Internal;


/// <summary>
/// Built-in conversion handle and context construction for
/// <see cref="ConversionHandles"/>.
/// </summary>
internal static class InternalConversionDelegates
{
    internal static IntegralConversionHandle CreateStaticSpanHandle(
        nint func,
        NumericValueConverters? table,
        in IntegralFormat input,
        in IntegralFormat output)
    {
        Debug.Assert(func != 0);
        nint converter = table?.GetConverterHandle(input.ValueType, output.ValueType) ?? 0;
        return new IntegralConversionHandle(
            func,
            converter,
            output.ConversionPolicy);
    }

    internal static ConversionContext? SpanContext_Default(
        IntegralConversionHandle handle)
    {
        if (handle.IsNull)
        {
            return null;
        }

        if (handle._numericConverter != 0)
        {
            return new NumericConversionContext(handle);
        }

        return new ConversionContext(handle);
    }

    internal static PlanarReaderContext? PlanarReaderContext_Default(
        IntegralConversionHandle handle,
        long planeCapacity,
        int blockCapacity,
        int inputPlaneIndex)
    {
        if (handle.IsNull)
        {
            return null;
        }

        return new PlanarReaderContext(
            handle,
            planeCapacity,
            blockCapacity,
            inputPlaneIndex);
    }

    internal static PlanarWriterContext? PlanarWriterContext_Default(
        IntegralConversionHandle handle,
        long planeCapacity,
        int blockCapacity,
        int outputPlaneIndex)
    {
        if (handle.IsNull)
        {
            return null;
        }

        return new PlanarWriterContext(
            handle,
            planeCapacity,
            blockCapacity,
            outputPlaneIndex);
    }

    internal static PlanarTransferContext? PlanarTransferContext_Default(
        IntegralConversionHandle handle,
        long planeCapacity,
        int blockCapacity,
        int inputPlaneIndex,
        int outputPlaneIndex)
    {
        if (handle.IsNull)
        {
            return null;
        }

        return new PlanarTransferContext(
            handle,
            planeCapacity,
            blockCapacity,
            inputPlaneIndex,
            outputPlaneIndex);
    }

    internal static InterleavedReaderContext? InterleavedReaderContext_Default(
        IntegralConversionHandle handle,
        int inputBlockCapacity,
        int index)
    {
        if (handle.IsNull)
        {
            return null;
        }

        return new InterleavedReaderContext(
            handle,
            inputBlockCapacity,
            index);
    }

    internal static InterleavedWriterContext? InterleavedWriterContext_Default(
        IntegralConversionHandle handle,
        int outputBlockCapacity,
        int index)
    {
        if (handle.IsNull)
        {
            return null;
        }

        return new InterleavedWriterContext(
            handle,
            outputBlockCapacity,
            index);
    }

    internal static InterleavedTransferContext? InterleavedTransferContext_Default(
        IntegralConversionHandle handle,
        int inputBlockCapacity,
        int inputValueIndex,
        int outputBlockCapacity,
        int outputValueIndex)
    {
        if (handle.IsNull)
        {
            return null;
        }

        return new InterleavedTransferContext(
            handle,
            inputBlockCapacity,
            inputValueIndex,
            outputBlockCapacity,
            outputValueIndex);
    }
}
