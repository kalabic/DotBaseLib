using DotBase.Buffers;
using DotBase.Integral.Conversion.Numeric;
using DotBase.Integral.Conversion.Numeric.Defaults;
using DotBase.Integral.Internal;
using System.Diagnostics;

namespace DotBase.Integral.Conversion.Internal.Standard.L2B;


internal static unsafe class L2BConvertToUInt16
{
    internal static void AddToTable(IConversionDelegateTable table)
    {
        table.SetCustomFunc(L2B_UInt8_To_UInt16, ByteOrder.LittleEndian, IntegralType.UInt8, ByteOrder.BigEndian, IntegralType.UInt16);
        table.SetCustomFunc(L2B_Int8_To_UInt16, ByteOrder.LittleEndian, IntegralType.Int8, ByteOrder.BigEndian, IntegralType.UInt16);
        table.SetCustomFunc(L2B_UInt16_To_UInt16, ByteOrder.LittleEndian, IntegralType.UInt16, ByteOrder.BigEndian, IntegralType.UInt16);
        table.SetCustomFunc(L2B_Int16_To_UInt16, ByteOrder.LittleEndian, IntegralType.Int16, ByteOrder.BigEndian, IntegralType.UInt16);
        table.SetCustomFunc(L2B_UInt32_To_UInt16, ByteOrder.LittleEndian, IntegralType.UInt32, ByteOrder.BigEndian, IntegralType.UInt16);
        table.SetCustomFunc(L2B_Int32_To_UInt16, ByteOrder.LittleEndian, IntegralType.Int32, ByteOrder.BigEndian, IntegralType.UInt16);
        table.SetCustomFunc(L2B_UInt64_To_UInt16, ByteOrder.LittleEndian, IntegralType.UInt64, ByteOrder.BigEndian, IntegralType.UInt16);
        table.SetCustomFunc(L2B_Int64_To_UInt16, ByteOrder.LittleEndian, IntegralType.Int64, ByteOrder.BigEndian, IntegralType.UInt16);
        table.SetCustomFunc(L2B_Float_To_UInt16, ByteOrder.LittleEndian, IntegralType.Float, ByteOrder.BigEndian, IntegralType.UInt16);
        table.SetCustomFunc(L2B_Double_To_UInt16, ByteOrder.LittleEndian, IntegralType.Double, ByteOrder.BigEndian, IntegralType.UInt16);

        table.SetDefaultFunc(L2B_UInt8_To_UInt16_Default, ByteOrder.LittleEndian, IntegralType.UInt8, ByteOrder.BigEndian, IntegralType.UInt16);
        table.SetDefaultFunc(L2B_Int8_To_UInt16_Default, ByteOrder.LittleEndian, IntegralType.Int8, ByteOrder.BigEndian, IntegralType.UInt16);
        table.SetDefaultFunc(L2B_UInt16_To_UInt16_Default, ByteOrder.LittleEndian, IntegralType.UInt16, ByteOrder.BigEndian, IntegralType.UInt16);
        table.SetDefaultFunc(L2B_Int16_To_UInt16_Default, ByteOrder.LittleEndian, IntegralType.Int16, ByteOrder.BigEndian, IntegralType.UInt16);
        table.SetDefaultFunc(L2B_UInt32_To_UInt16_Default, ByteOrder.LittleEndian, IntegralType.UInt32, ByteOrder.BigEndian, IntegralType.UInt16);
        table.SetDefaultFunc(L2B_Int32_To_UInt16_Default, ByteOrder.LittleEndian, IntegralType.Int32, ByteOrder.BigEndian, IntegralType.UInt16);
        table.SetDefaultFunc(L2B_UInt64_To_UInt16_Default, ByteOrder.LittleEndian, IntegralType.UInt64, ByteOrder.BigEndian, IntegralType.UInt16);
        table.SetDefaultFunc(L2B_Int64_To_UInt16_Default, ByteOrder.LittleEndian, IntegralType.Int64, ByteOrder.BigEndian, IntegralType.UInt16);
        table.SetDefaultFunc(L2B_Float_To_UInt16_Default, ByteOrder.LittleEndian, IntegralType.Float, ByteOrder.BigEndian, IntegralType.UInt16);
        table.SetDefaultFunc(L2B_Double_To_UInt16_Default, ByteOrder.LittleEndian, IntegralType.Double, ByteOrder.BigEndian, IntegralType.UInt16);
    }

