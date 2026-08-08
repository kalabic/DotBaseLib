using System.Runtime.CompilerServices;

namespace DotBase.Integral.Conversion.Numeric.Defaults;


public static class DefaultConversionsToDouble
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ConvertUInt8ToDouble_Default(byte value)
    {
        return (double)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ConvertInt8ToDouble_Default(sbyte value)
    {
        return (double)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ConvertUInt16ToDouble_Default(ushort value)
    {
        return (double)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ConvertInt16ToDouble_Default(short value)
    {
        return (double)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ConvertUInt32ToDouble_Default(uint value)
    {
        return (double)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ConvertInt32ToDouble_Default(int value)
    {
        return (double)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ConvertUInt64ToDouble_Default(ulong value)
    {
        return (double)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ConvertInt64ToDouble_Default(long value)
    {
        return (double)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ConvertFloatToDouble_Default(float value)
    {
        double d = value;
        if (double.IsNaN(d))
        {
            return 0;
        }
        if (double.IsPositiveInfinity(d))
        {
            return double.MaxValue;
        }
        if (double.IsNegativeInfinity(d))
        {
            return -double.MaxValue;
        }
        return d;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ConvertDoubleToDouble_Default(double value)
    {
        return value;
    }
}
