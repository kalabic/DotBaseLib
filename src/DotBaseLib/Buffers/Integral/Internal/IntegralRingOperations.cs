using DotBase.Integral;
using DotBase.Integral.Internal;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DotBase.Buffers.Integral.Internal;


internal static class IntegralRingOperations<TEndian>
    where TEndian : struct, IEndianCodec
{
    private const int ScratchByteCount = 512;

    internal static ByteOrder ByteOrder =>
        TEndian.ByteOrder switch
        {
            ByteOrder.Native => ByteOrder.Native,
            ByteOrder.LittleEndian => ByteOrder.LittleEndian,
            ByteOrder.BigEndian => ByteOrder.BigEndian,
            _ => throw new ArgumentOutOfRangeException(nameof(TEndian.ByteOrder)),
        };

    internal static int CapacityOf<T>(ref RingBufferStorage storage)
        where T : unmanaged
    {
        IntegralCodec<T, TEndian>.Validate();
        return storage.Capacity / IntegralCodec<T, TEndian>.Size;
    }

    internal static int CountOf<T>(ref RingBufferStorage storage)
        where T : unmanaged
    {
        IntegralCodec<T, TEndian>.Validate();
        return storage.Count / IntegralCodec<T, TEndian>.Size;
    }

    internal static void AdvanceBy<T>(
        ref RingBufferStorage storage,
        int count)
        where T : unmanaged
    {
        IntegralCodec<T, TEndian>.Validate();
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        int byteCount = checked(count * IntegralCodec<T, TEndian>.Size);
        storage.Advance(byteCount);
    }

    internal static unsafe bool TryReadScalar<T>(
        ref RingBufferStorage storage,
        out T value)
        where T : unmanaged
    {
        IntegralType valueType = GetDeclaredIntegralType<T>();
        if (valueType != IntegralType.NONE)
        {
            int declaredByteCount = valueType.Size();
            if (!storage.IsOpen || storage.Count < declaredByteCount)
            {
                value = default;
                return false;
            }

            T destination = default;
            IntegralSpan destinationSpan = CreateNativeSpan(
                &destination,
                1,
                valueType);
            bool completed = TryRead(
                ref storage,
                destinationSpan);
            Debug.Assert(completed);

            value = destination;
            return true;
        }

        IntegralCodec<T, TEndian>.Validate();

        int byteCount = IntegralCodec<T, TEndian>.Size;
        if (!storage.IsOpen || storage.Count < byteCount)
        {
            value = default;
            return false;
        }

        Span<byte> bytes = stackalloc byte[byteCount];
        int bytesRead = storage.Read(bytes);
        Debug.Assert(bytesRead == byteCount);

        value = IntegralCodec<T, TEndian>.Read(bytes);
        return true;
    }

    internal static unsafe bool TryWriteScalar<T>(
        ref RingBufferStorage storage,
        T value)
        where T : unmanaged
    {
        IntegralType valueType = GetDeclaredIntegralType<T>();
        if (valueType != IntegralType.NONE)
        {
            int declaredByteCount = valueType.Size();
            if (!storage.IsOpen ||
                storage.FreeCount < declaredByteCount)
            {
                return false;
            }

            IntegralSpan source = CreateNativeSpan(
                &value,
                1,
                valueType);
            return TryWrite(
                ref storage,
                source);
        }

        IntegralCodec<T, TEndian>.Validate();

        int byteCount = IntegralCodec<T, TEndian>.Size;
        if (!storage.IsOpen || storage.FreeCount < byteCount)
        {
            return false;
        }

        Span<byte> bytes = stackalloc byte[byteCount];
        IntegralCodec<T, TEndian>.Write(bytes, value);

        int bytesWritten = storage.Write(bytes);
        Debug.Assert(bytesWritten == byteCount);
        return true;
    }

    internal static unsafe int Read<T>(
        ref RingBufferStorage storage,
        Span<T> destination)
        where T : unmanaged
    {
        IntegralType valueType = GetDeclaredIntegralType<T>();
        if (valueType != IntegralType.NONE)
        {
            if (!storage.IsOpen || destination.IsEmpty)
            {
                return 0;
            }

            int valueByteCount = valueType.Size();
            int valueCount = Math.Min(
                destination.Length,
                storage.Count / valueByteCount);

            fixed (T* destinationPtr = destination)
            {
                IntegralSpan destinationSpan = CreateNativeSpan(
                    destinationPtr,
                    valueCount,
                    valueType);
                return Read(
                    ref storage,
                    destinationSpan);
            }
        }

        IntegralCodec<T, TEndian>.Validate();

        if (!storage.IsOpen || destination.IsEmpty)
        {
            return 0;
        }

        int elementSize = IntegralCodec<T, TEndian>.Size;
        int elementCount = Math.Min(destination.Length, storage.Count / elementSize);
        Span<T> transferred = destination[..elementCount];

        int bytesRead = storage.Read(MemoryMarshal.AsBytes(transferred));
        Debug.Assert(bytesRead == elementCount * elementSize);

        if (IntegralCodec<T, TEndian>.RequiresReversal)
        {
            IntegralCodec<T, TEndian>.ReverseEndianness(transferred, transferred);
        }

        return elementCount;
    }

    internal static unsafe bool TryRead<T>(
        ref RingBufferStorage storage,
        Span<T> destination)
        where T : unmanaged
    {
        IntegralType valueType = GetDeclaredIntegralType<T>();
        if (valueType != IntegralType.NONE)
        {
            int requiredByteCount = checked(
                destination.Length * valueType.Size());
            if (!storage.IsOpen ||
                storage.Count < requiredByteCount)
            {
                return false;
            }

            fixed (T* destinationPtr = destination)
            {
                IntegralSpan destinationSpan = CreateNativeSpan(
                    destinationPtr,
                    destination.Length,
                    valueType);
                return TryRead(
                    ref storage,
                    destinationSpan);
            }
        }

        IntegralCodec<T, TEndian>.Validate();

        int requiredBytes = checked(destination.Length * IntegralCodec<T, TEndian>.Size);
        if (!storage.IsOpen || storage.Count < requiredBytes)
        {
            return false;
        }

        int elementCount = Read(ref storage, destination);
        Debug.Assert(elementCount == destination.Length);
        return true;
    }

    internal static unsafe int Write<T>(
        ref RingBufferStorage storage,
        ReadOnlySpan<T> source)
        where T : unmanaged
    {
        IntegralType valueType = GetDeclaredIntegralType<T>();
        if (valueType != IntegralType.NONE)
        {
            if (!storage.IsOpen || source.IsEmpty)
            {
                return 0;
            }

            int valueByteCount = valueType.Size();
            int valueCount = Math.Min(
                source.Length,
                storage.FreeCount / valueByteCount);

            fixed (T* sourcePtr = source)
            {
                IntegralSpan sourceSpan = CreateNativeSpan(
                    sourcePtr,
                    valueCount,
                    valueType);
                return Write(
                    ref storage,
                    sourceSpan);
            }
        }

        IntegralCodec<T, TEndian>.Validate();

        if (!storage.IsOpen || source.IsEmpty)
        {
            return 0;
        }

        int elementSize = IntegralCodec<T, TEndian>.Size;
        int elementCount = Math.Min(source.Length, storage.FreeCount / elementSize);
        ReadOnlySpan<T> transferred = source[..elementCount];

        if (!IntegralCodec<T, TEndian>.RequiresReversal)
        {
            int bytesWritten = storage.Write(MemoryMarshal.AsBytes(transferred));
            Debug.Assert(bytesWritten == elementCount * elementSize);
            return elementCount;
        }

        WriteReversed(ref storage, transferred);
        return elementCount;
    }

    internal static unsafe bool TryWrite<T>(
        ref RingBufferStorage storage,
        ReadOnlySpan<T> source)
        where T : unmanaged
    {
        IntegralType valueType = GetDeclaredIntegralType<T>();
        if (valueType != IntegralType.NONE)
        {
            int requiredByteCount = checked(
                source.Length * valueType.Size());
            if (!storage.IsOpen ||
                storage.FreeCount < requiredByteCount)
            {
                return false;
            }

            fixed (T* sourcePtr = source)
            {
                IntegralSpan sourceSpan = CreateNativeSpan(
                    sourcePtr,
                    source.Length,
                    valueType);
                return TryWrite(
                    ref storage,
                    sourceSpan);
            }
        }

        IntegralCodec<T, TEndian>.Validate();

        int requiredBytes = checked(source.Length * IntegralCodec<T, TEndian>.Size);
        if (!storage.IsOpen || storage.FreeCount < requiredBytes)
        {
            return false;
        }

        int elementCount = Write(ref storage, source);
        Debug.Assert(elementCount == source.Length);
        return true;
    }

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

    internal static int GetRequestedByteCount(
        ref RingBufferStorage storage,
        in IntegralSpan span,
        string parameterName)
    {
        return ValidateSpan(
            ref storage,
            span,
            parameterName);
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

        if (!ByteOrdersMatch(
            TEndian.ByteOrder,
            destination.Format.ByteOrder))
        {
            IntegralSpan encodedValues = new(
                destination.DataPtr,
                0,
                byteCount,
                new IntegralFormat(
                    destination.IntegralValueType,
                    1,
                    TEndian.ByteOrder));
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

        if (ByteOrdersMatch(
            source.Format.ByteOrder,
            TEndian.ByteOrder))
        {
            int bytesWritten = storage.Write(
                source.DataPtr,
                byteCount);
            Debug.Assert(bytesWritten == byteCount);
            return valueCount;
        }

        return DispatchConvertedWrite(
            ref storage,
            source,
            valueCount);
    }

    private static unsafe int DispatchConvertedWrite(
        ref RingBufferStorage storage,
        in IntegralSpan source,
        int valueCount)
    {
        return source.Format.ByteOrder switch
        {
            ByteOrder.Native =>
                DispatchConvertedWrite<NativeEndianCodec>(
                    ref storage,
                    source,
                    valueCount),
            ByteOrder.LittleEndian =>
                DispatchConvertedWrite<LittleEndianCodec>(
                    ref storage,
                    source,
                    valueCount),
            ByteOrder.BigEndian =>
                DispatchConvertedWrite<BigEndianCodec>(
                    ref storage,
                    source,
                    valueCount),
            _ => throw new ArgumentOutOfRangeException(
                nameof(source.Format.ByteOrder)),
        };
    }

    private static unsafe int DispatchConvertedWrite<TSourceEndian>(
        ref RingBufferStorage storage,
        in IntegralSpan source,
        int valueCount)
        where TSourceEndian : struct, IEndianCodec
    {
        return source.IntegralValueType switch
        {
            IntegralType.UInt8 =>
                WriteConverted<byte, TSourceEndian>(
                    ref storage, source.DataPtr, valueCount),
            IntegralType.Int8 =>
                WriteConverted<sbyte, TSourceEndian>(
                    ref storage, source.DataPtr, valueCount),
            IntegralType.UInt16 =>
                WriteConverted<ushort, TSourceEndian>(
                    ref storage, source.DataPtr, valueCount),
            IntegralType.Int16 =>
                WriteConverted<short, TSourceEndian>(
                    ref storage, source.DataPtr, valueCount),
            IntegralType.UInt32 =>
                WriteConverted<uint, TSourceEndian>(
                    ref storage, source.DataPtr, valueCount),
            IntegralType.Int32 =>
                WriteConverted<int, TSourceEndian>(
                    ref storage, source.DataPtr, valueCount),
            IntegralType.UInt64 =>
                WriteConverted<ulong, TSourceEndian>(
                    ref storage, source.DataPtr, valueCount),
            IntegralType.Int64 =>
                WriteConverted<long, TSourceEndian>(
                    ref storage, source.DataPtr, valueCount),
            IntegralType.Float =>
                WriteConverted<float, TSourceEndian>(
                    ref storage, source.DataPtr, valueCount),
            IntegralType.Double =>
                WriteConverted<double, TSourceEndian>(
                    ref storage, source.DataPtr, valueCount),
            _ => throw new NotSupportedException(
                $"Integral type '{source.IntegralValueType}' is not supported."),
        };
    }

    private static unsafe int WriteConverted<T, TSourceEndian>(
        ref RingBufferStorage storage,
        byte* source,
        int valueCount)
        where T : unmanaged
        where TSourceEndian : struct, IEndianCodec
    {
        IntegralCodec<T, TSourceEndian>.Validate();
        IntegralCodec<T, TEndian>.Validate();

        int valueByteCount = IntegralCodec<T, TEndian>.Size;
        int scratchValueCount = Math.Max(
            1,
            ScratchByteCount / valueByteCount);
        byte* scratch = stackalloc byte[
            scratchValueCount * valueByteCount];

        int sourcePosition = 0;
        while (sourcePosition < valueCount)
        {
            int chunkValueCount = Math.Min(
                scratchValueCount,
                valueCount - sourcePosition);

            for (int index = 0; index < chunkValueCount; ++index)
            {
                T value = IntegralCodec<T, TSourceEndian>.Read(
                    source +
                    (sourcePosition + index) * valueByteCount);
                IntegralCodec<T, TEndian>.Write(
                    scratch + index * valueByteCount,
                    value);
            }

            int chunkByteCount = checked(
                chunkValueCount * valueByteCount);
            int bytesWritten = storage.Write(
                scratch,
                chunkByteCount);
            Debug.Assert(bytesWritten == chunkByteCount);
            sourcePosition += chunkValueCount;
        }

        return valueCount;
    }

    private static unsafe int ValidateSpan(
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

        int valueByteCount = GetValueByteCount(
            span.IntegralValueType);
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

    private static int GetValueByteCount(IntegralType type)
    {
        return type switch
        {
            IntegralType.UInt8 => sizeof(byte),
            IntegralType.Int8 => sizeof(sbyte),
            IntegralType.UInt16 => sizeof(ushort),
            IntegralType.Int16 => sizeof(short),
            IntegralType.UInt32 => sizeof(uint),
            IntegralType.Int32 => sizeof(int),
            IntegralType.UInt64 => sizeof(ulong),
            IntegralType.Int64 => sizeof(long),
            IntegralType.Float => sizeof(float),
            IntegralType.Double => sizeof(double),
            _ => throw new ArgumentException(
                $"Integral type '{type}' is not supported."),
        };
    }

    private static bool ByteOrdersMatch(
        ByteOrder first,
        ByteOrder second)
    {
        return ResolveByteOrder(first) ==
               ResolveByteOrder(second);
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

    private static IntegralType GetDeclaredIntegralType<T>()
        where T : unmanaged
    {
        return IntegralType.NONE.DefaultForType<T>();
    }

    private static unsafe IntegralSpan CreateNativeSpan<T>(
        T* pointer,
        int valueCount,
        IntegralType valueType)
        where T : unmanaged
    {
        return new IntegralSpan(
            (byte*)pointer,
            0,
            checked((long)valueCount * valueType.Size()),
            new IntegralFormat(
                valueType,
                1,
                ByteOrder.Native));
    }

    private static void WriteReversed<T>(
        ref RingBufferStorage storage,
        ReadOnlySpan<T> source)
        where T : unmanaged
    {
        int elementSize = IntegralCodec<T, TEndian>.Size;
        int scratchElementCount = Math.Max(1, ScratchByteCount / elementSize);
        Span<byte> scratchBytes = stackalloc byte[scratchElementCount * elementSize];
        Span<T> scratchValues = MemoryMarshal.Cast<byte, T>(scratchBytes);

        int sourcePosition = 0;
        while (sourcePosition < source.Length)
        {
            int count = Math.Min(scratchValues.Length, source.Length - sourcePosition);
            Span<T> encodedValues = scratchValues[..count];

            IntegralCodec<T, TEndian>.ReverseEndianness(
                source.Slice(sourcePosition, count),
                encodedValues);

            int bytesWritten = storage.Write(MemoryMarshal.AsBytes(encodedValues));
            Debug.Assert(bytesWritten == count * elementSize);
            sourcePosition += count;
        }
    }
}
