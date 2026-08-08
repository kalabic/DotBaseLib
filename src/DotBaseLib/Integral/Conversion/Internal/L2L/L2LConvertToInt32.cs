using DotBase.Buffers;
using DotBase.Integral.Conversion.Numeric;
using DotBase.Integral.Conversion.Numeric.Defaults;
using System.Diagnostics;

namespace DotBase.Integral.Conversion.Internal.L2L;


internal static unsafe class L2LConvertToInt32
{
    internal static void AddToTable(IConversionDelegateTable table)
    {
        table.SetCustomFunc(L2L_UInt8_To_Int32, ByteOrder.LittleEndian, IntegralType.UInt8, ByteOrder.LittleEndian, IntegralType.Int32);
        table.SetCustomFunc(L2L_Int8_To_Int32, ByteOrder.LittleEndian, IntegralType.Int8, ByteOrder.LittleEndian, IntegralType.Int32);
        table.SetCustomFunc(L2L_UInt16_To_Int32, ByteOrder.LittleEndian, IntegralType.UInt16, ByteOrder.LittleEndian, IntegralType.Int32);
        table.SetCustomFunc(L2L_Int16_To_Int32, ByteOrder.LittleEndian, IntegralType.Int16, ByteOrder.LittleEndian, IntegralType.Int32);
        table.SetCustomFunc(L2L_UInt32_To_Int32, ByteOrder.LittleEndian, IntegralType.UInt32, ByteOrder.LittleEndian, IntegralType.Int32);
        table.SetCustomFunc(L2L_Int32_To_Int32, ByteOrder.LittleEndian, IntegralType.Int32, ByteOrder.LittleEndian, IntegralType.Int32);
        table.SetCustomFunc(L2L_UInt64_To_Int32, ByteOrder.LittleEndian, IntegralType.UInt64, ByteOrder.LittleEndian, IntegralType.Int32);
        table.SetCustomFunc(L2L_Int64_To_Int32, ByteOrder.LittleEndian, IntegralType.Int64, ByteOrder.LittleEndian, IntegralType.Int32);
        table.SetCustomFunc(L2L_Float_To_Int32, ByteOrder.LittleEndian, IntegralType.Float, ByteOrder.LittleEndian, IntegralType.Int32);
        table.SetCustomFunc(L2L_Double_To_Int32, ByteOrder.LittleEndian, IntegralType.Double, ByteOrder.LittleEndian, IntegralType.Int32);

        table.SetDefaultFunc(L2L_UInt8_To_Int32_Default, ByteOrder.LittleEndian, IntegralType.UInt8, ByteOrder.LittleEndian, IntegralType.Int32);
        table.SetDefaultFunc(L2L_Int8_To_Int32_Default, ByteOrder.LittleEndian, IntegralType.Int8, ByteOrder.LittleEndian, IntegralType.Int32);
        table.SetDefaultFunc(L2L_UInt16_To_Int32_Default, ByteOrder.LittleEndian, IntegralType.UInt16, ByteOrder.LittleEndian, IntegralType.Int32);
        table.SetDefaultFunc(L2L_Int16_To_Int32_Default, ByteOrder.LittleEndian, IntegralType.Int16, ByteOrder.LittleEndian, IntegralType.Int32);
        table.SetDefaultFunc(L2L_UInt32_To_Int32_Default, ByteOrder.LittleEndian, IntegralType.UInt32, ByteOrder.LittleEndian, IntegralType.Int32);
        table.SetDefaultFunc(L2L_Int32_To_Int32_Default, ByteOrder.LittleEndian, IntegralType.Int32, ByteOrder.LittleEndian, IntegralType.Int32);
        table.SetDefaultFunc(L2L_UInt64_To_Int32_Default, ByteOrder.LittleEndian, IntegralType.UInt64, ByteOrder.LittleEndian, IntegralType.Int32);
        table.SetDefaultFunc(L2L_Int64_To_Int32_Default, ByteOrder.LittleEndian, IntegralType.Int64, ByteOrder.LittleEndian, IntegralType.Int32);
        table.SetDefaultFunc(L2L_Float_To_Int32_Default, ByteOrder.LittleEndian, IntegralType.Float, ByteOrder.LittleEndian, IntegralType.Int32);
        table.SetDefaultFunc(L2L_Double_To_Int32_Default, ByteOrder.LittleEndian, IntegralType.Double, ByteOrder.LittleEndian, IntegralType.Int32);
    }

