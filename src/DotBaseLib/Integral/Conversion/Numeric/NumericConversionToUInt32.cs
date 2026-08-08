using DotBase.Integral.Conversion.Numeric.Defaults;

namespace DotBase.Integral.Conversion.Numeric;


public delegate uint ConvertUInt8ToUInt32_Delegate(byte value);
public delegate uint ConvertInt8ToUInt32_Delegate(sbyte value);
public delegate uint ConvertUInt16ToUInt32_Delegate(ushort value);
public delegate uint ConvertInt16ToUInt32_Delegate(short value);
public delegate uint ConvertUInt32ToUInt32_Delegate(uint value);
public delegate uint ConvertInt32ToUInt32_Delegate(int value);
public delegate uint ConvertUInt64ToUInt32_Delegate(ulong value);
public delegate uint ConvertInt64ToUInt32_Delegate(long value);
public delegate uint ConvertFloatToUInt32_Delegate(float value);
public delegate uint ConvertDoubleToUInt32_Delegate(double value);


public struct ConversionToUInt32Delegates
{
    public ConvertUInt8ToUInt32_Delegate ConvertUInt8ToUInt32;
    public ConvertInt8ToUInt32_Delegate ConvertInt8ToUInt32;
    public ConvertUInt16ToUInt32_Delegate ConvertUInt16ToUInt32;
    public ConvertInt16ToUInt32_Delegate ConvertInt16ToUInt32;
    public ConvertUInt32ToUInt32_Delegate ConvertUInt32ToUInt32;
    public ConvertInt32ToUInt32_Delegate ConvertInt32ToUInt32;
    public ConvertUInt64ToUInt32_Delegate ConvertUInt64ToUInt32;
    public ConvertInt64ToUInt32_Delegate ConvertInt64ToUInt32;
    public ConvertFloatToUInt32_Delegate ConvertFloatToUInt32;
    public ConvertDoubleToUInt32_Delegate ConvertDoubleToUInt32;

    public void ResetToDefaults()
    {
        ConvertUInt8ToUInt32 = DefaultConversionsToUInt32.ConvertUInt8ToUInt32_Default;
        ConvertInt8ToUInt32 = DefaultConversionsToUInt32.ConvertInt8ToUInt32_Default;
        ConvertUInt16ToUInt32 = DefaultConversionsToUInt32.ConvertUInt16ToUInt32_Default;
        ConvertInt16ToUInt32 = DefaultConversionsToUInt32.ConvertInt16ToUInt32_Default;
        ConvertUInt32ToUInt32 = DefaultConversionsToUInt32.ConvertUInt32ToUInt32_Default;
        ConvertInt32ToUInt32 = DefaultConversionsToUInt32.ConvertInt32ToUInt32_Default;
        ConvertUInt64ToUInt32 = DefaultConversionsToUInt32.ConvertUInt64ToUInt32_Default;
        ConvertInt64ToUInt32 = DefaultConversionsToUInt32.ConvertInt64ToUInt32_Default;
        ConvertFloatToUInt32 = DefaultConversionsToUInt32.ConvertFloatToUInt32_Default;
        ConvertDoubleToUInt32 = DefaultConversionsToUInt32.ConvertDoubleToUInt32_Default;
    }
}


public class NumericConversionToUInt32
{
    public ConvertUInt8ToUInt32_Delegate ConvertUInt8ToUInt32 { get; }
    public ConvertInt8ToUInt32_Delegate ConvertInt8ToUInt32 { get; }
    public ConvertUInt16ToUInt32_Delegate ConvertUInt16ToUInt32 { get; }
    public ConvertInt16ToUInt32_Delegate ConvertInt16ToUInt32 { get; }
    public ConvertUInt32ToUInt32_Delegate ConvertUInt32ToUInt32 { get; }
    public ConvertInt32ToUInt32_Delegate ConvertInt32ToUInt32 { get; }
    public ConvertUInt64ToUInt32_Delegate ConvertUInt64ToUInt32 { get; }
    public ConvertInt64ToUInt32_Delegate ConvertInt64ToUInt32 { get; }
    public ConvertFloatToUInt32_Delegate ConvertFloatToUInt32 { get; }
    public ConvertDoubleToUInt32_Delegate ConvertDoubleToUInt32 { get; }

    public NumericConversionToUInt32(ConversionToUInt32Delegates conversionDelegates)
    {
        ConvertUInt8ToUInt32 = conversionDelegates.ConvertUInt8ToUInt32;
        ConvertInt8ToUInt32 = conversionDelegates.ConvertInt8ToUInt32;
        ConvertUInt16ToUInt32 = conversionDelegates.ConvertUInt16ToUInt32;
        ConvertInt16ToUInt32 = conversionDelegates.ConvertInt16ToUInt32;
        ConvertUInt32ToUInt32 = conversionDelegates.ConvertUInt32ToUInt32;
        ConvertInt32ToUInt32 = conversionDelegates.ConvertInt32ToUInt32;
        ConvertUInt64ToUInt32 = conversionDelegates.ConvertUInt64ToUInt32;
        ConvertInt64ToUInt32 = conversionDelegates.ConvertInt64ToUInt32;
        ConvertFloatToUInt32 = conversionDelegates.ConvertFloatToUInt32;
        ConvertDoubleToUInt32 = conversionDelegates.ConvertDoubleToUInt32;
    }
}
