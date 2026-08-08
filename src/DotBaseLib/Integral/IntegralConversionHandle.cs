using DotBase.Integral.Conversion;
using DotBase.Integral.Conversion.Numeric;

namespace DotBase.Integral;


/// <summary>
/// Holds a conversion function and its numeric conversion context.
/// Does not retain state between conversions; used only to supply
/// conversion parameters for a call.
/// <para>
/// The default value and a null function are equivalent null handles:
/// <see cref="IsNull"/> is true and <see cref="Convert"/> returns zero
/// without allocating or throwing.
/// </para>
/// </summary>
public readonly struct IntegralConversionHandle
{
    public static IntegralConversionHandle GetHandle(in IntegralFormat input, in IntegralFormat output)
    {
        return ConversionDelegateTable.Instance.GetConversionHandle(input, output);
    }

    private readonly IntegralSpanConversionFunc? _func;

    private readonly NumericConverters? _context;

    /// <summary>
    /// True when no conversion function is bound (including
    /// <c>default(IntegralConversionHandle)</c>).
    /// </summary>
    public bool IsNull => _func is null;

    /// <summary>
    /// Creates a handle that invokes <paramref name="func"/> with
    /// <paramref name="context"/>. A null <paramref name="func"/> yields a
    /// null handle (same as default).
    /// </summary>
    internal IntegralConversionHandle(
        IntegralSpanConversionFunc? func,
        NumericConverters context)
    {
        _func = func;
        _context = context;
    }

    /// <summary>
    /// Runs the bound conversion, or returns 0 when <see cref="IsNull"/>.
    /// </summary>
    public long Convert(in IntegralSpan input, in IntegralSpan output, long count)
    {
        IntegralSpanConversionFunc? func = _func;
        if (func is null)
        {
            return 0;
        }

        // Non-null func is always paired with a context by GetConversionHandle.
        return func(input, output, count, _context!);
    }
}
