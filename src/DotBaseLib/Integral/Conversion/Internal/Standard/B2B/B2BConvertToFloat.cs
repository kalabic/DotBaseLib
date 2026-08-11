using DotBase.Buffers;
using DotBase.Integral.Conversion.Numeric;
using DotBase.Integral.Conversion.Numeric.Defaults;
using System.Diagnostics;

namespace DotBase.Integral.Conversion.Internal.Standard.B2B;


internal static unsafe class B2BConvertToFloat
{
    internal static void AddToTable(IConversionDelegateTable table)
    {
        table.SetCustomFunc(B2B_UInt8_To_Float, ByteOrder.BigEndian, IntegralType.UInt8, ByteOrder.BigEndian, IntegralType.Float);
        table.SetCustomFunc(B2B_Int8_To_Float, ByteOrder.BigEndian, IntegralType.Int8, ByteOrder.BigEndian, IntegralType.Float);
        table.SetCustomFunc(B2B_UInt16_To_Float, ByteOrder.BigEndian, IntegralType.UInt16, ByteOrder.BigEndian, IntegralType.Float);
        table.SetCustomFunc(B2B_Int16_To_Float, ByteOrder.BigEndian, IntegralType.Int16, ByteOrder.BigEndian, IntegralType.Float);
        table.SetCustomFunc(B2B_UInt32_To_Float, ByteOrder.BigEndian, IntegralType.UInt32, ByteOrder.BigEndian, IntegralType.Float);
        table.SetCustomFunc(B2B_Int32_To_Float, ByteOrder.BigEndian, IntegralType.Int32, ByteOrder.BigEndian, IntegralType.Float);
        table.SetCustomFunc(B2B_UInt64_To_Float, ByteOrder.BigEndian, IntegralType.UInt64, ByteOrder.BigEndian, IntegralType.Float);
        table.SetCustomFunc(B2B_Int64_To_Float, ByteOrder.BigEndian, IntegralType.Int64, ByteOrder.BigEndian, IntegralType.Float);
        table.SetCustomFunc(B2B_Float_To_Float, ByteOrder.BigEndian, IntegralType.Float, ByteOrder.BigEndian, IntegralType.Float);
        table.SetCustomFunc(B2B_Double_To_Float, ByteOrder.BigEndian, IntegralType.Double, ByteOrder.BigEndian, IntegralType.Float);

        table.SetDefaultFunc(B2B_UInt8_To_Float_Default, ByteOrder.BigEndian, IntegralType.UInt8, ByteOrder.BigEndian, IntegralType.Float);
        table.SetDefaultFunc(B2B_Int8_To_Float_Default, ByteOrder.BigEndian, IntegralType.Int8, ByteOrder.BigEndian, IntegralType.Float);
        table.SetDefaultFunc(B2B_UInt16_To_Float_Default, ByteOrder.BigEndian, IntegralType.UInt16, ByteOrder.BigEndian, IntegralType.Float);
        table.SetDefaultFunc(B2B_Int16_To_Float_Default, ByteOrder.BigEndian, IntegralType.Int16, ByteOrder.BigEndian, IntegralType.Float);
        table.SetDefaultFunc(B2B_UInt32_To_Float_Default, ByteOrder.BigEndian, IntegralType.UInt32, ByteOrder.BigEndian, IntegralType.Float);
        table.SetDefaultFunc(B2B_Int32_To_Float_Default, ByteOrder.BigEndian, IntegralType.Int32, ByteOrder.BigEndian, IntegralType.Float);
        table.SetDefaultFunc(B2B_UInt64_To_Float_Default, ByteOrder.BigEndian, IntegralType.UInt64, ByteOrder.BigEndian, IntegralType.Float);
        table.SetDefaultFunc(B2B_Int64_To_Float_Default, ByteOrder.BigEndian, IntegralType.Int64, ByteOrder.BigEndian, IntegralType.Float);
        table.SetDefaultFunc(B2B_Float_To_Float_Default, ByteOrder.BigEndian, IntegralType.Float, ByteOrder.BigEndian, IntegralType.Float);
        table.SetDefaultFunc(B2B_Double_To_Float_Default, ByteOrder.BigEndian, IntegralType.Double, ByteOrder.BigEndian, IntegralType.Float);
    }

