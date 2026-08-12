using System.Diagnostics;
using DotBase.Integral.Conversion;
using DotBase.Integral.Conversion.Internal;

namespace DotBase.Integral;


/// <summary>
/// Unmanaged, process-local handle for a structural conversion function and an
/// optional managed scalar converter. Delegate tokens are borrowed from
/// process-lifetime conversion tables and policy registry entries.
/// </summary>
/// <remarks>
/// The borrowed tokens are meaningful only in the current process. Do not
/// persist this value or transfer it between processes.
/// </remarks>
public readonly struct IntegralConversionHandle
{
    /// <summary>
    /// <see langword="true"/> when a scalar converter or custom conversion policy
    /// requires a <see cref="ConversionContext"/>.
    /// </summary>
    public bool NeedsContext =>
        _numericConverter != 0 || _policy.RegistryIndex > 0;

    /// <summary>
    /// <see langword="true"/> when no conversion function is bound, including the default value.
    /// </summary>
    public bool IsNull => _func == 0;

    internal readonly nint _func;

    internal readonly nint _numericConverter;

    internal readonly IntegralConversionPolicy _policy;

    internal IntegralConversionHandle(
        nint func,
        nint numericConverter = 0,
        IntegralConversionPolicy policy = default)
    {
        _func = func;
        _numericConverter = numericConverter;
        _policy = policy;
    }

    internal IntegralSpanConversionFunc? ResolveFunc()
        => DelegateHandle.Resolve<IntegralSpanConversionFunc>(_func);

    internal Delegate? ResolveNumericConverter()
        => DelegateHandle.Resolve<Delegate>(_numericConverter);

    /// <summary>
    /// Performs contiguous conversion without a context. Valid only when
    /// <see cref="NeedsContext"/> is <see langword="false"/>.
    /// </summary>
    /// <remarks>
    /// When <see cref="NeedsContext"/> is <see langword="true"/>, obtain a
    /// context from <see cref="ConversionHandles"/> and call
    /// <see cref="ConversionContext.Convert"/> instead. Layout conversions also
    /// execute through their corresponding context.
    /// </remarks>
    public long Convert(
        in IntegralSpan input,
        in IntegralSpan output,
        long count)
    {
        if (IsNull)
        {
            return 0;
        }

        Debug.Assert(!NeedsContext, "Handle needs a ConversionContext.");
        if (NeedsContext)
        {
            return 0;
        }

        IntegralSpanConversionFunc? func = ResolveFunc();
        Debug.Assert(func is not null, "Structural conversion delegate could not be resolved.");
        return func is null ? 0 : func(input, output, count, null);
    }
}
