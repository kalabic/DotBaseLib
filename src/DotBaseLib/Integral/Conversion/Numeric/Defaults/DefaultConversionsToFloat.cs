using System.Runtime.CompilerServices;

namespace DotBase.Integral.Conversion.Numeric.Defaults;


public static class DefaultConversionsToFloat
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ConvertUInt8ToFloat_Default(byte value)
    {
        return (float)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ConvertInt8ToFloat_Default(sbyte value)
    {
        return (float)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ConvertUInt16ToFloat_Default(ushort value)
    {
        return (float)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ConvertInt16ToFloat_Default(short value)
    {
        return (float)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ConvertUInt32ToFloat_Default(uint value)
    {
        return (float)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ConvertInt32ToFloat_Default(int value)
    {
        return (float)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ConvertUInt64ToFloat_Default(ulong value)
    {
        return (float)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ConvertInt64ToFloat_Default(long value)
    {
        return (float)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ConvertFloatToFloat_Default(float value)
    {
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ConvertDoubleToFloat_Default(double value)
    {
        if (double.IsNaN(value))
        {
            return 0;
        }
        if (value >= float.MaxValue)
        {
            return float.MaxValue;
        }
        if (value <= -float.MaxValue)
        {
            return -float.MaxValue;
        }
        return (float)value;
    }
}
