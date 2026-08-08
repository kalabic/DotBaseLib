using System.Runtime.CompilerServices;

namespace DotBase.Integral.Conversion.Numeric.Defaults;


public static class DefaultConversionsToInt8
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte ConvertUInt8ToInt8_Default(byte value)
    {
        if (value >= (ulong)sbyte.MaxValue)
        {
            return sbyte.MaxValue;
        }
        return (sbyte)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte ConvertInt8ToInt8_Default(sbyte value)
    {
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte ConvertUInt16ToInt8_Default(ushort value)
    {
        if (value >= (ulong)sbyte.MaxValue)
        {
            return sbyte.MaxValue;
        }
        return (sbyte)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte ConvertInt16ToInt8_Default(short value)
    {
        long v = value;
        if (v < sbyte.MinValue)
        {
            return sbyte.MinValue;
        }
        if (v > sbyte.MaxValue)
        {
            return sbyte.MaxValue;
        }
        return (sbyte)v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte ConvertUInt32ToInt8_Default(uint value)
    {
        if (value >= (ulong)sbyte.MaxValue)
        {
            return sbyte.MaxValue;
        }
        return (sbyte)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte ConvertInt32ToInt8_Default(int value)
    {
        long v = value;
        if (v < sbyte.MinValue)
        {
            return sbyte.MinValue;
        }
        if (v > sbyte.MaxValue)
        {
            return sbyte.MaxValue;
        }
        return (sbyte)v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte ConvertUInt64ToInt8_Default(ulong value)
    {
        if (value >= (ulong)sbyte.MaxValue)
        {
            return sbyte.MaxValue;
        }
        return (sbyte)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte ConvertInt64ToInt8_Default(long value)
    {
        long v = value;
        if (v < sbyte.MinValue)
        {
            return sbyte.MinValue;
        }
        if (v > sbyte.MaxValue)
        {
            return sbyte.MaxValue;
        }
        return (sbyte)v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte ConvertFloatToInt8_Default(float value)
    {
        double d = value;
        if (double.IsNaN(d))
        {
            return 0;
        }
        if (d <= sbyte.MinValue)
        {
            return sbyte.MinValue;
        }
        if (d >= sbyte.MaxValue)
        {
            return sbyte.MaxValue;
        }
        return (sbyte)d;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte ConvertDoubleToInt8_Default(double value)
    {
        if (double.IsNaN(value))
        {
            return 0;
        }
        if (value <= sbyte.MinValue)
        {
            return sbyte.MinValue;
        }
        if (value >= sbyte.MaxValue)
        {
            return sbyte.MaxValue;
        }
        return (sbyte)value;
    }

}
