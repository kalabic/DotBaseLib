using DotBase.Integral.Conversion.Numeric.Defaults;

namespace DotBase.Integral.Conversion.Numeric;


public delegate short ConvertUInt8ToInt16_Delegate(byte value);
public delegate short ConvertInt8ToInt16_Delegate(sbyte value);
public delegate short ConvertUInt16ToInt16_Delegate(ushort value);
public delegate short ConvertInt16ToInt16_Delegate(short value);
public delegate short ConvertUInt32ToInt16_Delegate(uint value);
public delegate short ConvertInt32ToInt16_Delegate(int value);
public delegate short ConvertUInt64ToInt16_Delegate(ulong value);
public delegate short ConvertInt64ToInt16_Delegate(long value);
public delegate short ConvertFloatToInt16_Delegate(float value);
public delegate short ConvertDoubleToInt16_Delegate(double value);


public struct ConversionToInt16Delegates
{
    public ConvertUInt8ToInt16_Delegate ConvertUInt8ToInt16;
    public ConvertInt8ToInt16_Delegate ConvertInt8ToInt16;
    public ConvertUInt16ToInt16_Delegate ConvertUInt16ToInt16;
    public ConvertInt16ToInt16_Delegate ConvertInt16ToInt16;
    public ConvertUInt32ToInt16_Delegate ConvertUInt32ToInt16;
    public ConvertInt32ToInt16_Delegate ConvertInt32ToInt16;
    public ConvertUInt64ToInt16_Delegate ConvertUInt64ToInt16;
    public ConvertInt64ToInt16_Delegate ConvertInt64ToInt16;
    public ConvertFloatToInt16_Delegate ConvertFloatToInt16;
    public ConvertDoubleToInt16_Delegate ConvertDoubleToInt16;

    public void ResetToDefaults()
    {
        ConvertUInt8ToInt16 = DefaultConversionsToInt16.ConvertUInt8ToInt16_Default;
        ConvertInt8ToInt16 = DefaultConversionsToInt16.ConvertInt8ToInt16_Default;
        ConvertUInt16ToInt16 = DefaultConversionsToInt16.ConvertUInt16ToInt16_Default;
        ConvertInt16ToInt16 = DefaultConversionsToInt16.ConvertInt16ToInt16_Default;
        ConvertUInt32ToInt16 = DefaultConversionsToInt16.ConvertUInt32ToInt16_Default;
        ConvertInt32ToInt16 = DefaultConversionsToInt16.ConvertInt32ToInt16_Default;
        ConvertUInt64ToInt16 = DefaultConversionsToInt16.ConvertUInt64ToInt16_Default;
        ConvertInt64ToInt16 = DefaultConversionsToInt16.ConvertInt64ToInt16_Default;
        ConvertFloatToInt16 = DefaultConversionsToInt16.ConvertFloatToInt16_Default;
        ConvertDoubleToInt16 = DefaultConversionsToInt16.ConvertDoubleToInt16_Default;
    }
}


public class NumericConversionToInt16
{
    public ConvertUInt8ToInt16_Delegate ConvertUInt8ToInt16 { get; }
    public ConvertInt8ToInt16_Delegate ConvertInt8ToInt16 { get; }
    public ConvertUInt16ToInt16_Delegate ConvertUInt16ToInt16 { get; }
    public ConvertInt16ToInt16_Delegate ConvertInt16ToInt16 { get; }
    public ConvertUInt32ToInt16_Delegate ConvertUInt32ToInt16 { get; }
    public ConvertInt32ToInt16_Delegate ConvertInt32ToInt16 { get; }
    public ConvertUInt64ToInt16_Delegate ConvertUInt64ToInt16 { get; }
    public ConvertInt64ToInt16_Delegate ConvertInt64ToInt16 { get; }
    public ConvertFloatToInt16_Delegate ConvertFloatToInt16 { get; }
    public ConvertDoubleToInt16_Delegate ConvertDoubleToInt16 { get; }

    public NumericConversionToInt16(ConversionToInt16Delegates conversionDelegates)
    {
        ConvertUInt8ToInt16 = conversionDelegates.ConvertUInt8ToInt16;
        ConvertInt8ToInt16 = conversionDelegates.ConvertInt8ToInt16;
        ConvertUInt16ToInt16 = conversionDelegates.ConvertUInt16ToInt16;
        ConvertInt16ToInt16 = conversionDelegates.ConvertInt16ToInt16;
        ConvertUInt32ToInt16 = conversionDelegates.ConvertUInt32ToInt16;
        ConvertInt32ToInt16 = conversionDelegates.ConvertInt32ToInt16;
        ConvertUInt64ToInt16 = conversionDelegates.ConvertUInt64ToInt16;
        ConvertInt64ToInt16 = conversionDelegates.ConvertInt64ToInt16;
        ConvertFloatToInt16 = conversionDelegates.ConvertFloatToInt16;
        ConvertDoubleToInt16 = conversionDelegates.ConvertDoubleToInt16;
    }
}
