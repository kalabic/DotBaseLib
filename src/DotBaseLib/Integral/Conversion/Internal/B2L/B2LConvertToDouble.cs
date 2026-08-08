using DotBase.Buffers;
using DotBase.Integral.Conversion.Numeric;
using DotBase.Integral.Conversion.Numeric.Defaults;
using System.Diagnostics;

namespace DotBase.Integral.Conversion.Internal.B2L;


internal static unsafe class B2LConvertToDouble
{
    internal static void AddToTable(IConversionDelegateTable table)
    {
        table.SetCustomFunc(B2L_UInt8_To_Double, ByteOrder.BigEndian, IntegralType.UInt8, ByteOrder.LittleEndian, IntegralType.Double);
        table.SetCustomFunc(B2L_Int8_To_Double, ByteOrder.BigEndian, IntegralType.Int8, ByteOrder.LittleEndian, IntegralType.Double);
        table.SetCustomFunc(B2L_UInt16_To_Double, ByteOrder.BigEndian, IntegralType.UInt16, ByteOrder.LittleEndian, IntegralType.Double);
        table.SetCustomFunc(B2L_Int16_To_Double, ByteOrder.BigEndian, IntegralType.Int16, ByteOrder.LittleEndian, IntegralType.Double);
        table.SetCustomFunc(B2L_UInt32_To_Double, ByteOrder.BigEndian, IntegralType.UInt32, ByteOrder.LittleEndian, IntegralType.Double);
        table.SetCustomFunc(B2L_Int32_To_Double, ByteOrder.BigEndian, IntegralType.Int32, ByteOrder.LittleEndian, IntegralType.Double);
        table.SetCustomFunc(B2L_UInt64_To_Double, ByteOrder.BigEndian, IntegralType.UInt64, ByteOrder.LittleEndian, IntegralType.Double);
        table.SetCustomFunc(B2L_Int64_To_Double, ByteOrder.BigEndian, IntegralType.Int64, ByteOrder.LittleEndian, IntegralType.Double);
        table.SetCustomFunc(B2L_Float_To_Double, ByteOrder.BigEndian, IntegralType.Float, ByteOrder.LittleEndian, IntegralType.Double);
        table.SetCustomFunc(B2L_Double_To_Double, ByteOrder.BigEndian, IntegralType.Double, ByteOrder.LittleEndian, IntegralType.Double);

        table.SetDefaultFunc(B2L_UInt8_To_Double_Default, ByteOrder.BigEndian, IntegralType.UInt8, ByteOrder.LittleEndian, IntegralType.Double);
        table.SetDefaultFunc(B2L_Int8_To_Double_Default, ByteOrder.BigEndian, IntegralType.Int8, ByteOrder.LittleEndian, IntegralType.Double);
        table.SetDefaultFunc(B2L_UInt16_To_Double_Default, ByteOrder.BigEndian, IntegralType.UInt16, ByteOrder.LittleEndian, IntegralType.Double);
        table.SetDefaultFunc(B2L_Int16_To_Double_Default, ByteOrder.BigEndian, IntegralType.Int16, ByteOrder.LittleEndian, IntegralType.Double);
        table.SetDefaultFunc(B2L_UInt32_To_Double_Default, ByteOrder.BigEndian, IntegralType.UInt32, ByteOrder.LittleEndian, IntegralType.Double);
        table.SetDefaultFunc(B2L_Int32_To_Double_Default, ByteOrder.BigEndian, IntegralType.Int32, ByteOrder.LittleEndian, IntegralType.Double);
        table.SetDefaultFunc(B2L_UInt64_To_Double_Default, ByteOrder.BigEndian, IntegralType.UInt64, ByteOrder.LittleEndian, IntegralType.Double);
        table.SetDefaultFunc(B2L_Int64_To_Double_Default, ByteOrder.BigEndian, IntegralType.Int64, ByteOrder.LittleEndian, IntegralType.Double);
        table.SetDefaultFunc(B2L_Float_To_Double_Default, ByteOrder.BigEndian, IntegralType.Float, ByteOrder.LittleEndian, IntegralType.Double);
        table.SetDefaultFunc(B2L_Double_To_Double_Default, ByteOrder.BigEndian, IntegralType.Double, ByteOrder.LittleEndian, IntegralType.Double);
    }

