//
// Bulk 16-bit lane endian swap (byte-pair swap within each 16-bit cell).
// Applies equally to signed Int16 (audio PCM) and unsigned UInt16: pure
// adjacent-byte swap with no sign arithmetic.
//
// Algorithm adapted from Stack Overflow answer by Vozzie:
//   https://stackoverflow.com/a/49226621
// Modifications by community (see post Timeline); adapted for DotBase
// raw pointer spans (no CLR array-header length tricks).
// Retrieved: 2026-08-07
// License: CC BY-SA 3.0
//

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotBase.Integral.Internal;


internal static unsafe class EndianSwap
{
    /// <summary>
    /// Swaps the two bytes of every 16-bit lane from <paramref name="source"/>
    /// into <paramref name="destination"/>.
    /// <para>
    /// Covers both <see cref="IntegralType.Int16"/> and <see cref="IntegralType.UInt16"/>:
    /// endian conversion is a byte-pair swap and does not depend on signed interpretation
    /// of the bits.
    /// </para>
    /// <para>
    /// Regions must be non-overlapping, or <paramref name="source"/> and <paramref name="destination"/>
    /// must be identical (in-place). Bases are expected to be natural-aligned for 16-bit scalar wire ops.
    /// </para>
    /// </summary>
    internal static void Swap16BitLanes(byte* source, byte* destination, long valueCount)
    {
        if (valueCount <= 0)
        {
            return;
        }

        long byteCount = checked(valueCount * 2);
        Debug.Assert(
            source == destination ||
            destination + byteCount <= source ||
            source + byteCount <= destination,
            "EndianSwap16BitLanes requires non-overlapping or identical pointers.");

        // Process four 16-bit lanes per ulong, then two per uint, then one ushort.
        // Mask swaps adjacent bytes: [b0 b1 ...] → [b1 b0 ...] (not a full 64-bit reverse).
        long remaining = byteCount;
        byte* src = source;
        byte* dst = destination;

        while (remaining >= 8)
        {
            ulong v = *(ulong*)src;
            v = ((v >> 8) & 0x00FF00FF00FF00FFUL)
              | ((v << 8) & 0xFF00FF00FF00FF00UL);
            *(ulong*)dst = v;
            src += 8;
            dst += 8;
            remaining -= 8;
        }

        if (remaining >= 4)
        {
            uint v = *(uint*)src;
            v = ((v >> 8) & 0x00FF00FFU)
              | ((v << 8) & 0xFF00FF00U);
            *(uint*)dst = v;
            src += 4;
            dst += 4;
            remaining -= 4;
        }

        if (remaining >= 2)
        {
            ushort v = *(ushort*)src;
            v = (ushort)((v >> 8) | (v << 8));
            *(ushort*)dst = v;
        }
    }

    internal static void Swap16BitLanesUnaligned(byte* source, byte* destination, long valueCount)
    {
        if (valueCount <= 0)
        {
            return;
        }

        long byteCount = checked(valueCount * 2);
        Debug.Assert(
            source == destination ||
            destination + byteCount <= source ||
            source + byteCount <= destination,
            "EndianSwap16BitLanes requires non-overlapping or identical pointers.");

        // Process four 16-bit lanes per ulong, then two per uint, then one ushort.
        // Mask swaps adjacent bytes: [b0 b1 ...] → [b1 b0 ...] (not a full 64-bit reverse).
        // Unaligned read/write: bases need not be 4- or 8-byte aligned.
        long remaining = byteCount;
        byte* src = source;
        byte* dst = destination;

        while (remaining >= 8)
        {
            ulong v = Unsafe.ReadUnaligned<ulong>(src);
            v = ((v >> 8) & 0x00FF00FF00FF00FFUL)
              | ((v << 8) & 0xFF00FF00FF00FF00UL);
            Unsafe.WriteUnaligned(dst, v);
            src += 8;
            dst += 8;
            remaining -= 8;
        }

        if (remaining >= 4)
        {
            uint v = Unsafe.ReadUnaligned<uint>(src);
            v = ((v >> 8) & 0x00FF00FFU)
              | ((v << 8) & 0xFF00FF00U);
            Unsafe.WriteUnaligned(dst, v);
            src += 4;
            dst += 4;
            remaining -= 4;
        }

        if (remaining >= 2)
        {
            ushort v = Unsafe.ReadUnaligned<ushort>(src);
            v = (ushort)((v >> 8) | (v << 8));
            Unsafe.WriteUnaligned(dst, v);
        }
    }
}
