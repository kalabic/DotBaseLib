using DotBase.Integral.Conversion.Numeric.Defaults;

namespace DotBase.Integral.Conversion.Numeric;


public delegate float ConvertUInt8ToFloat_Delegate(byte value);
public delegate float ConvertInt8ToFloat_Delegate(sbyte value);
public delegate float ConvertUInt16ToFloat_Delegate(ushort value);
public delegate float ConvertInt16ToFloat_Delegate(short value);
public delegate float ConvertUInt32ToFloat_Delegate(uint value);
public delegate float ConvertInt32ToFloat_Delegate(int value);
public delegate float ConvertUInt64ToFloat_Delegate(ulong value);
public delegate float ConvertInt64ToFloat_Delegate(long value);
public delegate float ConvertFloatToFloat_Delegate(float value);
public delegate float ConvertDoubleToFloat_Delegate(double value);


public struct ConversionToFloatDelegates
{
    public ConvertUInt8ToFloat_Delegate ConvertUInt8ToFloat;
    public ConvertInt8ToFloat_Delegate ConvertInt8ToFloat;
    public ConvertUInt16ToFloat_Delegate ConvertUInt16ToFloat;
    public ConvertInt16ToFloat_Delegate ConvertInt16ToFloat;
    public ConvertUInt32ToFloat_Delegate ConvertUInt32ToFloat;
    public ConvertInt32ToFloat_Delegate ConvertInt32ToFloat;
    public ConvertUInt64ToFloat_Delegate ConvertUInt64ToFloat;
    public ConvertInt64ToFloat_Delegate ConvertInt64ToFloat;
    public ConvertFloatToFloat_Delegate ConvertFloatToFloat;
    public ConvertDoubleToFloat_Delegate ConvertDoubleToFloat;

    public void ResetToDefaults()
    {
        ConvertUInt8ToFloat = DefaultConversionsToFloat.ConvertUInt8ToFloat_Default;
        ConvertInt8ToFloat = DefaultConversionsToFloat.ConvertInt8ToFloat_Default;
        ConvertUInt16ToFloat = DefaultConversionsToFloat.ConvertUInt16ToFloat_Default;
        ConvertInt16ToFloat = DefaultConversionsToFloat.ConvertInt16ToFloat_Default;
        ConvertUInt32ToFloat = DefaultConversionsToFloat.ConvertUInt32ToFloat_Default;
        ConvertInt32ToFloat = DefaultConversionsToFloat.ConvertInt32ToFloat_Default;
        ConvertUInt64ToFloat = DefaultConversionsToFloat.ConvertUInt64ToFloat_Default;
        ConvertInt64ToFloat = DefaultConversionsToFloat.ConvertInt64ToFloat_Default;
        ConvertFloatToFloat = DefaultConversionsToFloat.ConvertFloatToFloat_Default;
        ConvertDoubleToFloat = DefaultConversionsToFloat.ConvertDoubleToFloat_Default;
    }
}


public class NumericConversionToFloat
{
    public ConvertUInt8ToFloat_Delegate ConvertUInt8ToFloat { get; }
    public ConvertInt8ToFloat_Delegate ConvertInt8ToFloat { get; }
    public ConvertUInt16ToFloat_Delegate ConvertUInt16ToFloat { get; }
    public ConvertInt16ToFloat_Delegate ConvertInt16ToFloat { get; }
    public ConvertUInt32ToFloat_Delegate ConvertUInt32ToFloat { get; }
    public ConvertInt32ToFloat_Delegate ConvertInt32ToFloat { get; }
    public ConvertUInt64ToFloat_Delegate ConvertUInt64ToFloat { get; }
    public ConvertInt64ToFloat_Delegate ConvertInt64ToFloat { get; }
    public ConvertFloatToFloat_Delegate ConvertFloatToFloat { get; }
    public ConvertDoubleToFloat_Delegate ConvertDoubleToFloat { get; }

    public NumericConversionToFloat(ConversionToFloatDelegates conversionDelegates)
    {
        ConvertUInt8ToFloat = conversionDelegates.ConvertUInt8ToFloat;
        ConvertInt8ToFloat = conversionDelegates.ConvertInt8ToFloat;
        ConvertUInt16ToFloat = conversionDelegates.ConvertUInt16ToFloat;
        ConvertInt16ToFloat = conversionDelegates.ConvertInt16ToFloat;
        ConvertUInt32ToFloat = conversionDelegates.ConvertUInt32ToFloat;
        ConvertInt32ToFloat = conversionDelegates.ConvertInt32ToFloat;
        ConvertUInt64ToFloat = conversionDelegates.ConvertUInt64ToFloat;
        ConvertInt64ToFloat = conversionDelegates.ConvertInt64ToFloat;
        ConvertFloatToFloat = conversionDelegates.ConvertFloatToFloat;
        ConvertDoubleToFloat = conversionDelegates.ConvertDoubleToFloat;
    }
}
