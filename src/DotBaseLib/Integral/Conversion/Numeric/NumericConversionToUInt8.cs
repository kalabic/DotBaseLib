using DotBase.Integral.Conversion.Numeric.Defaults;

namespace DotBase.Integral.Conversion.Numeric;


public delegate byte ConvertUInt8ToUInt8_Delegate(byte value);
public delegate byte ConvertInt8ToUInt8_Delegate(sbyte value);
public delegate byte ConvertUInt16ToUInt8_Delegate(ushort value);
public delegate byte ConvertInt16ToUInt8_Delegate(short value);
public delegate byte ConvertUInt32ToUInt8_Delegate(uint value);
public delegate byte ConvertInt32ToUInt8_Delegate(int value);
public delegate byte ConvertUInt64ToUInt8_Delegate(ulong value);
public delegate byte ConvertInt64ToUInt8_Delegate(long value);
public delegate byte ConvertFloatToUInt8_Delegate(float value);
public delegate byte ConvertDoubleToUInt8_Delegate(double value);


public struct ConversionToUInt8Delegates
{
    public ConvertUInt8ToUInt8_Delegate ConvertUInt8ToUInt8;
    public ConvertInt8ToUInt8_Delegate ConvertInt8ToUInt8;
    public ConvertUInt16ToUInt8_Delegate ConvertUInt16ToUInt8;
    public ConvertInt16ToUInt8_Delegate ConvertInt16ToUInt8;
    public ConvertUInt32ToUInt8_Delegate ConvertUInt32ToUInt8;
    public ConvertInt32ToUInt8_Delegate ConvertInt32ToUInt8;
    public ConvertUInt64ToUInt8_Delegate ConvertUInt64ToUInt8;
    public ConvertInt64ToUInt8_Delegate ConvertInt64ToUInt8;
    public ConvertFloatToUInt8_Delegate ConvertFloatToUInt8;
    public ConvertDoubleToUInt8_Delegate ConvertDoubleToUInt8;

    public void ResetToDefaults()
    {
        ConvertUInt8ToUInt8 = DefaultConversionsToUInt8.ConvertUInt8ToUInt8_Default;
        ConvertInt8ToUInt8 = DefaultConversionsToUInt8.ConvertInt8ToUInt8_Default;
        ConvertUInt16ToUInt8 = DefaultConversionsToUInt8.ConvertUInt16ToUInt8_Default;
        ConvertInt16ToUInt8 = DefaultConversionsToUInt8.ConvertInt16ToUInt8_Default;
        ConvertUInt32ToUInt8 = DefaultConversionsToUInt8.ConvertUInt32ToUInt8_Default;
        ConvertInt32ToUInt8 = DefaultConversionsToUInt8.ConvertInt32ToUInt8_Default;
        ConvertUInt64ToUInt8 = DefaultConversionsToUInt8.ConvertUInt64ToUInt8_Default;
        ConvertInt64ToUInt8 = DefaultConversionsToUInt8.ConvertInt64ToUInt8_Default;
        ConvertFloatToUInt8 = DefaultConversionsToUInt8.ConvertFloatToUInt8_Default;
        ConvertDoubleToUInt8 = DefaultConversionsToUInt8.ConvertDoubleToUInt8_Default;
    }
}


public class NumericConversionToUInt8
{
    public ConvertUInt8ToUInt8_Delegate ConvertUInt8ToUInt8 { get; }
    public ConvertInt8ToUInt8_Delegate ConvertInt8ToUInt8 { get; }
    public ConvertUInt16ToUInt8_Delegate ConvertUInt16ToUInt8 { get; }
    public ConvertInt16ToUInt8_Delegate ConvertInt16ToUInt8 { get; }
    public ConvertUInt32ToUInt8_Delegate ConvertUInt32ToUInt8 { get; }
    public ConvertInt32ToUInt8_Delegate ConvertInt32ToUInt8 { get; }
    public ConvertUInt64ToUInt8_Delegate ConvertUInt64ToUInt8 { get; }
    public ConvertInt64ToUInt8_Delegate ConvertInt64ToUInt8 { get; }
    public ConvertFloatToUInt8_Delegate ConvertFloatToUInt8 { get; }
    public ConvertDoubleToUInt8_Delegate ConvertDoubleToUInt8 { get; }

    public NumericConversionToUInt8(ConversionToUInt8Delegates conversionDelegates)
    {
        ConvertUInt8ToUInt8 = conversionDelegates.ConvertUInt8ToUInt8;
        ConvertInt8ToUInt8 = conversionDelegates.ConvertInt8ToUInt8;
        ConvertUInt16ToUInt8 = conversionDelegates.ConvertUInt16ToUInt8;
        ConvertInt16ToUInt8 = conversionDelegates.ConvertInt16ToUInt8;
        ConvertUInt32ToUInt8 = conversionDelegates.ConvertUInt32ToUInt8;
        ConvertInt32ToUInt8 = conversionDelegates.ConvertInt32ToUInt8;
        ConvertUInt64ToUInt8 = conversionDelegates.ConvertUInt64ToUInt8;
        ConvertInt64ToUInt8 = conversionDelegates.ConvertInt64ToUInt8;
        ConvertFloatToUInt8 = conversionDelegates.ConvertFloatToUInt8;
        ConvertDoubleToUInt8 = conversionDelegates.ConvertDoubleToUInt8;
    }
}
