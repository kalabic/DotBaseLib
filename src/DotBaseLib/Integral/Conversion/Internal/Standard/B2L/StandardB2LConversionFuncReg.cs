namespace DotBase.Integral.Conversion.Internal.Standard.B2L;

internal static class StandardB2LConversionFuncReg
{
    internal static void AddToTable(IConversionDelegateTable table)
    {
        B2LConvertToUInt8.AddToTable(table);
        B2LConvertToInt8.AddToTable(table);
        B2LConvertToUInt16.AddToTable(table);
        B2LConvertToInt16.AddToTable(table);
        B2LConvertToUInt32.AddToTable(table);
        B2LConvertToInt32.AddToTable(table);
        B2LConvertToUInt64.AddToTable(table);
        B2LConvertToInt64.AddToTable(table);
        B2LConvertToFloat.AddToTable(table);
        B2LConvertToDouble.AddToTable(table);
    }
}
