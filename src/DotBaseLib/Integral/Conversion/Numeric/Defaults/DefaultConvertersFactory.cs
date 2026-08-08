namespace DotBase.Integral.Conversion.Numeric.Defaults;


internal class DefaultConvertersFactory
    : INumericConvertersFactory
{
    public static INumericConvertersFactory Instance = new DefaultConvertersFactory();

    private readonly ConversionToUInt8Delegates _toUInt8 = new();
    private readonly ConversionToInt8Delegates _toInt8 = new();
    private readonly ConversionToUInt16Delegates _toUInt16 = new();
    private readonly ConversionToInt16Delegates _toInt16 = new();
    private readonly ConversionToUInt32Delegates _toUInt32 = new();
    private readonly ConversionToInt32Delegates _toInt32 = new();
    private readonly ConversionToUInt64Delegates _toUInt64 = new();
    private readonly ConversionToInt64Delegates _toInt64 = new();
    private readonly ConversionToFloatDelegates _toFloat = new();
    private readonly ConversionToDoubleDelegates _toDouble = new();

    internal DefaultConvertersFactory()
    {
        _toUInt8.ResetToDefaults();
        _toInt8.ResetToDefaults();
        _toUInt16.ResetToDefaults();
        _toInt16.ResetToDefaults();
        _toUInt32.ResetToDefaults();
        _toInt32.ResetToDefaults();
        _toUInt64.ResetToDefaults();
        _toInt64.ResetToDefaults();
        _toFloat.ResetToDefaults();
        _toDouble.ResetToDefaults();
    }

    NumericConversionToDouble INumericConvertersFactory.DoubleConversion()
        => new NumericConversionToDouble(_toDouble);

    NumericConversionToFloat INumericConvertersFactory.FloatConversion()
        => new NumericConversionToFloat(_toFloat);

    NumericConversionToInt16 INumericConvertersFactory.Int16Conversion()
        => new NumericConversionToInt16(_toInt16);

    NumericConversionToInt32 INumericConvertersFactory.Int32Conversion()
        => new NumericConversionToInt32(_toInt32);

    NumericConversionToInt64 INumericConvertersFactory.Int64Conversion()
        => new NumericConversionToInt64(_toInt64);

    NumericConversionToInt8 INumericConvertersFactory.Int8Conversion()
        => new NumericConversionToInt8(_toInt8);

    NumericConversionToUInt16 INumericConvertersFactory.UInt16Conversion()
        => new NumericConversionToUInt16(_toUInt16);

    NumericConversionToUInt32 INumericConvertersFactory.UInt32Conversion()
        => new NumericConversionToUInt32(_toUInt32);

    NumericConversionToUInt64 INumericConvertersFactory.UInt64Conversion()
        => new NumericConversionToUInt64(_toUInt64);

    NumericConversionToUInt8 INumericConvertersFactory.UInt8Conversion()
        => new NumericConversionToUInt8(_toUInt8);
}
