namespace DotBase.Integral.Conversion;


/// <summary>
/// Conversion context exposing the managed per-value scalar converter carried by
/// its conversion handle.
/// </summary>
public class NumericConversionContext
    : ConversionContext
{
    public override Delegate? NumericFunc => _numericFunc;

    private readonly bool _hasNumericFunc;

    private readonly Delegate? _numericFunc;

    public NumericConversionContext(IntegralConversionHandle handle)
        : base(handle)
    {
        _hasNumericFunc = (handle._numericConverter != 0);
        _numericFunc = handle.ResolveNumericConverter();
    }

    public override bool AssureResolved()
        => (!_hasNumericFunc || _numericFunc is not null) && base.AssureResolved();
}
