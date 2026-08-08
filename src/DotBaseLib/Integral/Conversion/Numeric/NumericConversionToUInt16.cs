using DotBase.Integral.Conversion.Numeric.Defaults;

namespace DotBase.Integral.Conversion.Numeric;


public delegate ushort ConvertUInt8ToUInt16_Delegate(byte value);
public delegate ushort ConvertInt8ToUInt16_Delegate(sbyte value);
public delegate ushort ConvertUInt16ToUInt16_Delegate(ushort value);
public delegate ushort ConvertInt16ToUInt16_Delegate(short value);
public delegate ushort ConvertUInt32ToUInt16_Delegate(uint value);
public delegate ushort ConvertInt32ToUInt16_Delegate(int value);
public delegate ushort ConvertUInt64ToUInt16_Delegate(ulong value);
public delegate ushort ConvertInt64ToUInt16_Delegate(long value);
public delegate ushort ConvertFloatToUInt16_Delegate(float value);
public delegate ushort ConvertDoubleToUInt16_Delegate(double value);


public struct ConversionToUInt16Delegates
{
    public ConvertUInt8ToUInt16_Delegate ConvertUInt8ToUInt16;
    public ConvertInt8ToUInt16_Delegate ConvertInt8ToUInt16;
    public ConvertUInt16ToUInt16_Delegate ConvertUInt16ToUInt16;
    public ConvertInt16ToUInt16_Delegate ConvertInt16ToUInt16;
    public ConvertUInt32ToUInt16_Delegate ConvertUInt32ToUInt16;
    public ConvertInt32ToUInt16_Delegate ConvertInt32ToUInt16;
    public ConvertUInt64ToUInt16_Delegate ConvertUInt64ToUInt16;
    public ConvertInt64ToUInt16_Delegate ConvertInt64ToUInt16;
    public ConvertFloatToUInt16_Delegate ConvertFloatToUInt16;
    public ConvertDoubleToUInt16_Delegate ConvertDoubleToUInt16;

    public void ResetToDefaults()
    {
        ConvertUInt8ToUInt16 = DefaultConversionsToUInt16.ConvertUInt8ToUInt16_Default;
        ConvertInt8ToUInt16 = DefaultConversionsToUInt16.ConvertInt8ToUInt16_Default;
        ConvertUInt16ToUInt16 = DefaultConversionsToUInt16.ConvertUInt16ToUInt16_Default;
        ConvertInt16ToUInt16 = DefaultConversionsToUInt16.ConvertInt16ToUInt16_Default;
        ConvertUInt32ToUInt16 = DefaultConversionsToUInt16.ConvertUInt32ToUInt16_Default;
        ConvertInt32ToUInt16 = DefaultConversionsToUInt16.ConvertInt32ToUInt16_Default;
        ConvertUInt64ToUInt16 = DefaultConversionsToUInt16.ConvertUInt64ToUInt16_Default;
        ConvertInt64ToUInt16 = DefaultConversionsToUInt16.ConvertInt64ToUInt16_Default;
        ConvertFloatToUInt16 = DefaultConversionsToUInt16.ConvertFloatToUInt16_Default;
        ConvertDoubleToUInt16 = DefaultConversionsToUInt16.ConvertDoubleToUInt16_Default;
    }
}


public class NumericConversionToUInt16
{
    public ConvertUInt8ToUInt16_Delegate ConvertUInt8ToUInt16 { get; }
    public ConvertInt8ToUInt16_Delegate ConvertInt8ToUInt16 { get; }
    public ConvertUInt16ToUInt16_Delegate ConvertUInt16ToUInt16 { get; }
    public ConvertInt16ToUInt16_Delegate ConvertInt16ToUInt16 { get; }
    public ConvertUInt32ToUInt16_Delegate ConvertUInt32ToUInt16 { get; }
    public ConvertInt32ToUInt16_Delegate ConvertInt32ToUInt16 { get; }
    public ConvertUInt64ToUInt16_Delegate ConvertUInt64ToUInt16 { get; }
    public ConvertInt64ToUInt16_Delegate ConvertInt64ToUInt16 { get; }
    public ConvertFloatToUInt16_Delegate ConvertFloatToUInt16 { get; }
    public ConvertDoubleToUInt16_Delegate ConvertDoubleToUInt16 { get; }

    public NumericConversionToUInt16(ConversionToUInt16Delegates conversionDelegates)
    {
        ConvertUInt8ToUInt16 = conversionDelegates.ConvertUInt8ToUInt16;
        ConvertInt8ToUInt16 = conversionDelegates.ConvertInt8ToUInt16;
        ConvertUInt16ToUInt16 = conversionDelegates.ConvertUInt16ToUInt16;
        ConvertInt16ToUInt16 = conversionDelegates.ConvertInt16ToUInt16;
        ConvertUInt32ToUInt16 = conversionDelegates.ConvertUInt32ToUInt16;
        ConvertInt32ToUInt16 = conversionDelegates.ConvertInt32ToUInt16;
        ConvertUInt64ToUInt16 = conversionDelegates.ConvertUInt64ToUInt16;
        ConvertInt64ToUInt16 = conversionDelegates.ConvertInt64ToUInt16;
        ConvertFloatToUInt16 = conversionDelegates.ConvertFloatToUInt16;
        ConvertDoubleToUInt16 = conversionDelegates.ConvertDoubleToUInt16;
    }
}
