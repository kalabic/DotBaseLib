using System.Diagnostics;
// InterleavedReaderContext / InterleavedWriterContext live in parent Conversion namespace.

namespace DotBase.Integral.Conversion.Internal.Interleaved;


/// <summary>
/// Resolves reader/writer layout from <see cref="IntegralSpanConversionFunc"/> context
/// and computes the effective lane-transfer count.
/// </summary>
internal static class InterleavedAccess
{
    /// <summary>
    /// Interprets <paramref name="context"/> as reader or writer layout.
    /// Returns effective lane count (0 if none). Asserts on invalid context/layout.
    /// </summary>
    internal static long Resolve(
        object? context,
        in IntegralSpan input,
        in IntegralSpan output,
        long laneCount,
        out int srcStride,
        out int dstStride,
        out int srcLane,
        out int dstLane)
    {
        if (context is InterleavedReaderContext reader)
        {
            Debug.Assert(reader.InputBlockCapacity > 1);
            Debug.Assert(reader.InputBlockCapacity == input.Format.BlockCapacity);
            Debug.Assert(output.Format.BlockCapacity == 1);
            Debug.Assert((uint)reader.ValueIndex < (uint)reader.InputBlockCapacity);

            srcStride = reader.InputBlockCapacity;
            dstStride = 1;
            srcLane = reader.ValueIndex;
            dstLane = 0;

            return ConversionCount.EffectiveInterleavedReader(
                input,
                output,
                laneCount,
                reader.InputBlockCapacity);
        }

        if (context is InterleavedWriterContext writer)
        {
            Debug.Assert(writer.OutputBlockCapacity > 1);
            Debug.Assert(writer.OutputBlockCapacity == output.Format.BlockCapacity);
            Debug.Assert(input.Format.BlockCapacity == 1);
            Debug.Assert((uint)writer.ValueIndex < (uint)writer.OutputBlockCapacity);

            srcStride = 1;
            dstStride = writer.OutputBlockCapacity;
            srcLane = 0;
            dstLane = writer.ValueIndex;

            return ConversionCount.EffectiveInterleavedWriter(
                input,
                output,
                laneCount,
                writer.OutputBlockCapacity);
        }

        Debug.Assert(
            false,
            "Interleaved conversion requires InterleavedReaderContext or InterleavedWriterContext.");
        srcStride = 1;
        dstStride = 1;
        srcLane = 0;
        dstLane = 0;
        return 0;
    }
}
