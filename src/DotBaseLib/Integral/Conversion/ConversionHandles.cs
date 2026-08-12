using DotBase.Integral.Conversion.Internal;
using DotBase.Integral.Conversion.Internal.Interleaved;
using DotBase.Integral.Conversion.Internal.Standard;

namespace DotBase.Integral.Conversion;


/// <summary>
/// Public façade for obtaining conversion handles and contexts.
/// Policy dispatch (built-in / refuse / managed registry entry) lives here;
/// conversion tables stay policy-free.
/// </summary>
/// <remarks>
/// Context factories return <see langword="null"/> when no conversion handle is
/// available. Execute context-backed and layout conversions through the returned
/// <see cref="ConversionContext"/>.
/// </remarks>
public static class ConversionHandles
{
    // -------------------------------------------------------------------------
    // Handles
    // -------------------------------------------------------------------------

    public static IntegralConversionHandle GetHandle(
        in IntegralFormat input,
        in IntegralFormat output)
    {
        int registryIndex = output.ConversionPolicy.RegistryIndex;
        if (registryIndex == ConversionPolicyRegistry.Refuse)
        {
            return default;
        }

        if (registryIndex == ConversionPolicyRegistry.Default)
        {
            return StandardDelegateTable.Instance.GetDefaultHandle(input, output);
        }

        ConversionPolicyEntry entry =
            ConversionPolicyRegistry.Resolve(registryIndex);
        return entry.Kind switch
        {
            ConversionPolicyKind.ValueConverters =>
                StandardDelegateTable.Instance.GetCustomHandle(
                    input,
                    output,
                    entry.ValueConverters!),
            ConversionPolicyKind.StaticSpanFunction =>
                InternalConversionDelegates.CreateStaticSpanHandle(
                    entry.SpanFunctionHandle,
                    entry.ValueConverters,
                    input,
                    output),
            _ => throw new InvalidOperationException(
                $"Unsupported conversion policy kind '{entry.Kind}'."),
        };
    }

    public static IntegralConversionHandle GetHandle(
        in IntegralFormat input,
        in IntegralFormat output,
        int blockCapacity)
    {
        _ = blockCapacity;
        return GetHandle(input, output);
    }

    public static IntegralConversionHandle GetInterleavedHandle(
        in IntegralFormat input,
        in IntegralFormat output)
    {
        int registryIndex = output.ConversionPolicy.RegistryIndex;
        if (registryIndex == ConversionPolicyRegistry.Refuse)
        {
            return default;
        }

        if (registryIndex == ConversionPolicyRegistry.Default)
        {
            return InterleavedDelegateTable.Instance.GetDefaultHandle(input, output);
        }

        ConversionPolicyEntry entry =
            ConversionPolicyRegistry.Resolve(registryIndex);
        return entry.Kind == ConversionPolicyKind.ValueConverters
            ? InterleavedDelegateTable.Instance.GetCustomHandle(
                input,
                output,
                entry.ValueConverters!)
            : InterleavedDelegateTable.Instance.GetDefaultHandle(input, output);
    }

    public static IntegralConversionHandle GetPlanarHandle(
        in IntegralFormat input,
        in IntegralFormat output)
    {
        int registryIndex = output.ConversionPolicy.RegistryIndex;
        if (registryIndex == ConversionPolicyRegistry.Refuse)
        {
            return default;
        }

        if (registryIndex == ConversionPolicyRegistry.Default)
        {
            return StandardDelegateTable.Instance.GetDefaultHandle(input, output);
        }

        ConversionPolicyEntry entry =
            ConversionPolicyRegistry.Resolve(registryIndex);
        return entry.Kind == ConversionPolicyKind.ValueConverters
            ? StandardDelegateTable.Instance.GetCustomHandle(
                input,
                output,
                entry.ValueConverters!)
            : StandardDelegateTable.Instance.GetDefaultHandle(input, output);
    }

    /// <summary>Alias for <see cref="GetInterleavedHandle"/>.</summary>
    public static IntegralConversionHandle GetInterleaved(
        in IntegralFormat input,
        in IntegralFormat output)
        => GetInterleavedHandle(input, output);

    /// <summary>Alias for <see cref="GetPlanarHandle"/>.</summary>
    public static IntegralConversionHandle GetPlanar(
        in IntegralFormat input,
        in IntegralFormat output)
        => GetPlanarHandle(input, output);

    // -------------------------------------------------------------------------
    // Contexts
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates the default contiguous context bound to <paramref name="handle"/>.
    /// </summary>
    /// <returns>A matching context, or <see langword="null"/> for a null handle.</returns>
    public static ConversionContext? GetContext(
        IntegralConversionHandle handle)
    {
        return InternalConversionDelegates.SpanContext_Default(handle);
    }

    /// <summary>Obtains a contiguous handle, then creates its default context.</summary>
    /// <returns>A matching context, or <see langword="null"/> when conversion is unavailable.</returns>
    public static ConversionContext? GetContext(
        in IntegralFormat input,
        in IntegralFormat output)
    {
        return GetContext(GetHandle(input, output));
    }

    // -------------------------------------------------------------------------
    // Planar Contexts
    // -------------------------------------------------------------------------

    /// <summary>Obtain the planar handle, then create its reader context.</summary>
    public static PlanarReaderContext? GetPlanarReaderContext(
        in IntegralFormat input,
        in IntegralFormat output,
        long planeCapacity,
        int blockCapacity,
        int inputPlaneIndex)
    {
        IntegralConversionHandle handle = GetPlanarHandle(input, output);
        return InternalConversionDelegates.PlanarReaderContext_Default(
            handle, planeCapacity, blockCapacity, inputPlaneIndex);
    }

