using System.Diagnostics;
using System.Runtime.InteropServices;
using DotBase.Integral.Conversion.Internal;
using DotBase.Integral.Conversion.Internal.Interleaved;
using DotBase.Integral.Conversion.Internal.Standard;

namespace DotBase.Integral.Conversion;


/// <summary>
/// Public façade for obtaining conversion handles and contexts.
/// Policy dispatch (default / refuse / factory) lives here; tables stay policy-free.
/// </summary>
public static class ConversionHandles
{
    // -------------------------------------------------------------------------
    // Handles
    // -------------------------------------------------------------------------

    public static IntegralConversionHandle GetHandle(
        in IntegralFormat input,
        in IntegralFormat output)
    {
        nint slot = output.ConversionPolicy.SpanHandleFactorySlot;
        if (ConversionPolicySlot.IsRefuse(slot))
        {
            return default;
        }

        if (ConversionPolicySlot.IsDefault(slot))
        {
            return StandardDelegateTable.Instance.GetDefaultHandle(input, output);
        }

        IntegralSpanConversionHandleFunc factory =
            ConversionPolicySlot.ResolveFactory<IntegralSpanConversionHandleFunc>(slot);
        return factory(input, output);
    }

    public static IntegralConversionHandle GetHandle(
        in IntegralFormat input,
        in IntegralFormat output,
        int blockCapacity)
    {
        _ = blockCapacity;
        return GetHandle(input, output);
    }

    public static IntegralConversionHandle GetInterleavedReaderHandle(
        in IntegralFormat input,
        in IntegralFormat output)
    {
        nint slot = output.ConversionPolicy.ReaderHandleFactorySlot;
        if (ConversionPolicySlot.IsRefuse(slot))
        {
            return default;
        }

        if (ConversionPolicySlot.IsDefault(slot))
        {
            return InterleavedDelegateTable.Instance.GetDefaultHandle(input, output);
        }

        InterleavedReaderConversionHandleFunc factory =
            ConversionPolicySlot.ResolveFactory<InterleavedReaderConversionHandleFunc>(slot);
        return factory(input, output);
    }

    public static IntegralConversionHandle GetInterleavedWriterHandle(
        in IntegralFormat input,
        in IntegralFormat output)
    {
        nint slot = output.ConversionPolicy.WriterHandleFactorySlot;
        if (ConversionPolicySlot.IsRefuse(slot))
        {
            return default;
        }

        if (ConversionPolicySlot.IsDefault(slot))
        {
            return InterleavedDelegateTable.Instance.GetDefaultHandle(input, output);
        }

        InterleavedWriterConversionHandleFunc factory =
            ConversionPolicySlot.ResolveFactory<InterleavedWriterConversionHandleFunc>(slot);
        return factory(input, output);
    }

    /// <summary>Alias for <see cref="GetInterleavedReaderHandle"/>.</summary>
    public static IntegralConversionHandle GetInterleavedReader(
        in IntegralFormat input,
        in IntegralFormat output)
        => GetInterleavedReaderHandle(input, output);

    /// <summary>Alias for <see cref="GetInterleavedWriterHandle"/>.</summary>
    public static IntegralConversionHandle GetInterleavedWriter(
        in IntegralFormat input,
        in IntegralFormat output)
        => GetInterleavedWriterHandle(input, output);

    // -------------------------------------------------------------------------
    // Contexts
    // -------------------------------------------------------------------------

    public static ConversionContext? GetContext(
        IntegralConversionHandle handle,
        in IntegralFormat input,
        in IntegralFormat output)
    {
        if (handle.IsNull)
        {
            return null;
        }

        nint slot = handle._contextFactory;
        if (slot == 0)
        {
            return InternalConversionDelegates.SpanContext_Default(handle, input, output);
        }

        Debug.Assert(ConversionPolicySlot.IsFactory(slot));
        var factory = (IntegralSpanConversionContextFunc)GCHandle.FromIntPtr(slot).Target!;
        return factory(input, output);
    }

    /// <summary>Obtain handle then default span context.</summary>
    public static ConversionContext? GetContext(
        in IntegralFormat input,
        in IntegralFormat output)
    {
        return GetContext(GetHandle(input, output), input, output);
    }

    public static ConversionContext? GetInterleavedReaderContext(
        IntegralConversionHandle handle,
        in IntegralFormat input,
        in IntegralFormat output,
        int inputBlockCapacity,
        int index)
    {
        if (handle.IsNull)
        {
            return null;
        }

        nint slot = handle._contextFactory;
        if (slot == 0)
        {
            return InternalConversionDelegates.ReaderContext_Default(
                handle, input, output, inputBlockCapacity, index);
        }

        Debug.Assert(ConversionPolicySlot.IsFactory(slot));
        var factory = (InterleavedReaderConversionContextFunc)GCHandle.FromIntPtr(slot).Target!;
        return factory(input, output, inputBlockCapacity, index);
    }

    /// <summary>
    /// Reader layout context (built-in). Prefer passing formats when a custom context factory is used.
    /// </summary>
    public static InterleavedReaderContext GetInterleavedReaderContext(
        IntegralConversionHandle handle,
        int inputBlockCapacity,
        int index)
    {
        return (InterleavedReaderContext)InternalConversionDelegates.ReaderContext_Default(
            handle,
            default,
            default,
            inputBlockCapacity,
            index)!;
    }

    public static ConversionContext? GetInterleavedWriterContext(
        IntegralConversionHandle handle,
        in IntegralFormat input,
        in IntegralFormat output,
        int outputBlockCapacity,
        int index)
    {
        if (handle.IsNull)
        {
            return null;
        }

        nint slot = handle._contextFactory;
        if (slot == 0)
        {
            return InternalConversionDelegates.WriterContext_Default(
                handle, input, output, outputBlockCapacity, index);
        }

        Debug.Assert(ConversionPolicySlot.IsFactory(slot));
        var factory = (InterleavedWriterConversionContextFunc)GCHandle.FromIntPtr(slot).Target!;
        return factory(input, output, outputBlockCapacity, index);
    }

    /// <summary>
    /// Writer layout context (built-in). Prefer passing formats when a custom context factory is used.
    /// </summary>
    public static InterleavedWriterContext GetInterleavedWriterContext(
        IntegralConversionHandle handle,
        int outputBlockCapacity,
        int index)
    {
        return (InterleavedWriterContext)InternalConversionDelegates.WriterContext_Default(
            handle,
            default,
            default,
            outputBlockCapacity,
            index)!;
    }
}
