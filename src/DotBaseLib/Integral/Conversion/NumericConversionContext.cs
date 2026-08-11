using System.Runtime.InteropServices;

namespace DotBase.Integral.Conversion;


/// <summary>
/// Conversion context that can hold a resolved per-value scalar converter
/// (<see cref="NumericFunc"/>) from the handle's GCHandle root.
/// </summary>
public class NumericConversionContext
    : ConversionContext
{
    public override Delegate? NumericFunc => _numericFunc;

    private Delegate? _numericFunc;

    public NumericConversionContext(IntegralConversionHandle handle)
        : base(handle)
    {
    }

    /// <summary>
    /// Resolves <see cref="NumericFunc"/> from the handle when a scalar converter is present.
    /// </summary>
    public override bool AssureResolved()
    {
        if (_handle._numericFunc == 0)
        {
            _numericFunc = null;
            return true;
        }

        if (_numericFunc is not null)
        {
            return true;
        }

        try
        {
            object? target = GCHandle.FromIntPtr(_handle._numericFunc).Target;
            if (target is Delegate func)
            {
                _numericFunc = func;
                return true;
            }
        }
        catch
        {
            // Invalid or freed GCHandle.
        }

        return false;
    }
}
