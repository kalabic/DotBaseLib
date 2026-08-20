using System.Diagnostics;
using DotBase.Integral;

namespace DotBase.Buffers.Integral.Internal;


/// <summary>
/// Shared IntegralSpan ring transfer helpers.
/// <para>
/// On <see cref="RingBufferStorage"/>, <c>ReadLE*</c>/<c>WriteLE*</c> reverse
/// external bytes relative to stream order; <c>ReadBE*</c>/<c>WriteBE*</c> copy
/// stream order as-is. Flip paths therefore use the LE* helpers on both LE and
/// BE rings — the name means reverse-on-transfer, not ring wire endian.
/// </para>
/// </summary>
internal static unsafe class IntegralRingSpanOps
{
    private const int ScratchByteCount = 512;

    internal static void ValidateSpan(
        in IntegralSpan span,
        string parameterName)
    {
        if (!span.Capacity.IsValueAligned() ||
            span.Capacity.ByteCount != span.Length ||
            span.Capacity.BlockCapacity != span.Format.BlockCapacity)
        {
            throw new ArgumentException(
                "The integral span has inconsistent capacity metadata.",
                parameterName);
        }

        // Reject undefined / inconsistent formats (including poisoned byte order).
        span.Format.Validate();

        if (span.Length == 0 &&
            span.IntegralValueType == IntegralType.None)
        {
            return;
        }

        int valueByteCount = span.Format.ValueSize;
        if (span.Capacity.ValueByteCount != valueByteCount ||
            span.Format.BlockCapacity <= 0 ||
            span.Length % valueByteCount != 0 ||
            span.Offset % valueByteCount != 0 ||
            (span.Length > 0 && span.DataPtr is null))
        {
            throw new ArgumentException(
                "The integral span is not a complete scalar-value descriptor.",
                parameterName);
        }

    }

    /// <summary>
    /// How many scalar values lie in complete blocks of <paramref name="span"/>
    /// that also fit in <paramref name="availableBytes"/> of ring free/stored.
    /// Trailing partial blocks are never counted.
    /// </summary>
    internal static long CountBlockCompleteValues(
        in IntegralSpan span,
        int availableBytes)
    {
        int blockCapacity = span.Capacity.BlockCapacity;
        long blockByteCount = span.Capacity.BlockByteCount;
        if (blockCapacity <= 0 ||
            blockByteCount <= 0 ||
            availableBytes <= 0 ||
            span.Length == 0)
        {
            return 0;
        }

        long spanBlocks = span.BlockCount;
        long availableBlocks = availableBytes / blockByteCount;
        long blocks = Math.Min(spanBlocks, availableBlocks);
        return blocks * blockCapacity;
    }

    /// <summary>
    /// Byte length of all complete blocks in <paramref name="span"/> (excludes trailing values).
    /// </summary>
    internal static long BlockCompleteByteCount(in IntegralSpan span)
    {
        long blockByteCount = span.Capacity.BlockByteCount;
        if (blockByteCount <= 0)
        {
            return 0;
        }

        return span.BlockCount * blockByteCount;
    }

    /// <summary>
    /// Read <paramref name="valueCount"/> values from the ring into
    /// <paramref name="destination"/>, reversing each lane relative to stream
    /// order (foreign span endian).
    /// </summary>
    /// <returns>Scalar values transferred (same as <paramref name="valueCount"/> on success).</returns>
    internal static long ReadEndianFlip(
        ref RingBufferStorage storage,
        byte* destination,
        long valueCount,
        int elementSize)
    {
        if (valueCount == 0)
        {
            return 0;
        }

        if (elementSize <= 1)
        {
            int byteCount = checked((int)(valueCount * elementSize));
            int read = storage.Read(destination, byteCount);
            Debug.Assert(read == byteCount);
            return valueCount;
        }

        // Single value: reverse-read directly from the ring stream.
        if (valueCount == 1)
        {
            switch (elementSize)
            {
                case 2:
                    storage.ReadLE2(destination);
                    break;
                case 4:
                    storage.ReadLE4(destination);
                    break;
                case 8:
                    storage.ReadLE8(destination);
                    break;
                default:
                    throw new NotSupportedException(
                        $"Element size {elementSize} is not supported.");
            }

            return 1;
        }

        int byteCountMulti = checked((int)(valueCount * elementSize));
        int bytesRead = storage.Read(destination, byteCountMulti);
        Debug.Assert(bytesRead == byteCountMulti);
        IntegralPrimitives.ReverseLanesInPlace(
            destination,
            valueCount,
            elementSize);
        return valueCount;
    }

