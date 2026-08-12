using DotBase.Buffers;
using DotBase.Integral.Conversion.Numeric;
using DotBase.Integral.Conversion.Numeric.Defaults;
using System.Diagnostics;

namespace DotBase.Integral.Conversion.Internal.Standard.L2B;


internal static unsafe class L2BConvertToInt32
{

    internal static StandardConversionDelegates Resolve(
        in IntegralFormat input)
    {
        return input.ValueType switch
        {
            IntegralType.UInt8 => new(L2B_UInt8_To_Int32, L2B_UInt8_To_Int32_Default),
            IntegralType.Int8 => new(L2B_Int8_To_Int32, L2B_Int8_To_Int32_Default),
            IntegralType.UInt16 => new(L2B_UInt16_To_Int32, L2B_UInt16_To_Int32_Default),
            IntegralType.Int16 => new(L2B_Int16_To_Int32, L2B_Int16_To_Int32_Default),
            IntegralType.UInt32 => new(L2B_UInt32_To_Int32, L2B_UInt32_To_Int32_Default),
            IntegralType.Int32 => new(L2B_Int32_To_Int32, L2B_Int32_To_Int32_Default),
            IntegralType.UInt64 => new(L2B_UInt64_To_Int32, L2B_UInt64_To_Int32_Default),
            IntegralType.Int64 => new(L2B_Int64_To_Int32, L2B_Int64_To_Int32_Default),
            IntegralType.Float => new(L2B_Float_To_Int32, L2B_Float_To_Int32_Default),
            IntegralType.Double => new(L2B_Double_To_Int32, L2B_Double_To_Int32_Default),
            _ => throw new ArgumentOutOfRangeException(nameof(input)),
        };
    }

    public static long L2B_UInt8_To_Int32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt8ToInt32_Delegate convertUInt8ToInt32 = (ConvertUInt8ToInt32_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            byte* src = (byte*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                int d = convertUInt8ToInt32(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                int d = convertUInt8ToInt32(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt8_To_Int32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            byte* src = (byte*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                int d = ((int)s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                int d = ((int)s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int8_To_Int32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt8ToInt32_Delegate convertInt8ToInt32 = (ConvertInt8ToInt32_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            sbyte* src = (sbyte*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                int d = convertInt8ToInt32(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                int d = convertInt8ToInt32(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int8_To_Int32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            sbyte* src = (sbyte*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                int d = ((int)s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                int d = ((int)s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt16_To_Int32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt16ToInt32_Delegate convertUInt16ToInt32 = (ConvertUInt16ToInt32_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            ushort* src = (ushort*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = *src++;
                int d = convertUInt16ToInt32(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                int d = convertUInt16ToInt32(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt16_To_Int32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            ushort* src = (ushort*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = *src++;
                int d = ((int)s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                int d = ((int)s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int16_To_Int32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt16ToInt32_Delegate convertInt16ToInt32 = (ConvertInt16ToInt32_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            short* src = (short*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = *src++;
                int d = convertInt16ToInt32(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                int d = convertInt16ToInt32(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int16_To_Int32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            short* src = (short*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = *src++;
                int d = ((int)s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                int d = ((int)s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt32_To_Int32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt32ToInt32_Delegate convertUInt32ToInt32 = (ConvertUInt32ToInt32_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            uint* src = (uint*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = *src++;
                int d = convertUInt32ToInt32(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                int d = convertUInt32ToInt32(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt32_To_Int32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            uint* src = (uint*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = *src++;
                int d = DefaultConversionsToInt32.ConvertUInt32ToInt32_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                int d = DefaultConversionsToInt32.ConvertUInt32ToInt32_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int32_To_Int32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt32ToInt32_Delegate convertInt32ToInt32 = (ConvertInt32ToInt32_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = *src++;
                int d = convertInt32ToInt32(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                int d = convertInt32ToInt32(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int32_To_Int32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
            }
            return n;
        }
    }

    public static long L2B_UInt64_To_Int32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt64ToInt32_Delegate convertUInt64ToInt32 = (ConvertUInt64ToInt32_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = *src++;
                int d = convertUInt64ToInt32(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                int d = convertUInt64ToInt32(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt64_To_Int32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = *src++;
                int d = DefaultConversionsToInt32.ConvertUInt64ToInt32_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                int d = DefaultConversionsToInt32.ConvertUInt64ToInt32_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int64_To_Int32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt64ToInt32_Delegate convertInt64ToInt32 = (ConvertInt64ToInt32_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            long* src = (long*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = *src++;
                int d = convertInt64ToInt32(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                int d = convertInt64ToInt32(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int64_To_Int32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            long* src = (long*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = *src++;
                int d = DefaultConversionsToInt32.ConvertInt64ToInt32_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                int d = DefaultConversionsToInt32.ConvertInt64ToInt32_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Float_To_Int32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertFloatToInt32_Delegate convertFloatToInt32 = (ConvertFloatToInt32_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            float* src = (float*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = *src++;
                int d = convertFloatToInt32(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(uint*)src);
                src++;
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                int d = convertFloatToInt32(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Float_To_Int32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            float* src = (float*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = *src++;
                int d = DefaultConversionsToInt32.ConvertFloatToInt32_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(uint*)src);
                src++;
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                int d = DefaultConversionsToInt32.ConvertFloatToInt32_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Double_To_Int32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertDoubleToInt32_Delegate convertDoubleToInt32 = (ConvertDoubleToInt32_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = *src++;
                int d = convertDoubleToInt32(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(ulong*)src);
                src++;
                double s = System.BitConverter.UInt64BitsToDouble(s_bits);
                int d = convertDoubleToInt32(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Double_To_Int32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = *src++;
                int d = DefaultConversionsToInt32.ConvertDoubleToInt32_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(ulong*)src);
                src++;
                double s = System.BitConverter.UInt64BitsToDouble(s_bits);
                int d = DefaultConversionsToInt32.ConvertDoubleToInt32_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

}
