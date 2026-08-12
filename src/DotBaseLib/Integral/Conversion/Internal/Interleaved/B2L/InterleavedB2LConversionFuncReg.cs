namespace DotBase.Integral.Conversion.Internal.Interleaved.B2L;

internal static class InterleavedB2LConversionFuncReg
{

    internal static InterleavedConversionDelegates Resolve(
        in IntegralFormat input,
        in IntegralFormat output)
    {
        return output.ValueType switch
        {
            IntegralType.UInt8 => B2LConvertToUInt8.Resolve(input),
            IntegralType.Int8 => B2LConvertToInt8.Resolve(input),
            IntegralType.UInt16 => B2LConvertToUInt16.Resolve(input),
            IntegralType.Int16 => B2LConvertToInt16.Resolve(input),
            IntegralType.UInt32 => B2LConvertToUInt32.Resolve(input),
            IntegralType.Int32 => B2LConvertToInt32.Resolve(input),
            IntegralType.UInt64 => B2LConvertToUInt64.Resolve(input),
            IntegralType.Int64 => B2LConvertToInt64.Resolve(input),
            IntegralType.Float => B2LConvertToFloat.Resolve(input),
            IntegralType.Double => B2LConvertToDouble.Resolve(input),
            _ => throw new ArgumentOutOfRangeException(nameof(output)),
        };
    }
}
