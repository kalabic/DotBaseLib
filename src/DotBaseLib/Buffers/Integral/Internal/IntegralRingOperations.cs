using System.Diagnostics;
using System.Runtime.CompilerServices;
using DotBase.Integral;
using DotBase.Integral.Internal;

namespace DotBase.Buffers.Integral.Internal;


/// <summary>
/// IntegralSpan convert-on-transfer only. Typed scalar/bulk R/W lives on the ring types.
/// </summary>
internal static class IntegralRingOperationsLE
{
    private const int ScratchByteCount = 512;

    internal static unsafe int Read(
        ref RingBufferStorage storage,
        in IntegralSpan destination)
    {
        int requestedByteCount = ValidateSpan(
            ref storage,
            destination,
            nameof(destination));

        if (!storage.IsOpen || requestedByteCount == 0)
        {
            return 0;
        }

        int valueByteCount = destination.CountOf.ValueByteCount;
        int valueCount = Math.Min(
            requestedByteCount / valueByteCount,
            storage.Count / valueByteCount);

        return ReadValues(
            ref storage,
            destination,
            valueCount);
    }

    internal static unsafe bool TryRead(
        ref RingBufferStorage storage,
        in IntegralSpan destination)
    {
        int requiredByteCount = ValidateSpan(
            ref storage,
            destination,
            nameof(destination));

        if (!storage.IsOpen ||
            storage.Count < requiredByteCount)
        {
            return false;
        }

        int valueCount = requiredByteCount == 0
            ? 0
            : requiredByteCount / destination.CountOf.ValueByteCount;
        int readCount = ReadValues(
            ref storage,
            destination,
            valueCount);
        Debug.Assert(readCount == valueCount);
        return true;
    }

    internal static unsafe int Write(
        ref RingBufferStorage storage,
        in IntegralSpan source)
    {
        int requestedByteCount = ValidateSpan(
            ref storage,
            source,
            nameof(source));

        if (!storage.IsOpen || requestedByteCount == 0)
        {
            return 0;
        }

        int valueByteCount = source.CountOf.ValueByteCount;
        int valueCount = Math.Min(
            requestedByteCount / valueByteCount,
            storage.FreeCount / valueByteCount);

        return WriteValues(
            ref storage,
            source,
            valueCount);
    }

    internal static unsafe bool TryWrite(
        ref RingBufferStorage storage,
        in IntegralSpan source)
    {
        int requiredByteCount = ValidateSpan(
            ref storage,
            source,
            nameof(source));

        if (!storage.IsOpen ||
            storage.FreeCount < requiredByteCount)
        {
            return false;
        }

        int valueCount = requiredByteCount == 0
            ? 0
            : requiredByteCount / source.CountOf.ValueByteCount;
        int writtenCount = WriteValues(
            ref storage,
            source,
            valueCount);
        Debug.Assert(writtenCount == valueCount);
        return true;
    }