    public static long L2B_UInt8_To_UInt16(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt16);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt8ToUInt16_Delegate convertUInt8ToUInt16 = (ConvertUInt8ToUInt16_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            byte* src = (byte*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                ushort d = convertUInt8ToUInt16(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                ushort d = convertUInt8ToUInt16(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt8_To_UInt16_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt16);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            byte* src = (byte*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                ushort d = ((ushort)s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                ushort d = ((ushort)s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int8_To_UInt16(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt16);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt8ToUInt16_Delegate convertInt8ToUInt16 = (ConvertInt8ToUInt16_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            sbyte* src = (sbyte*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                ushort d = convertInt8ToUInt16(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                ushort d = convertInt8ToUInt16(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int8_To_UInt16_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt16);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            sbyte* src = (sbyte*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                ushort d = DefaultConversionsToUInt16.ConvertInt8ToUInt16_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                ushort d = DefaultConversionsToUInt16.ConvertInt8ToUInt16_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt16_To_UInt16(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt16);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt16ToUInt16_Delegate convertUInt16ToUInt16 = (ConvertUInt16ToUInt16_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            ushort* src = (ushort*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = *src++;
                ushort d = convertUInt16ToUInt16(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                ushort d = convertUInt16ToUInt16(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt16_To_UInt16_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt16);
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

    public static long L2B_Int16_To_UInt16(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt16);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt16ToUInt16_Delegate convertInt16ToUInt16 = (ConvertInt16ToUInt16_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            short* src = (short*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = *src++;
                ushort d = convertInt16ToUInt16(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                ushort d = convertInt16ToUInt16(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int16_To_UInt16_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt16);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            short* src = (short*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = *src++;
                ushort d = DefaultConversionsToUInt16.ConvertInt16ToUInt16_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                ushort d = DefaultConversionsToUInt16.ConvertInt16ToUInt16_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt32_To_UInt16(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt16);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt32ToUInt16_Delegate convertUInt32ToUInt16 = (ConvertUInt32ToUInt16_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            uint* src = (uint*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = *src++;
                ushort d = convertUInt32ToUInt16(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                ushort d = convertUInt32ToUInt16(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt32_To_UInt16_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt16);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            uint* src = (uint*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = *src++;
                ushort d = DefaultConversionsToUInt16.ConvertUInt32ToUInt16_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                ushort d = DefaultConversionsToUInt16.ConvertUInt32ToUInt16_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int32_To_UInt16(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt16);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt32ToUInt16_Delegate convertInt32ToUInt16 = (ConvertInt32ToUInt16_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = *src++;
                ushort d = convertInt32ToUInt16(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                ushort d = convertInt32ToUInt16(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int32_To_UInt16_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt16);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = *src++;
                ushort d = DefaultConversionsToUInt16.ConvertInt32ToUInt16_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                ushort d = DefaultConversionsToUInt16.ConvertInt32ToUInt16_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt64_To_UInt16(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt16);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt64ToUInt16_Delegate convertUInt64ToUInt16 = (ConvertUInt64ToUInt16_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = *src++;
                ushort d = convertUInt64ToUInt16(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                ushort d = convertUInt64ToUInt16(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_UInt64_To_UInt16_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt16);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = *src++;
                ushort d = DefaultConversionsToUInt16.ConvertUInt64ToUInt16_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                ushort d = DefaultConversionsToUInt16.ConvertUInt64ToUInt16_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int64_To_UInt16(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt16);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt64ToUInt16_Delegate convertInt64ToUInt16 = (ConvertInt64ToUInt16_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            long* src = (long*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = *src++;
                ushort d = convertInt64ToUInt16(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                ushort d = convertInt64ToUInt16(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Int64_To_UInt16_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt16);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            long* src = (long*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = *src++;
                ushort d = DefaultConversionsToUInt16.ConvertInt64ToUInt16_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                ushort d = DefaultConversionsToUInt16.ConvertInt64ToUInt16_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Float_To_UInt16(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt16);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertFloatToUInt16_Delegate convertFloatToUInt16 = (ConvertFloatToUInt16_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            float* src = (float*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = *src++;
                ushort d = convertFloatToUInt16(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(uint*)src);
                src++;
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                ushort d = convertFloatToUInt16(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Float_To_UInt16_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt16);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            float* src = (float*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = *src++;
                ushort d = DefaultConversionsToUInt16.ConvertFloatToUInt16_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(uint*)src);
                src++;
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                ushort d = DefaultConversionsToUInt16.ConvertFloatToUInt16_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Double_To_UInt16(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt16);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertDoubleToUInt16_Delegate convertDoubleToUInt16 = (ConvertDoubleToUInt16_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = *src++;
                ushort d = convertDoubleToUInt16(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(ulong*)src);
                src++;
                double s = System.BitConverter.UInt64BitsToDouble(s_bits);
                ushort d = convertDoubleToUInt16(s);
                *dst++ = d;
            }
            return n;
        }
    }

    public static long L2B_Double_To_UInt16_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt16);
                _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = *src++;
                ushort d = DefaultConversionsToUInt16.ConvertDoubleToUInt16_Default(s);
                *dst++ = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            ushort* dst = (ushort*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(ulong*)src);
                src++;
                double s = System.BitConverter.UInt64BitsToDouble(s_bits);
                ushort d = DefaultConversionsToUInt16.ConvertDoubleToUInt16_Default(s);
                *dst++ = d;
            }
            return n;
        }
    }

}
