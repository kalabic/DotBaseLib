namespace DotBase.Integral.Conversion.Numeric;



public interface INumericConvertersFactory
{
    NumericConversionToUInt8 UInt8Conversion();

    NumericConversionToInt8 Int8Conversion();

    NumericConversionToUInt16 UInt16Conversion();

    NumericConversionToInt16 Int16Conversion();

    NumericConversionToUInt32 UInt32Conversion();

    NumericConversionToInt32 Int32Conversion();

    NumericConversionToUInt64 UInt64Conversion();

    NumericConversionToInt64 Int64Conversion();

    NumericConversionToFloat FloatConversion();

    NumericConversionToDouble DoubleConversion();
}


/// <summary>
/// Catalog of scalar numeric converters grouped by destination type.
/// Default implementations are specialized (non-generic) identity conversions
/// matching historical IntegralNumericConversion rules.
/// </summary>
public sealed class NumericConverters
{
    public readonly NumericConversionToUInt8   ToUInt8;
    public readonly NumericConversionToInt8    ToInt8;
    public readonly NumericConversionToUInt16  ToUInt16;
    public readonly NumericConversionToInt16   ToInt16;
    public readonly NumericConversionToUInt32  ToUInt32;
    public readonly NumericConversionToInt32   ToInt32;
    public readonly NumericConversionToUInt64  ToUInt64;
    public readonly NumericConversionToInt64   ToInt64;
    public readonly NumericConversionToFloat   ToFloat;
    public readonly NumericConversionToDouble  ToDouble;

    public NumericConverters(INumericConvertersFactory ncf)
    {
        ToUInt8 = ncf.UInt8Conversion();
        ToInt8 = ncf.Int8Conversion();
        ToUInt16 = ncf.UInt16Conversion();
        ToInt16 = ncf.Int16Conversion();
        ToUInt32 = ncf.UInt32Conversion();
        ToInt32 = ncf.Int32Conversion();
        ToUInt64 = ncf.UInt64Conversion();
        ToInt64 = ncf.Int64Conversion();
        ToFloat = ncf.FloatConversion();
        ToDouble = ncf.DoubleConversion();
    }
}
