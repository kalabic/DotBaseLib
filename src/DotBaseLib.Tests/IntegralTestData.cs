using DotBase.Buffers;
using DotBase.Integral;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DotBaseLib.Tests;


internal static unsafe class IntegralTestData
{
    internal static readonly IntegralType[] Types =
    [
        IntegralType.UInt8,
        IntegralType.Int8,
        IntegralType.UInt16,
        IntegralType.Int16,
        IntegralType.UInt32,
        IntegralType.Int32,
        IntegralType.UInt64,
        IntegralType.Int64,
        IntegralType.Float,
        IntegralType.Double,
    ];

    internal static readonly ByteOrder[] ByteOrders =
    [
        ByteOrder.Native,
        ByteOrder.LittleEndian,
        ByteOrder.BigEndian,
    ];

    internal static int SizeOf(IntegralType type)
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
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };
    }

    internal static IntegralSpan CreateSpan(
        byte* pointer,
        long valueCount,
        IntegralType type,
        ByteOrder byteOrder = ByteOrder.Native,
        int blockCapacity = 1,
        long byteOffset = 0)
    {
        return new IntegralSpan(
            pointer,
            byteOffset,
            checked(valueCount * SizeOf(type)),
            new IntegralFormat(
                type,
                blockCapacity,
                byteOrder));
    }

    /// <summary>
    /// Allocate storage natural-aligned for scalar wire ops (default 8-byte).
    /// Caller must <see cref="AlignedFree"/>.
    /// </summary>
    internal static byte* AlignedAlloc(int byteCount, int alignment = 8)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(byteCount, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(alignment, 1);
        nuint size = byteCount == 0 ? 1u : (nuint)byteCount;
        return (byte*)NativeMemory.AlignedAlloc(size, (nuint)alignment);
    }

    internal static void AlignedFree(byte* pointer)
    {
        if (pointer is not null)
        {
            NativeMemory.AlignedFree(pointer);
        }
    }

    internal static void SetNumber(
        in IntegralSpan span,
        long index,
        double value)
    {
        switch (span.IntegralValueType)
        {
            case IntegralType.UInt8:
                span.SetAtIndex(index, (byte)value);
                return;
            case IntegralType.Int8:
                span.SetAtIndex(index, (sbyte)value);
                return;
            case IntegralType.UInt16:
                span.SetAtIndex(index, (ushort)value);
                return;
            case IntegralType.Int16:
                span.SetAtIndex(index, (short)value);
                return;
            case IntegralType.UInt32:
                span.SetAtIndex(index, (uint)value);
                return;
            case IntegralType.Int32:
                span.SetAtIndex(index, (int)value);
                return;
            case IntegralType.UInt64:
                span.SetAtIndex(index, (ulong)value);
                return;
            case IntegralType.Int64:
                span.SetAtIndex(index, (long)value);
                return;
            case IntegralType.Float:
                span.SetAtIndex(index, (float)value);
                return;
            case IntegralType.Double:
                span.SetAtIndex(index, value);
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(span));
        }
    }

    internal static double GetNumber(
        in IntegralSpan span,
        long index)
    {
        return span.IntegralValueType switch
        {
            IntegralType.UInt8 => span.AtIndex<byte>(index),
            IntegralType.Int8 => span.AtIndex<sbyte>(index),
            IntegralType.UInt16 => span.AtIndex<ushort>(index),
            IntegralType.Int16 => span.AtIndex<short>(index),
            IntegralType.UInt32 => span.AtIndex<uint>(index),
            IntegralType.Int32 => span.AtIndex<int>(index),
            IntegralType.UInt64 => span.AtIndex<ulong>(index),
            IntegralType.Int64 => span.AtIndex<long>(index),
            IntegralType.Float => span.AtIndex<float>(index),
            IntegralType.Double => span.AtIndex<double>(index),
            _ => throw new ArgumentOutOfRangeException(nameof(span)),
        };
    }

    internal static byte[] NativeBytes<T>(T value)
        where T : unmanaged
    {
        byte[] bytes = new byte[Unsafe.SizeOf<T>()];
        MemoryMarshal.Write(bytes, in value);
        return bytes;
    }

    internal static byte[] EncodedBytes<T>(
        T value,
        ByteOrder byteOrder)
        where T : unmanaged
    {
        byte[] bytes = NativeBytes(value);
        ByteOrder resolved = ResolveByteOrder(byteOrder);

        if (bytes.Length > 1 &&
            ((resolved == ByteOrder.LittleEndian) !=
             BitConverter.IsLittleEndian))
        {
            Array.Reverse(bytes);
        }

        return bytes;
    }

    /// <summary>
    /// Writes <paramref name="value"/> into <paramref name="dest"/> at
    /// <paramref name="index"/> using explicit wire endianness (not a host store).
    /// </summary>
    internal static void WriteEncoded<T>(
        byte* dest,
        long index,
        T value,
        ByteOrder byteOrder)
        where T : unmanaged
    {
        byte[] encoded = EncodedBytes(value, byteOrder);
        long offset = checked(index * encoded.Length);
        for (int i = 0; i < encoded.Length; ++i)
        {
            dest[offset + i] = encoded[i];
        }
    }

    /// <summary>
    /// Reads a host <typeparamref name="T"/> from wire bytes at
    /// <paramref name="index"/> under <paramref name="byteOrder"/>.
    /// </summary>
    internal static T ReadEncoded<T>(
        byte* src,
        long index,
        ByteOrder byteOrder)
        where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        long offset = checked(index * size);
        byte[] bytes = new byte[size];
        for (int i = 0; i < size; ++i)
        {
            bytes[i] = src[offset + i];
        }

        ByteOrder resolved = ResolveByteOrder(byteOrder);
        if (size > 1 &&
            ((resolved == ByteOrder.LittleEndian) !=
             BitConverter.IsLittleEndian))
        {
            Array.Reverse(bytes);
        }

        return MemoryMarshal.Read<T>(bytes);
    }

    /// <summary>
    /// Asserts that wire bytes at <paramref name="index"/> match
    /// <paramref name="expected"/> encoded under <paramref name="byteOrder"/>.
    /// </summary>
    internal static void AssertEncodedEqual<T>(
        T expected,
        byte* actual,
        long index,
        ByteOrder byteOrder)
        where T : unmanaged
    {
        byte[] expectedBytes = EncodedBytes(expected, byteOrder);
        int size = expectedBytes.Length;
        long offset = checked(index * size);
        Assert.Equal(
            expectedBytes,
            new ReadOnlySpan<byte>(actual + offset, size).ToArray());
    }

    internal static ByteOrder ResolveByteOrder(ByteOrder byteOrder)
    {
        if (byteOrder == ByteOrder.Native)
        {
            return BitConverter.IsLittleEndian
                ? ByteOrder.LittleEndian
                : ByteOrder.BigEndian;
        }

        return byteOrder;
    }

}