    /// <summary>
    /// Write <paramref name="valueCount"/> values from <paramref name="source"/>
    /// into the ring, reversing each lane relative to stream order (foreign
    /// span endian). Uses storage reverse-write helpers for the single-value path.
    /// </summary>
    /// <returns>Scalar values transferred (same as <paramref name="valueCount"/> on success).</returns>
    internal static long WriteEndianFlip(
        ref RingBufferStorage storage,
        byte* source,
        long valueCount,
        int elementSize)
    {
        if (valueCount == 0)
        {
            return 0;
        }

        if (elementSize <= 1)
        {
            int byteCount = checked((int)(valueCount * elementSize));
            int written = storage.Write(source, byteCount);
            Debug.Assert(written == byteCount);
            return valueCount;
        }

        // Single value: reverse-write directly into the ring stream.
        if (valueCount == 1)
        {
            switch (elementSize)
            {
                case 2:
                    storage.WriteLE2(source);
                    break;
                case 4:
                    storage.WriteLE4(source);
                    break;
                case 8:
                    storage.WriteLE8(source);
                    break;
                default:
                    throw new NotSupportedException(
                        $"Element size {elementSize} is not supported.");
            }

            return 1;
        }

        int scratchValueCount = Math.Max(1, ScratchByteCount / elementSize);
        byte* scratch = stackalloc byte[scratchValueCount * elementSize];

        long sourcePosition = 0;
        while (sourcePosition < valueCount)
        {
            int chunkValueCount = (int)Math.Min(
                scratchValueCount,
                valueCount - sourcePosition);
            byte* src = source + sourcePosition * elementSize;
            IntegralPrimitives.ReverseCopyLanes(
                src,
                scratch,
                chunkValueCount,
                elementSize);

            int chunkByteCount = checked((int)(
                (long)chunkValueCount * elementSize));
            int bytesWritten = storage.Write(scratch, chunkByteCount);
            Debug.Assert(bytesWritten == chunkByteCount);
            sourcePosition += chunkValueCount;
        }

        return valueCount;
    }
}


/// <summary>
/// IntegralSpan transfer for little-endian ring stream order.
/// Typed scalar/bulk R/W lives on the ring types.
/// </summary>
internal static class IntegralRingOperationsLE
{
    private const ByteOrder RingByteOrder = ByteOrder.LittleEndian;

    /// <summary>
    /// Trusted (matches <see cref="IIntegralRingBuffer.Read"/>): no span validation.
    /// Block-complete values only.
    /// </summary>
    internal static unsafe int Read(
        ref RingBufferStorage storage,
        in IntegralSpan destination)
    {
        if (!storage.IsOpen || destination.Length == 0)
        {
            return 0;
        }

        long valueCount = IntegralRingSpanOps.CountBlockCompleteValues(
            destination,
            storage.StoredBytes);

        return checked((int)ReadValues(
            ref storage,
            destination,
            valueCount));
    }

    /// <summary>
    /// Checked (matches <see cref="IIntegralRingBuffer.ReadChecked"/>): validates, then <see cref="Read"/>.
    /// </summary>
    internal static unsafe int ReadChecked(
        ref RingBufferStorage storage,
        in IntegralSpan destination)
    {
        IntegralRingSpanOps.ValidateSpan(
            destination,
            nameof(destination));
        return Read(ref storage, destination);
    }

    /// <summary>
    /// Trusted try-read: all complete blocks of the span, or false.
    /// </summary>
    internal static unsafe bool TryRead(
        ref RingBufferStorage storage,
        in IntegralSpan destination)
    {
        long requiredByteCount = IntegralRingSpanOps.BlockCompleteByteCount(destination);
        if (!storage.IsOpen ||
            storage.StoredBytes < requiredByteCount)
        {
            return false;
        }

        long valueCount = IntegralRingSpanOps.CountBlockCompleteValues(
            destination,
            storage.StoredBytes);
        long readCount = ReadValues(
            ref storage,
            destination,
            valueCount);
        Debug.Assert(readCount == valueCount);
        return true;
    }

    internal static unsafe bool TryReadChecked(
        ref RingBufferStorage storage,
        in IntegralSpan destination)
    {
        IntegralRingSpanOps.ValidateSpan(
            destination,
            nameof(destination));
        return TryRead(ref storage, destination);
    }

