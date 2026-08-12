namespace DotBase.Integral.Conversion.Internal.Standard.L2B;

internal static class StandardL2BConversionFuncReg
{

    internal static StandardConversionDelegates Resolve(
        in IntegralFormat input,
        in IntegralFormat output)
    {
        return output.ValueType switch
        {
            IntegralType.UInt8 => L2BConvertToUInt8.Resolve(input),
            IntegralType.Int8 => L2BConvertToInt8.Resolve(input),
            IntegralType.UInt16 => L2BConvertToUInt16.Resolve(input),
            IntegralType.Int16 => L2BConvertToInt16.Resolve(input),
            IntegralType.UInt32 => L2BConvertToUInt32.Resolve(input),
            IntegralType.Int32 => L2BConvertToInt32.Resolve(input),
            IntegralType.UInt64 => L2BConvertToUInt64.Resolve(input),
            IntegralType.Int64 => L2BConvertToInt64.Resolve(input),
            IntegralType.Float => L2BConvertToFloat.Resolve(input),
            IntegralType.Double => L2BConvertToDouble.Resolve(input),
            _ => throw new ArgumentOutOfRangeException(nameof(output)),
        };
    }
}
