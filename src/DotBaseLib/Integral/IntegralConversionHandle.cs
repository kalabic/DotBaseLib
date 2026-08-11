using System.Diagnostics;
using DotBase.Integral.Conversion;

namespace DotBase.Integral;


/// <summary>
/// Holds a structural conversion function, optional scalar converter GCHandle,
/// and optional context-factory GCHandle.
/// When a scalar converter is present, callers must obtain a context from the
/// factory path (at least a <see cref="NumericConversionContext"/>) and use
/// <see cref="Convert(in IntegralSpan, in IntegralSpan, long, ConversionContext)"/>.
/// </summary>
public readonly struct IntegralConversionHandle
{
    /// <summary>
    /// True when no conversion function is bound (including <c>default(IntegralConversionHandle)</c>).
    /// </summary>
    public bool IsNull => _func is null;

    internal readonly IntegralSpanConversionFunc? _func;

    /// <summary>
    /// GCHandle address for the resolved scalar converter, or zero when unused.
    /// Resolved into <see cref="NumericConversionContext"/> via
    /// <see cref="ConversionContext.AssureResolved"/> before invoke.
    /// </summary>
    internal readonly nint _numericFunc;

    /// <summary>
    /// GCHandle address for a context factory delegate, or zero for built-in context creation.
    /// </summary>
    internal readonly nint _contextFactory;

    internal IntegralConversionHandle(
        IntegralSpanConversionFunc? func,
        nint numericFunc = 0,
        nint contextFactory = 0)
    {
        _func = func;
        _numericFunc = numericFunc;
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Contiguous conversion with no context.
    /// Valid only when this handle has no scalar converter (<c>_numericFunc == 0</c>).
    /// </summary>
    public long Convert(
        in IntegralSpan input,
        in IntegralSpan output,
        long count)
    {
        IntegralSpanConversionFunc? func = _func;
        if (func is null)
        {
            return 0;
        }

        Debug.Assert(
            _numericFunc == 0,
            "Handle has a scalar converter; create a NumericConversionContext (via GetContext / factory) and call Convert(..., context).");

        return func(input, output, count, context: null);
    }

    /// <summary>
    /// Conversion with an explicit context (interleaved layout and/or resolved scalar converter).
    /// Factories must supply at least a <see cref="NumericConversionContext"/> when
    /// <see cref="_numericFunc"/> is non-zero.
    /// </summary>
    public long Convert(
        in IntegralSpan input,
        in IntegralSpan output,
        long count,
        ConversionContext context)
    {
        IntegralSpanConversionFunc? func = _func;
        if (func is null)
        {
            return 0;
        }

        if (!context.AssureResolved())
        {
            return 0;
        }

        return func(input, output, count, context);
    }
}