    public static long B2L_UInt8_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt8ToDouble_Delegate convertUInt8ToDouble = context.ToDouble.ConvertUInt8ToDouble;

        if (BitConverter.IsLittleEndian)
        {
            byte* src = (byte*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                double d = convertUInt8ToDouble(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                double d = convertUInt8ToDouble(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
    }

    public static long B2L_UInt8_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt8);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
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
                byte s = *src++;
                double d = DefaultConversionsToDouble.ConvertUInt8ToDouble_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            byte* src = (byte*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                byte s = *src++;
                double d = DefaultConversionsToDouble.ConvertUInt8ToDouble_Default(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
    }

    public static long B2L_Int8_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt8ToDouble_Delegate convertInt8ToDouble = context.ToDouble.ConvertInt8ToDouble;

        if (BitConverter.IsLittleEndian)
        {
            sbyte* src = (sbyte*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                double d = convertInt8ToDouble(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                double d = convertInt8ToDouble(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
    }

    public static long B2L_Int8_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int8);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
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
                sbyte s = *src++;
                double d = DefaultConversionsToDouble.ConvertInt8ToDouble_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            sbyte* src = (sbyte*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                sbyte s = *src++;
                double d = DefaultConversionsToDouble.ConvertInt8ToDouble_Default(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
    }

    public static long B2L_UInt16_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt16ToDouble_Delegate convertUInt16ToDouble = context.ToDouble.ConvertUInt16ToDouble;

        if (BitConverter.IsLittleEndian)
        {
            ushort* src = (ushort*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = convertUInt16ToDouble(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = *src++;
                double d = convertUInt16ToDouble(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
    }

    public static long B2L_UInt16_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt16);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
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
                ushort s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = DefaultConversionsToDouble.ConvertUInt16ToDouble_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            ushort* src = (ushort*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ushort s = *src++;
                double d = DefaultConversionsToDouble.ConvertUInt16ToDouble_Default(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
    }

    public static long B2L_Int16_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt16ToDouble_Delegate convertInt16ToDouble = context.ToDouble.ConvertInt16ToDouble;

        if (BitConverter.IsLittleEndian)
        {
            short* src = (short*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = convertInt16ToDouble(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = *src++;
                double d = convertInt16ToDouble(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
    }

    public static long B2L_Int16_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int16);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
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
                short s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = DefaultConversionsToDouble.ConvertInt16ToDouble_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            short* src = (short*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                short s = *src++;
                double d = DefaultConversionsToDouble.ConvertInt16ToDouble_Default(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
    }

    public static long B2L_UInt32_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt32ToDouble_Delegate convertUInt32ToDouble = context.ToDouble.ConvertUInt32ToDouble;

        if (BitConverter.IsLittleEndian)
        {
            uint* src = (uint*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = convertUInt32ToDouble(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = *src++;
                double d = convertUInt32ToDouble(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
    }

    public static long B2L_UInt32_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt32);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
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
                uint s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = DefaultConversionsToDouble.ConvertUInt32ToDouble_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            uint* src = (uint*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s = *src++;
                double d = DefaultConversionsToDouble.ConvertUInt32ToDouble_Default(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
    }

    public static long B2L_Int32_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt32ToDouble_Delegate convertInt32ToDouble = context.ToDouble.ConvertInt32ToDouble;

        if (BitConverter.IsLittleEndian)
        {
            int* src = (int*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = convertInt32ToDouble(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = *src++;
                double d = convertInt32ToDouble(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
    }

    public static long B2L_Int32_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int32);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
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
                int s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = DefaultConversionsToDouble.ConvertInt32ToDouble_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            int* src = (int*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                int s = *src++;
                double d = DefaultConversionsToDouble.ConvertInt32ToDouble_Default(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
    }

    public static long B2L_UInt64_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertUInt64ToDouble_Delegate convertUInt64ToDouble = context.ToDouble.ConvertUInt64ToDouble;

        if (BitConverter.IsLittleEndian)
        {
            ulong* src = (ulong*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = convertUInt64ToDouble(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = *src++;
                double d = convertUInt64ToDouble(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
    }

    public static long B2L_UInt64_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.UInt64);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
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
                ulong s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = DefaultConversionsToDouble.ConvertUInt64ToDouble_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            ulong* src = (ulong*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s = *src++;
                double d = DefaultConversionsToDouble.ConvertUInt64ToDouble_Default(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
    }

    public static long B2L_Int64_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertInt64ToDouble_Delegate convertInt64ToDouble = context.ToDouble.ConvertInt64ToDouble;

        if (BitConverter.IsLittleEndian)
        {
            long* src = (long*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = convertInt64ToDouble(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = *src++;
                double d = convertInt64ToDouble(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
    }

    public static long B2L_Int64_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Int64);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
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
                long s = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*src++);
                double d = DefaultConversionsToDouble.ConvertInt64ToDouble_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            long* src = (long*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                long s = *src++;
                double d = DefaultConversionsToDouble.ConvertInt64ToDouble_Default(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
    }

    public static long B2L_Float_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertFloatToDouble_Delegate convertFloatToDouble = context.ToDouble.ConvertFloatToDouble;

        if (BitConverter.IsLittleEndian)
        {
            float* src = (float*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(uint*)src);
                src++;
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                double d = convertFloatToDouble(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = *src++;
                double d = convertFloatToDouble(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
    }

    public static long B2L_Float_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Float);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
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
                uint s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(uint*)src);
                src++;
                float s = System.BitConverter.UInt32BitsToSingle(s_bits);
                double d = DefaultConversionsToDouble.ConvertFloatToDouble_Default(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            float* src = (float*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                float s = *src++;
                double d = DefaultConversionsToDouble.ConvertFloatToDouble_Default(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
    }

    public static long B2L_Double_To_Double(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);

        long n = ConversionCount.Effective(input, output, valuesCount);
        if (n == 0)
        {
            return 0;
        }

        ConvertDoubleToDouble_Delegate convertDoubleToDouble = context.ToDouble.ConvertDoubleToDouble;

        if (BitConverter.IsLittleEndian)
        {
            double* src = (double*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                ulong s_bits = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(*(ulong*)src);
                src++;
                double s = System.BitConverter.UInt64BitsToDouble(s_bits);
                double d = convertDoubleToDouble(s);
                *dst++ = d;
            }
            return n;
        }
        else
        {
            double* src = (double*)input.DataPtr;
            double* dst = (double*)output.DataPtr;
            for (long i = 0; i < n; ++i)
            {
                double s = *src++;
                double d = convertDoubleToDouble(s);
                *(ulong*)dst = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(System.BitConverter.DoubleToUInt64Bits(d));
                dst++;
            }
            return n;
        }
    }

    public static long B2L_Double_To_Double_Default(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        NumericConverters context)
    {
        Debug.Assert(input.Format.ByteOrder.Resolve() == ByteOrder.BigEndian);
        Debug.Assert(output.Format.ByteOrder.Resolve() == ByteOrder.LittleEndian);
        Debug.Assert(input.IntegralValueType == IntegralType.Double);
        Debug.Assert(output.IntegralValueType == IntegralType.Double);
        _ = context;

        long n = ConversionCount.Effective(input, output, valuesCount);
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
                *(ulong*)dst = bits;
                dst++;
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
                *(ulong*)dst = bits;
                dst++;
            }
            return n;
        }
    }

}
