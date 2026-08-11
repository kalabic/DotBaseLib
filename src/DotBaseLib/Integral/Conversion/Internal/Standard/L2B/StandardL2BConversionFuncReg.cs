namespace DotBase.Integral.Conversion.Internal.Standard.L2B;

internal static class StandardL2BConversionFuncReg
{
    internal static void AddToTable(IConversionDelegateTable table)
    {
        L2BConvertToUInt8.AddToTable(table);
        L2BConvertToInt8.AddToTable(table);
        L2BConvertToUInt16.AddToTable(table);
        L2BConvertToInt16.AddToTable(table);
        L2BConvertToUInt32.AddToTable(table);
        L2BConvertToInt32.AddToTable(table);
        L2BConvertToUInt64.AddToTable(table);
        L2BConvertToInt64.AddToTable(table);
        L2BConvertToFloat.AddToTable(table);
        L2BConvertToDouble.AddToTable(table);
    }
}
