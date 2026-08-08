namespace DotBase.Integral.Conversion.Internal;


/// <summary>
/// Shared count clamping for conversion kernels.
/// </summary>
internal static class ConversionCount
{
    /// <summary>
    /// Effective value count constrained by request and both spans' capacities.
    /// Negative <paramref name="valuesCount"/> is treated as zero.
    /// </summary>
    internal static long Effective(
        in IntegralSpan input,
        in IntegralSpan output,
        long valuesCount)
    {
        if (valuesCount <= 0)
        {
            return 0;
        }

        long available = input.ValueCount;
        if (available <= 0)
        {
            return 0;
        }

        long free = output.ValueCount;
        if (free <= 0)
        {
            return 0;
        }

        long n = valuesCount;
        if (n > available)
        {
            n = available;
        }

        if (n > free)
        {
            n = free;
        }

        return n;
    }
}
