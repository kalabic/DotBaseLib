using DotBase.Integral.Internal;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace DotBase.Integral;


/// <summary>
/// Aligned 2/4/8-byte pointer ops for integral primitives.
/// Non-generic - call sites size-switch and land here.
/// <para>
/// <see cref="IntegralSpan"/> keeps value addresses aligned to the scalar size
/// (offset multiples of <c>ValueByteCount</c> from an aligned base). Compatible
/// host/primitive endian is a single aligned word load/store; opposite endian is
/// <see cref="Swap2"/> / <see cref="Swap4"/> / <see cref="Swap8"/>.
/// </para>
/// </summary>
internal static unsafe class IntegralPrimitives
{
    // ---- Aligned copy (same byte layout) ----

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Copy2(byte* destination, byte* source)
    {
        *(ushort*)destination = *(ushort*)source;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Copy4(byte* destination, byte* source)
    {
        *(uint*)destination = *(uint*)source;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Copy8(byte* destination, byte* source)
    {
        *(ulong*)destination = *(ulong*)source;
    }

    // ---- Aligned endian swap (primitive ↔ host when layouts differ) ----

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Swap2(byte* destination, byte* source)
    {
        *(ushort*)destination = BinaryPrimitives.ReverseEndianness(*(ushort*)source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Swap4(byte* destination, byte* source)
    {
        *(uint*)destination = BinaryPrimitives.ReverseEndianness(*(uint*)source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Swap8(byte* destination, byte* source)
    {
        *(ulong*)destination = BinaryPrimitives.ReverseEndianness(*(ulong*)source);
    }

    /// <summary>
    /// Reverse <paramref name="valueCount"/> lanes of size 2/4/8 from
    /// <paramref name="source"/> into non-overlapping <paramref name="destination"/>.
    /// Size 1 is a plain copy.
    /// </summary>
    internal static void ReverseCopyLanes(
        byte* source,
        byte* destination,
        long valueCount,
        int valueByteCount)
    {
        if (valueCount <= 0)
        {
            return;
        }

        switch (valueByteCount)
        {
            case 1:
                Buffer.MemoryCopy(
                    source,
                    destination,
                    (ulong)valueCount,
                    (ulong)valueCount);
                return;
            case 2:
                // Bulk byte-pair swap (Int16 PCM and UInt16); see EndianSwap16BitLanes.
                EndianSwap.Swap16BitLanes(source, destination, valueCount);
                return;
            case 4:
                for (long i = 0; i < valueCount; ++i)
                {
                    Swap4(destination + (i * 4), source + (i * 4));
                }

                return;
            case 8:
                for (long i = 0; i < valueCount; ++i)
                {
                    Swap8(destination + (i * 8), source + (i * 8));
                }

                return;
            default:
                throw new NotSupportedException(
                    $"Value size {valueByteCount} is not supported.");
        }
    }

    /// <summary>
    /// In-place reverse of <paramref name="valueCount"/> lanes at <paramref name="data"/>.
    /// </summary>
    internal static void ReverseLanesInPlace(
        byte* data,
        long valueCount,
        int valueByteCount)
    {
        if (valueCount <= 0 || valueByteCount <= 1)
        {
            return;
        }

        switch (valueByteCount)
        {
            case 2:
                // Bulk byte-pair swap in place (Int16 PCM and UInt16).
                EndianSwap.Swap16BitLanes(data, data, valueCount);
                return;
            case 4:
                for (long i = 0; i < valueCount; ++i)
                {
                    byte* p = data + (i * 4);
                    Swap4(p, p);
                }

                return;
            case 8:
                for (long i = 0; i < valueCount; ++i)
                {
                    byte* p = data + (i * 8);
                    Swap8(p, p);
                }

                return;
            default:
                throw new NotSupportedException(
                    $"Value size {valueByteCount} is not supported.");
        }
    }
}
