using DotBase.Integral.Conversion.Numeric.Defaults;

namespace DotBase.Integral.Conversion.Numeric;


public delegate double ConvertUInt8ToDouble_Delegate(byte value);
public delegate double ConvertInt8ToDouble_Delegate(sbyte value);
public delegate double ConvertUInt16ToDouble_Delegate(ushort value);
public delegate double ConvertInt16ToDouble_Delegate(short value);
public delegate double ConvertUInt32ToDouble_Delegate(uint value);
public delegate double ConvertInt32ToDouble_Delegate(int value);
public delegate double ConvertUInt64ToDouble_Delegate(ulong value);
public delegate double ConvertInt64ToDouble_Delegate(long value);
public delegate double ConvertFloatToDouble_Delegate(float value);
public delegate double ConvertDoubleToDouble_Delegate(double value);


public struct ConversionToDoubleDelegates
{
    public ConvertUInt8ToDouble_Delegate ConvertUInt8ToDouble;
    public ConvertInt8ToDouble_Delegate ConvertInt8ToDouble;
    public ConvertUInt16ToDouble_Delegate ConvertUInt16ToDouble;
    public ConvertInt16ToDouble_Delegate ConvertInt16ToDouble;
    public ConvertUInt32ToDouble_Delegate ConvertUInt32ToDouble;
    public ConvertInt32ToDouble_Delegate ConvertInt32ToDouble;
    public ConvertUInt64ToDouble_Delegate ConvertUInt64ToDouble;
    public ConvertInt64ToDouble_Delegate ConvertInt64ToDouble;
    public ConvertFloatToDouble_Delegate ConvertFloatToDouble;
    public ConvertDoubleToDouble_Delegate ConvertDoubleToDouble;

    public void ResetToDefaults()
    {
        ConvertUInt8ToDouble = DefaultConversionsToDouble.ConvertUInt8ToDouble_Default;
        ConvertInt8ToDouble = DefaultConversionsToDouble.ConvertInt8ToDouble_Default;
        ConvertUInt16ToDouble = DefaultConversionsToDouble.ConvertUInt16ToDouble_Default;
        ConvertInt16ToDouble = DefaultConversionsToDouble.ConvertInt16ToDouble_Default;
        ConvertUInt32ToDouble = DefaultConversionsToDouble.ConvertUInt32ToDouble_Default;
        ConvertInt32ToDouble = DefaultConversionsToDouble.ConvertInt32ToDouble_Default;
        ConvertUInt64ToDouble = DefaultConversionsToDouble.ConvertUInt64ToDouble_Default;
        ConvertInt64ToDouble = DefaultConversionsToDouble.ConvertInt64ToDouble_Default;
        ConvertFloatToDouble = DefaultConversionsToDouble.ConvertFloatToDouble_Default;
        ConvertDoubleToDouble = DefaultConversionsToDouble.ConvertDoubleToDouble_Default;
    }
}


public class NumericConversionToDouble
{
    public ConvertUInt8ToDouble_Delegate ConvertUInt8ToDouble { get; }
    public ConvertInt8ToDouble_Delegate ConvertInt8ToDouble { get; }
    public ConvertUInt16ToDouble_Delegate ConvertUInt16ToDouble { get; }
    public ConvertInt16ToDouble_Delegate ConvertInt16ToDouble { get; }
    public ConvertUInt32ToDouble_Delegate ConvertUInt32ToDouble { get; }
    public ConvertInt32ToDouble_Delegate ConvertInt32ToDouble { get; }
    public ConvertUInt64ToDouble_Delegate ConvertUInt64ToDouble { get; }
    public ConvertInt64ToDouble_Delegate ConvertInt64ToDouble { get; }
    public ConvertFloatToDouble_Delegate ConvertFloatToDouble { get; }
    public ConvertDoubleToDouble_Delegate ConvertDoubleToDouble { get; }

    public NumericConversionToDouble(ConversionToDoubleDelegates conversionDelegates)
    {
        ConvertUInt8ToDouble = conversionDelegates.ConvertUInt8ToDouble;
        ConvertInt8ToDouble = conversionDelegates.ConvertInt8ToDouble;
        ConvertUInt16ToDouble = conversionDelegates.ConvertUInt16ToDouble;
        ConvertInt16ToDouble = conversionDelegates.ConvertInt16ToDouble;
        ConvertUInt32ToDouble = conversionDelegates.ConvertUInt32ToDouble;
        ConvertInt32ToDouble = conversionDelegates.ConvertInt32ToDouble;
        ConvertUInt64ToDouble = conversionDelegates.ConvertUInt64ToDouble;
        ConvertInt64ToDouble = conversionDelegates.ConvertInt64ToDouble;
        ConvertFloatToDouble = conversionDelegates.ConvertFloatToDouble;
        ConvertDoubleToDouble = conversionDelegates.ConvertDoubleToDouble;
    }
}
