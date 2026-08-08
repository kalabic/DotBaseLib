using DotBase.Integral.Conversion.Numeric.Defaults;

namespace DotBase.Integral.Conversion.Numeric;


public delegate sbyte ConvertUInt8ToInt8_Delegate(byte value);
public delegate sbyte ConvertInt8ToInt8_Delegate(sbyte value);
public delegate sbyte ConvertUInt16ToInt8_Delegate(ushort value);
public delegate sbyte ConvertInt16ToInt8_Delegate(short value);
public delegate sbyte ConvertUInt32ToInt8_Delegate(uint value);
public delegate sbyte ConvertInt32ToInt8_Delegate(int value);
public delegate sbyte ConvertUInt64ToInt8_Delegate(ulong value);
public delegate sbyte ConvertInt64ToInt8_Delegate(long value);
public delegate sbyte ConvertFloatToInt8_Delegate(float value);
public delegate sbyte ConvertDoubleToInt8_Delegate(double value);


public struct ConversionToInt8Delegates
{
    public ConvertUInt8ToInt8_Delegate ConvertUInt8ToInt8;
    public ConvertInt8ToInt8_Delegate ConvertInt8ToInt8;
    public ConvertUInt16ToInt8_Delegate ConvertUInt16ToInt8;
    public ConvertInt16ToInt8_Delegate ConvertInt16ToInt8;
    public ConvertUInt32ToInt8_Delegate ConvertUInt32ToInt8;
    public ConvertInt32ToInt8_Delegate ConvertInt32ToInt8;
    public ConvertUInt64ToInt8_Delegate ConvertUInt64ToInt8;
    public ConvertInt64ToInt8_Delegate ConvertInt64ToInt8;
    public ConvertFloatToInt8_Delegate ConvertFloatToInt8;
    public ConvertDoubleToInt8_Delegate ConvertDoubleToInt8;

    public void ResetToDefaults()
    {
        ConvertUInt8ToInt8 = DefaultConversionsToInt8.ConvertUInt8ToInt8_Default;
        ConvertInt8ToInt8 = DefaultConversionsToInt8.ConvertInt8ToInt8_Default;
        ConvertUInt16ToInt8 = DefaultConversionsToInt8.ConvertUInt16ToInt8_Default;
        ConvertInt16ToInt8 = DefaultConversionsToInt8.ConvertInt16ToInt8_Default;
        ConvertUInt32ToInt8 = DefaultConversionsToInt8.ConvertUInt32ToInt8_Default;
        ConvertInt32ToInt8 = DefaultConversionsToInt8.ConvertInt32ToInt8_Default;
        ConvertUInt64ToInt8 = DefaultConversionsToInt8.ConvertUInt64ToInt8_Default;
        ConvertInt64ToInt8 = DefaultConversionsToInt8.ConvertInt64ToInt8_Default;
        ConvertFloatToInt8 = DefaultConversionsToInt8.ConvertFloatToInt8_Default;
        ConvertDoubleToInt8 = DefaultConversionsToInt8.ConvertDoubleToInt8_Default;
    }
}


public class NumericConversionToInt8
{
    public ConvertUInt8ToInt8_Delegate ConvertUInt8ToInt8 { get; }
    public ConvertInt8ToInt8_Delegate ConvertInt8ToInt8 { get; }
    public ConvertUInt16ToInt8_Delegate ConvertUInt16ToInt8 { get; }
    public ConvertInt16ToInt8_Delegate ConvertInt16ToInt8 { get; }
    public ConvertUInt32ToInt8_Delegate ConvertUInt32ToInt8 { get; }
    public ConvertInt32ToInt8_Delegate ConvertInt32ToInt8 { get; }
    public ConvertUInt64ToInt8_Delegate ConvertUInt64ToInt8 { get; }
    public ConvertInt64ToInt8_Delegate ConvertInt64ToInt8 { get; }
    public ConvertFloatToInt8_Delegate ConvertFloatToInt8 { get; }
    public ConvertDoubleToInt8_Delegate ConvertDoubleToInt8 { get; }

    public NumericConversionToInt8(ConversionToInt8Delegates conversionDelegates)
    {
        ConvertUInt8ToInt8 = conversionDelegates.ConvertUInt8ToInt8;
        ConvertInt8ToInt8 = conversionDelegates.ConvertInt8ToInt8;
        ConvertUInt16ToInt8 = conversionDelegates.ConvertUInt16ToInt8;
        ConvertInt16ToInt8 = conversionDelegates.ConvertInt16ToInt8;
        ConvertUInt32ToInt8 = conversionDelegates.ConvertUInt32ToInt8;
        ConvertInt32ToInt8 = conversionDelegates.ConvertInt32ToInt8;
        ConvertUInt64ToInt8 = conversionDelegates.ConvertUInt64ToInt8;
        ConvertInt64ToInt8 = conversionDelegates.ConvertInt64ToInt8;
        ConvertFloatToInt8 = conversionDelegates.ConvertFloatToInt8;
        ConvertDoubleToInt8 = conversionDelegates.ConvertDoubleToInt8;
    }
}
