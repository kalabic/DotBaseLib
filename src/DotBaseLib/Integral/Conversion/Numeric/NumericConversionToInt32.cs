using DotBase.Integral.Conversion.Numeric.Defaults;

namespace DotBase.Integral.Conversion.Numeric;


public delegate int ConvertUInt8ToInt32_Delegate(byte value);
public delegate int ConvertInt8ToInt32_Delegate(sbyte value);
public delegate int ConvertUInt16ToInt32_Delegate(ushort value);
public delegate int ConvertInt16ToInt32_Delegate(short value);
public delegate int ConvertUInt32ToInt32_Delegate(uint value);
public delegate int ConvertInt32ToInt32_Delegate(int value);
public delegate int ConvertUInt64ToInt32_Delegate(ulong value);
public delegate int ConvertInt64ToInt32_Delegate(long value);
public delegate int ConvertFloatToInt32_Delegate(float value);
public delegate int ConvertDoubleToInt32_Delegate(double value);


public struct ConversionToInt32Delegates
{
    public ConvertUInt8ToInt32_Delegate ConvertUInt8ToInt32;
    public ConvertInt8ToInt32_Delegate ConvertInt8ToInt32;
    public ConvertUInt16ToInt32_Delegate ConvertUInt16ToInt32;
    public ConvertInt16ToInt32_Delegate ConvertInt16ToInt32;
    public ConvertUInt32ToInt32_Delegate ConvertUInt32ToInt32;
    public ConvertInt32ToInt32_Delegate ConvertInt32ToInt32;
    public ConvertUInt64ToInt32_Delegate ConvertUInt64ToInt32;
    public ConvertInt64ToInt32_Delegate ConvertInt64ToInt32;
    public ConvertFloatToInt32_Delegate ConvertFloatToInt32;
    public ConvertDoubleToInt32_Delegate ConvertDoubleToInt32;

    public void ResetToDefaults()
    {
        ConvertUInt8ToInt32 = DefaultConversionsToInt32.ConvertUInt8ToInt32_Default;
        ConvertInt8ToInt32 = DefaultConversionsToInt32.ConvertInt8ToInt32_Default;
        ConvertUInt16ToInt32 = DefaultConversionsToInt32.ConvertUInt16ToInt32_Default;
        ConvertInt16ToInt32 = DefaultConversionsToInt32.ConvertInt16ToInt32_Default;
        ConvertUInt32ToInt32 = DefaultConversionsToInt32.ConvertUInt32ToInt32_Default;
        ConvertInt32ToInt32 = DefaultConversionsToInt32.ConvertInt32ToInt32_Default;
        ConvertUInt64ToInt32 = DefaultConversionsToInt32.ConvertUInt64ToInt32_Default;
        ConvertInt64ToInt32 = DefaultConversionsToInt32.ConvertInt64ToInt32_Default;
        ConvertFloatToInt32 = DefaultConversionsToInt32.ConvertFloatToInt32_Default;
        ConvertDoubleToInt32 = DefaultConversionsToInt32.ConvertDoubleToInt32_Default;
    }
}


public class NumericConversionToInt32
{
    public ConvertUInt8ToInt32_Delegate ConvertUInt8ToInt32 { get; }
    public ConvertInt8ToInt32_Delegate ConvertInt8ToInt32 { get; }
    public ConvertUInt16ToInt32_Delegate ConvertUInt16ToInt32 { get; }
    public ConvertInt16ToInt32_Delegate ConvertInt16ToInt32 { get; }
    public ConvertUInt32ToInt32_Delegate ConvertUInt32ToInt32 { get; }
    public ConvertInt32ToInt32_Delegate ConvertInt32ToInt32 { get; }
    public ConvertUInt64ToInt32_Delegate ConvertUInt64ToInt32 { get; }
    public ConvertInt64ToInt32_Delegate ConvertInt64ToInt32 { get; }
    public ConvertFloatToInt32_Delegate ConvertFloatToInt32 { get; }
    public ConvertDoubleToInt32_Delegate ConvertDoubleToInt32 { get; }

    public NumericConversionToInt32(ConversionToInt32Delegates conversionDelegates)
    {
        ConvertUInt8ToInt32 = conversionDelegates.ConvertUInt8ToInt32;
        ConvertInt8ToInt32 = conversionDelegates.ConvertInt8ToInt32;
        ConvertUInt16ToInt32 = conversionDelegates.ConvertUInt16ToInt32;
        ConvertInt16ToInt32 = conversionDelegates.ConvertInt16ToInt32;
        ConvertUInt32ToInt32 = conversionDelegates.ConvertUInt32ToInt32;
        ConvertInt32ToInt32 = conversionDelegates.ConvertInt32ToInt32;
        ConvertUInt64ToInt32 = conversionDelegates.ConvertUInt64ToInt32;
        ConvertInt64ToInt32 = conversionDelegates.ConvertInt64ToInt32;
        ConvertFloatToInt32 = conversionDelegates.ConvertFloatToInt32;
        ConvertDoubleToInt32 = conversionDelegates.ConvertDoubleToInt32;
    }
}
