using DotBase.Integral.Conversion.Numeric;

namespace DotBase.Integral.Conversion;


public interface IIntegralValueConverter
{
    public IntegralSpanConversionFunc? Func { get; }

    public NumericConverters? Converters { get; }
}
