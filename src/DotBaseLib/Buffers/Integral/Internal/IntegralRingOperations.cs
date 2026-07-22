using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DotBase.Buffers.Integral.Internal;


internal static class IntegralRingOperations<TEndian>
    where TEndian : struct, IEndianCodec
{
    private const int ScratchByteCount = 512;

    internal static ByteOrder ByteOrder => TEndian.ByteOrder;

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

    internal static bool TryReadScalar<T>(
        ref RingBufferStorage storage,
        out T value)
        where T : unmanaged
    {
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

    internal static bool TryWriteScalar<T>(
        ref RingBufferStorage storage,
        T value)
        where T : unmanaged
    {
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

    internal static int Read<T>(
        ref RingBufferStorage storage,
        Span<T> destination)
        where T : unmanaged
    {
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

    internal static bool TryRead<T>(
        ref RingBufferStorage storage,
        Span<T> destination)
        where T : unmanaged
    {
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

    internal static int Write<T>(
        ref RingBufferStorage storage,
        ReadOnlySpan<T> source)
        where T : unmanaged
    {
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

    internal static bool TryWrite<T>(
        ref RingBufferStorage storage,
        ReadOnlySpan<T> source)
        where T : unmanaged
    {
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
