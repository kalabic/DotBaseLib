namespace DotBase.Integral.Conversion.Internal.Interleaved.L2L;

internal static class InterleavedL2LConversionFuncReg
{

    internal static InterleavedConversionDelegates Resolve(
        in IntegralFormat input,
        in IntegralFormat output)
    {
        return output.ValueType switch
        {
            IntegralType.UInt8 => L2LConvertToUInt8.Resolve(input),
            IntegralType.Int8 => L2LConvertToInt8.Resolve(input),
            IntegralType.UInt16 => L2LConvertToUInt16.Resolve(input),
            IntegralType.Int16 => L2LConvertToInt16.Resolve(input),
            IntegralType.UInt32 => L2LConvertToUInt32.Resolve(input),
            IntegralType.Int32 => L2LConvertToInt32.Resolve(input),
            IntegralType.UInt64 => L2LConvertToUInt64.Resolve(input),
            IntegralType.Int64 => L2LConvertToInt64.Resolve(input),
            IntegralType.Float => L2LConvertToFloat.Resolve(input),
            IntegralType.Double => L2LConvertToDouble.Resolve(input),
            _ => throw new ArgumentOutOfRangeException(nameof(output)),
        };
    }
}
