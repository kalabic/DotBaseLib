using DotBase.Buffers;
using DotBase.Integral.Conversion.Numeric;
using DotBase.Integral.Conversion.Numeric.Defaults;
using System.Diagnostics;

namespace DotBase.Integral.Conversion.Internal.Standard.L2L;


internal static unsafe class L2LConvertToUInt32
{
    internal static void AddToTable(IConversionDelegateTable table)
    {
        table.SetCustomFunc(L2L_UInt8_To_UInt32, ByteOrder.LittleEndian, IntegralType.UInt8, ByteOrder.LittleEndian, IntegralType.UInt32);
        table.SetCustomFunc(L2L_Int8_To_UInt32, ByteOrder.LittleEndian, IntegralType.Int8, ByteOrder.LittleEndian, IntegralType.UInt32);
        table.SetCustomFunc(L2L_UInt16_To_UInt32, ByteOrder.LittleEndian, IntegralType.UInt16, ByteOrder.LittleEndian, IntegralType.UInt32);
        table.SetCustomFunc(L2L_Int16_To_UInt32, ByteOrder.LittleEndian, IntegralType.Int16, ByteOrder.LittleEndian, IntegralType.UInt32);
        table.SetCustomFunc(L2L_UInt32_To_UInt32, ByteOrder.LittleEndian, IntegralType.UInt32, ByteOrder.LittleEndian, IntegralType.UInt32);
        table.SetCustomFunc(L2L_Int32_To_UInt32, ByteOrder.LittleEndian, IntegralType.Int32, ByteOrder.LittleEndian, IntegralType.UInt32);
        table.SetCustomFunc(L2L_UInt64_To_UInt32, ByteOrder.LittleEndian, IntegralType.UInt64, ByteOrder.LittleEndian, IntegralType.UInt32);
        table.SetCustomFunc(L2L_Int64_To_UInt32, ByteOrder.LittleEndian, IntegralType.Int64, ByteOrder.LittleEndian, IntegralType.UInt32);
        table.SetCustomFunc(L2L_Float_To_UInt32, ByteOrder.LittleEndian, IntegralType.Float, ByteOrder.LittleEndian, IntegralType.UInt32);
        table.SetCustomFunc(L2L_Double_To_UInt32, ByteOrder.LittleEndian, IntegralType.Double, ByteOrder.LittleEndian, IntegralType.UInt32);

        table.SetDefaultFunc(L2L_UInt8_To_UInt32_Default, ByteOrder.LittleEndian, IntegralType.UInt8, ByteOrder.LittleEndian, IntegralType.UInt32);
        table.SetDefaultFunc(L2L_Int8_To_UInt32_Default, ByteOrder.LittleEndian, IntegralType.Int8, ByteOrder.LittleEndian, IntegralType.UInt32);
        table.SetDefaultFunc(L2L_UInt16_To_UInt32_Default, ByteOrder.LittleEndian, IntegralType.UInt16, ByteOrder.LittleEndian, IntegralType.UInt32);
        table.SetDefaultFunc(L2L_Int16_To_UInt32_Default, ByteOrder.LittleEndian, IntegralType.Int16, ByteOrder.LittleEndian, IntegralType.UInt32);
        table.SetDefaultFunc(L2L_UInt32_To_UInt32_Default, ByteOrder.LittleEndian, IntegralType.UInt32, ByteOrder.LittleEndian, IntegralType.UInt32);
        table.SetDefaultFunc(L2L_Int32_To_UInt32_Default, ByteOrder.LittleEndian, IntegralType.Int32, ByteOrder.LittleEndian, IntegralType.UInt32);
        table.SetDefaultFunc(L2L_UInt64_To_UInt32_Default, ByteOrder.LittleEndian, IntegralType.UInt64, ByteOrder.LittleEndian, IntegralType.UInt32);
        table.SetDefaultFunc(L2L_Int64_To_UInt32_Default, ByteOrder.LittleEndian, IntegralType.Int64, ByteOrder.LittleEndian, IntegralType.UInt32);
        table.SetDefaultFunc(L2L_Float_To_UInt32_Default, ByteOrder.LittleEndian, IntegralType.Float, ByteOrder.LittleEndian, IntegralType.UInt32);
        table.SetDefaultFunc(L2L_Double_To_UInt32_Default, ByteOrder.LittleEndian, IntegralType.Double, ByteOrder.LittleEndian, IntegralType.UInt32);
    }

