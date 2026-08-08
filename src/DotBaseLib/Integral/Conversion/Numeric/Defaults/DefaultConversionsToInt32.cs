using System.Runtime.CompilerServices;

namespace DotBase.Integral.Conversion.Numeric.Defaults;


public static class DefaultConversionsToInt32
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ConvertUInt8ToInt32_Default(byte value)
    {
        return (int)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ConvertInt8ToInt32_Default(sbyte value)
    {
        return (int)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ConvertUInt16ToInt32_Default(ushort value)
    {
        return (int)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ConvertInt16ToInt32_Default(short value)
    {
        return (int)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ConvertUInt32ToInt32_Default(uint value)
    {
        if (value >= (ulong)int.MaxValue)
        {
            return int.MaxValue;
        }
        return (int)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ConvertInt32ToInt32_Default(int value)
    {
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ConvertUInt64ToInt32_Default(ulong value)
    {
        if (value >= (ulong)int.MaxValue)
        {
            return int.MaxValue;
        }
        return (int)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ConvertInt64ToInt32_Default(long value)
    {
        long v = value;
        if (v < int.MinValue)
        {
            return int.MinValue;
        }
        if (v > int.MaxValue)
        {
            return int.MaxValue;
        }
        return (int)v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ConvertFloatToInt32_Default(float value)
    {
        double d = value;
        if (double.IsNaN(d))
        {
            return 0;
        }
        if (d <= int.MinValue)
        {
            return int.MinValue;
        }
        if (d >= int.MaxValue)
        {
            return int.MaxValue;
        }
        return (int)d;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ConvertDoubleToInt32_Default(double value)
    {
        if (double.IsNaN(value))
        {
            return 0;
        }
        if (value <= int.MinValue)
        {
            return int.MinValue;
        }
        if (value >= int.MaxValue)
        {
            return int.MaxValue;
        }
        return (int)value;
    }
}
