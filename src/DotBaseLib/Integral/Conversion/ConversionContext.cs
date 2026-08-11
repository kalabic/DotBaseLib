namespace DotBase.Integral.Conversion;


/// <summary>
/// Per-call conversion bag (layout, resolved scalar converter, etc.).
/// Context factories must create a <see cref="NumericConversionContext"/> (or subclass)
/// when the handle carries a scalar converter.
/// </summary>
public class ConversionContext
{
    public virtual Delegate? NumericFunc => null;

    protected readonly IntegralConversionHandle _handle;

    public ConversionContext(IntegralConversionHandle handle)
    {
        _handle = handle;
    }

    public long Convert(
        in IntegralSpan input,
        in IntegralSpan output,
        long count)
    {
        return _handle.Convert(input, output, count, this);
    }

    /// <summary>
    /// Ensures any handle-bound state required by kernels is ready.
    /// Base implementation succeeds; <see cref="NumericConversionContext"/> resolves
    /// <see cref="NumericConversionContext.NumericFunc"/> from the handle.
    /// </summary>
    public virtual bool AssureResolved() => true;
}
