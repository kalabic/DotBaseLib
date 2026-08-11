using DotBase.Buffers;
using DotBase.Integral.Conversion.Numeric;
using DotBase.Integral.Conversion.Numeric.Defaults;
using System.Diagnostics;

namespace DotBase.Integral.Conversion.Internal.Interleaved.L2B;


internal static unsafe class L2BConvertToDouble
{
    internal static void AddToTable(IConversionDelegateTable table)
    {
        table.SetCustomFunc(L2B_UInt8_To_Double, ByteOrder.LittleEndian, IntegralType.UInt8, ByteOrder.BigEndian, IntegralType.Double);
        table.SetCustomFunc(L2B_Int8_To_Double, ByteOrder.LittleEndian, IntegralType.Int8, ByteOrder.BigEndian, IntegralType.Double);
        table.SetCustomFunc(L2B_UInt16_To_Double, ByteOrder.LittleEndian, IntegralType.UInt16, ByteOrder.BigEndian, IntegralType.Double);
        table.SetCustomFunc(L2B_Int16_To_Double, ByteOrder.LittleEndian, IntegralType.Int16, ByteOrder.BigEndian, IntegralType.Double);
        table.SetCustomFunc(L2B_UInt32_To_Double, ByteOrder.LittleEndian, IntegralType.UInt32, ByteOrder.BigEndian, IntegralType.Double);
        table.SetCustomFunc(L2B_Int32_To_Double, ByteOrder.LittleEndian, IntegralType.Int32, ByteOrder.BigEndian, IntegralType.Double);
        table.SetCustomFunc(L2B_UInt64_To_Double, ByteOrder.LittleEndian, IntegralType.UInt64, ByteOrder.BigEndian, IntegralType.Double);
        table.SetCustomFunc(L2B_Int64_To_Double, ByteOrder.LittleEndian, IntegralType.Int64, ByteOrder.BigEndian, IntegralType.Double);
        table.SetCustomFunc(L2B_Float_To_Double, ByteOrder.LittleEndian, IntegralType.Float, ByteOrder.BigEndian, IntegralType.Double);
        table.SetCustomFunc(L2B_Double_To_Double, ByteOrder.LittleEndian, IntegralType.Double, ByteOrder.BigEndian, IntegralType.Double);

        table.SetDefaultFunc(L2B_UInt8_To_Double_Default, ByteOrder.LittleEndian, IntegralType.UInt8, ByteOrder.BigEndian, IntegralType.Double);
        table.SetDefaultFunc(L2B_Int8_To_Double_Default, ByteOrder.LittleEndian, IntegralType.Int8, ByteOrder.BigEndian, IntegralType.Double);
        table.SetDefaultFunc(L2B_UInt16_To_Double_Default, ByteOrder.LittleEndian, IntegralType.UInt16, ByteOrder.BigEndian, IntegralType.Double);
        table.SetDefaultFunc(L2B_Int16_To_Double_Default, ByteOrder.LittleEndian, IntegralType.Int16, ByteOrder.BigEndian, IntegralType.Double);
        table.SetDefaultFunc(L2B_UInt32_To_Double_Default, ByteOrder.LittleEndian, IntegralType.UInt32, ByteOrder.BigEndian, IntegralType.Double);
        table.SetDefaultFunc(L2B_Int32_To_Double_Default, ByteOrder.LittleEndian, IntegralType.Int32, ByteOrder.BigEndian, IntegralType.Double);
        table.SetDefaultFunc(L2B_UInt64_To_Double_Default, ByteOrder.LittleEndian, IntegralType.UInt64, ByteOrder.BigEndian, IntegralType.Double);
        table.SetDefaultFunc(L2B_Int64_To_Double_Default, ByteOrder.LittleEndian, IntegralType.Int64, ByteOrder.BigEndian, IntegralType.Double);
        table.SetDefaultFunc(L2B_Float_To_Double_Default, ByteOrder.LittleEndian, IntegralType.Float, ByteOrder.BigEndian, IntegralType.Double);
        table.SetDefaultFunc(L2B_Double_To_Double_Default, ByteOrder.LittleEndian, IntegralType.Double, ByteOrder.BigEndian, IntegralType.Double);
    }

    public static long L2B_UInt8_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

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

