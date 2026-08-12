namespace DotBase.Integral.Conversion.Internal.Standard.B2B;

internal static class StandardB2BConversionFuncReg
{

    internal static StandardConversionDelegates Resolve(
        in IntegralFormat input,
        in IntegralFormat output)
    {
        return output.ValueType switch
        {
            IntegralType.UInt8 => B2BConvertToUInt8.Resolve(input),
            IntegralType.Int8 => B2BConvertToInt8.Resolve(input),
            IntegralType.UInt16 => B2BConvertToUInt16.Resolve(input),
            IntegralType.Int16 => B2BConvertToInt16.Resolve(input),
            IntegralType.UInt32 => B2BConvertToUInt32.Resolve(input),
            IntegralType.Int32 => B2BConvertToInt32.Resolve(input),
            IntegralType.UInt64 => B2BConvertToUInt64.Resolve(input),
            IntegralType.Int64 => B2BConvertToInt64.Resolve(input),
            IntegralType.Float => B2BConvertToFloat.Resolve(input),
            IntegralType.Double => B2BConvertToDouble.Resolve(input),
            _ => throw new ArgumentOutOfRangeException(nameof(output)),
        };
    }
}