    public static long L2L_UInt8_To_UInt32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt8ToUInt32_Delegate convertUInt8ToUInt32 = (ConvertUInt8ToUInt32_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            byte* src = (byte*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                uint d = convertUInt8ToUInt32(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                uint d = convertUInt8ToUInt32(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_UInt8_To_UInt32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt32);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            byte* src = (byte*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                uint d = ((uint)s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                uint d = ((uint)s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Int8_To_UInt32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt8ToUInt32_Delegate convertInt8ToUInt32 = (ConvertInt8ToUInt32_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            sbyte* src = (sbyte*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                uint d = convertInt8ToUInt32(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                uint d = convertInt8ToUInt32(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Int8_To_UInt32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt32);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            sbyte* src = (sbyte*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                uint d = DefaultConversionsToUInt32.ConvertInt8ToUInt32_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                uint d = DefaultConversionsToUInt32.ConvertInt8ToUInt32_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_UInt16_To_UInt32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt16ToUInt32_Delegate convertUInt16ToUInt32 = (ConvertUInt16ToUInt32_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            ushort* src = (ushort*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = *src++;
                uint d = convertUInt16ToUInt32(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                uint d = convertUInt16ToUInt32(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_UInt16_To_UInt32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt32);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            ushort* src = (ushort*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = *src++;
                uint d = ((uint)s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                uint d = ((uint)s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Int16_To_UInt32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt16ToUInt32_Delegate convertInt16ToUInt32 = (ConvertInt16ToUInt32_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            short* src = (short*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = *src++;
                uint d = convertInt16ToUInt32(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                uint d = convertInt16ToUInt32(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Int16_To_UInt32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt32);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            short* src = (short*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = *src++;
                uint d = DefaultConversionsToUInt32.ConvertInt16ToUInt32_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                uint d = DefaultConversionsToUInt32.ConvertInt16ToUInt32_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_UInt32_To_UInt32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt32ToUInt32_Delegate convertUInt32ToUInt32 = (ConvertUInt32ToUInt32_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            uint* src = (uint*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = *src++;
                uint d = convertUInt32ToUInt32(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                uint d = convertUInt32ToUInt32(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_UInt32_To_UInt32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt32);
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

    public static long L2L_Int32_To_UInt32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt32ToUInt32_Delegate convertInt32ToUInt32 = (ConvertInt32ToUInt32_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = *src++;
                uint d = convertInt32ToUInt32(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                uint d = convertInt32ToUInt32(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Int32_To_UInt32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt32);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = *src++;
                uint d = DefaultConversionsToUInt32.ConvertInt32ToUInt32_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                uint d = DefaultConversionsToUInt32.ConvertInt32ToUInt32_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_UInt64_To_UInt32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt64ToUInt32_Delegate convertUInt64ToUInt32 = (ConvertUInt64ToUInt32_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = *src++;
                uint d = convertUInt64ToUInt32(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                uint d = convertUInt64ToUInt32(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_UInt64_To_UInt32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt32);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = *src++;
                uint d = DefaultConversionsToUInt32.ConvertUInt64ToUInt32_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                uint d = DefaultConversionsToUInt32.ConvertUInt64ToUInt32_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Int64_To_UInt32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt64ToUInt32_Delegate convertInt64ToUInt32 = (ConvertInt64ToUInt32_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            long* src = (long*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = *src++;
                uint d = convertInt64ToUInt32(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                uint d = convertInt64ToUInt32(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Int64_To_UInt32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt32);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            long* src = (long*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = *src++;
                uint d = DefaultConversionsToUInt32.ConvertInt64ToUInt32_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                uint d = DefaultConversionsToUInt32.ConvertInt64ToUInt32_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Float_To_UInt32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertFloatToUInt32_Delegate convertFloatToUInt32 = (ConvertFloatToUInt32_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            float* src = (float*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = *src++;
                uint d = convertFloatToUInt32(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(uint*)src);
                src++;
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                uint d = convertFloatToUInt32(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Float_To_UInt32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt32);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            float* src = (float*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = *src++;
                uint d = DefaultConversionsToUInt32.ConvertFloatToUInt32_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(uint*)src);
                src++;
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                uint d = DefaultConversionsToUInt32.ConvertFloatToUInt32_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Double_To_UInt32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertDoubleToUInt32_Delegate convertDoubleToUInt32 = (ConvertDoubleToUInt32_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = *src++;
                uint d = convertDoubleToUInt32(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(ulong*)src);
                src++;
                double s = System.BitConverter.UInt64BitsToDouble(s_bits);
                uint d = convertDoubleToUInt32(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Double_To_UInt32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt32);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = *src++;
                uint d = DefaultConversionsToUInt32.ConvertDoubleToUInt32_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            uint* dst = (uint*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(ulong*)src);
                src++;
                double s = System.BitConverter.UInt64BitsToDouble(s_bits);
                uint d = DefaultConversionsToUInt32.ConvertDoubleToUInt32_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

}