    /// <summary>
    /// Planar reader layout context using built-in context construction.
    /// Call <see cref="PlanarReaderContext.Convert"/> so the selected plane is sliced.
    /// </summary>
    public static PlanarReaderContext? GetPlanarReaderContext(
        IntegralConversionHandle handle,
        long planeCapacity,
        int blockCapacity,
        int inputPlaneIndex)
    {
        return InternalConversionDelegates.PlanarReaderContext_Default(
            handle, planeCapacity, blockCapacity, inputPlaneIndex);
    }

    /// <summary>Obtain the planar handle, then create its writer context.</summary>
    public static PlanarWriterContext? GetPlanarWriterContext(
        in IntegralFormat input,
        in IntegralFormat output,
        long planeCapacity,
        int blockCapacity,
        int outputPlaneIndex)
    {
        IntegralConversionHandle handle = GetPlanarHandle(input, output);
        return InternalConversionDelegates.PlanarWriterContext_Default(
            handle, planeCapacity, blockCapacity, outputPlaneIndex);
    }

    /// <summary>
    /// Planar writer layout context using built-in context construction.
    /// Call <see cref="PlanarWriterContext.Convert"/> so the selected plane is sliced.
    /// </summary>
    public static PlanarWriterContext? GetPlanarWriterContext(
        IntegralConversionHandle handle,
        long planeCapacity,
        int blockCapacity,
        int outputPlaneIndex)
    {
        return InternalConversionDelegates.PlanarWriterContext_Default(
            handle, planeCapacity, blockCapacity, outputPlaneIndex);
    }

    /// <summary>Obtain the planar handle, then create its transfer context.</summary>
    public static PlanarTransferContext? GetPlanarTransferContext(
        in IntegralFormat input,
        in IntegralFormat output,
        long planeCapacity,
        int blockCapacity,
        int inputPlaneIndex,
        int outputPlaneIndex)
    {
        IntegralConversionHandle handle = GetPlanarHandle(input, output);
        return InternalConversionDelegates.PlanarTransferContext_Default(
            handle, planeCapacity, blockCapacity, inputPlaneIndex, outputPlaneIndex);
    }

    /// <summary>
    /// Planar transfer layout context using built-in context construction.
    /// Call <see cref="PlanarTransferContext.Convert"/> so the selected planes are sliced.
    /// </summary>
    public static PlanarTransferContext? GetPlanarTransferContext(
        IntegralConversionHandle handle,
        long planeCapacity,
        int blockCapacity,
        int inputPlaneIndex,
        int outputPlaneIndex)
    {
        return InternalConversionDelegates.PlanarTransferContext_Default(
            handle, planeCapacity, blockCapacity, inputPlaneIndex, outputPlaneIndex);
    }

    // -------------------------------------------------------------------------
    // Interleaved Contexts
    // -------------------------------------------------------------------------

    /// <summary>Obtain the interleaved handle, then create its reader context.</summary>
    public static InterleavedReaderContext? GetInterleavedReaderContext(
        in IntegralFormat input,
        in IntegralFormat output,
        int inputBlockCapacity,
        int index)
    {
        IntegralConversionHandle handle = GetInterleavedHandle(input, output);
        return InternalConversionDelegates.InterleavedReaderContext_Default(
            handle, inputBlockCapacity, index);
    }

    /// <summary>
    /// Interleaved reader layout context using built-in context construction.
    /// </summary>
    public static InterleavedReaderContext? GetInterleavedReaderContext(
        IntegralConversionHandle handle,
        int inputBlockCapacity,
        int index)
    {
        return InternalConversionDelegates.InterleavedReaderContext_Default(
            handle, inputBlockCapacity, index);
    }

    /// <summary>Obtain the interleaved handle, then create its writer context.</summary>
    public static InterleavedWriterContext? GetInterleavedWriterContext(
        in IntegralFormat input,
        in IntegralFormat output,
        int outputBlockCapacity,
        int index)
    {
        IntegralConversionHandle handle = GetInterleavedHandle(input, output);
        return InternalConversionDelegates.InterleavedWriterContext_Default(
            handle, outputBlockCapacity, index);
    }

    /// <summary>
    /// Interleaved writer layout context using built-in context construction.
    /// </summary>
    public static InterleavedWriterContext? GetInterleavedWriterContext(
        IntegralConversionHandle handle,
        int outputBlockCapacity,
        int index)
    {
        return InternalConversionDelegates.InterleavedWriterContext_Default(
            handle, outputBlockCapacity, index);
    }

    /// <summary>Obtain the interleaved handle, then create its transfer context.</summary>
    public static InterleavedTransferContext? GetInterleavedTransferContext(
        in IntegralFormat input,
        in IntegralFormat output,
        int inputBlockCapacity,
        int inputValueIndex,
        int outputBlockCapacity,
        int outputValueIndex)
    {
        IntegralConversionHandle handle = GetInterleavedHandle(input, output);
        return InternalConversionDelegates.InterleavedTransferContext_Default(
            handle, inputBlockCapacity, inputValueIndex, outputBlockCapacity, outputValueIndex);
    }

    /// <summary>
    /// Interleaved transfer layout context using built-in context construction.
    /// </summary>
    public static InterleavedTransferContext? GetInterleavedTransferContext(
        IntegralConversionHandle handle,
        int inputBlockCapacity,
        int inputValueIndex,
        int outputBlockCapacity,
        int outputValueIndex)
    {
        return InternalConversionDelegates.InterleavedTransferContext_Default(
            handle, inputBlockCapacity, inputValueIndex, outputBlockCapacity, outputValueIndex);
    }
}
