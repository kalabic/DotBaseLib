using System.Runtime.CompilerServices;

namespace DotBase.Integral.Internal;


internal static class IntegralNumericConversion<TSource, TDestination>
    where TSource : unmanaged
    where TDestination : unmanaged
{
    private static readonly bool SourceIsSignedInteger =
        typeof(TSource) == typeof(sbyte) ||
        typeof(TSource) == typeof(short) ||
        typeof(TSource) == typeof(int) ||
        typeof(TSource) == typeof(long);

    private static readonly bool SourceIsUnsignedInteger =
        typeof(TSource) == typeof(byte) ||
        typeof(TSource) == typeof(ushort) ||
        typeof(TSource) == typeof(uint) ||
        typeof(TSource) == typeof(ulong);

    private static readonly bool DestinationIsSignedInteger =
        typeof(TDestination) == typeof(sbyte) ||
        typeof(TDestination) == typeof(short) ||
        typeof(TDestination) == typeof(int) ||
        typeof(TDestination) == typeof(long);

    private static readonly bool DestinationIsUnsignedInteger =
        typeof(TDestination) == typeof(byte) ||
        typeof(TDestination) == typeof(ushort) ||
        typeof(TDestination) == typeof(uint) ||
        typeof(TDestination) == typeof(ulong);

    /// <summary>
    /// Same-type identity is BitCast; integer↔integer identity widen/narrow uses
    /// <see cref="ConvertInteger"/> (no double). Scale/bias and float paths use double.
    /// </summary>
    internal static TDestination Convert(
        TSource source,
        in IntegralConversion conversion)
    {
        if (conversion.IsIdentity &&
            typeof(TSource) == typeof(TDestination))
        {
            return Unsafe.BitCast<TSource, TDestination>(source);
        }

        // Integer widen/narrow without scale/bias: stay in integer domain.
        if (conversion.IsIdentity &&
            (SourceIsSignedInteger || SourceIsUnsignedInteger) &&
            (DestinationIsSignedInteger || DestinationIsUnsignedInteger))
        {
            return ConvertInteger(source);
        }

        double value = ToDouble(source);
        if (!conversion.IsIdentity)
        {
            value = value * conversion.Scale + conversion.Bias;
        }

        return FromDouble(value);
    }

    private static TDestination ConvertInteger(TSource source)
    {
        if (DestinationIsSignedInteger)
        {
            long minimum = DestinationSignedMinimum();
            long maximum = DestinationSignedMaximum();
            long value;

            if (SourceIsSignedInteger)
            {
                value = ToSignedInteger(source);
                value = Math.Clamp(value, minimum, maximum);
            }
            else
            {
                ulong unsignedValue = ToUnsignedInteger(source);
                value = unsignedValue >= (ulong)maximum
                    ? maximum
                    : (long)unsignedValue;
            }

            return FromSignedInteger(value);
        }

        ulong destinationMaximum = DestinationUnsignedMaximum();
        ulong result;

        if (SourceIsSignedInteger)
        {
            long signedValue = ToSignedInteger(source);
            result = signedValue <= 0
                ? 0
                : Math.Min((ulong)signedValue, destinationMaximum);
        }
        else
        {
            result = Math.Min(
                ToUnsignedInteger(source),
                destinationMaximum);
        }

        return FromUnsignedInteger(result);
    }

    private static double ToDouble(TSource value)
    {
        if (typeof(TSource) == typeof(sbyte))
            return Unsafe.BitCast<TSource, sbyte>(value);
        if (typeof(TSource) == typeof(byte))
            return Unsafe.BitCast<TSource, byte>(value);
        if (typeof(TSource) == typeof(short))
            return Unsafe.BitCast<TSource, short>(value);
        if (typeof(TSource) == typeof(ushort))
            return Unsafe.BitCast<TSource, ushort>(value);
        if (typeof(TSource) == typeof(int))
            return Unsafe.BitCast<TSource, int>(value);
        if (typeof(TSource) == typeof(uint))
            return Unsafe.BitCast<TSource, uint>(value);
        if (typeof(TSource) == typeof(long))
            return Unsafe.BitCast<TSource, long>(value);
        if (typeof(TSource) == typeof(ulong))
            return Unsafe.BitCast<TSource, ulong>(value);
        if (typeof(TSource) == typeof(float))
            return Unsafe.BitCast<TSource, float>(value);
        if (typeof(TSource) == typeof(double))
            return Unsafe.BitCast<TSource, double>(value);

        throw new NotSupportedException(
            $"Type '{typeof(TSource)}' is not a supported source type.");
    }

    private static TDestination FromDouble(double value)
    {
        if (typeof(TDestination) == typeof(sbyte))
            return BitCast<sbyte>((sbyte)SaturateSigned(value, sbyte.MinValue, sbyte.MaxValue));
        if (typeof(TDestination) == typeof(byte))
            return BitCast<byte>((byte)SaturateUnsigned(value, byte.MaxValue));
        if (typeof(TDestination) == typeof(short))
            return BitCast<short>((short)SaturateSigned(value, short.MinValue, short.MaxValue));
        if (typeof(TDestination) == typeof(ushort))
            return BitCast<ushort>((ushort)SaturateUnsigned(value, ushort.MaxValue));
        if (typeof(TDestination) == typeof(int))
            return BitCast<int>((int)SaturateSigned(value, int.MinValue, int.MaxValue));
        if (typeof(TDestination) == typeof(uint))
            return BitCast<uint>((uint)SaturateUnsigned(value, uint.MaxValue));
        if (typeof(TDestination) == typeof(long))
            return BitCast<long>(SaturateInt64(value));
        if (typeof(TDestination) == typeof(ulong))
            return BitCast<ulong>(SaturateUInt64(value));
        if (typeof(TDestination) == typeof(float))
            return BitCast<float>(SaturateSingle(value));
        if (typeof(TDestination) == typeof(double))
            return BitCast<double>(SaturateDouble(value));

        throw new NotSupportedException(
            $"Type '{typeof(TDestination)}' is not a supported destination type.");
    }

    private static long ToSignedInteger(TSource value)
    {
        if (typeof(TSource) == typeof(sbyte))
            return Unsafe.BitCast<TSource, sbyte>(value);
        if (typeof(TSource) == typeof(short))
            return Unsafe.BitCast<TSource, short>(value);
        if (typeof(TSource) == typeof(int))
            return Unsafe.BitCast<TSource, int>(value);
        if (typeof(TSource) == typeof(long))
            return Unsafe.BitCast<TSource, long>(value);

        throw new InvalidOperationException(
            $"Type '{typeof(TSource)}' is not a signed integer.");
    }

    private static ulong ToUnsignedInteger(TSource value)
    {
        if (typeof(TSource) == typeof(byte))
            return Unsafe.BitCast<TSource, byte>(value);
        if (typeof(TSource) == typeof(ushort))
            return Unsafe.BitCast<TSource, ushort>(value);
        if (typeof(TSource) == typeof(uint))
            return Unsafe.BitCast<TSource, uint>(value);
        if (typeof(TSource) == typeof(ulong))
            return Unsafe.BitCast<TSource, ulong>(value);

        throw new InvalidOperationException(
            $"Type '{typeof(TSource)}' is not an unsigned integer.");
    }

    private static TDestination FromSignedInteger(long value)
    {
        if (typeof(TDestination) == typeof(sbyte))
            return BitCast<sbyte>((sbyte)value);
        if (typeof(TDestination) == typeof(short))
            return BitCast<short>((short)value);
        if (typeof(TDestination) == typeof(int))
            return BitCast<int>((int)value);
        if (typeof(TDestination) == typeof(long))
            return BitCast<long>(value);

        throw new InvalidOperationException(
            $"Type '{typeof(TDestination)}' is not a signed integer.");
    }

    private static TDestination FromUnsignedInteger(ulong value)
    {
        if (typeof(TDestination) == typeof(byte))
            return BitCast<byte>((byte)value);
        if (typeof(TDestination) == typeof(ushort))
            return BitCast<ushort>((ushort)value);
        if (typeof(TDestination) == typeof(uint))
            return BitCast<uint>((uint)value);
        if (typeof(TDestination) == typeof(ulong))
            return BitCast<ulong>(value);

        throw new InvalidOperationException(
            $"Type '{typeof(TDestination)}' is not an unsigned integer.");
    }

    private static long DestinationSignedMinimum()
    {
        if (typeof(TDestination) == typeof(sbyte))
            return sbyte.MinValue;
        if (typeof(TDestination) == typeof(short))
            return short.MinValue;
        if (typeof(TDestination) == typeof(int))
            return int.MinValue;
        if (typeof(TDestination) == typeof(long))
            return long.MinValue;

        throw new InvalidOperationException();
    }

    private static long DestinationSignedMaximum()
    {
        if (typeof(TDestination) == typeof(sbyte))
            return sbyte.MaxValue;
        if (typeof(TDestination) == typeof(short))
            return short.MaxValue;
        if (typeof(TDestination) == typeof(int))
            return int.MaxValue;
        if (typeof(TDestination) == typeof(long))
            return long.MaxValue;

        throw new InvalidOperationException();
    }

    private static ulong DestinationUnsignedMaximum()
    {
        if (typeof(TDestination) == typeof(byte))
            return byte.MaxValue;
        if (typeof(TDestination) == typeof(ushort))
            return ushort.MaxValue;
        if (typeof(TDestination) == typeof(uint))
            return uint.MaxValue;
        if (typeof(TDestination) == typeof(ulong))
            return ulong.MaxValue;

        throw new InvalidOperationException();
    }

    private static long SaturateSigned(
        double value,
        long minimum,
        long maximum)
    {
        if (double.IsNaN(value))
            return 0;
        if (value <= minimum)
            return minimum;
        if (value >= maximum)
            return maximum;

        return (long)value;
    }

    private static ulong SaturateUnsigned(
        double value,
        ulong maximum)
    {
        if (double.IsNaN(value) || value <= 0)
            return 0;
        if (value >= maximum)
            return maximum;

        return (ulong)value;
    }

    private static long SaturateInt64(double value)
    {
        const double UpperExclusive = 9223372036854775808d;

        if (double.IsNaN(value))
            return 0;
        if (value <= long.MinValue)
            return long.MinValue;
        if (value >= UpperExclusive)
            return long.MaxValue;

        return (long)value;
    }

    private static ulong SaturateUInt64(double value)
    {
        const double UpperExclusive = 18446744073709551616d;

        if (double.IsNaN(value) || value <= 0)
            return 0;
        if (value >= UpperExclusive)
            return ulong.MaxValue;

        return (ulong)value;
    }

    private static float SaturateSingle(double value)
    {
        if (double.IsNaN(value))
            return 0;
        if (value >= float.MaxValue)
            return float.MaxValue;
        if (value <= -float.MaxValue)
            return -float.MaxValue;

        return (float)value;
    }

    private static double SaturateDouble(double value)
    {
        if (double.IsNaN(value))
            return 0;
        if (double.IsPositiveInfinity(value))
            return double.MaxValue;
        if (double.IsNegativeInfinity(value))
            return -double.MaxValue;

        return value;
    }

    private static TDestination BitCast<TValue>(TValue value)
        where TValue : unmanaged
    {
        return Unsafe.BitCast<TValue, TDestination>(value);
    }
}
