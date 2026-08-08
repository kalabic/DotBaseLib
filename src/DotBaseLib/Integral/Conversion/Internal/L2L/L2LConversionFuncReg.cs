namespace DotBase.Integral.Conversion.Internal.L2L;

internal static class L2LConversionFuncReg
{
    internal static void AddToTable(IConversionDelegateTable table)
    {
        L2LConvertToUInt8.AddToTable(table);
        L2LConvertToInt8.AddToTable(table);
        L2LConvertToUInt16.AddToTable(table);
        L2LConvertToInt16.AddToTable(table);
        L2LConvertToUInt32.AddToTable(table);
        L2LConvertToInt32.AddToTable(table);
        L2LConvertToUInt64.AddToTable(table);
        L2LConvertToInt64.AddToTable(table);
        L2LConvertToFloat.AddToTable(table);
        L2LConvertToDouble.AddToTable(table);
    }
}
