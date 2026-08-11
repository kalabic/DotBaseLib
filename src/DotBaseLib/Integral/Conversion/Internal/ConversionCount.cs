using System.Diagnostics;

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

    /// <summary>
    /// Lane-transfer count for interleaved reader: complete input blocks only,
    /// limited by output value capacity. <paramref name="laneCount"/> is the
    /// requested number of lanes (one per complete input block).
    /// </summary>
    internal static long EffectiveInterleavedReader(
        in IntegralSpan input,
        in IntegralSpan output,
        long laneCount,
        int inputBlockCapacity)
    {
        Debug.Assert(inputBlockCapacity > 1);

        if (laneCount <= 0)
        {
            return 0;
        }

        long completeBlocks = input.ValueCount / inputBlockCapacity;
        if (completeBlocks <= 0)
        {
            return 0;
        }

        long free = output.ValueCount;
        if (free <= 0)
        {
            return 0;
        }

        long n = laneCount;
        if (n > completeBlocks)
        {
            n = completeBlocks;
        }

        if (n > free)
        {
            n = free;
        }

        return n;
    }

    /// <summary>
    /// Lane-transfer count for interleaved writer: complete output blocks only,
    /// limited by input value capacity. <paramref name="laneCount"/> is the
    /// requested number of lanes (one per complete output block).
    /// </summary>
    internal static long EffectiveInterleavedWriter(
        in IntegralSpan input,
        in IntegralSpan output,
        long laneCount,
        int outputBlockCapacity)
    {
        Debug.Assert(outputBlockCapacity > 1);

        if (laneCount <= 0)
        {
            return 0;
        }

        long available = input.ValueCount;
        if (available <= 0)
        {
            return 0;
        }

        long completeBlocks = output.ValueCount / outputBlockCapacity;
        if (completeBlocks <= 0)
        {
            return 0;
        }

        long n = laneCount;
        if (n > available)
        {
            n = available;
        }

        if (n > completeBlocks)
        {
            n = completeBlocks;
        }

        return n;
    }
}
