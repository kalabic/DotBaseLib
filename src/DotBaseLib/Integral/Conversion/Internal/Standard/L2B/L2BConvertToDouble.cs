using DotBase.Buffers;
using DotBase.Integral.Conversion.Numeric;
using DotBase.Integral.Conversion.Numeric.Defaults;
using System.Diagnostics;

namespace DotBase.Integral.Conversion.Internal.Standard.L2B;


internal static unsafe class L2BConvertToDouble
{

    internal static StandardConversionDelegates Resolve(
        in IntegralFormat input)
    {
        return input.ValueType switch
        {
            IntegralType.UInt8 => new(L2B_UInt8_To_Double, L2B_UInt8_To_Double_Default),
            IntegralType.Int8 => new(L2B_Int8_To_Double, L2B_Int8_To_Double_Default),
            IntegralType.UInt16 => new(L2B_UInt16_To_Double, L2B_UInt16_To_Double_Default),
            IntegralType.Int16 => new(L2B_Int16_To_Double, L2B_Int16_To_Double_Default),
            IntegralType.UInt32 => new(L2B_UInt32_To_Double, L2B_UInt32_To_Double_Default),
            IntegralType.Int32 => new(L2B_Int32_To_Double, L2B_Int32_To_Double_Default),
            IntegralType.UInt64 => new(L2B_UInt64_To_Double, L2B_UInt64_To_Double_Default),
            IntegralType.Int64 => new(L2B_Int64_To_Double, L2B_Int64_To_Double_Default),
            IntegralType.Float => new(L2B_Float_To_Double, L2B_Float_To_Double_Default),
            IntegralType.Double => new(L2B_Double_To_Double, L2B_Double_To_Double_Default),
            _ => throw new ArgumentOutOfRangeException(nameof(input)),
        };
    }

    public static long L2B_UInt8_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt8ToDouble_Delegate convertUInt8ToDouble = (ConvertUInt8ToDouble_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            byte* src = (byte*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                double d = convertUInt8ToDouble(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                double d = convertUInt8ToDouble(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt8_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            byte* src = (byte*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                double d = ((double)s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                double d = ((double)s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int8_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt8ToDouble_Delegate convertInt8ToDouble = (ConvertInt8ToDouble_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            sbyte* src = (sbyte*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                double d = convertInt8ToDouble(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                double d = convertInt8ToDouble(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int8_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            sbyte* src = (sbyte*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                double d = ((double)s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                double d = ((double)s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt16_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt16ToDouble_Delegate convertUInt16ToDouble = (ConvertUInt16ToDouble_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            ushort* src = (ushort*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = *src++;
                double d = convertUInt16ToDouble(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = convertUInt16ToDouble(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt16_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            ushort* src = (ushort*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = *src++;
                double d = ((double)s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = ((double)s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int16_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt16ToDouble_Delegate convertInt16ToDouble = (ConvertInt16ToDouble_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            short* src = (short*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = *src++;
                double d = convertInt16ToDouble(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = convertInt16ToDouble(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int16_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            short* src = (short*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = *src++;
                double d = ((double)s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = ((double)s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt32_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt32ToDouble_Delegate convertUInt32ToDouble = (ConvertUInt32ToDouble_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            uint* src = (uint*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = *src++;
                double d = convertUInt32ToDouble(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = convertUInt32ToDouble(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt32_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            uint* src = (uint*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = *src++;
                double d = ((double)s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = ((double)s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int32_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt32ToDouble_Delegate convertInt32ToDouble = (ConvertInt32ToDouble_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = *src++;
                double d = convertInt32ToDouble(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = convertInt32ToDouble(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int32_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = *src++;
                double d = ((double)s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = ((double)s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt64_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt64ToDouble_Delegate convertUInt64ToDouble = (ConvertUInt64ToDouble_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = *src++;
                double d = convertUInt64ToDouble(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = convertUInt64ToDouble(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt64_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = *src++;
                double d = ((double)s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = ((double)s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int64_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt64ToDouble_Delegate convertInt64ToDouble = (ConvertInt64ToDouble_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            long* src = (long*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = *src++;
                double d = convertInt64ToDouble(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = convertInt64ToDouble(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int64_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            long* src = (long*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = *src++;
                double d = ((double)s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = ((double)s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Float_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertFloatToDouble_Delegate convertFloatToDouble = (ConvertFloatToDouble_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            float* src = (float*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = *src++;
                double d = convertFloatToDouble(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(uint*)src);
                src++;
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                double d = convertFloatToDouble(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Float_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            float* src = (float*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = *src++;
                double d = DefaultConversionsToDouble.ConvertFloatToDouble_Default(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(uint*)src);
                src++;
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                double d = DefaultConversionsToDouble.ConvertFloatToDouble_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Double_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertDoubleToDouble_Delegate convertDoubleToDouble = (ConvertDoubleToDouble_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = *src++;
                double d = convertDoubleToDouble(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(ulong*)src);
                src++;
                double s = System.BitConverter.UInt64BitsToDouble(s_bits);
                double d = convertDoubleToDouble(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Double_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(ulong*)src);
                src++;
                *(ulong*)dst = bits;
                dst++;
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(ulong*)src);
                src++;
                *(ulong*)dst = bits;
                dst++;
            }
            return n;
        }
    }

}