        ConvertUInt8ToDouble_Delegate convertUInt8ToDouble = (ConvertUInt8ToDouble_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            byte* src = (byte*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = src[i * srcStride + srcLane];
                double d = convertUInt8ToDouble(s);
                ((ulong*)dst)[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = src[i * srcStride + srcLane];
                double d = convertUInt8ToDouble(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_UInt8_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

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
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = src[i * srcStride + srcLane];
                double d = ((double)s);
                ((ulong*)dst)[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = src[i * srcStride + srcLane];
                double d = ((double)s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Int8_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

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

        ConvertInt8ToDouble_Delegate convertInt8ToDouble = (ConvertInt8ToDouble_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            sbyte* src = (sbyte*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = src[i * srcStride + srcLane];
                double d = convertInt8ToDouble(s);
                ((ulong*)dst)[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = src[i * srcStride + srcLane];
                double d = convertInt8ToDouble(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Int8_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

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
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = src[i * srcStride + srcLane];
                double d = ((double)s);
                ((ulong*)dst)[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = src[i * srcStride + srcLane];
                double d = ((double)s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_UInt16_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

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

        ConvertUInt16ToDouble_Delegate convertUInt16ToDouble = (ConvertUInt16ToDouble_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            ushort* src = (ushort*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = src[i * srcStride + srcLane];
                double d = convertUInt16ToDouble(s);
                ((ulong*)dst)[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                double d = convertUInt16ToDouble(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_UInt16_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

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
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = src[i * srcStride + srcLane];
                double d = ((double)s);
                ((ulong*)dst)[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                double d = ((double)s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Int16_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

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

        ConvertInt16ToDouble_Delegate convertInt16ToDouble = (ConvertInt16ToDouble_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            short* src = (short*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = src[i * srcStride + srcLane];
                double d = convertInt16ToDouble(s);
                ((ulong*)dst)[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                double d = convertInt16ToDouble(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Int16_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

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
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = src[i * srcStride + srcLane];
                double d = ((double)s);
                ((ulong*)dst)[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                double d = ((double)s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_UInt32_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

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

        ConvertUInt32ToDouble_Delegate convertUInt32ToDouble = (ConvertUInt32ToDouble_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            uint* src = (uint*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = src[i * srcStride + srcLane];
                double d = convertUInt32ToDouble(s);
                ((ulong*)dst)[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                double d = convertUInt32ToDouble(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_UInt32_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

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
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = src[i * srcStride + srcLane];
                double d = ((double)s);
                ((ulong*)dst)[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                double d = ((double)s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Int32_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

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

        ConvertInt32ToDouble_Delegate convertInt32ToDouble = (ConvertInt32ToDouble_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = src[i * srcStride + srcLane];
                double d = convertInt32ToDouble(s);
                ((ulong*)dst)[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                double d = convertInt32ToDouble(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Int32_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

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
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = src[i * srcStride + srcLane];
                double d = ((double)s);
                ((ulong*)dst)[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                double d = ((double)s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_UInt64_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

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

        ConvertUInt64ToDouble_Delegate convertUInt64ToDouble = (ConvertUInt64ToDouble_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = src[i * srcStride + srcLane];
                double d = convertUInt64ToDouble(s);
                ((ulong*)dst)[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                double d = convertUInt64ToDouble(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_UInt64_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

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
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = src[i * srcStride + srcLane];
                double d = ((double)s);
                ((ulong*)dst)[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                double d = ((double)s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Int64_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

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

        ConvertInt64ToDouble_Delegate convertInt64ToDouble = (ConvertInt64ToDouble_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            long* src = (long*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = src[i * srcStride + srcLane];
                double d = convertInt64ToDouble(s);
                ((ulong*)dst)[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                double d = convertInt64ToDouble(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Int64_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

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
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = src[i * srcStride + srcLane];
                double d = ((double)s);
                ((ulong*)dst)[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(src[i * srcStride + srcLane]);
                double d = ((double)s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Float_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

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

        ConvertFloatToDouble_Delegate convertFloatToDouble = (ConvertFloatToDouble_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            float* src = (float*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = src[i * srcStride + srcLane];
                double d = convertFloatToDouble(s);
                ((ulong*)dst)[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(((uint*)src)[i * srcStride + srcLane]);
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                double d = convertFloatToDouble(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Float_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

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
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = src[i * srcStride + srcLane];
                double d = DefaultConversionsToDouble.ConvertFloatToDouble_Default(s);
                ((ulong*)dst)[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(((uint*)src)[i * srcStride + srcLane]);
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                double d = DefaultConversionsToDouble.ConvertFloatToDouble_Default(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Double_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

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

        ConvertDoubleToDouble_Delegate convertDoubleToDouble = (ConvertDoubleToDouble_Delegate)(context!.NumericFunc!);

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = src[i * srcStride + srcLane];
                double d = convertDoubleToDouble(s);
                ((ulong*)dst)[i * dstStride + dstLane] = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(((ulong*)src)[i * srcStride + srcLane]);
                double s = System.BitConverter.UInt64BitsToDouble(s_bits);
                double d = convertDoubleToDouble(s);
                dst[i * dstStride + dstLane] = d;
            }
            return n;
        }
    }

    public static long L2B_Double_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

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
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(ulong*)src);
                src++;
                ((ulong*)dst)[i * dstStride + dstLane] = bits;
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(ulong*)src);
                src++;
                ((ulong*)dst)[i * dstStride + dstLane] = bits;
            }
            return n;
        }
    }

}
