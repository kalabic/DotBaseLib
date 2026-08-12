using System.Diagnostics;

namespace DotBase.Integral.Conversion;


/// <summary>
/// Per-call conversion state and execution surface bound to a conversion handle.
/// Context factories must create a <see cref="NumericConversionContext"/> (or subclass)
/// when the handle carries a scalar converter.
/// </summary>
public class ConversionContext
{
    public virtual Delegate? NumericFunc { get { return null; } }

    internal readonly IntegralSpanConversionFunc? _func;

    public ConversionContext(IntegralConversionHandle handle)
    {
        _func = handle.ResolveFunc();
    }

    /// <summary>
    /// Executes the structural conversion function bound by this context.
    /// Layout subclasses may first reshape or slice the input and output views.
    /// </summary>
    public virtual long Convert(
        in IntegralSpan input,
        in IntegralSpan output,
        long count)
    {
        bool resolved = AssureResolved();
        Debug.Assert(resolved, "Context not resolved.");
        if (!resolved)
        {
            return 0;
        }

        return _func!(input, output, count, this);
    }

    /// <summary>
    /// Ensures any handle-bound state required by kernels is ready.
    /// Base implementation succeeds; <see cref="NumericConversionContext"/> resolves
    /// <see cref="NumericConversionContext.NumericFunc"/> from the handle.
    /// </summary>
    public virtual bool AssureResolved() => _func is not null;
}
