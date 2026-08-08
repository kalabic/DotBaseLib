using DotBase.Buffers;
using DotBase.Integral.Conversion.Numeric;
using DotBase.Integral.Conversion.Numeric.Defaults;
using System.Diagnostics;

namespace DotBase.Integral.Conversion.Internal.B2L;


internal static unsafe class B2LConvertToUInt64
{
    internal static void AddToTable(IConversionDelegateTable table)
    {
        table.SetCustomFunc(B2L_UInt8_To_UInt64, ByteOrder.BigEndian, IntegralType.UInt8, ByteOrder.LittleEndian, IntegralType.UInt64);
        table.SetCustomFunc(B2L_Int8_To_UInt64, ByteOrder.BigEndian, IntegralType.Int8, ByteOrder.LittleEndian, IntegralType.UInt64);
        table.SetCustomFunc(B2L_UInt16_To_UInt64, ByteOrder.BigEndian, IntegralType.UInt16, ByteOrder.LittleEndian, IntegralType.UInt64);
        table.SetCustomFunc(B2L_Int16_To_UInt64, ByteOrder.BigEndian, IntegralType.Int16, ByteOrder.LittleEndian, IntegralType.UInt64);
        table.SetCustomFunc(B2L_UInt32_To_UInt64, ByteOrder.BigEndian, IntegralType.UInt32, ByteOrder.LittleEndian, IntegralType.UInt64);
        table.SetCustomFunc(B2L_Int32_To_UInt64, ByteOrder.BigEndian, IntegralType.Int32, ByteOrder.LittleEndian, IntegralType.UInt64);
        table.SetCustomFunc(B2L_UInt64_To_UInt64, ByteOrder.BigEndian, IntegralType.UInt64, ByteOrder.LittleEndian, IntegralType.UInt64);
        table.SetCustomFunc(B2L_Int64_To_UInt64, ByteOrder.BigEndian, IntegralType.Int64, ByteOrder.LittleEndian, IntegralType.UInt64);
        table.SetCustomFunc(B2L_Float_To_UInt64, ByteOrder.BigEndian, IntegralType.Float, ByteOrder.LittleEndian, IntegralType.UInt64);
        table.SetCustomFunc(B2L_Double_To_UInt64, ByteOrder.BigEndian, IntegralType.Double, ByteOrder.LittleEndian, IntegralType.UInt64);

        table.SetDefaultFunc(B2L_UInt8_To_UInt64_Default, ByteOrder.BigEndian, IntegralType.UInt8, ByteOrder.LittleEndian, IntegralType.UInt64);
        table.SetDefaultFunc(B2L_Int8_To_UInt64_Default, ByteOrder.BigEndian, IntegralType.Int8, ByteOrder.LittleEndian, IntegralType.UInt64);
        table.SetDefaultFunc(B2L_UInt16_To_UInt64_Default, ByteOrder.BigEndian, IntegralType.UInt16, ByteOrder.LittleEndian, IntegralType.UInt64);
        table.SetDefaultFunc(B2L_Int16_To_UInt64_Default, ByteOrder.BigEndian, IntegralType.Int16, ByteOrder.LittleEndian, IntegralType.UInt64);
        table.SetDefaultFunc(B2L_UInt32_To_UInt64_Default, ByteOrder.BigEndian, IntegralType.UInt32, ByteOrder.LittleEndian, IntegralType.UInt64);
        table.SetDefaultFunc(B2L_Int32_To_UInt64_Default, ByteOrder.BigEndian, IntegralType.Int32, ByteOrder.LittleEndian, IntegralType.UInt64);
        table.SetDefaultFunc(B2L_UInt64_To_UInt64_Default, ByteOrder.BigEndian, IntegralType.UInt64, ByteOrder.LittleEndian, IntegralType.UInt64);
        table.SetDefaultFunc(B2L_Int64_To_UInt64_Default, ByteOrder.BigEndian, IntegralType.Int64, ByteOrder.LittleEndian, IntegralType.UInt64);
        table.SetDefaultFunc(B2L_Float_To_UInt64_Default, ByteOrder.BigEndian, IntegralType.Float, ByteOrder.LittleEndian, IntegralType.UInt64);
        table.SetDefaultFunc(B2L_Double_To_UInt64_Default, ByteOrder.BigEndian, IntegralType.Double, ByteOrder.LittleEndian, IntegralType.UInt64);
    }

    public static long B2L_UInt8_To_UInt64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt8ToUInt64_Delegate convertUInt8ToUInt64 = context.ToUInt64.ConvertUInt8ToUInt64;