    public static long B2B_UInt8_To_Float(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.Float);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt8ToFloat_Delegate convertUInt8ToFloat = (ConvertUInt8ToFloat_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            byte* src = (byte*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                float d = convertUInt8ToFloat(s);
                *(uint*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.SingleToUInt32Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                float d = convertUInt8ToFloat(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_UInt8_To_Float_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.Float);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            byte* src = (byte*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                float d = ((float)s);
                *(uint*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.SingleToUInt32Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                float d = ((float)s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_Int8_To_Float(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.Float);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt8ToFloat_Delegate convertInt8ToFloat = (ConvertInt8ToFloat_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            sbyte* src = (sbyte*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                float d = convertInt8ToFloat(s);
                *(uint*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.SingleToUInt32Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                float d = convertInt8ToFloat(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_Int8_To_Float_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.Float);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            sbyte* src = (sbyte*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                float d = ((float)s);
                *(uint*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.SingleToUInt32Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                float d = ((float)s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_UInt16_To_Float(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.Float);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt16ToFloat_Delegate convertUInt16ToFloat = (ConvertUInt16ToFloat_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            ushort* src = (ushort*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                float d = convertUInt16ToFloat(s);
                *(uint*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.SingleToUInt32Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = *src++;
                float d = convertUInt16ToFloat(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_UInt16_To_Float_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.Float);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            ushort* src = (ushort*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                float d = ((float)s);
                *(uint*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.SingleToUInt32Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = *src++;
                float d = ((float)s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_Int16_To_Float(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.Float);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt16ToFloat_Delegate convertInt16ToFloat = (ConvertInt16ToFloat_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            short* src = (short*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                float d = convertInt16ToFloat(s);
                *(uint*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.SingleToUInt32Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = *src++;
                float d = convertInt16ToFloat(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_Int16_To_Float_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.Float);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            short* src = (short*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                float d = ((float)s);
                *(uint*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.SingleToUInt32Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = *src++;
                float d = ((float)s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_UInt32_To_Float(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.Float);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt32ToFloat_Delegate convertUInt32ToFloat = (ConvertUInt32ToFloat_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            uint* src = (uint*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                float d = convertUInt32ToFloat(s);
                *(uint*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.SingleToUInt32Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = *src++;
                float d = convertUInt32ToFloat(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_UInt32_To_Float_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.Float);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            uint* src = (uint*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                float d = ((float)s);
                *(uint*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.SingleToUInt32Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = *src++;
                float d = ((float)s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_Int32_To_Float(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.Float);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt32ToFloat_Delegate convertInt32ToFloat = (ConvertInt32ToFloat_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                float d = convertInt32ToFloat(s);
                *(uint*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.SingleToUInt32Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = *src++;
                float d = convertInt32ToFloat(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_Int32_To_Float_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.Float);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                float d = ((float)s);
                *(uint*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.SingleToUInt32Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = *src++;
                float d = ((float)s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_UInt64_To_Float(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.Float);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt64ToFloat_Delegate convertUInt64ToFloat = (ConvertUInt64ToFloat_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                float d = convertUInt64ToFloat(s);
                *(uint*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.SingleToUInt32Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = *src++;
                float d = convertUInt64ToFloat(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_UInt64_To_Float_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.Float);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                float d = ((float)s);
                *(uint*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.SingleToUInt32Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = *src++;
                float d = ((float)s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_Int64_To_Float(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.Float);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt64ToFloat_Delegate convertInt64ToFloat = (ConvertInt64ToFloat_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            long* src = (long*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                float d = convertInt64ToFloat(s);
                *(uint*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.SingleToUInt32Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = *src++;
                float d = convertInt64ToFloat(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_Int64_To_Float_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.Float);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            long* src = (long*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                float d = ((float)s);
                *(uint*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.SingleToUInt32Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = *src++;
                float d = ((float)s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_Float_To_Float(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.Float);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertFloatToFloat_Delegate convertFloatToFloat = (ConvertFloatToFloat_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            float* src = (float*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(uint*)src);
                src++;
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                float d = convertFloatToFloat(s);
                *(uint*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.SingleToUInt32Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = *src++;
                float d = convertFloatToFloat(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_Float_To_Float_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.Float);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        nuint byteCount = checked((nuint)n * 4);
        Buffer.MemoryCopy(
            input.DataPtr,
            output.DataPtr,
            (ulong)byteCount,
            (ulong)byteCount);
        return n;
    }

    public static long B2B_Double_To_Float(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.Float);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertDoubleToFloat_Delegate convertDoubleToFloat = (ConvertDoubleToFloat_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(ulong*)src);
                src++;
                double s = System.BitConverter.UInt64BitsToDouble(s_bits);
                float d = convertDoubleToFloat(s);
                *(uint*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.SingleToUInt32Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = *src++;
                float d = convertDoubleToFloat(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long B2B_Double_To_Float_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.Float);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(ulong*)src);
                src++;
                double s = System.BitConverter.UInt64BitsToDouble(s_bits);
                float d = DefaultConversionsToFloat.ConvertDoubleToFloat_Default(s);
                *(uint*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.SingleToUInt32Bits(d));
                dst++;
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            float* dst = (float*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = *src++;
                float d = DefaultConversionsToFloat.ConvertDoubleToFloat_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

}
