using DotBase.Integral.Conversion.Numeric.Defaults;

namespace DotBase.Integral.Conversion.Numeric;


public delegate long ConvertUInt8ToInt64_Delegate(byte value);
public delegate long ConvertInt8ToInt64_Delegate(sbyte value);
public delegate long ConvertUInt16ToInt64_Delegate(ushort value);
public delegate long ConvertInt16ToInt64_Delegate(short value);
public delegate long ConvertUInt32ToInt64_Delegate(uint value);
public delegate long ConvertInt32ToInt64_Delegate(int value);
public delegate long ConvertUInt64ToInt64_Delegate(ulong value);
public delegate long ConvertInt64ToInt64_Delegate(long value);
public delegate long ConvertFloatToInt64_Delegate(float value);
public delegate long ConvertDoubleToInt64_Delegate(double value);


public struct ConversionToInt64Delegates
{
    public ConvertUInt8ToInt64_Delegate ConvertUInt8ToInt64;
    public ConvertInt8ToInt64_Delegate ConvertInt8ToInt64;
    public ConvertUInt16ToInt64_Delegate ConvertUInt16ToInt64;
    public ConvertInt16ToInt64_Delegate ConvertInt16ToInt64;
    public ConvertUInt32ToInt64_Delegate ConvertUInt32ToInt64;
    public ConvertInt32ToInt64_Delegate ConvertInt32ToInt64;
    public ConvertUInt64ToInt64_Delegate ConvertUInt64ToInt64;
    public ConvertInt64ToInt64_Delegate ConvertInt64ToInt64;
    public ConvertFloatToInt64_Delegate ConvertFloatToInt64;
    public ConvertDoubleToInt64_Delegate ConvertDoubleToInt64;

    public void ResetToDefaults()
    {
        ConvertUInt8ToInt64 = DefaultConversionsToInt64.ConvertUInt8ToInt64_Default;
        ConvertInt8ToInt64 = DefaultConversionsToInt64.ConvertInt8ToInt64_Default;
        ConvertUInt16ToInt64 = DefaultConversionsToInt64.ConvertUInt16ToInt64_Default;
        ConvertInt16ToInt64 = DefaultConversionsToInt64.ConvertInt16ToInt64_Default;
        ConvertUInt32ToInt64 = DefaultConversionsToInt64.ConvertUInt32ToInt64_Default;
        ConvertInt32ToInt64 = DefaultConversionsToInt64.ConvertInt32ToInt64_Default;
        ConvertUInt64ToInt64 = DefaultConversionsToInt64.ConvertUInt64ToInt64_Default;
        ConvertInt64ToInt64 = DefaultConversionsToInt64.ConvertInt64ToInt64_Default;
        ConvertFloatToInt64 = DefaultConversionsToInt64.ConvertFloatToInt64_Default;
        ConvertDoubleToInt64 = DefaultConversionsToInt64.ConvertDoubleToInt64_Default;
    }
}


public class NumericConversionToInt64
{
    public ConvertUInt8ToInt64_Delegate ConvertUInt8ToInt64 { get; }
    public ConvertInt8ToInt64_Delegate ConvertInt8ToInt64 { get; }
    public ConvertUInt16ToInt64_Delegate ConvertUInt16ToInt64 { get; }
    public ConvertInt16ToInt64_Delegate ConvertInt16ToInt64 { get; }
    public ConvertUInt32ToInt64_Delegate ConvertUInt32ToInt64 { get; }
    public ConvertInt32ToInt64_Delegate ConvertInt32ToInt64 { get; }
    public ConvertUInt64ToInt64_Delegate ConvertUInt64ToInt64 { get; }
    public ConvertInt64ToInt64_Delegate ConvertInt64ToInt64 { get; }
    public ConvertFloatToInt64_Delegate ConvertFloatToInt64 { get; }
    public ConvertDoubleToInt64_Delegate ConvertDoubleToInt64 { get; }

    public NumericConversionToInt64(ConversionToInt64Delegates conversionDelegates)
    {
        ConvertUInt8ToInt64 = conversionDelegates.ConvertUInt8ToInt64;
        ConvertInt8ToInt64 = conversionDelegates.ConvertInt8ToInt64;
        ConvertUInt16ToInt64 = conversionDelegates.ConvertUInt16ToInt64;
        ConvertInt16ToInt64 = conversionDelegates.ConvertInt16ToInt64;
        ConvertUInt32ToInt64 = conversionDelegates.ConvertUInt32ToInt64;
        ConvertInt32ToInt64 = conversionDelegates.ConvertInt32ToInt64;
        ConvertUInt64ToInt64 = conversionDelegates.ConvertUInt64ToInt64;
        ConvertInt64ToInt64 = conversionDelegates.ConvertInt64ToInt64;
        ConvertFloatToInt64 = conversionDelegates.ConvertFloatToInt64;
        ConvertDoubleToInt64 = conversionDelegates.ConvertDoubleToInt64;
    }
}