    internal static unsafe int ValidateSpan(
        ref RingBufferStorage storage,
        in IntegralSpan span,
        string parameterName)
    {
        if (!span.CountOf.IsValid() ||
            span.CountOf.ByteCount != span.Length ||
            span.CountOf.BlockCapacity != span.Format.BlockCapacity)
        {
            throw new ArgumentException(
                "The integral span has inconsistent capacity metadata.",
                parameterName);
        }

        ResolveByteOrder(span.Format.ByteOrder);

        if (span.Length == 0 &&
            span.IntegralValueType == IntegralType.NONE)
        {
            return 0;
        }

        int valueByteCount = span.IntegralValueType.Size();
        if (span.CountOf.ValueByteCount != valueByteCount ||
            span.Format.BlockCapacity <= 0 ||
            span.Length % valueByteCount != 0 ||
            span.Offset % valueByteCount != 0 ||
            (span.Length > 0 && span.DataPtr is null))
        {
            throw new ArgumentException(
                "The integral span is not a complete scalar-value descriptor.",
                parameterName);
        }

        if (span.Length > int.MaxValue ||
            (storage.IsOpen && span.Length > storage.Capacity))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                span.Length,
                "The requested byte size exceeds the ring capacity.");
        }

        return (int)span.Length;
    }

    private static unsafe int ReadValues(
        ref RingBufferStorage storage,
        in IntegralSpan destination,
        int valueCount)
    {
        if (valueCount == 0)
        {
            return 0;
        }

        int byteCount = checked(
            valueCount * destination.CountOf.ValueByteCount);
        int bytesRead = storage.Read(
            destination.DataPtr,
            byteCount);
        Debug.Assert(bytesRead == byteCount);

        if (ResolveByteOrder(destination.Format.ByteOrder) != ByteOrder.LittleEndian)
        {
            IntegralSpan encodedValues = new(
                destination.DataPtr,
                0,
                byteCount,
                new IntegralFormat(
                    destination.IntegralValueType,
                    1,
                    ByteOrder.LittleEndian));
            IntegralSpan decodedValues = new(
                destination.DataPtr,
                0,
                byteCount,
                new IntegralFormat(
                    destination.IntegralValueType,
                    1,
                    destination.Format.ByteOrder));

            IntegralMemory.Move(
                encodedValues,
                decodedValues,
                valueCount);
        }

        return valueCount;
    }

    private static unsafe int WriteValues(
        ref RingBufferStorage storage,
        in IntegralSpan source,
        int valueCount)
    {
        if (valueCount == 0)
        {
            return 0;
        }

        int byteCount = checked(
            valueCount * source.CountOf.ValueByteCount);

        if (ResolveByteOrder(source.Format.ByteOrder) == ByteOrder.LittleEndian)
        {
            int bytesWritten = storage.Write(
                source.DataPtr,
                byteCount);
            Debug.Assert(bytesWritten == byteCount);
            return valueCount;
        }

        // Source is not LE wire — reverse each lane into LE ring stream order.
        return WriteEndianFlip(
            ref storage,
            source.DataPtr,
            valueCount,
            source.CountOf.ValueByteCount);
    }

    /// <summary>
    /// Wire endian flip only: reverse lanes into ring stream.
    /// Storage WriteLE* reverses external bytes into the stream (both LE/BE rings).
    /// </summary>
    private static unsafe int WriteEndianFlip(
        ref RingBufferStorage storage,
        byte* source,
        int valueCount,
        int elementSize)
    {
        if (elementSize <= 1)
        {
            int byteCount = checked(valueCount * elementSize);
            int written = storage.Write(source, byteCount);
            Debug.Assert(written == byteCount);
            return valueCount;
        }

        // Single value: reverse-write directly into the ring.
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

        int sourcePosition = 0;
        while (sourcePosition < valueCount)
        {
            int chunkValueCount = Math.Min(
                scratchValueCount,
                valueCount - sourcePosition);
            byte* src = source + sourcePosition * elementSize;
            IntegralWire.ReverseCopyLanes(
                src,
                scratch,
                chunkValueCount,
                elementSize);

            int chunkByteCount = checked(chunkValueCount * elementSize);
            int bytesWritten = storage.Write(scratch, chunkByteCount);
            Debug.Assert(bytesWritten == chunkByteCount);
            sourcePosition += chunkValueCount;
        }

        return valueCount;
    }

    private static ByteOrder ResolveByteOrder(ByteOrder byteOrder)
    {
        return byteOrder switch
        {
            ByteOrder.Native => BitConverter.IsLittleEndian
                ? ByteOrder.LittleEndian
                : ByteOrder.BigEndian,
            ByteOrder.LittleEndian => ByteOrder.LittleEndian,
            ByteOrder.BigEndian => ByteOrder.BigEndian,
            _ => throw new ArgumentOutOfRangeException(nameof(byteOrder)),
        };
    }
}


internal static class IntegralRingOperationsBE
{
    private const int ScratchByteCount = 512;

    internal static unsafe int Read(
        ref RingBufferStorage storage,
        in IntegralSpan destination)
    {
        int requestedByteCount = ValidateSpan(
            ref storage,
            destination,
            nameof(destination));

        if (!storage.IsOpen || requestedByteCount == 0)
        {
            return 0;
        }

        int valueByteCount = destination.CountOf.ValueByteCount;
        int valueCount = Math.Min(
            requestedByteCount / valueByteCount,
            storage.Count / valueByteCount);

        return ReadValues(
            ref storage,
            destination,
            valueCount);
    }

    internal static unsafe bool TryRead(
        ref RingBufferStorage storage,
        in IntegralSpan destination)
    {
        int requiredByteCount = ValidateSpan(
            ref storage,
            destination,
            nameof(destination));

        if (!storage.IsOpen ||
            storage.Count < requiredByteCount)
        {
            return false;
        }

        int valueCount = requiredByteCount == 0
            ? 0
            : requiredByteCount / destination.CountOf.ValueByteCount;
        int readCount = ReadValues(
            ref storage,
            destination,
            valueCount);
        Debug.Assert(readCount == valueCount);
        return true;
    }

    internal static unsafe int Write(
        ref RingBufferStorage storage,
        in IntegralSpan source)
    {
        int requestedByteCount = ValidateSpan(
            ref storage,
            source,
            nameof(source));

        if (!storage.IsOpen || requestedByteCount == 0)
        {
            return 0;
        }

        int valueByteCount = source.CountOf.ValueByteCount;
        int valueCount = Math.Min(
            requestedByteCount / valueByteCount,
            storage.FreeCount / valueByteCount);

        return WriteValues(
            ref storage,
            source,
            valueCount);
    }

    internal static unsafe bool TryWrite(
        ref RingBufferStorage storage,
        in IntegralSpan source)
    {
        int requiredByteCount = ValidateSpan(
            ref storage,
            source,
            nameof(source));

        if (!storage.IsOpen ||
            storage.FreeCount < requiredByteCount)
        {
            return false;
        }

        int valueCount = requiredByteCount == 0
            ? 0
            : requiredByteCount / source.CountOf.ValueByteCount;
        int writtenCount = WriteValues(
            ref storage,
            source,
            valueCount);
        Debug.Assert(writtenCount == valueCount);
        return true;
    }

