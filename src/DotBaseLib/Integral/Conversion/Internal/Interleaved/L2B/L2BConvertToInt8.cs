using DotBase.Buffers;
using DotBase.Integral.Conversion.Numeric;
using DotBase.Integral.Conversion.Numeric.Defaults;
using System.Diagnostics;

namespace DotBase.Integral.Conversion.Internal.Interleaved.L2B;


internal static unsafe class L2BConvertToInt8
{
    internal static void AddToTable(IConversionDelegateTable table)
    {
        table.SetCustomFunc(L2B_UInt8_To_Int8, ByteOrder.LittleEndian, IntegralType.UInt8, ByteOrder.BigEndian, IntegralType.Int8);
        table.SetCustomFunc(L2B_Int8_To_Int8, ByteOrder.LittleEndian, IntegralType.Int8, ByteOrder.BigEndian, IntegralType.Int8);
        table.SetCustomFunc(L2B_UInt16_To_Int8, ByteOrder.LittleEndian, IntegralType.UInt16, ByteOrder.BigEndian, IntegralType.Int8);
        table.SetCustomFunc(L2B_Int16_To_Int8, ByteOrder.LittleEndian, IntegralType.Int16, ByteOrder.BigEndian, IntegralType.Int8);
        table.SetCustomFunc(L2B_UInt32_To_Int8, ByteOrder.LittleEndian, IntegralType.UInt32, ByteOrder.BigEndian, IntegralType.Int8);
        table.SetCustomFunc(L2B_Int32_To_Int8, ByteOrder.LittleEndian, IntegralType.Int32, ByteOrder.BigEndian, IntegralType.Int8);
        table.SetCustomFunc(L2B_UInt64_To_Int8, ByteOrder.LittleEndian, IntegralType.UInt64, ByteOrder.BigEndian, IntegralType.Int8);
        table.SetCustomFunc(L2B_Int64_To_Int8, ByteOrder.LittleEndian, IntegralType.Int64, ByteOrder.BigEndian, IntegralType.Int8);
        table.SetCustomFunc(L2B_Float_To_Int8, ByteOrder.LittleEndian, IntegralType.Float, ByteOrder.BigEndian, IntegralType.Int8);
        table.SetCustomFunc(L2B_Double_To_Int8, ByteOrder.LittleEndian, IntegralType.Double, ByteOrder.BigEndian, IntegralType.Int8);

        table.SetDefaultFunc(L2B_UInt8_To_Int8_Default, ByteOrder.LittleEndian, IntegralType.UInt8, ByteOrder.BigEndian, IntegralType.Int8);
        table.SetDefaultFunc(L2B_Int8_To_Int8_Default, ByteOrder.LittleEndian, IntegralType.Int8, ByteOrder.BigEndian, IntegralType.Int8);
        table.SetDefaultFunc(L2B_UInt16_To_Int8_Default, ByteOrder.LittleEndian, IntegralType.UInt16, ByteOrder.BigEndian, IntegralType.Int8);
        table.SetDefaultFunc(L2B_Int16_To_Int8_Default, ByteOrder.LittleEndian, IntegralType.Int16, ByteOrder.BigEndian, IntegralType.Int8);
        table.SetDefaultFunc(L2B_UInt32_To_Int8_Default, ByteOrder.LittleEndian, IntegralType.UInt32, ByteOrder.BigEndian, IntegralType.Int8);
        table.SetDefaultFunc(L2B_Int32_To_Int8_Default, ByteOrder.LittleEndian, IntegralType.Int32, ByteOrder.BigEndian, IntegralType.Int8);
        table.SetDefaultFunc(L2B_UInt64_To_Int8_Default, ByteOrder.LittleEndian, IntegralType.UInt64, ByteOrder.BigEndian, IntegralType.Int8);
        table.SetDefaultFunc(L2B_Int64_To_Int8_Default, ByteOrder.LittleEndian, IntegralType.Int64, ByteOrder.BigEndian, IntegralType.Int8);
        table.SetDefaultFunc(L2B_Float_To_Int8_Default, ByteOrder.LittleEndian, IntegralType.Float, ByteOrder.BigEndian, IntegralType.Int8);
        table.SetDefaultFunc(L2B_Double_To_Int8_Default, ByteOrder.LittleEndian, IntegralType.Double, ByteOrder.BigEndian, IntegralType.Int8);
    }

    public static long L2B_UInt8_To_Int8(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.Int8);

