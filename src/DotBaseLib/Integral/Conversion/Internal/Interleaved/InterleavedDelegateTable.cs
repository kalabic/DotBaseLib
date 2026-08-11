using DotBase.Buffers;
using System.Diagnostics;
using DotBase.Integral.Conversion.Internal.Interleaved.L2L;
using DotBase.Integral.Conversion.Internal.Interleaved.B2L;
using DotBase.Integral.Conversion.Internal.Interleaved.L2B;
using DotBase.Integral.Conversion.Internal.Interleaved.B2B;

namespace DotBase.Integral.Conversion.Internal.Interleaved;


/// <summary>
/// Global conversion dispatch table. Built once at type initialization and then
/// immutable from the public surface: registration is only available through the
/// internal <see cref="IConversionDelegateTable"/> during construction.
/// </summary>
internal sealed class InterleavedDelegateTable
    : IConversionDelegateTable
{
    /// <summary>2 wire endians × 2 wire endians × 10 types × 10 types.</summary>
    public const int TableSize = 2 * 2 * 10 * 10;

    public static InterleavedDelegateTable Instance { get; } = CreateInstance();

    private static InterleavedDelegateTable CreateInstance()
    {
        var table = new InterleavedDelegateTable();
        IConversionDelegateTable registration = table;

        InterleavedL2LConversionFuncReg.AddToTable(registration);
        InterleavedL2BConversionFuncReg.AddToTable(registration);
        InterleavedB2LConversionFuncReg.AddToTable(registration);
        InterleavedB2BConversionFuncReg.AddToTable(registration);

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

    private InterleavedDelegateTable()
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
    /// Default-policy interleaved handle (ignores format conversion policy).
    /// Policy dispatch lives in <see cref="ConversionHandles"/>.
    /// </summary>
    public IntegralConversionHandle GetDefaultHandle(in IntegralFormat input, in IntegralFormat output)
    {
        IntegralSpanConversionFunc? func = GetDefaultFunc(input, output);
        return new IntegralConversionHandle(func, numericFunc: 0, contextFactory: 0);
    }

    /// <summary>
    /// Custom-policy interleaved handle using <paramref name="numericTable"/> for scalar converters.
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
    public IntegralConversionHandle GetInterleavedReaderHandle(
        in IntegralFormat input,
        in IntegralFormat output)
        => GetDefaultHandle(input, output);

    /// <summary>Legacy alias for <see cref="GetDefaultHandle"/>.</summary>
    public IntegralConversionHandle GetInterleavedWriterHandle(
        in IntegralFormat input,
        in IntegralFormat output)
        => GetDefaultHandle(input, output);

    /// <summary>
    /// Builds the table index from the given spans and returns the custom-policy conversion function.
    /// </summary>
    private IntegralSpanConversionFunc? GetCustomFunc(in IntegralSpan input, in IntegralSpan output)
    {
        int index = TableIndex(input, output);
        Debug.Assert(index >= 0 && index < TableSize);
        return _customFuncTable[index];
    }

    private IntegralSpanConversionFunc? GetCustomFunc(in IntegralFormat input, in IntegralFormat output)
    {
        int index = TableIndex(input, output);
        Debug.Assert(index >= 0 && index < TableSize);
        return _customFuncTable[index];
    }

    /// <summary>
    /// Builds the table index from the given spans and returns the default-policy conversion function.
    /// </summary>
    private IntegralSpanConversionFunc? GetDefaultFunc(in IntegralSpan input, in IntegralSpan output)
    {
        int index = TableIndex(input, output);
        Debug.Assert(index >= 0 && index < TableSize);
        return _defaultFuncTable[index];
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
