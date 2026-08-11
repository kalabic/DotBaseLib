using DotBase.Buffers;
using DotBase.Integral.Conversion.Numeric;
using DotBase.Integral.Conversion.Numeric.Defaults;
using System.Diagnostics;

namespace DotBase.Integral.Conversion.Internal.Interleaved.B2B;


internal static unsafe class B2BConvertToUInt64
{
    internal static void AddToTable(IConversionDelegateTable table)
    {
        table.SetCustomFunc(B2B_UInt8_To_UInt64, ByteOrder.BigEndian, IntegralType.UInt8, ByteOrder.BigEndian, IntegralType.UInt64);
        table.SetCustomFunc(B2B_Int8_To_UInt64, ByteOrder.BigEndian, IntegralType.Int8, ByteOrder.BigEndian, IntegralType.UInt64);
        table.SetCustomFunc(B2B_UInt16_To_UInt64, ByteOrder.BigEndian, IntegralType.UInt16, ByteOrder.BigEndian, IntegralType.UInt64);
        table.SetCustomFunc(B2B_Int16_To_UInt64, ByteOrder.BigEndian, IntegralType.Int16, ByteOrder.BigEndian, IntegralType.UInt64);
        table.SetCustomFunc(B2B_UInt32_To_UInt64, ByteOrder.BigEndian, IntegralType.UInt32, ByteOrder.BigEndian, IntegralType.UInt64);
        table.SetCustomFunc(B2B_Int32_To_UInt64, ByteOrder.BigEndian, IntegralType.Int32, ByteOrder.BigEndian, IntegralType.UInt64);
        table.SetCustomFunc(B2B_UInt64_To_UInt64, ByteOrder.BigEndian, IntegralType.UInt64, ByteOrder.BigEndian, IntegralType.UInt64);
        table.SetCustomFunc(B2B_Int64_To_UInt64, ByteOrder.BigEndian, IntegralType.Int64, ByteOrder.BigEndian, IntegralType.UInt64);
        table.SetCustomFunc(B2B_Float_To_UInt64, ByteOrder.BigEndian, IntegralType.Float, ByteOrder.BigEndian, IntegralType.UInt64);
        table.SetCustomFunc(B2B_Double_To_UInt64, ByteOrder.BigEndian, IntegralType.Double, ByteOrder.BigEndian, IntegralType.UInt64);

        table.SetDefaultFunc(B2B_UInt8_To_UInt64_Default, ByteOrder.BigEndian, IntegralType.UInt8, ByteOrder.BigEndian, IntegralType.UInt64);
        table.SetDefaultFunc(B2B_Int8_To_UInt64_Default, ByteOrder.BigEndian, IntegralType.Int8, ByteOrder.BigEndian, IntegralType.UInt64);
        table.SetDefaultFunc(B2B_UInt16_To_UInt64_Default, ByteOrder.BigEndian, IntegralType.UInt16, ByteOrder.BigEndian, IntegralType.UInt64);
        table.SetDefaultFunc(B2B_Int16_To_UInt64_Default, ByteOrder.BigEndian, IntegralType.Int16, ByteOrder.BigEndian, IntegralType.UInt64);
        table.SetDefaultFunc(B2B_UInt32_To_UInt64_Default, ByteOrder.BigEndian, IntegralType.UInt32, ByteOrder.BigEndian, IntegralType.UInt64);
        table.SetDefaultFunc(B2B_Int32_To_UInt64_Default, ByteOrder.BigEndian, IntegralType.Int32, ByteOrder.BigEndian, IntegralType.UInt64);
        table.SetDefaultFunc(B2B_UInt64_To_UInt64_Default, ByteOrder.BigEndian, IntegralType.UInt64, ByteOrder.BigEndian, IntegralType.UInt64);
        table.SetDefaultFunc(B2B_Int64_To_UInt64_Default, ByteOrder.BigEndian, IntegralType.Int64, ByteOrder.BigEndian, IntegralType.UInt64);
        table.SetDefaultFunc(B2B_Float_To_UInt64_Default, ByteOrder.BigEndian, IntegralType.Float, ByteOrder.BigEndian, IntegralType.UInt64);
        table.SetDefaultFunc(B2B_Double_To_UInt64_Default, ByteOrder.BigEndian, IntegralType.Double, ByteOrder.BigEndian, IntegralType.UInt64);
    }

    public static long B2B_UInt8_To_UInt64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

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