        if (BitConverter.IsLittleEndian)
        {
            byte* src = (byte*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                ulong d = convertUInt8ToUInt64(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                ulong d = convertUInt8ToUInt64(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long B2L_UInt8_To_UInt64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            byte* src = (byte*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                ulong d = DefaultConversionsToUInt64.ConvertUInt8ToUInt64_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                ulong d = DefaultConversionsToUInt64.ConvertUInt8ToUInt64_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long B2L_Int8_To_UInt64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt8ToUInt64_Delegate convertInt8ToUInt64 = context.ToUInt64.ConvertInt8ToUInt64;

        if (BitConverter.IsLittleEndian)
        {
            sbyte* src = (sbyte*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                ulong d = convertInt8ToUInt64(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                ulong d = convertInt8ToUInt64(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long B2L_Int8_To_UInt64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            sbyte* src = (sbyte*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                ulong d = DefaultConversionsToUInt64.ConvertInt8ToUInt64_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                ulong d = DefaultConversionsToUInt64.ConvertInt8ToUInt64_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long B2L_UInt16_To_UInt64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt16ToUInt64_Delegate convertUInt16ToUInt64 = context.ToUInt64.ConvertUInt16ToUInt64;

        if (BitConverter.IsLittleEndian)
        {
            ushort* src = (ushort*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                ulong d = convertUInt16ToUInt64(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = *src++;
                ulong d = convertUInt16ToUInt64(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long B2L_UInt16_To_UInt64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            ushort* src = (ushort*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                ulong d = DefaultConversionsToUInt64.ConvertUInt16ToUInt64_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = *src++;
                ulong d = DefaultConversionsToUInt64.ConvertUInt16ToUInt64_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long B2L_Int16_To_UInt64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt16ToUInt64_Delegate convertInt16ToUInt64 = context.ToUInt64.ConvertInt16ToUInt64;

        if (BitConverter.IsLittleEndian)
        {
            short* src = (short*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                ulong d = convertInt16ToUInt64(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = *src++;
                ulong d = convertInt16ToUInt64(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long B2L_Int16_To_UInt64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            short* src = (short*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                ulong d = DefaultConversionsToUInt64.ConvertInt16ToUInt64_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = *src++;
                ulong d = DefaultConversionsToUInt64.ConvertInt16ToUInt64_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long B2L_UInt32_To_UInt64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt32ToUInt64_Delegate convertUInt32ToUInt64 = context.ToUInt64.ConvertUInt32ToUInt64;

        if (BitConverter.IsLittleEndian)
        {
            uint* src = (uint*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                ulong d = convertUInt32ToUInt64(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = *src++;
                ulong d = convertUInt32ToUInt64(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long B2L_UInt32_To_UInt64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            uint* src = (uint*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                ulong d = DefaultConversionsToUInt64.ConvertUInt32ToUInt64_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = *src++;
                ulong d = DefaultConversionsToUInt64.ConvertUInt32ToUInt64_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long B2L_Int32_To_UInt64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt32ToUInt64_Delegate convertInt32ToUInt64 = context.ToUInt64.ConvertInt32ToUInt64;

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                ulong d = convertInt32ToUInt64(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = *src++;
                ulong d = convertInt32ToUInt64(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long B2L_Int32_To_UInt64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                ulong d = DefaultConversionsToUInt64.ConvertInt32ToUInt64_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = *src++;
                ulong d = DefaultConversionsToUInt64.ConvertInt32ToUInt64_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long B2L_UInt64_To_UInt64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt64ToUInt64_Delegate convertUInt64ToUInt64 = context.ToUInt64.ConvertUInt64ToUInt64;

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                ulong d = convertUInt64ToUInt64(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = *src++;
                ulong d = convertUInt64ToUInt64(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long B2L_UInt64_To_UInt64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
            }
            return n;
        }
    }

    public static long B2L_Int64_To_UInt64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt64ToUInt64_Delegate convertInt64ToUInt64 = context.ToUInt64.ConvertInt64ToUInt64;

        if (BitConverter.IsLittleEndian)
        {
            long* src = (long*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                ulong d = convertInt64ToUInt64(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = *src++;
                ulong d = convertInt64ToUInt64(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long B2L_Int64_To_UInt64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            long* src = (long*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                ulong d = DefaultConversionsToUInt64.ConvertInt64ToUInt64_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = *src++;
                ulong d = DefaultConversionsToUInt64.ConvertInt64ToUInt64_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long B2L_Float_To_UInt64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertFloatToUInt64_Delegate convertFloatToUInt64 = context.ToUInt64.ConvertFloatToUInt64;

        if (BitConverter.IsLittleEndian)
        {
            float* src = (float*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(uint*)src);
                src++;
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                ulong d = convertFloatToUInt64(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = *src++;
                ulong d = convertFloatToUInt64(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long B2L_Float_To_UInt64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            float* src = (float*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(uint*)src);
                src++;
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                ulong d = DefaultConversionsToUInt64.ConvertFloatToUInt64_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = *src++;
                ulong d = DefaultConversionsToUInt64.ConvertFloatToUInt64_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long B2L_Double_To_UInt64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertDoubleToUInt64_Delegate convertDoubleToUInt64 = context.ToUInt64.ConvertDoubleToUInt64;

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(ulong*)src);
                src++;
                double s = System.BitConverter.UInt64BitsToDouble(s_bits);
                ulong d = convertDoubleToUInt64(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = *src++;
                ulong d = convertDoubleToUInt64(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

    public static long B2L_Double_To_UInt64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(ulong*)src);
                src++;
                double s = System.BitConverter.UInt64BitsToDouble(s_bits);
                ulong d = DefaultConversionsToUInt64.ConvertDoubleToUInt64_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = *src++;
                ulong d = DefaultConversionsToUInt64.ConvertDoubleToUInt64_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
    }

}
