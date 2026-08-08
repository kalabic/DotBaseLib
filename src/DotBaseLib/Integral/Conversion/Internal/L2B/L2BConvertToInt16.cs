using DotBase.Buffers;
using DotBase.Integral.Conversion.Numeric;
using DotBase.Integral.Conversion.Numeric.Defaults;
using DotBase.Integral.Internal;
using System.Diagnostics;

namespace DotBase.Integral.Conversion.Internal.L2B;


internal static unsafe class L2BConvertToInt16
{
    internal static void AddToTable(IConversionDelegateTable table)
    {
        table.SetCustomFunc(L2B_UInt8_To_Int16, ByteOrder.LittleEndian, IntegralType.UInt8, ByteOrder.BigEndian, IntegralType.Int16);
        table.SetCustomFunc(L2B_Int8_To_Int16, ByteOrder.LittleEndian, IntegralType.Int8, ByteOrder.BigEndian, IntegralType.Int16);
        table.SetCustomFunc(L2B_UInt16_To_Int16, ByteOrder.LittleEndian, IntegralType.UInt16, ByteOrder.BigEndian, IntegralType.Int16);
        table.SetCustomFunc(L2B_Int16_To_Int16, ByteOrder.LittleEndian, IntegralType.Int16, ByteOrder.BigEndian, IntegralType.Int16);
        table.SetCustomFunc(L2B_UInt32_To_Int16, ByteOrder.LittleEndian, IntegralType.UInt32, ByteOrder.BigEndian, IntegralType.Int16);
        table.SetCustomFunc(L2B_Int32_To_Int16, ByteOrder.LittleEndian, IntegralType.Int32, ByteOrder.BigEndian, IntegralType.Int16);
        table.SetCustomFunc(L2B_UInt64_To_Int16, ByteOrder.LittleEndian, IntegralType.UInt64, ByteOrder.BigEndian, IntegralType.Int16);
        table.SetCustomFunc(L2B_Int64_To_Int16, ByteOrder.LittleEndian, IntegralType.Int64, ByteOrder.BigEndian, IntegralType.Int16);
        table.SetCustomFunc(L2B_Float_To_Int16, ByteOrder.LittleEndian, IntegralType.Float, ByteOrder.BigEndian, IntegralType.Int16);
        table.SetCustomFunc(L2B_Double_To_Int16, ByteOrder.LittleEndian, IntegralType.Double, ByteOrder.BigEndian, IntegralType.Int16);

        table.SetDefaultFunc(L2B_UInt8_To_Int16_Default, ByteOrder.LittleEndian, IntegralType.UInt8, ByteOrder.BigEndian, IntegralType.Int16);
        table.SetDefaultFunc(L2B_Int8_To_Int16_Default, ByteOrder.LittleEndian, IntegralType.Int8, ByteOrder.BigEndian, IntegralType.Int16);
        table.SetDefaultFunc(L2B_UInt16_To_Int16_Default, ByteOrder.LittleEndian, IntegralType.UInt16, ByteOrder.BigEndian, IntegralType.Int16);
        table.SetDefaultFunc(L2B_Int16_To_Int16_Default, ByteOrder.LittleEndian, IntegralType.Int16, ByteOrder.BigEndian, IntegralType.Int16);
        table.SetDefaultFunc(L2B_UInt32_To_Int16_Default, ByteOrder.LittleEndian, IntegralType.UInt32, ByteOrder.BigEndian, IntegralType.Int16);
        table.SetDefaultFunc(L2B_Int32_To_Int16_Default, ByteOrder.LittleEndian, IntegralType.Int32, ByteOrder.BigEndian, IntegralType.Int16);
        table.SetDefaultFunc(L2B_UInt64_To_Int16_Default, ByteOrder.LittleEndian, IntegralType.UInt64, ByteOrder.BigEndian, IntegralType.Int16);
        table.SetDefaultFunc(L2B_Int64_To_Int16_Default, ByteOrder.LittleEndian, IntegralType.Int64, ByteOrder.BigEndian, IntegralType.Int16);
        table.SetDefaultFunc(L2B_Float_To_Int16_Default, ByteOrder.LittleEndian, IntegralType.Float, ByteOrder.BigEndian, IntegralType.Int16);
        table.SetDefaultFunc(L2B_Double_To_Int16_Default, ByteOrder.LittleEndian, IntegralType.Double, ByteOrder.BigEndian, IntegralType.Int16);
    }

    public static long L2B_UInt8_To_Int16(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.Int16);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt8ToInt16_Delegate convertUInt8ToInt16 = context.ToInt16.ConvertUInt8ToInt16;

