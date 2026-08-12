using DotBase.Buffers;
using DotBase.Integral.Conversion.Numeric;
using DotBase.Integral.Conversion.Numeric.Defaults;
using System.Diagnostics;

namespace DotBase.Integral.Conversion.Internal.Standard.B2B;


internal static unsafe class B2BConvertToUInt8
{

    internal static StandardConversionDelegates Resolve(
        in IntegralFormat input)
    {
        return input.ValueType switch
        {
            IntegralType.UInt8 => new(B2B_UInt8_To_UInt8, B2B_UInt8_To_UInt8_Default),
            IntegralType.Int8 => new(B2B_Int8_To_UInt8, B2B_Int8_To_UInt8_Default),
            IntegralType.UInt16 => new(B2B_UInt16_To_UInt8, B2B_UInt16_To_UInt8_Default),
            IntegralType.Int16 => new(B2B_Int16_To_UInt8, B2B_Int16_To_UInt8_Default),
            IntegralType.UInt32 => new(B2B_UInt32_To_UInt8, B2B_UInt32_To_UInt8_Default),
            IntegralType.Int32 => new(B2B_Int32_To_UInt8, B2B_Int32_To_UInt8_Default),
            IntegralType.UInt64 => new(B2B_UInt64_To_UInt8, B2B_UInt64_To_UInt8_Default),
            IntegralType.Int64 => new(B2B_Int64_To_UInt8, B2B_Int64_To_UInt8_Default),
            IntegralType.Float => new(B2B_Float_To_UInt8, B2B_Float_To_UInt8_Default),
            IntegralType.Double => new(B2B_Double_To_UInt8, B2B_Double_To_UInt8_Default),
            _ => throw new ArgumentOutOfRangeException(nameof(input)),
        };
    }

    public static long B2B_UInt8_To_UInt8(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt8);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt8ToUInt8_Delegate convertUInt8ToUInt8 = (ConvertUInt8ToUInt8_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            byte* src = (byte*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                byte d = convertUInt8ToUInt8(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                byte d = convertUInt8ToUInt8(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_UInt8_To_UInt8_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt8);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        nuint byteCount = checked((nuint)n * 1);
        Buffer.MemoryCopy(
            input.DataPtr,
            output.DataPtr,
            (ulong)byteCount,
            (ulong)byteCount);
        return n;
    }

    public static long B2B_Int8_To_UInt8(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt8);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt8ToUInt8_Delegate convertInt8ToUInt8 = (ConvertInt8ToUInt8_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            sbyte* src = (sbyte*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                byte d = convertInt8ToUInt8(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                byte d = convertInt8ToUInt8(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_Int8_To_UInt8_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt8);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            sbyte* src = (sbyte*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                byte d = DefaultConversionsToUInt8.ConvertInt8ToUInt8_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                byte d = DefaultConversionsToUInt8.ConvertInt8ToUInt8_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_UInt16_To_UInt8(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt8);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt16ToUInt8_Delegate convertUInt16ToUInt8 = (ConvertUInt16ToUInt8_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            ushort* src = (ushort*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                byte d = convertUInt16ToUInt8(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = *src++;
                byte d = convertUInt16ToUInt8(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_UInt16_To_UInt8_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt8);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            ushort* src = (ushort*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                byte d = DefaultConversionsToUInt8.ConvertUInt16ToUInt8_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = *src++;
                byte d = DefaultConversionsToUInt8.ConvertUInt16ToUInt8_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_Int16_To_UInt8(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt8);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt16ToUInt8_Delegate convertInt16ToUInt8 = (ConvertInt16ToUInt8_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            short* src = (short*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                byte d = convertInt16ToUInt8(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = *src++;
                byte d = convertInt16ToUInt8(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_Int16_To_UInt8_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt8);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            short* src = (short*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                byte d = DefaultConversionsToUInt8.ConvertInt16ToUInt8_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = *src++;
                byte d = DefaultConversionsToUInt8.ConvertInt16ToUInt8_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_UInt32_To_UInt8(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt8);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt32ToUInt8_Delegate convertUInt32ToUInt8 = (ConvertUInt32ToUInt8_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            uint* src = (uint*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                byte d = convertUInt32ToUInt8(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = *src++;
                byte d = convertUInt32ToUInt8(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_UInt32_To_UInt8_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt8);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            uint* src = (uint*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                byte d = DefaultConversionsToUInt8.ConvertUInt32ToUInt8_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = *src++;
                byte d = DefaultConversionsToUInt8.ConvertUInt32ToUInt8_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_Int32_To_UInt8(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt8);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt32ToUInt8_Delegate convertInt32ToUInt8 = (ConvertInt32ToUInt8_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                byte d = convertInt32ToUInt8(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = *src++;
                byte d = convertInt32ToUInt8(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_Int32_To_UInt8_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt8);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                byte d = DefaultConversionsToUInt8.ConvertInt32ToUInt8_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = *src++;
                byte d = DefaultConversionsToUInt8.ConvertInt32ToUInt8_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_UInt64_To_UInt8(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt8);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt64ToUInt8_Delegate convertUInt64ToUInt8 = (ConvertUInt64ToUInt8_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                byte d = convertUInt64ToUInt8(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = *src++;
                byte d = convertUInt64ToUInt8(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_UInt64_To_UInt8_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt8);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                byte d = DefaultConversionsToUInt8.ConvertUInt64ToUInt8_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = *src++;
                byte d = DefaultConversionsToUInt8.ConvertUInt64ToUInt8_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_Int64_To_UInt8(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt8);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt64ToUInt8_Delegate convertInt64ToUInt8 = (ConvertInt64ToUInt8_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            long* src = (long*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                byte d = convertInt64ToUInt8(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = *src++;
                byte d = convertInt64ToUInt8(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_Int64_To_UInt8_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt8);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            long* src = (long*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                byte d = DefaultConversionsToUInt8.ConvertInt64ToUInt8_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = *src++;
                byte d = DefaultConversionsToUInt8.ConvertInt64ToUInt8_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_Float_To_UInt8(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt8);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertFloatToUInt8_Delegate convertFloatToUInt8 = (ConvertFloatToUInt8_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            float* src = (float*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(uint*)src);
                src++;
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                byte d = convertFloatToUInt8(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = *src++;
                byte d = convertFloatToUInt8(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_Float_To_UInt8_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt8);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            float* src = (float*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(uint*)src);
                src++;
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                byte d = DefaultConversionsToUInt8.ConvertFloatToUInt8_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = *src++;
                byte d = DefaultConversionsToUInt8.ConvertFloatToUInt8_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_Double_To_UInt8(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt8);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertDoubleToUInt8_Delegate convertDoubleToUInt8 = (ConvertDoubleToUInt8_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(ulong*)src);
                src++;
                double s = System.BitConverter.UInt64BitsToDouble(s_bits);
                byte d = convertDoubleToUInt8(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = *src++;
                byte d = convertDoubleToUInt8(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_Double_To_UInt8_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt8);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(ulong*)src);
                src++;
                double s = System.BitConverter.UInt64BitsToDouble(s_bits);
                byte d = DefaultConversionsToUInt8.ConvertDoubleToUInt8_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            byte* dst = (byte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = *src++;
                byte d = DefaultConversionsToUInt8.ConvertDoubleToUInt8_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

}
