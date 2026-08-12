using DotBase.Buffers;
using System.Diagnostics;
using DotBase.Integral.Conversion.Internal.Standard.L2L;
using DotBase.Integral.Conversion.Internal.Standard.B2L;
using DotBase.Integral.Conversion.Internal.Standard.L2B;
using DotBase.Integral.Conversion.Internal.Standard.B2B;

namespace DotBase.Integral.Conversion.Internal.Standard;


/// <summary>
/// Global conversion dispatch table. Each immutable custom/default delegate-handle
/// pair is resolved and published on first use of its exact endian/type slot.
/// </summary>
internal sealed class StandardDelegateTable
{
    /// <summary>2 wire endians × 2 wire endians × 10 types × 10 types.</summary>
    public const int TableSize = 2 * 2 * 10 * 10;

    public static StandardDelegateTable Instance { get; } = new StandardDelegateTable();

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

    private readonly StandardConversionDelegates?[] _funcTable;

    private readonly object _resolveLock = new();

    private StandardDelegateTable()
    {
        _funcTable = new StandardConversionDelegates?[TableSize];
    }

    private static StandardConversionDelegates ResolveFunctions(
        in IntegralFormat input,
        in IntegralFormat output)
    {
        return input.ByteOrder.Resolve() switch
        {
            ByteOrder.LittleEndian => output.ByteOrder.Resolve() switch
            {
                ByteOrder.LittleEndian =>
                    StandardL2LConversionFuncReg.Resolve(input, output),
                ByteOrder.BigEndian =>
                    StandardL2BConversionFuncReg.Resolve(input, output),
                _ => throw new InvalidOperationException(
                    "The output byte order cannot be resolved."),
            },
            ByteOrder.BigEndian => output.ByteOrder.Resolve() switch
            {
                ByteOrder.LittleEndian =>
                    StandardB2LConversionFuncReg.Resolve(input, output),
                ByteOrder.BigEndian =>
                    StandardB2BConversionFuncReg.Resolve(input, output),
                _ => throw new InvalidOperationException(
                    "The output byte order cannot be resolved."),
            },
            _ => throw new InvalidOperationException(
                "The input byte order cannot be resolved."),
        };
    }

    private StandardConversionDelegates GetFunctions(
        int index,
        in IntegralFormat input,
        in IntegralFormat output)
    {
        StandardConversionDelegates? functions =
            Volatile.Read(ref _funcTable[index]);
        if (functions is not null)
        {
            return functions;
        }

        lock (_resolveLock)
        {
            functions = _funcTable[index];
            if (functions is null)
            {
                functions = ResolveFunctions(input, output);
                Volatile.Write(ref _funcTable[index], functions);
            }

            return functions;
        }
    }

    /// <summary>
    /// Default-policy contiguous handle (ignores format conversion policy).
    /// Policy dispatch lives in <see cref="ConversionHandles"/>.
    /// </summary>
    public IntegralConversionHandle GetDefaultHandle(in IntegralFormat input, in IntegralFormat output)
    {
        nint func = GetDefaultFunc(input, output);
        return new IntegralConversionHandle(func);
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
        nint func = GetCustomFunc(input, output);
        nint converter = numericTable.GetConverterHandle(input.ValueType, output.ValueType);
        Debug.Assert(converter != 0);
        return new IntegralConversionHandle(
            func,
            converter,
            output.ConversionPolicy);
    }

    /// <summary>Legacy alias for <see cref="GetDefaultHandle"/>.</summary>
    public IntegralConversionHandle GetConversionHandle(in IntegralFormat input, in IntegralFormat output)
        => GetDefaultHandle(input, output);

    private nint GetCustomFunc(in IntegralFormat input, in IntegralFormat output)
    {
        int index = TableIndex(input, output);
        Debug.Assert(index >= 0 && index < TableSize);
        return GetFunctions(index, input, output).Custom;
    }

    private nint GetDefaultFunc(in IntegralFormat input, in IntegralFormat output)
    {
        int index = TableIndex(input, output);
        Debug.Assert(index >= 0 && index < TableSize);
        return GetFunctions(index, input, output).Default;
    }

}
