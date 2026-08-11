namespace DotBase.Integral.Conversion.Internal.Standard.B2B;

internal static class StandardB2BConversionFuncReg
{
    internal static void AddToTable(IConversionDelegateTable table)
    {
        B2BConvertToUInt8.AddToTable(table);
        B2BConvertToInt8.AddToTable(table);
        B2BConvertToUInt16.AddToTable(table);
        B2BConvertToInt16.AddToTable(table);
        B2BConvertToUInt32.AddToTable(table);
        B2BConvertToInt32.AddToTable(table);
        B2BConvertToUInt64.AddToTable(table);
        B2BConvertToInt64.AddToTable(table);
        B2BConvertToFloat.AddToTable(table);
        B2BConvertToDouble.AddToTable(table);
    }
}
