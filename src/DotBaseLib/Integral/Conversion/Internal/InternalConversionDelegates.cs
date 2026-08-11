using DotBase.Integral.Conversion.Internal.Interleaved;
using DotBase.Integral.Conversion.Internal.Standard;

namespace DotBase.Integral.Conversion.Internal;


/// <summary>
/// Built-in handle and context factory implementations for <see cref="ConversionHandles"/>.
/// </summary>
internal static class InternalConversionDelegates
{
    // -------------------------------------------------------------------------
    // Handle factories — default (no NumericValueConverters)
    // -------------------------------------------------------------------------

    internal static IntegralConversionHandle SpanHandle_Default(
        in IntegralFormat input,
        in IntegralFormat output)
    {
        return StandardDelegateTable.Instance.GetDefaultHandle(input, output);
    }

    internal static IntegralConversionHandle ReaderHandle_Default(
        in IntegralFormat input,
        in IntegralFormat output)
    {
        return InterleavedDelegateTable.Instance.GetDefaultHandle(input, output);
    }

    internal static IntegralConversionHandle WriterHandle_Default(
        in IntegralFormat input,
        in IntegralFormat output)
    {
        return InterleavedDelegateTable.Instance.GetDefaultHandle(input, output);
    }

    // -------------------------------------------------------------------------
    // Handle factory builders — with NumericValueConverters (closed over table)
    // -------------------------------------------------------------------------

    internal static IntegralSpanConversionHandleFunc MakeSpanHandle_WithConverters(
        NumericValueConverters table)
    {
        ArgumentNullException.ThrowIfNull(table);
        return (in IntegralFormat input, in IntegralFormat output) =>
            StandardDelegateTable.Instance.GetCustomHandle(input, output, table);
    }

    internal static InterleavedReaderConversionHandleFunc MakeReaderHandle_WithConverters(
        NumericValueConverters table)
    {
        ArgumentNullException.ThrowIfNull(table);
        return (in IntegralFormat input, in IntegralFormat output) =>
            InterleavedDelegateTable.Instance.GetCustomHandle(input, output, table);
    }

    internal static InterleavedWriterConversionHandleFunc MakeWriterHandle_WithConverters(
        NumericValueConverters table)
    {
        ArgumentNullException.ThrowIfNull(table);
        return (in IntegralFormat input, in IntegralFormat output) =>
            InterleavedDelegateTable.Instance.GetCustomHandle(input, output, table);
    }

    // -------------------------------------------------------------------------
    // Handle factory builders — user structural func (+ optional converters)
    // -------------------------------------------------------------------------

    internal static IntegralSpanConversionHandleFunc MakeSpanHandle_FromFunc(
        IntegralSpanConversionFunc func,
        NumericValueConverters? table)
    {
        ArgumentNullException.ThrowIfNull(func);
        return (in IntegralFormat input, in IntegralFormat output) =>
        {
            nint numeric = 0;
            if (table is not null)
            {
                numeric = table.GetConverterFunctionPointer(input.ValueType, output.ValueType);
            }

            return new IntegralConversionHandle(func, numeric, contextFactory: 0);
        };
    }

    // -------------------------------------------------------------------------
    // Context factories — default
    // -------------------------------------------------------------------------

    internal static ConversionContext? SpanContext_Default(
        IntegralConversionHandle handle,
        in IntegralFormat input,
        in IntegralFormat output)
    {
        _ = input;
        _ = output;
        if (handle.IsNull)
        {
            return null;
        }

        // Prefer NumericConversionContext whenever a scalar converter may be needed.
        if (handle._numericFunc != 0)
        {
            return new NumericConversionContext(handle);
        }

        return new ConversionContext(handle);
    }

    internal static ConversionContext? ReaderContext_Default(
        IntegralConversionHandle handle,
        in IntegralFormat input,
        in IntegralFormat output,
        int inputBlockCapacity,
        int index)
    {
        _ = input;
        _ = output;
        if (handle.IsNull)
        {
            return null;
        }

        // InterleavedReaderContext : NumericConversionContext — ready for custom scalars.
        return new InterleavedReaderContext(handle, inputBlockCapacity, index);
    }

    internal static ConversionContext? WriterContext_Default(
        IntegralConversionHandle handle,
        in IntegralFormat input,
        in IntegralFormat output,
        int outputBlockCapacity,
        int index)
    {
        _ = input;
        _ = output;
        if (handle.IsNull)
        {
            return null;
        }

        return new InterleavedWriterContext(handle, outputBlockCapacity, index);
    }
}
