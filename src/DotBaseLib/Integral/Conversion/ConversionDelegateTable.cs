using DotBase.Buffers;
using DotBase.Integral.Conversion.Internal.L2L;
using DotBase.Integral.Conversion.Internal.L2B;
using DotBase.Integral.Conversion.Internal.B2L;
using DotBase.Integral.Conversion.Internal.B2B;
using System.Diagnostics;
using DotBase.Integral.Conversion.Numeric;
using DotBase.Integral.Conversion.Numeric.Defaults;

namespace DotBase.Integral.Conversion;


/// <summary>
/// Global conversion dispatch table. Built once at type initialization and then
/// immutable from the public surface: registration is only available through the
/// internal <see cref="IConversionDelegateTable"/> during construction.
/// </summary>
public sealed class ConversionDelegateTable
    : IConversionDelegateTable
{
    /// <summary>2 wire endians × 2 wire endians × 10 types × 10 types.</summary>
    public const int TableSize = 2 * 2 * 10 * 10;

    public static ConversionDelegateTable Instance { get; } = CreateInstance();

    private static ConversionDelegateTable CreateInstance()
    {
        var table = new ConversionDelegateTable();
        IConversionDelegateTable registration = table;

        L2LConversionFuncReg.AddToTable(registration);
        L2BConversionFuncReg.AddToTable(registration);
        B2LConversionFuncReg.AddToTable(registration);
        B2BConversionFuncReg.AddToTable(registration);

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

    private readonly NumericConverters _numConverters = new NumericConverters(DefaultConvertersFactory.Instance);

    /// <summary>Single shared Noop instance used as the unregistered-slot sentinel.</summary>
    private static readonly IntegralSpanConversionFunc NoopFunc = Noop;

    private ConversionDelegateTable()
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
        NumericConverters context)
    {
        _ = input;
        _ = output;
        _ = valuesCount;
        _ = context;
        return 0;
    }

    /// <summary>
    /// Returns a handle for the converter matching the spans' types and wire endians.
    /// <list type="bullet">
    ///   <item>
    ///     If <paramref name="output"/> does not provide its own <see cref="IIntegralValueConverter"/>,
    ///     conversion will use only 'default' conversion functions.
    ///   </item>
    ///   <item>
    ///     If <paramref name="output"/> provides only value converter delegate <see cref="IIntegralValueConverter.Converters"/>,
    ///     conversion will use custom conversion function that will invoke provided delegate for each and every value.
    ///   </item>
    ///   <item>
    ///     If <paramref name="output"/> provides <see cref="IIntegralValueConverter.Func"/>, then
    ///     it is responsible for every step of conversion process.
    ///   </item>
    /// </list>
    /// </summary>
    public IntegralConversionHandle GetConversionHandle(in IntegralSpan input, in IntegralSpan output)
    {
        IntegralSpanConversionFunc? func = output.Format.Converter?.Func;
        NumericConverters? numericConverter = output.Format.Converter?.Converters;
        if (func is not null)
        {
            if (numericConverter is null)
            {
                return new IntegralConversionHandle(func, _numConverters);
            }
            else
            {
                return new IntegralConversionHandle(func, numericConverter);
            }
        }
        else
        {
            if (numericConverter is null)
            {
                func = GetDefaultFunc(input, output);
                return new IntegralConversionHandle(func, _numConverters);
            }
            else
            {
                func = GetCustomFunc(input, output);
                return new IntegralConversionHandle(func, numericConverter);
            }
        }
    }

    /// <summary>
    /// Builds the table index from the given spans and returns the custom-policy conversion function.
    /// </summary>
    private IntegralSpanConversionFunc? GetCustomFunc(in IntegralSpan input, in IntegralSpan output)
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