        if (BitConverter.IsLittleEndian)
        {
            byte* src = (byte*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                short d = convertUInt8ToInt16(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                short d = convertUInt8ToInt16(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt8_To_Int16_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.Int16);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            byte* src = (byte*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                short d = DefaultConversionsToInt16.ConvertUInt8ToInt16_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                short d = DefaultConversionsToInt16.ConvertUInt8ToInt16_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int8_To_Int16(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.Int16);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt8ToInt16_Delegate convertInt8ToInt16 = context.ToInt16.ConvertInt8ToInt16;

        if (BitConverter.IsLittleEndian)
        {
            sbyte* src = (sbyte*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                short d = convertInt8ToInt16(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                short d = convertInt8ToInt16(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int8_To_Int16_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.Int16);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            sbyte* src = (sbyte*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                short d = DefaultConversionsToInt16.ConvertInt8ToInt16_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                short d = DefaultConversionsToInt16.ConvertInt8ToInt16_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt16_To_Int16(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.Int16);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt16ToInt16_Delegate convertUInt16ToInt16 = context.ToInt16.ConvertUInt16ToInt16;

        if (BitConverter.IsLittleEndian)
        {
            ushort* src = (ushort*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = *src++;
                short d = convertUInt16ToInt16(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                short d = convertUInt16ToInt16(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt16_To_Int16_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.Int16);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            ushort* src = (ushort*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = *src++;
                short d = DefaultConversionsToInt16.ConvertUInt16ToInt16_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                short d = DefaultConversionsToInt16.ConvertUInt16ToInt16_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int16_To_Int16(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.Int16);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt16ToInt16_Delegate convertInt16ToInt16 = context.ToInt16.ConvertInt16ToInt16;

        if (BitConverter.IsLittleEndian)
        {
            short* src = (short*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = *src++;
                short d = convertInt16ToInt16(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                short d = convertInt16ToInt16(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int16_To_Int16_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.Int16);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        // Pure 16-bit wire reverse; bulk byte-pair swap (see EndianSwap16BitLanes).
        EndianSwap.Swap16BitLanes(input.DataPtr, output.DataPtr, n);
        return n;
    }

    public static long L2B_UInt32_To_Int16(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.Int16);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt32ToInt16_Delegate convertUInt32ToInt16 = context.ToInt16.ConvertUInt32ToInt16;

        if (BitConverter.IsLittleEndian)
        {
            uint* src = (uint*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = *src++;
                short d = convertUInt32ToInt16(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                short d = convertUInt32ToInt16(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt32_To_Int16_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.Int16);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            uint* src = (uint*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = *src++;
                short d = DefaultConversionsToInt16.ConvertUInt32ToInt16_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                short d = DefaultConversionsToInt16.ConvertUInt32ToInt16_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int32_To_Int16(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.Int16);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt32ToInt16_Delegate convertInt32ToInt16 = context.ToInt16.ConvertInt32ToInt16;

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = *src++;
                short d = convertInt32ToInt16(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                short d = convertInt32ToInt16(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int32_To_Int16_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.Int16);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = *src++;
                short d = DefaultConversionsToInt16.ConvertInt32ToInt16_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                short d = DefaultConversionsToInt16.ConvertInt32ToInt16_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt64_To_Int16(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.Int16);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt64ToInt16_Delegate convertUInt64ToInt16 = context.ToInt16.ConvertUInt64ToInt16;

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = *src++;
                short d = convertUInt64ToInt16(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                short d = convertUInt64ToInt16(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt64_To_Int16_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.Int16);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = *src++;
                short d = DefaultConversionsToInt16.ConvertUInt64ToInt16_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                short d = DefaultConversionsToInt16.ConvertUInt64ToInt16_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int64_To_Int16(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.Int16);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt64ToInt16_Delegate convertInt64ToInt16 = context.ToInt16.ConvertInt64ToInt16;

        if (BitConverter.IsLittleEndian)
        {
            long* src = (long*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = *src++;
                short d = convertInt64ToInt16(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                short d = convertInt64ToInt16(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int64_To_Int16_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.Int16);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            long* src = (long*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = *src++;
                short d = DefaultConversionsToInt16.ConvertInt64ToInt16_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                short d = DefaultConversionsToInt16.ConvertInt64ToInt16_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Float_To_Int16(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.Int16);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertFloatToInt16_Delegate convertFloatToInt16 = context.ToInt16.ConvertFloatToInt16;

        if (BitConverter.IsLittleEndian)
        {
            float* src = (float*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = *src++;
                short d = convertFloatToInt16(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(uint*)src);
                src++;
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                short d = convertFloatToInt16(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Float_To_Int16_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.Int16);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            float* src = (float*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = *src++;
                short d = DefaultConversionsToInt16.ConvertFloatToInt16_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(uint*)src);
                src++;
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                short d = DefaultConversionsToInt16.ConvertFloatToInt16_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Double_To_Int16(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.Int16);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertDoubleToInt16_Delegate convertDoubleToInt16 = context.ToInt16.ConvertDoubleToInt16;

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = *src++;
                short d = convertDoubleToInt16(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(ulong*)src);
                src++;
                double s = System.BitConverter.UInt64BitsToDouble(s_bits);
                short d = convertDoubleToInt16(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Double_To_Int16_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.Int16);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = *src++;
                short d = DefaultConversionsToInt16.ConvertDoubleToInt16_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            short* dst = (short*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(ulong*)src);
                src++;
                double s = System.BitConverter.UInt64BitsToDouble(s_bits);
                short d = DefaultConversionsToInt16.ConvertDoubleToInt16_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

}