        ConvertUInt8ToUInt64_Delegate convertUInt8ToUInt64 = (ConvertUInt8ToUInt64_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            byte* src = (byte*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = src[i * srcStride + srcLane];
                ulong d = convertUInt8ToUInt64(s);
                dst[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = src[i * srcStride + srcLane];
                ulong d = convertUInt8ToUInt64(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long B2B_UInt8_To_UInt64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

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
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = src[i * srcStride + srcLane];
                ulong d = ((ulong)s);
                dst[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = src[i * srcStride + srcLane];
                ulong d = ((ulong)s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long B2B_Int8_To_UInt64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

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

        ConvertInt8ToUInt64_Delegate convertInt8ToUInt64 = (ConvertInt8ToUInt64_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            sbyte* src = (sbyte*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = src[i * srcStride + srcLane];
                ulong d = convertInt8ToUInt64(s);
                dst[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = src[i * srcStride + srcLane];
                ulong d = convertInt8ToUInt64(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long B2B_Int8_To_UInt64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

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
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = src[i * srcStride + srcLane];
                ulong d = DefaultConversionsToUInt64.ConvertInt8ToUInt64_Default(s);
                dst[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = src[i * srcStride + srcLane];
                ulong d = DefaultConversionsToUInt64.ConvertInt8ToUInt64_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long B2B_UInt16_To_UInt64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

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

        ConvertUInt16ToUInt64_Delegate convertUInt16ToUInt64 = (ConvertUInt16ToUInt64_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            ushort* src = (ushort*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                ulong d = convertUInt16ToUInt64(s);
                dst[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = src[i * srcStride + srcLane];
                ulong d = convertUInt16ToUInt64(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long B2B_UInt16_To_UInt64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

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
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                ulong d = ((ulong)s);
                dst[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = src[i * srcStride + srcLane];
                ulong d = ((ulong)s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long B2B_Int16_To_UInt64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

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

        ConvertInt16ToUInt64_Delegate convertInt16ToUInt64 = (ConvertInt16ToUInt64_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            short* src = (short*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                ulong d = convertInt16ToUInt64(s);
                dst[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = src[i * srcStride + srcLane];
                ulong d = convertInt16ToUInt64(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long B2B_Int16_To_UInt64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

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
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                ulong d = DefaultConversionsToUInt64.ConvertInt16ToUInt64_Default(s);
                dst[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = src[i * srcStride + srcLane];
                ulong d = DefaultConversionsToUInt64.ConvertInt16ToUInt64_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long B2B_UInt32_To_UInt64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

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

        ConvertUInt32ToUInt64_Delegate convertUInt32ToUInt64 = (ConvertUInt32ToUInt64_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            uint* src = (uint*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                ulong d = convertUInt32ToUInt64(s);
                dst[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = src[i * srcStride + srcLane];
                ulong d = convertUInt32ToUInt64(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long B2B_UInt32_To_UInt64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

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
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                ulong d = ((ulong)s);
                dst[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = src[i * srcStride + srcLane];
                ulong d = ((ulong)s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long B2B_Int32_To_UInt64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

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

        ConvertInt32ToUInt64_Delegate convertInt32ToUInt64 = (ConvertInt32ToUInt64_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                ulong d = convertInt32ToUInt64(s);
                dst[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = src[i * srcStride + srcLane];
                ulong d = convertInt32ToUInt64(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long B2B_Int32_To_UInt64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

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
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                ulong d = DefaultConversionsToUInt64.ConvertInt32ToUInt64_Default(s);
                dst[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = src[i * srcStride + srcLane];
                ulong d = DefaultConversionsToUInt64.ConvertInt32ToUInt64_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long B2B_UInt64_To_UInt64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

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

        ConvertUInt64ToUInt64_Delegate convertUInt64ToUInt64 = (ConvertUInt64ToUInt64_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                ulong d = convertUInt64ToUInt64(s);
                dst[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = src[i * srcStride + srcLane];
                ulong d = convertUInt64ToUInt64(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long B2B_UInt64_To_UInt64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

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
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = src[i * srcStride + srcLane];
                dst[i * dstStride + dstLane] = s;
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                dst[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(s);
            }
            return n;
        }
    }

    public static long B2B_Int64_To_UInt64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

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

        ConvertInt64ToUInt64_Delegate convertInt64ToUInt64 = (ConvertInt64ToUInt64_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            long* src = (long*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                ulong d = convertInt64ToUInt64(s);
                dst[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = src[i * srcStride + srcLane];
                ulong d = convertInt64ToUInt64(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long B2B_Int64_To_UInt64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

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
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                ulong d = DefaultConversionsToUInt64.ConvertInt64ToUInt64_Default(s);
                dst[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = src[i * srcStride + srcLane];
                ulong d = DefaultConversionsToUInt64.ConvertInt64ToUInt64_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long B2B_Float_To_UInt64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

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

        ConvertFloatToUInt64_Delegate convertFloatToUInt64 = (ConvertFloatToUInt64_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            float* src = (float*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(((uint*)src)[i * srcStride + srcLane]);
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                ulong d = convertFloatToUInt64(s);
                dst[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = src[i * srcStride + srcLane];
                ulong d = convertFloatToUInt64(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long B2B_Float_To_UInt64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

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
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(((uint*)src)[i * srcStride + srcLane]);
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                ulong d = DefaultConversionsToUInt64.ConvertFloatToUInt64_Default(s);
                dst[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = src[i * srcStride + srcLane];
                ulong d = DefaultConversionsToUInt64.ConvertFloatToUInt64_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long B2B_Double_To_UInt64(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

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

        ConvertDoubleToUInt64_Delegate convertDoubleToUInt64 = (ConvertDoubleToUInt64_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(((ulong*)src)[i * srcStride + srcLane]);
                double s = System.BitConverter.UInt64BitsToDouble(s_bits);
                ulong d = convertDoubleToUInt64(s);
                dst[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = src[i * srcStride + srcLane];
                ulong d = convertDoubleToUInt64(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long B2B_Double_To_UInt64_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.UInt64);

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
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(((ulong*)src)[i * srcStride + srcLane]);
                double s = System.BitConverter.UInt64BitsToDouble(s_bits);
                ulong d = DefaultConversionsToUInt64.ConvertDoubleToUInt64_Default(s);
                dst[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(d);
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            ulong* dst = (ulong*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = src[i * srcStride + srcLane];
                ulong d = DefaultConversionsToUInt64.ConvertDoubleToUInt64_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

}
