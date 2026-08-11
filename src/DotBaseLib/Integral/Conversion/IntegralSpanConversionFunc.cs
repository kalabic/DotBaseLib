namespace DotBase.Integral.Conversion;


/// <summary>
/// Converts (or copies) integral values from <paramref name="input"/> into
/// <paramref name="output"/>.
/// </summary>
/// <param name="input">Source span of integral values.</param>
/// <param name="output">Destination span for converted or copied values.</param>
/// <param name="valueCount">Requested number of values to convert or copy.</param>
/// <param name="context">
/// Optional conversion context. When a per-value scalar converter is required,
/// this is a <see cref="NumericConversionContext"/> (or subclass) whose
/// <see cref="NumericConversionContext.NumericFunc"/> has been resolved from the handle.
/// Default-policy kernels may receive null.
/// </param>
/// <returns>
/// The number of values actually copied, limited by the values available in
/// <paramref name="input"/> and the remaining capacity of <paramref name="output"/>.
/// </returns>
public delegate long IntegralSpanConversionFunc(
    in IntegralSpan input,
    in IntegralSpan output,
    long valueCount,
    ConversionContext? context
);
