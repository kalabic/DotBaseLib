using System.Runtime.CompilerServices;

namespace DotBase.Integral.Conversion.Numeric.Defaults;


public static class DefaultConversionsToUInt16
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ConvertUInt8ToUInt16_Default(byte value)
    {
        return (ushort)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ConvertInt8ToUInt16_Default(sbyte value)
    {
        if (value <= 0)
        {
            return 0;
        }
        return (ushort)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ConvertUInt16ToUInt16_Default(ushort value)
    {
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ConvertInt16ToUInt16_Default(short value)
    {
        if (value <= 0)
        {
            return 0;
        }
        return (ushort)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ConvertUInt32ToUInt16_Default(uint value)
    {
        ulong u = value;
        return u > ushort.MaxValue ? ushort.MaxValue : (ushort)u;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ConvertInt32ToUInt16_Default(int value)
    {
        if (value <= 0)
        {
            return 0;
        }
        ulong u = (ulong)value;
        return u > ushort.MaxValue ? ushort.MaxValue : (ushort)u;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ConvertUInt64ToUInt16_Default(ulong value)
    {
        return value > ushort.MaxValue ? ushort.MaxValue : (ushort)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ConvertInt64ToUInt16_Default(long value)
    {
        if (value <= 0)
        {
            return 0;
        }
        ulong u = (ulong)value;
        return u > ushort.MaxValue ? ushort.MaxValue : (ushort)u;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ConvertFloatToUInt16_Default(float value)
    {
        double d = value;
        if (double.IsNaN(d) || d <= 0)
        {
            return 0;
        }
        if (d >= ushort.MaxValue)
        {
            return ushort.MaxValue;
        }
        return (ushort)d;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ConvertDoubleToUInt16_Default(double value)
    {
        if (double.IsNaN(value) || value <= 0)
        {
            return 0;
        }
        if (value >= ushort.MaxValue)
        {
            return ushort.MaxValue;
        }
        return (ushort)value;
    }

}