        long n = InterleavedAccess.Resolve(
            context,
            input,
            output,
            valuesCount,
            out int srcStride,
            out int dstStride,
            out int srcLane,
            out int dstLane);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt8ToInt8_Delegate convertUInt8ToInt8 = (ConvertUInt8ToInt8_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            byte* src = (byte*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = src[i * srcStride + srcLane];
                sbyte d = convertUInt8ToInt8(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = src[i * srcStride + srcLane];
                sbyte d = convertUInt8ToInt8(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_UInt8_To_Int8_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.Int8);

        long n = InterleavedAccess.Resolve(
            context,
            input,
            output,
            valuesCount,
            out int srcStride,
            out int dstStride,
            out int srcLane,
            out int dstLane);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            byte* src = (byte*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = src[i * srcStride + srcLane];
                sbyte d = DefaultConversionsToInt8.ConvertUInt8ToInt8_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = src[i * srcStride + srcLane];
                sbyte d = DefaultConversionsToInt8.ConvertUInt8ToInt8_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Int8_To_Int8(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.Int8);

        long n = InterleavedAccess.Resolve(
            context,
            input,
            output,
            valuesCount,
            out int srcStride,
            out int dstStride,
            out int srcLane,
            out int dstLane);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt8ToInt8_Delegate convertInt8ToInt8 = (ConvertInt8ToInt8_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            sbyte* src = (sbyte*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = src[i * srcStride + srcLane];
                sbyte d = convertInt8ToInt8(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = src[i * srcStride + srcLane];
                sbyte d = convertInt8ToInt8(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Int8_To_Int8_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.Int8);

        long n = InterleavedAccess.Resolve(
            context,
            input,
            output,
            valuesCount,
            out int srcStride,
            out int dstStride,
            out int srcLane,
            out int dstLane);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            sbyte* src = (sbyte*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                dst[i * dstStride + dstLane] = src[i * srcStride + srcLane];
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                dst[i * dstStride + dstLane] = src[i * srcStride + srcLane];
            }
            return n;
        }
    }

    public static long L2B_UInt16_To_Int8(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.Int8);

        long n = InterleavedAccess.Resolve(
            context,
            input,
            output,
            valuesCount,
            out int srcStride,
            out int dstStride,
            out int srcLane,
            out int dstLane);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt16ToInt8_Delegate convertUInt16ToInt8 = (ConvertUInt16ToInt8_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            ushort* src = (ushort*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = src[i * srcStride + srcLane];
                sbyte d = convertUInt16ToInt8(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                sbyte d = convertUInt16ToInt8(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_UInt16_To_Int8_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.Int8);

        long n = InterleavedAccess.Resolve(
            context,
            input,
            output,
            valuesCount,
            out int srcStride,
            out int dstStride,
            out int srcLane,
            out int dstLane);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            ushort* src = (ushort*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = src[i * srcStride + srcLane];
                sbyte d = DefaultConversionsToInt8.ConvertUInt16ToInt8_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                sbyte d = DefaultConversionsToInt8.ConvertUInt16ToInt8_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Int16_To_Int8(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.Int8);

        long n = InterleavedAccess.Resolve(
            context,
            input,
            output,
            valuesCount,
            out int srcStride,
            out int dstStride,
            out int srcLane,
            out int dstLane);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt16ToInt8_Delegate convertInt16ToInt8 = (ConvertInt16ToInt8_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            short* src = (short*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = src[i * srcStride + srcLane];
                sbyte d = convertInt16ToInt8(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                sbyte d = convertInt16ToInt8(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Int16_To_Int8_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.Int8);

        long n = InterleavedAccess.Resolve(
            context,
            input,
            output,
            valuesCount,
            out int srcStride,
            out int dstStride,
            out int srcLane,
            out int dstLane);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            short* src = (short*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = src[i * srcStride + srcLane];
                sbyte d = DefaultConversionsToInt8.ConvertInt16ToInt8_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                sbyte d = DefaultConversionsToInt8.ConvertInt16ToInt8_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_UInt32_To_Int8(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.Int8);

        long n = InterleavedAccess.Resolve(
            context,
            input,
            output,
            valuesCount,
            out int srcStride,
            out int dstStride,
            out int srcLane,
            out int dstLane);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt32ToInt8_Delegate convertUInt32ToInt8 = (ConvertUInt32ToInt8_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            uint* src = (uint*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = src[i * srcStride + srcLane];
                sbyte d = convertUInt32ToInt8(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                sbyte d = convertUInt32ToInt8(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_UInt32_To_Int8_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.Int8);

        long n = InterleavedAccess.Resolve(
            context,
            input,
            output,
            valuesCount,
            out int srcStride,
            out int dstStride,
            out int srcLane,
            out int dstLane);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            uint* src = (uint*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = src[i * srcStride + srcLane];
                sbyte d = DefaultConversionsToInt8.ConvertUInt32ToInt8_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                sbyte d = DefaultConversionsToInt8.ConvertUInt32ToInt8_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Int32_To_Int8(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.Int8);

        long n = InterleavedAccess.Resolve(
            context,
            input,
            output,
            valuesCount,
            out int srcStride,
            out int dstStride,
            out int srcLane,
            out int dstLane);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt32ToInt8_Delegate convertInt32ToInt8 = (ConvertInt32ToInt8_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = src[i * srcStride + srcLane];
                sbyte d = convertInt32ToInt8(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                sbyte d = convertInt32ToInt8(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Int32_To_Int8_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.Int8);

        long n = InterleavedAccess.Resolve(
            context,
            input,
            output,
            valuesCount,
            out int srcStride,
            out int dstStride,
            out int srcLane,
            out int dstLane);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = src[i * srcStride + srcLane];
                sbyte d = DefaultConversionsToInt8.ConvertInt32ToInt8_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                sbyte d = DefaultConversionsToInt8.ConvertInt32ToInt8_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_UInt64_To_Int8(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.Int8);

        long n = InterleavedAccess.Resolve(
            context,
            input,
            output,
            valuesCount,
            out int srcStride,
            out int dstStride,
            out int srcLane,
            out int dstLane);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt64ToInt8_Delegate convertUInt64ToInt8 = (ConvertUInt64ToInt8_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = src[i * srcStride + srcLane];
                sbyte d = convertUInt64ToInt8(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                sbyte d = convertUInt64ToInt8(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_UInt64_To_Int8_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.Int8);

        long n = InterleavedAccess.Resolve(
            context,
            input,
            output,
            valuesCount,
            out int srcStride,
            out int dstStride,
            out int srcLane,
            out int dstLane);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = src[i * srcStride + srcLane];
                sbyte d = DefaultConversionsToInt8.ConvertUInt64ToInt8_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                sbyte d = DefaultConversionsToInt8.ConvertUInt64ToInt8_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Int64_To_Int8(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.Int8);

        long n = InterleavedAccess.Resolve(
            context,
            input,
            output,
            valuesCount,
            out int srcStride,
            out int dstStride,
            out int srcLane,
            out int dstLane);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt64ToInt8_Delegate convertInt64ToInt8 = (ConvertInt64ToInt8_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            long* src = (long*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = src[i * srcStride + srcLane];
                sbyte d = convertInt64ToInt8(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                sbyte d = convertInt64ToInt8(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Int64_To_Int8_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.Int8);

        long n = InterleavedAccess.Resolve(
            context,
            input,
            output,
            valuesCount,
            out int srcStride,
            out int dstStride,
            out int srcLane,
            out int dstLane);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            long* src = (long*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = src[i * srcStride + srcLane];
                sbyte d = DefaultConversionsToInt8.ConvertInt64ToInt8_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                sbyte d = DefaultConversionsToInt8.ConvertInt64ToInt8_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Float_To_Int8(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.Int8);

        long n = InterleavedAccess.Resolve(
            context,
            input,
            output,
            valuesCount,
            out int srcStride,
            out int dstStride,
            out int srcLane,
            out int dstLane);
        if (n == 0)
        {
            return 0;
        }

        ConvertFloatToInt8_Delegate convertFloatToInt8 = (ConvertFloatToInt8_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            float* src = (float*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = src[i * srcStride + srcLane];
                sbyte d = convertFloatToInt8(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(((uint*)src)[i * srcStride + srcLane]);
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                sbyte d = convertFloatToInt8(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Float_To_Int8_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.Int8);

        long n = InterleavedAccess.Resolve(
            context,
            input,
            output,
            valuesCount,
            out int srcStride,
            out int dstStride,
            out int srcLane,
            out int dstLane);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            float* src = (float*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = src[i * srcStride + srcLane];
                sbyte d = DefaultConversionsToInt8.ConvertFloatToInt8_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(((uint*)src)[i * srcStride + srcLane]);
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                sbyte d = DefaultConversionsToInt8.ConvertFloatToInt8_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Double_To_Int8(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.Int8);

        long n = InterleavedAccess.Resolve(
            context,
            input,
            output,
            valuesCount,
            out int srcStride,
            out int dstStride,
            out int srcLane,
            out int dstLane);
        if (n == 0)
        {
            return 0;
        }

        ConvertDoubleToInt8_Delegate convertDoubleToInt8 = (ConvertDoubleToInt8_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = src[i * srcStride + srcLane];
                sbyte d = convertDoubleToInt8(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(((ulong*)src)[i * srcStride + srcLane]);
                double s = System.BitConverter.UInt64BitsToDouble(s_bits);
                sbyte d = convertDoubleToInt8(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Double_To_Int8_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.Int8);

        long n = InterleavedAccess.Resolve(
            context,
            input,
            output,
            valuesCount,
            out int srcStride,
            out int dstStride,
            out int srcLane,
            out int dstLane);
        if (n == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = src[i * srcStride + srcLane];
                sbyte d = DefaultConversionsToInt8.ConvertDoubleToInt8_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            sbyte* dst = (sbyte*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(((ulong*)src)[i * srcStride + srcLane]);
                double s = System.BitConverter.UInt64BitsToDouble(s_bits);
                sbyte d = DefaultConversionsToInt8.ConvertDoubleToInt8_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

}
