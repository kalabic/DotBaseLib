using DotBase.Integral.Conversion.Numeric.Defaults;

namespace DotBase.Integral.Conversion.Numeric;


public delegate ulong ConvertUInt8ToUInt64_Delegate(byte value);
public delegate ulong ConvertInt8ToUInt64_Delegate(sbyte value);
public delegate ulong ConvertUInt16ToUInt64_Delegate(ushort value);
public delegate ulong ConvertInt16ToUInt64_Delegate(short value);
public delegate ulong ConvertUInt32ToUInt64_Delegate(uint value);
public delegate ulong ConvertInt32ToUInt64_Delegate(int value);
public delegate ulong ConvertUInt64ToUInt64_Delegate(ulong value);
public delegate ulong ConvertInt64ToUInt64_Delegate(long value);
public delegate ulong ConvertFloatToUInt64_Delegate(float value);
public delegate ulong ConvertDoubleToUInt64_Delegate(double value);


public struct ConversionToUInt64Delegates
{
    public ConvertUInt8ToUInt64_Delegate ConvertUInt8ToUInt64;
    public ConvertInt8ToUInt64_Delegate ConvertInt8ToUInt64;
    public ConvertUInt16ToUInt64_Delegate ConvertUInt16ToUInt64;
    public ConvertInt16ToUInt64_Delegate ConvertInt16ToUInt64;
    public ConvertUInt32ToUInt64_Delegate ConvertUInt32ToUInt64;
    public ConvertInt32ToUInt64_Delegate ConvertInt32ToUInt64;
    public ConvertUInt64ToUInt64_Delegate ConvertUInt64ToUInt64;
    public ConvertInt64ToUInt64_Delegate ConvertInt64ToUInt64;
    public ConvertFloatToUInt64_Delegate ConvertFloatToUInt64;
    public ConvertDoubleToUInt64_Delegate ConvertDoubleToUInt64;

    public void ResetToDefaults()
    {
        ConvertUInt8ToUInt64 = DefaultConversionsToUInt64.ConvertUInt8ToUInt64_Default;
        ConvertInt8ToUInt64 = DefaultConversionsToUInt64.ConvertInt8ToUInt64_Default;
        ConvertUInt16ToUInt64 = DefaultConversionsToUInt64.ConvertUInt16ToUInt64_Default;
        ConvertInt16ToUInt64 = DefaultConversionsToUInt64.ConvertInt16ToUInt64_Default;
        ConvertUInt32ToUInt64 = DefaultConversionsToUInt64.ConvertUInt32ToUInt64_Default;
        ConvertInt32ToUInt64 = DefaultConversionsToUInt64.ConvertInt32ToUInt64_Default;
        ConvertUInt64ToUInt64 = DefaultConversionsToUInt64.ConvertUInt64ToUInt64_Default;
        ConvertInt64ToUInt64 = DefaultConversionsToUInt64.ConvertInt64ToUInt64_Default;
        ConvertFloatToUInt64 = DefaultConversionsToUInt64.ConvertFloatToUInt64_Default;
        ConvertDoubleToUInt64 = DefaultConversionsToUInt64.ConvertDoubleToUInt64_Default;
    }
}


public class NumericConversionToUInt64
{
    public ConvertUInt8ToUInt64_Delegate ConvertUInt8ToUInt64 { get; }
    public ConvertInt8ToUInt64_Delegate ConvertInt8ToUInt64 { get; }
    public ConvertUInt16ToUInt64_Delegate ConvertUInt16ToUInt64 { get; }
    public ConvertInt16ToUInt64_Delegate ConvertInt16ToUInt64 { get; }
    public ConvertUInt32ToUInt64_Delegate ConvertUInt32ToUInt64 { get; }
    public ConvertInt32ToUInt64_Delegate ConvertInt32ToUInt64 { get; }
    public ConvertUInt64ToUInt64_Delegate ConvertUInt64ToUInt64 { get; }
    public ConvertInt64ToUInt64_Delegate ConvertInt64ToUInt64 { get; }
    public ConvertFloatToUInt64_Delegate ConvertFloatToUInt64 { get; }
    public ConvertDoubleToUInt64_Delegate ConvertDoubleToUInt64 { get; }

    public NumericConversionToUInt64(ConversionToUInt64Delegates conversionDelegates)
    {
        ConvertUInt8ToUInt64 = conversionDelegates.ConvertUInt8ToUInt64;
        ConvertInt8ToUInt64 = conversionDelegates.ConvertInt8ToUInt64;
        ConvertUInt16ToUInt64 = conversionDelegates.ConvertUInt16ToUInt64;
        ConvertInt16ToUInt64 = conversionDelegates.ConvertInt16ToUInt64;
        ConvertUInt32ToUInt64 = conversionDelegates.ConvertUInt32ToUInt64;
        ConvertInt32ToUInt64 = conversionDelegates.ConvertInt32ToUInt64;
        ConvertUInt64ToUInt64 = conversionDelegates.ConvertUInt64ToUInt64;
        ConvertInt64ToUInt64 = conversionDelegates.ConvertInt64ToUInt64;
        ConvertFloatToUInt64 = conversionDelegates.ConvertFloatToUInt64;
        ConvertDoubleToUInt64 = conversionDelegates.ConvertDoubleToUInt64;
    }
}
