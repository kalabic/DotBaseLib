using System.Runtime.CompilerServices;

namespace DotBase.Integral.Conversion.Numeric.Defaults;


public static class DefaultConversionsToUInt8
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ConvertUInt8ToUInt8_Default(byte value)
    {
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ConvertInt8ToUInt8_Default(sbyte value)
    {
        if (value <= 0)
        {
            return 0;
        }
        return (byte)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ConvertUInt16ToUInt8_Default(ushort value)
    {
        ulong u = value;
        return u > byte.MaxValue ? byte.MaxValue : (byte)u;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ConvertInt16ToUInt8_Default(short value)
    {
        if (value <= 0)
        {
            return 0;
        }
        ulong u = (ulong)value;
        return u > byte.MaxValue ? byte.MaxValue : (byte)u;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ConvertUInt32ToUInt8_Default(uint value)
    {
        ulong u = value;
        return u > byte.MaxValue ? byte.MaxValue : (byte)u;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ConvertInt32ToUInt8_Default(int value)
    {
        if (value <= 0)
        {
            return 0;
        }
        ulong u = (ulong)value;
        return u > byte.MaxValue ? byte.MaxValue : (byte)u;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ConvertUInt64ToUInt8_Default(ulong value)
    {
        return value > byte.MaxValue ? byte.MaxValue : (byte)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ConvertInt64ToUInt8_Default(long value)
    {
        if (value <= 0)
        {
            return 0;
        }
        ulong u = (ulong)value;
        return u > byte.MaxValue ? byte.MaxValue : (byte)u;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ConvertFloatToUInt8_Default(float value)
    {
        double d = value;
        if (double.IsNaN(d) || d <= 0)
        {
            return 0;
        }
        if (d >= byte.MaxValue)
        {
            return byte.MaxValue;
        }
        return (byte)d;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ConvertDoubleToUInt8_Default(double value)
    {
        if (double.IsNaN(value) || value <= 0)
        {
            return 0;
        }
        if (value >= byte.MaxValue)
        {
            return byte.MaxValue;
        }
        return (byte)value;
    }

}