    public static long L2L_UInt8_To_Int32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt8ToInt32_Delegate convertUInt8ToInt32 = context.ToInt32.ConvertUInt8ToInt32;

        if (BitConverter.IsLittleEndian)
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
        else
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
    }

    public static long L2L_UInt8_To_Int32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
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
                int d = DefaultConversionsToInt32.ConvertUInt8ToInt32_Default(s);
                *dst++ = d;
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
                int d = DefaultConversionsToInt32.ConvertUInt8ToInt32_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Int8_To_Int32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt8ToInt32_Delegate convertInt8ToInt32 = context.ToInt32.ConvertInt8ToInt32;

        if (BitConverter.IsLittleEndian)
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
        else
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
    }

    public static long L2L_Int8_To_Int32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
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
                int d = DefaultConversionsToInt32.ConvertInt8ToInt32_Default(s);
                *dst++ = d;
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
                int d = DefaultConversionsToInt32.ConvertInt8ToInt32_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_UInt16_To_Int32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt16ToInt32_Delegate convertUInt16ToInt32 = context.ToInt32.ConvertUInt16ToInt32;

        if (BitConverter.IsLittleEndian)
        {
            ushort* src = (ushort*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = *src++;
                int d = convertUInt16ToInt32(s);
                *dst++ = d;
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
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_UInt16_To_Int32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
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
                int d = DefaultConversionsToInt32.ConvertUInt16ToInt32_Default(s);
                *dst++ = d;
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
                int d = DefaultConversionsToInt32.ConvertUInt16ToInt32_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Int16_To_Int32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt16ToInt32_Delegate convertInt16ToInt32 = context.ToInt32.ConvertInt16ToInt32;

        if (BitConverter.IsLittleEndian)
        {
            short* src = (short*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = *src++;
                int d = convertInt16ToInt32(s);
                *dst++ = d;
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
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Int16_To_Int32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
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
                int d = DefaultConversionsToInt32.ConvertInt16ToInt32_Default(s);
                *dst++ = d;
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
                int d = DefaultConversionsToInt32.ConvertInt16ToInt32_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_UInt32_To_Int32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt32ToInt32_Delegate convertUInt32ToInt32 = context.ToInt32.ConvertUInt32ToInt32;

        if (BitConverter.IsLittleEndian)
        {
            uint* src = (uint*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = *src++;
                int d = convertUInt32ToInt32(s);
                *dst++ = d;
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
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_UInt32_To_Int32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
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
                *dst++ = d;
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
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Int32_To_Int32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt32ToInt32_Delegate convertInt32ToInt32 = context.ToInt32.ConvertInt32ToInt32;

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = *src++;
                int d = convertInt32ToInt32(s);
                *dst++ = d;
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
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Int32_To_Int32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);
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

    public static long L2L_UInt64_To_Int32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt64ToInt32_Delegate convertUInt64ToInt32 = context.ToInt32.ConvertUInt64ToInt32;

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = *src++;
                int d = convertUInt64ToInt32(s);
                *dst++ = d;
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
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_UInt64_To_Int32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
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
                *dst++ = d;
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
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Int64_To_Int32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt64ToInt32_Delegate convertInt64ToInt32 = context.ToInt32.ConvertInt64ToInt32;

        if (BitConverter.IsLittleEndian)
        {
            long* src = (long*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = *src++;
                int d = convertInt64ToInt32(s);
                *dst++ = d;
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
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Int64_To_Int32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
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
                *dst++ = d;
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
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Float_To_Int32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertFloatToInt32_Delegate convertFloatToInt32 = context.ToInt32.ConvertFloatToInt32;

        if (BitConverter.IsLittleEndian)
        {
            float* src = (float*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = *src++;
                int d = convertFloatToInt32(s);
                *dst++ = d;
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
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Float_To_Int32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
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
                *dst++ = d;
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
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Double_To_Int32(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.Int32);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertDoubleToInt32_Delegate convertDoubleToInt32 = context.ToInt32.ConvertDoubleToInt32;

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            int* dst = (int*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = *src++;
                int d = convertDoubleToInt32(s);
                *dst++ = d;
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
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long L2L_Double_To_Int32_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
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
                *dst++ = d;
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
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

}
