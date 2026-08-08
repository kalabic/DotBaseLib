using System.Runtime.CompilerServices;

namespace DotBase.Integral.Conversion.Numeric.Defaults;


public static class DefaultConversionsToUInt32
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ConvertUInt8ToUInt32_Default(byte value)
    {
        return (uint)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ConvertInt8ToUInt32_Default(sbyte value)
    {
        if (value <= 0)
        {
            return 0;
        }
        return (uint)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ConvertUInt16ToUInt32_Default(ushort value)
    {
        return (uint)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ConvertInt16ToUInt32_Default(short value)
    {
        if (value <= 0)
        {
            return 0;
        }
        return (uint)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ConvertUInt32ToUInt32_Default(uint value)
    {
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ConvertInt32ToUInt32_Default(int value)
    {
        if (value <= 0)
        {
            return 0;
        }
        return (uint)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ConvertUInt64ToUInt32_Default(ulong value)
    {
        return value > uint.MaxValue ? uint.MaxValue : (uint)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ConvertInt64ToUInt32_Default(long value)
    {
        if (value <= 0)
        {
            return 0;
        }
        ulong u = (ulong)value;
        return u > uint.MaxValue ? uint.MaxValue : (uint)u;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ConvertFloatToUInt32_Default(float value)
    {
        double d = value;
        if (double.IsNaN(d) || d <= 0)
        {
            return 0;
        }
        if (d >= uint.MaxValue)
        {
            return uint.MaxValue;
        }
        return (uint)d;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ConvertDoubleToUInt32_Default(double value)
    {
        if (double.IsNaN(value) || value <= 0)
        {
            return 0;
        }
        if (value >= uint.MaxValue)
        {
            return uint.MaxValue;
        }
        return (uint)value;
    }

}
