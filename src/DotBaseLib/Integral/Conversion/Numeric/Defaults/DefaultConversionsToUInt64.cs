using System.Runtime.CompilerServices;

namespace DotBase.Integral.Conversion.Numeric.Defaults;


public static class DefaultConversionsToUInt64
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ConvertUInt8ToUInt64_Default(byte value)
    {
        return (ulong)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ConvertInt8ToUInt64_Default(sbyte value)
    {
        if (value <= 0)
        {
            return 0;
        }
        return (ulong)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ConvertUInt16ToUInt64_Default(ushort value)
    {
        return (ulong)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ConvertInt16ToUInt64_Default(short value)
    {
        if (value <= 0)
        {
            return 0;
        }
        return (ulong)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ConvertUInt32ToUInt64_Default(uint value)
    {
        return (ulong)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ConvertInt32ToUInt64_Default(int value)
    {
        if (value <= 0)
        {
            return 0;
        }
        return (ulong)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ConvertUInt64ToUInt64_Default(ulong value)
    {
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ConvertInt64ToUInt64_Default(long value)
    {
        if (value <= 0)
        {
            return 0;
        }
        return (ulong)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ConvertFloatToUInt64_Default(float value)
    {
        double d = value;
        const double UpperExclusive = 18446744073709551616d;
        if (double.IsNaN(d) || d <= 0)
        {
            return 0;
        }
        if (d >= UpperExclusive)
        {
            return ulong.MaxValue;
        }
        return (ulong)d;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ConvertDoubleToUInt64_Default(double value)
    {
        const double UpperExclusive = 18446744073709551616d;
        if (double.IsNaN(value) || value <= 0)
        {
            return 0;
        }
        if (value >= UpperExclusive)
        {
            return ulong.MaxValue;
        }
        return (ulong)value;
    }

}
