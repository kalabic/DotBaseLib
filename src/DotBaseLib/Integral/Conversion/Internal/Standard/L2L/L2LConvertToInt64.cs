using DotBase.Buffers;
using DotBase.Integral.Conversion.Numeric;
using DotBase.Integral.Conversion.Numeric.Defaults;
using System.Diagnostics;

namespace DotBase.Integral.Conversion.Internal.Standard.L2L;


internal static unsafe class L2LConvertToInt64
{

    internal static StandardConversionDelegates Resolve(
        in IntegralFormat input)
    {
        return input.ValueType switch
        {
            IntegralType.UInt8 => new(L2L_UInt8_To_Int64, L2L_UInt8_To_Int64_Default),
            IntegralType.Int8 => new(L2L_Int8_To_Int64, L2L_Int8_To_Int64_Default),
            IntegralType.UInt16 => new(L2L_UInt16_To_Int64, L2L_UInt16_To_Int64_Default),
            IntegralType.Int16 => new(L2L_Int16_To_Int64, L2L_Int16_To_Int64_Default),
            IntegralType.UInt32 => new(L2L_UInt32_To_Int64, L2L_UInt32_To_Int64_Default),
            IntegralType.Int32 => new(L2L_Int32_To_Int64, L2L_Int32_To_Int64_Default),
            IntegralType.UInt64 => new(L2L_UInt64_To_Int64, L2L_UInt64_To_Int64_Default),
            IntegralType.Int64 => new(L2L_Int64_To_Int64, L2L_Int64_To_Int64_Default),
            IntegralType.Float => new(L2L_Float_To_Int64, L2L_Float_To_Int64_Default),
            IntegralType.Double => new(L2L_Double_To_Int64, L2L_Double_To_Int64_Default),
            _ => throw new ArgumentOutOfRangeException(nameof(input)),
        };
    }

    public static long L2L_UInt8_To_Int64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.Int64);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt8ToInt64_Delegate convertUInt8ToInt64 = (ConvertUInt8ToInt64_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            byte* src = (byte*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                long d = convertUInt8ToInt64(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                long d = convertUInt8ToInt64(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_UInt8_To_Int64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.Int64);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            byte* src = (byte*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                long d = ((long)s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                long d = ((long)s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Int8_To_Int64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.Int64);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt8ToInt64_Delegate convertInt8ToInt64 = (ConvertInt8ToInt64_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            sbyte* src = (sbyte*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                long d = convertInt8ToInt64(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                long d = convertInt8ToInt64(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Int8_To_Int64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.Int64);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            sbyte* src = (sbyte*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                long d = ((long)s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                long d = ((long)s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_UInt16_To_Int64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.Int64);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt16ToInt64_Delegate convertUInt16ToInt64 = (ConvertUInt16ToInt64_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            ushort* src = (ushort*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = *src++;
                long d = convertUInt16ToInt64(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                long d = convertUInt16ToInt64(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_UInt16_To_Int64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.Int64);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            ushort* src = (ushort*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = *src++;
                long d = ((long)s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                long d = ((long)s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Int16_To_Int64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.Int64);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt16ToInt64_Delegate convertInt16ToInt64 = (ConvertInt16ToInt64_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            short* src = (short*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = *src++;
                long d = convertInt16ToInt64(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                long d = convertInt16ToInt64(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Int16_To_Int64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.Int64);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            short* src = (short*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = *src++;
                long d = ((long)s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                long d = ((long)s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_UInt32_To_Int64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.Int64);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt32ToInt64_Delegate convertUInt32ToInt64 = (ConvertUInt32ToInt64_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            uint* src = (uint*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = *src++;
                long d = convertUInt32ToInt64(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                long d = convertUInt32ToInt64(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_UInt32_To_Int64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.Int64);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            uint* src = (uint*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = *src++;
                long d = ((long)s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                long d = ((long)s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Int32_To_Int64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.Int64);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt32ToInt64_Delegate convertInt32ToInt64 = (ConvertInt32ToInt64_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = *src++;
                long d = convertInt32ToInt64(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                long d = convertInt32ToInt64(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Int32_To_Int64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.Int64);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = *src++;
                long d = ((long)s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                long d = ((long)s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_UInt64_To_Int64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.Int64);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt64ToInt64_Delegate convertUInt64ToInt64 = (ConvertUInt64ToInt64_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = *src++;
                long d = convertUInt64ToInt64(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                long d = convertUInt64ToInt64(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_UInt64_To_Int64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.Int64);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = *src++;
                long d = DefaultConversionsToInt64.ConvertUInt64ToInt64_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                long d = DefaultConversionsToInt64.ConvertUInt64ToInt64_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Int64_To_Int64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.Int64);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt64ToInt64_Delegate convertInt64ToInt64 = (ConvertInt64ToInt64_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            long* src = (long*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = *src++;
                long d = convertInt64ToInt64(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                long d = convertInt64ToInt64(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Int64_To_Int64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.Int64);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        nuint byteCount = checked((nuint)n * 8);
        Buffer.MemoryCopy(
            input.DataPtr,
            output.DataPtr,
            (ulong)byteCount,
            (ulong)byteCount);
        return n;
    }

    public static long L2L_Float_To_Int64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.Int64);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertFloatToInt64_Delegate convertFloatToInt64 = (ConvertFloatToInt64_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            float* src = (float*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = *src++;
                long d = convertFloatToInt64(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(uint*)src);
                src++;
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                long d = convertFloatToInt64(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Float_To_Int64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.Int64);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            float* src = (float*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = *src++;
                long d = DefaultConversionsToInt64.ConvertFloatToInt64_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(uint*)src);
                src++;
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                long d = DefaultConversionsToInt64.ConvertFloatToInt64_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Double_To_Int64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.Int64);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertDoubleToInt64_Delegate convertDoubleToInt64 = (ConvertDoubleToInt64_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = *src++;
                long d = convertDoubleToInt64(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(ulong*)src);
                src++;
                double s = System.BitConverter.UInt64BitsToDouble(s_bits);
                long d = convertDoubleToInt64(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Double_To_Int64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.Int64);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = *src++;
                long d = DefaultConversionsToInt64.ConvertDoubleToInt64_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            long* dst = (long*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(ulong*)src);
                src++;
                double s = System.BitConverter.UInt64BitsToDouble(s_bits);
                long d = DefaultConversionsToInt64.ConvertDoubleToInt64_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

}