    /// <summary>
    /// Trusted write: partial, block-complete only.
    /// </summary>
    internal static unsafe int Write(
        ref RingBufferStorage storage,
        in IntegralSpan source)
    {
        if (!storage.IsOpen || source.Length == 0)
        {
            return 0;
        }

        long valueCount = IntegralRingSpanOps.CountBlockCompleteValues(
            source,
            storage.FreeBytes);

        return checked((int)WriteValues(
            ref storage,
            source,
            valueCount));
    }

    internal static unsafe int WriteChecked(
        ref RingBufferStorage storage,
        in IntegralSpan source)
    {
        IntegralRingSpanOps.ValidateSpan(
            source,
            nameof(source));
        return Write(ref storage, source);
    }

    /// <summary>
    /// Trusted try-write: all complete blocks of the span, or false.
    /// </summary>
    internal static unsafe bool TryWrite(
        ref RingBufferStorage storage,
        in IntegralSpan source)
    {
        long requiredByteCount = IntegralRingSpanOps.BlockCompleteByteCount(source);
        if (!storage.IsOpen ||
            storage.FreeBytes < requiredByteCount)
        {
            return false;
        }

        long valueCount = IntegralRingSpanOps.CountBlockCompleteValues(
            source,
            storage.FreeBytes);
        long writtenCount = WriteValues(
            ref storage,
            source,
            valueCount);
        Debug.Assert(writtenCount == valueCount);
        return true;
    }

    internal static unsafe bool TryWriteChecked(
        ref RingBufferStorage storage,
        in IntegralSpan source)
    {
        IntegralRingSpanOps.ValidateSpan(
            source,
            nameof(source));
        return TryWrite(ref storage, source);
    }

    /// <summary>Delegates to shared structural validation.</summary>
    internal static unsafe void ValidateSpan(
        in IntegralSpan span,
        string parameterName)
    {
        IntegralRingSpanOps.ValidateSpan(
            span,
            parameterName);
    }

    private static unsafe long ReadValues(
        ref RingBufferStorage storage,
        in IntegralSpan destination,
        long valueCount)
    {
        if (valueCount == 0)
        {
            return 0;
        }

        int elementSize = destination.Capacity.ValueByteCount;
        if (destination.Format.ByteOrder.Resolve() ==
            RingByteOrder)
        {
            int byteCount = checked((int)(
                valueCount * elementSize));
            int bytesRead = storage.Read(
                destination.DataPtr,
                byteCount);
            Debug.Assert(bytesRead == byteCount);
            return valueCount;
        }

        // Destination is not LE wire — reverse each lane out of LE ring stream.
        return IntegralRingSpanOps.ReadEndianFlip(
            ref storage,
            destination.DataPtr,
            valueCount,
            elementSize);
    }

    private static unsafe long WriteValues(
        ref RingBufferStorage storage,
        in IntegralSpan source,
        long valueCount)
    {
        if (valueCount == 0)
        {
            return 0;
        }

        int elementSize = source.Capacity.ValueByteCount;
        if (source.Format.ByteOrder.Resolve() ==
            RingByteOrder)
        {
            int byteCount = checked((int)(
                valueCount * elementSize));
            int bytesWritten = storage.Write(
                source.DataPtr,
                byteCount);
            Debug.Assert(bytesWritten == byteCount);
            return valueCount;
        }

        // Source is not LE wire — reverse each lane into LE ring stream order.
        return IntegralRingSpanOps.WriteEndianFlip(
            ref storage,
            source.DataPtr,
            valueCount,
            elementSize);
    }
}


/// <summary>
/// IntegralSpan transfer for big-endian ring stream order.
/// Typed scalar/bulk R/W lives on the ring types.
/// </summary>
internal static class IntegralRingOperationsBE
{
    private const ByteOrder RingByteOrder = ByteOrder.BigEndian;

    /// <summary>
    /// Trusted (matches <see cref="IIntegralRingBuffer.Read"/>): no span validation.
    /// Block-complete values only.
    /// </summary>
    internal static unsafe int Read(
        ref RingBufferStorage storage,
        in IntegralSpan destination)
    {
        if (!storage.IsOpen || destination.Length == 0)
        {
            return 0;
        }

        long valueCount = IntegralRingSpanOps.CountBlockCompleteValues(
            destination,
            storage.StoredBytes);

        return checked((int)ReadValues(
            ref storage,
            destination,
            valueCount));
    }

    /// <summary>
    /// Checked (matches <see cref="IIntegralRingBuffer.ReadChecked"/>): validates, then <see cref="Read"/>.
    /// </summary>
    internal static unsafe int ReadChecked(
        ref RingBufferStorage storage,
        in IntegralSpan destination)
    {
        IntegralRingSpanOps.ValidateSpan(
            destination,
            nameof(destination));
        return Read(ref storage, destination);
    }

