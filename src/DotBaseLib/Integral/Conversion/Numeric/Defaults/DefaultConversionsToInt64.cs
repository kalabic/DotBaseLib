using System.Runtime.CompilerServices;

namespace DotBase.Integral.Conversion.Numeric.Defaults;


public static class DefaultConversionsToInt64
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ConvertUInt8ToInt64_Default(byte value)
    {
        return (long)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ConvertInt8ToInt64_Default(sbyte value)
    {
        return (long)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ConvertUInt16ToInt64_Default(ushort value)
    {
        return (long)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ConvertInt16ToInt64_Default(short value)
    {
        return (long)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ConvertUInt32ToInt64_Default(uint value)
    {
        return (long)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ConvertInt32ToInt64_Default(int value)
    {
        return (long)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ConvertUInt64ToInt64_Default(ulong value)
    {
        if (value >= (ulong)long.MaxValue)
        {
            return long.MaxValue;
        }
        return (long)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ConvertInt64ToInt64_Default(long value)
    {
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ConvertFloatToInt64_Default(float value)
    {
        double d = value;
        const double UpperExclusive = 9223372036854775808d;
        if (double.IsNaN(d))
        {
            return 0;
        }
        if (d <= long.MinValue)
        {
            return long.MinValue;
        }
        if (d >= UpperExclusive)
        {
            return long.MaxValue;
        }
        return (long)d;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ConvertDoubleToInt64_Default(double value)
    {
        const double UpperExclusive = 9223372036854775808d;
        if (double.IsNaN(value))
        {
            return 0;
        }
        if (value <= long.MinValue)
        {
            return long.MinValue;
        }
        if (value >= UpperExclusive)
        {
            return long.MaxValue;
        }
        return (long)value;
    }
}
