using DotBase.Buffers;
using System.Diagnostics;
using DotBase.Integral.Conversion.Internal.Standard.L2L;
using DotBase.Integral.Conversion.Internal.Standard.B2L;
using DotBase.Integral.Conversion.Internal.Standard.L2B;
using DotBase.Integral.Conversion.Internal.Standard.B2B;

namespace DotBase.Integral.Conversion.Internal.Standard;


/// <summary>
/// Global conversion dispatch table. Built once at type initialization and then
/// immutable from the public surface: registration is only available through the
/// internal <see cref="IConversionDelegateTable"/> during construction.
/// </summary>
internal sealed class StandardDelegateTable
    : IConversionDelegateTable
{
    /// <summary>2 wire endians × 2 wire endians × 10 types × 10 types.</summary>
    public const int TableSize = 2 * 2 * 10 * 10;

    public static StandardDelegateTable Instance { get; } = CreateInstance();

    private static StandardDelegateTable CreateInstance()
    {
        var table = new StandardDelegateTable();
        IConversionDelegateTable registration = table;

        StandardL2LConversionFuncReg.AddToTable(registration);
        StandardL2BConversionFuncReg.AddToTable(registration);
        StandardB2LConversionFuncReg.AddToTable(registration);
        StandardB2BConversionFuncReg.AddToTable(registration);

        table.AssertTablesFullyRegistered();
        return table;
    }

    [Conditional("DEBUG")]
    private void AssertTablesFullyRegistered()
    {
        for (int i = 0; i < TableSize; ++i)
        {
            Debug.Assert(
                !ReferenceEquals(_customFuncTable[i], NoopFunc),
                $"Custom conversion table slot {i} is still Noop.");
            Debug.Assert(
                !ReferenceEquals(_defaultFuncTable[i], NoopFunc),
                $"Default conversion table slot {i} is still Noop.");
        }
    }

    /// <summary>
    /// In order to create continuous range of keys starting with 0, enum values
    /// also need to be adjusted to fall into range of [0 .. last enum].
    /// Byte orders are resolved (Native → LE/BE) before indexing.
    /// </summary>
    private static int TableIndex(in IntegralSpan input, in IntegralSpan output)
    {
        return TableIndex(
            input.Format.ByteOrder,
            input.IntegralValueType,
            output.Format.ByteOrder,
            output.IntegralValueType);
    }

    private static int TableIndex(in IntegralFormat input, in IntegralFormat output)
    {
        return TableIndex(input.ByteOrder, input.ValueType, output.ByteOrder, output.ValueType);
    }

    private static int TableIndex(
        ByteOrder inputByteOrder,
        IntegralType inputType,
        ByteOrder outputByteOrder,
        IntegralType outputType)
    {
        ByteOrder inBo = inputByteOrder.Resolve();
        ByteOrder outBo = outputByteOrder.Resolve();

        Debug.Assert(inBo == ByteOrder.LittleEndian || inBo == ByteOrder.BigEndian);
        Debug.Assert(outBo == ByteOrder.LittleEndian || outBo == ByteOrder.BigEndian);
        Debug.Assert(inputType != IntegralType.None);
        Debug.Assert(outputType != IntegralType.None);
        Debug.Assert((int)inputType >= 1 && (int)inputType <= 10);
        Debug.Assert((int)outputType >= 1 && (int)outputType <= 10);

        int in_bo = (int)inBo - 1;
        int out_bo = (int)outBo - 1;
        int in_type = (int)inputType - 1;
        int out_type = (int)outputType - 1;
        return in_bo + 2 * out_bo + 4 * (in_type + 10 * out_type);
    }

    private readonly IntegralSpanConversionFunc[] _customFuncTable;

    private readonly IntegralSpanConversionFunc[] _defaultFuncTable;

    /// <summary>Single shared Noop instance used as the unregistered-slot sentinel.</summary>
    private static readonly IntegralSpanConversionFunc NoopFunc = Noop;

    private StandardDelegateTable()
    {
        _customFuncTable = new IntegralSpanConversionFunc[TableSize];
        _defaultFuncTable = new IntegralSpanConversionFunc[TableSize];
        for (int i = 0; i < TableSize; ++i)
        {
            _customFuncTable[i] = NoopFunc;
            _defaultFuncTable[i] = NoopFunc;
        }
    }

    private static long Noop(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount,
        ConversionContext? context)
    {
        _ = input;
        _ = output;
        _ = valuesCount;
                _ = context;
        return 0;
    }

    /// <summary>
    /// Default-policy contiguous handle (ignores format conversion policy).
    /// Policy dispatch lives in <see cref="ConversionHandles"/>.
    /// </summary>
    public IntegralConversionHandle GetDefaultHandle(in IntegralFormat input, in IntegralFormat output)
    {
        IntegralSpanConversionFunc? func = GetDefaultFunc(input, output);
        return new IntegralConversionHandle(func, numericFunc: 0, contextFactory: 0);
    }

    /// <summary>
    /// Custom-policy contiguous handle using <paramref name="numericTable"/> for scalar converters.
    /// </summary>
    public IntegralConversionHandle GetCustomHandle(
        in IntegralFormat input,
        in IntegralFormat output,
        NumericValueConverters numericTable)
    {
        ArgumentNullException.ThrowIfNull(numericTable);
        IntegralSpanConversionFunc? func = GetCustomFunc(input, output);
        nint converter = numericTable.GetConverterFunctionPointer(input.ValueType, output.ValueType);
        return new IntegralConversionHandle(func, converter, contextFactory: 0);
    }

    /// <summary>Legacy alias for <see cref="GetDefaultHandle"/>.</summary>
    public IntegralConversionHandle GetConversionHandle(in IntegralFormat input, in IntegralFormat output)
        => GetDefaultHandle(input, output);

    private IntegralSpanConversionFunc? GetCustomFunc(in IntegralFormat input, in IntegralFormat output)
    {
        int index = TableIndex(input, output);
        Debug.Assert(index >= 0 && index < TableSize);
        return _customFuncTable[index];
    }

    private IntegralSpanConversionFunc? GetDefaultFunc(in IntegralFormat input, in IntegralFormat output)
    {
        int index = TableIndex(input, output);
        Debug.Assert(index >= 0 && index < TableSize);
        return _defaultFuncTable[index];
    }

    /// <summary>
    /// Stores a custom-policy conversion function (invokes per-value converter delegates).
    /// Available only through <see cref="IConversionDelegateTable"/> (init path).
    /// </summary>
    void IConversionDelegateTable.SetCustomFunc(
        IntegralSpanConversionFunc func,
        ByteOrder inputByteOrder,
        IntegralType inputType,
        ByteOrder outputByteOrder,
        IntegralType outputType)
    {
        int index = TableIndex(inputByteOrder, inputType, outputByteOrder, outputType);
        Debug.Assert(index >= 0 && index < TableSize);
        _customFuncTable[index] = func;
    }

    /// <summary>
    /// Stores a default-policy conversion function (built-in numeric rules).
    /// Available only through <see cref="IConversionDelegateTable"/> (init path).
    /// </summary>
    void IConversionDelegateTable.SetDefaultFunc(
        IntegralSpanConversionFunc func,
        ByteOrder inputByteOrder,
        IntegralType inputType,
        ByteOrder outputByteOrder,
        IntegralType outputType)
    {
        int index = TableIndex(inputByteOrder, inputType, outputByteOrder, outputType);
        Debug.Assert(index >= 0 && index < TableSize);
        _defaultFuncTable[index] = func;
    }
}