    /// <summary>
    /// Trusted try-read: all complete blocks of the span, or false.
    /// </summary>
    internal static unsafe bool TryRead(
        ref RingBufferStorage storage,
        in IntegralSpan destination)
    {
        long requiredByteCount = IntegralRingSpanOps.BlockCompleteByteCount(destination);
        if (!storage.IsOpen ||
            storage.StoredBytes < requiredByteCount)
        {
            return false;
        }

        long valueCount = IntegralRingSpanOps.CountBlockCompleteValues(
            destination,
            storage.StoredBytes);
        long readCount = ReadValues(
            ref storage,
            destination,
            valueCount);
        Debug.Assert(readCount == valueCount);
        return true;
    }

    internal static unsafe bool TryReadChecked(
        ref RingBufferStorage storage,
        in IntegralSpan destination)
    {
        IntegralRingSpanOps.ValidateSpan(
            destination,
            nameof(destination));
        return TryRead(ref storage, destination);
    }

    /// <summary>
    /// Trusted write: partial, block-complete only.
    /// </summary>
    internal static unsafe int Write(
        ref RingBufferStorage storage,
        in IntegralSpan source)
    {
        if (!storage.IsOpen || source.Length == 0)
        {
            return 0;
        }

        long valueCount = IntegralRingSpanOps.CountBlockCompleteValues(
            source,
            storage.FreeBytes);

        return checked((int)WriteValues(
            ref storage,
            source,
            valueCount));
    }

    internal static unsafe int WriteChecked(
        ref RingBufferStorage storage,
        in IntegralSpan source)
    {
        IntegralRingSpanOps.ValidateSpan(
            source,
            nameof(source));
        return Write(ref storage, source);
    }

    /// <summary>
    /// Trusted try-write: all complete blocks of the span, or false.
    /// </summary>
    internal static unsafe bool TryWrite(
        ref RingBufferStorage storage,
        in IntegralSpan source)
    {
        long requiredByteCount = IntegralRingSpanOps.BlockCompleteByteCount(source);
        if (!storage.IsOpen ||
            storage.FreeBytes < requiredByteCount)
        {
            return false;
        }

        long valueCount = IntegralRingSpanOps.CountBlockCompleteValues(
            source,
            storage.FreeBytes);
        long writtenCount = WriteValues(
            ref storage,
            source,
            valueCount);
        Debug.Assert(writtenCount == valueCount);
        return true;
    }

    internal static unsafe bool TryWriteChecked(
        ref RingBufferStorage storage,
        in IntegralSpan source)
    {
        IntegralRingSpanOps.ValidateSpan(
            source,
            nameof(source));
        return TryWrite(ref storage, source);
    }

    /// <summary>Delegates to shared structural validation.</summary>
    internal static unsafe void ValidateSpan(
        in IntegralSpan span,
        string parameterName)
    {
        IntegralRingSpanOps.ValidateSpan(
            span,
            parameterName);
    }

    private static unsafe long ReadValues(
        ref RingBufferStorage storage,
        in IntegralSpan destination,
        long valueCount)
    {
        if (valueCount == 0)
        {
            return 0;
        }

        int elementSize = destination.Capacity.ValueByteCount;
        if (destination.Format.ByteOrder.Resolve() ==
            RingByteOrder)
        {
            int byteCount = checked((int)(
                valueCount * elementSize));
            int bytesRead = storage.Read(
                destination.DataPtr,
                byteCount);
            Debug.Assert(bytesRead == byteCount);
            return valueCount;
        }

        // Destination is not BE wire — reverse each lane out of BE ring stream.
        return IntegralRingSpanOps.ReadEndianFlip(
            ref storage,
            destination.DataPtr,
            valueCount,
            elementSize);
    }

    private static unsafe long WriteValues(
        ref RingBufferStorage storage,
        in IntegralSpan source,
        long valueCount)
    {
        if (valueCount == 0)
        {
            return 0;
        }

        int elementSize = source.Capacity.ValueByteCount;
        if (source.Format.ByteOrder.Resolve() ==
            RingByteOrder)
        {
            int byteCount = checked((int)(
                valueCount * elementSize));
            int bytesWritten = storage.Write(
                source.DataPtr,
                byteCount);
            Debug.Assert(bytesWritten == byteCount);
            return valueCount;
        }

        // Source is not BE wire — reverse each lane into BE ring stream order.
        return IntegralRingSpanOps.WriteEndianFlip(
            ref storage,
            source.DataPtr,
            valueCount,
            elementSize);
    }
}
