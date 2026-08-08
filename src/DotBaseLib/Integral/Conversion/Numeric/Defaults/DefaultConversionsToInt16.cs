using System.Runtime.CompilerServices;

namespace DotBase.Integral.Conversion.Numeric.Defaults;


public static class DefaultConversionsToInt16
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ConvertUInt8ToInt16_Default(byte value)
    {
        return (short)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ConvertInt8ToInt16_Default(sbyte value)
    {
        return (short)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ConvertUInt16ToInt16_Default(ushort value)
    {
        if (value >= (ulong)short.MaxValue)
        {
            return short.MaxValue;
        }
        return (short)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ConvertInt16ToInt16_Default(short value)
    {
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ConvertUInt32ToInt16_Default(uint value)
    {
        if (value >= (ulong)short.MaxValue)
        {
            return short.MaxValue;
        }
        return (short)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ConvertInt32ToInt16_Default(int value)
    {
        long v = value;
        if (v < short.MinValue)
        {
            return short.MinValue;
        }
        if (v > short.MaxValue)
        {
            return short.MaxValue;
        }
        return (short)v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ConvertUInt64ToInt16_Default(ulong value)
    {
        if (value >= (ulong)short.MaxValue)
        {
            return short.MaxValue;
        }
        return (short)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ConvertInt64ToInt16_Default(long value)
    {
        long v = value;
        if (v < short.MinValue)
        {
            return short.MinValue;
        }
        if (v > short.MaxValue)
        {
            return short.MaxValue;
        }
        return (short)v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ConvertFloatToInt16_Default(float value)
    {
        double d = value;
        if (double.IsNaN(d))
        {
            return 0;
        }
        if (d <= short.MinValue)
        {
            return short.MinValue;
        }
        if (d >= short.MaxValue)
        {
            return short.MaxValue;
        }
        return (short)d;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ConvertDoubleToInt16_Default(double value)
    {
        if (double.IsNaN(value))
        {
            return 0;
        }
        if (value <= short.MinValue)
        {
            return short.MinValue;
        }
        if (value >= short.MaxValue)
        {
            return short.MaxValue;
        }
        return (short)value;
    }
}