    internal static unsafe int ValidateSpan(
        ref RingBufferStorage storage,
        in IntegralSpan span,
        string parameterName)
    {
        if (!span.CountOf.IsValid() ||
            span.CountOf.ByteCount != span.Length ||
            span.CountOf.BlockCapacity != span.Format.BlockCapacity)
        {
            throw new ArgumentException(
                "The integral span has inconsistent capacity metadata.",
                parameterName);
        }

        ResolveByteOrder(span.Format.ByteOrder);

        if (span.Length == 0 &&
            span.IntegralValueType == IntegralType.NONE)
        {
            return 0;
        }

        int valueByteCount = span.IntegralValueType.Size();
        if (span.CountOf.ValueByteCount != valueByteCount ||
            span.Format.BlockCapacity <= 0 ||
            span.Length % valueByteCount != 0 ||
            span.Offset % valueByteCount != 0 ||
            (span.Length > 0 && span.DataPtr is null))
        {
            throw new ArgumentException(
                "The integral span is not a complete scalar-value descriptor.",
                parameterName);
        }

        if (span.Length > int.MaxValue ||
            (storage.IsOpen && span.Length > storage.Capacity))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                span.Length,
                "The requested byte size exceeds the ring capacity.");
        }

        return (int)span.Length;
    }

    private static unsafe int ReadValues(
        ref RingBufferStorage storage,
        in IntegralSpan destination,
        int valueCount)
    {
        if (valueCount == 0)
        {
            return 0;
        }

        int byteCount = checked(
            valueCount * destination.CountOf.ValueByteCount);
        int bytesRead = storage.Read(
            destination.DataPtr,
            byteCount);
        Debug.Assert(bytesRead == byteCount);

        if (ResolveByteOrder(destination.Format.ByteOrder) != ByteOrder.BigEndian)
        {
            IntegralSpan encodedValues = new(
                destination.DataPtr,
                0,
                byteCount,
                new IntegralFormat(
                    destination.IntegralValueType,
                    1,
                    ByteOrder.BigEndian));
            IntegralSpan decodedValues = new(
                destination.DataPtr,
                0,
                byteCount,
                new IntegralFormat(
                    destination.IntegralValueType,
                    1,
                    destination.Format.ByteOrder));

            IntegralMemory.Move(
                encodedValues,
                decodedValues,
                valueCount);
        }

        return valueCount;
    }

    private static unsafe int WriteValues(
        ref RingBufferStorage storage,
        in IntegralSpan source,
        int valueCount)
    {
        if (valueCount == 0)
        {
            return 0;
        }

        int byteCount = checked(
            valueCount * source.CountOf.ValueByteCount);

        if (ResolveByteOrder(source.Format.ByteOrder) == ByteOrder.BigEndian)
        {
            int bytesWritten = storage.Write(
                source.DataPtr,
                byteCount);
            Debug.Assert(bytesWritten == byteCount);
            return valueCount;
        }

        // Source is not BE wire — reverse each lane into BE ring stream order.
        return WriteEndianFlip(
            ref storage,
            source.DataPtr,
            valueCount,
            source.CountOf.ValueByteCount);
    }

    /// <summary>
    /// Wire endian flip only: reverse lanes into ring stream via WriteLE* / scratch.
    /// </summary>
    private static unsafe int WriteEndianFlip(
        ref RingBufferStorage storage,
        byte* source,
        int valueCount,
        int elementSize)
    {
        if (elementSize <= 1)
        {
            int byteCount = checked(valueCount * elementSize);
            int written = storage.Write(source, byteCount);
            Debug.Assert(written == byteCount);
            return valueCount;
        }

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

        int sourcePosition = 0;
        while (sourcePosition < valueCount)
        {
            int chunkValueCount = Math.Min(
                scratchValueCount,
                valueCount - sourcePosition);
            byte* src = source + sourcePosition * elementSize;
            IntegralWire.ReverseCopyLanes(
                src,
                scratch,
                chunkValueCount,
                elementSize);

            int chunkByteCount = checked(chunkValueCount * elementSize);
            int bytesWritten = storage.Write(scratch, chunkByteCount);
            Debug.Assert(bytesWritten == chunkByteCount);
            sourcePosition += chunkValueCount;
        }

        return valueCount;
    }

    private static ByteOrder ResolveByteOrder(ByteOrder byteOrder)
    {
        return byteOrder switch
        {
            ByteOrder.Native => BitConverter.IsLittleEndian
                ? ByteOrder.LittleEndian
                : ByteOrder.BigEndian,
            ByteOrder.LittleEndian => ByteOrder.LittleEndian,
            ByteOrder.BigEndian => ByteOrder.BigEndian,
            _ => throw new ArgumentOutOfRangeException(nameof(byteOrder)),
        };
    }
}
