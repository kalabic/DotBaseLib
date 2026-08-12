using System.Diagnostics;
// Interleaved*Context types live in parent Conversion namespace.

namespace DotBase.Integral.Conversion.Internal.Interleaved;


/// <summary>
/// Resolves reader/writer/transfer layout from <see cref="IntegralSpanConversionFunc"/> context
/// and computes the effective lane-transfer count.
/// </summary>
internal static class InterleavedAccess
{
    /// <summary>
    /// Interprets <paramref name="context"/> as reader, writer, or transfer layout.
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

        if (context is InterleavedTransferContext transfer)
        {
            Debug.Assert(transfer.InputBlockCapacity > 1);
            Debug.Assert(transfer.OutputBlockCapacity > 1);
            Debug.Assert(transfer.InputBlockCapacity == input.Format.BlockCapacity);
            Debug.Assert(transfer.OutputBlockCapacity == output.Format.BlockCapacity);
            Debug.Assert((uint)transfer.InputValueIndex < (uint)transfer.InputBlockCapacity);
            Debug.Assert((uint)transfer.OutputValueIndex < (uint)transfer.OutputBlockCapacity);

            srcStride = transfer.InputBlockCapacity;
            dstStride = transfer.OutputBlockCapacity;
            srcLane = transfer.InputValueIndex;
            dstLane = transfer.OutputValueIndex;

            return ConversionCount.EffectiveInterleavedTransfer(
                input,
                output,
                laneCount,
                transfer.InputBlockCapacity,
                transfer.OutputBlockCapacity);
        }

        Debug.Assert(
            false,
            "Interleaved conversion requires InterleavedReaderContext, InterleavedWriterContext, or InterleavedTransferContext.");
        srcStride = 1;
        dstStride = 1;
        srcLane = 0;
        dstLane = 0;
        return 0;
    }
}
