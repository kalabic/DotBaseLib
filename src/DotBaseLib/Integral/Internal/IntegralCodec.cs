using DotBase.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DotBase.Integral.Internal;


internal static class IntegralCodec<T, TEndian>
    where T : unmanaged
    where TEndian : struct, IEndianCodec
{
    internal static readonly int Size = Unsafe.SizeOf<T>();

    private static readonly bool IsSupported =
        typeof(T) == typeof(sbyte) ||
        typeof(T) == typeof(byte) ||
        typeof(T) == typeof(short) ||
        typeof(T) == typeof(ushort) ||
        typeof(T) == typeof(int) ||
        typeof(T) == typeof(uint) ||
        typeof(T) == typeof(long) ||
        typeof(T) == typeof(ulong) ||
        typeof(T) == typeof(nint) ||
        typeof(T) == typeof(nuint) ||
        typeof(T) == typeof(char) ||
        typeof(T) == typeof(float) ||
        typeof(T) == typeof(double);

    internal static bool RequiresReversal =>
        !TEndian.ByteOrder.IsNativeCompatible() &&
        Size > 1;

    internal static void Validate()
    {
        if (!IsSupported)
        {
            throw new NotSupportedException(
                $"Type '{typeof(T)}' is not a supported scalar type.");
        }
    }

    internal static unsafe T Read(byte* source)
    {
        Validate();

        T value = Unsafe.ReadUnaligned<T>(source);
        if (!RequiresReversal)
        {
            return value;
        }

        return ReverseEndianness(value);
    }

    internal static unsafe void Write(byte* destination, T value)
    {
        Validate();

        if (RequiresReversal)
        {
            value = ReverseEndianness(value);
        }

        Unsafe.WriteUnaligned(destination, value);
    }

    internal static unsafe T Read(ReadOnlySpan<byte> source)
    {
        if (source.Length < Size)
        {
            throw new ArgumentException(
                $"Source must contain at least {Size} bytes.",
                nameof(source));
        }

        fixed (byte* sourcePtr = source)
        {
            return Read(sourcePtr);
        }
    }

    internal static unsafe void Write(Span<byte> destination, T value)
    {
        if (destination.Length < Size)
        {
            throw new ArgumentException(
                $"Destination must contain at least {Size} bytes.",
                nameof(destination));
        }

        fixed (byte* destinationPtr = destination)
        {
            Write(destinationPtr, value);
        }
    }

    internal static void ReverseEndianness(
        ReadOnlySpan<T> source,
        Span<T> destination)
    {
        Validate();

        switch (Size)
        {
            case 1:
                source.CopyTo(destination);
                return;

            case 2:
                BinaryPrimitives.ReverseEndianness(
                    MemoryMarshal.Cast<T, ushort>(source),
                    MemoryMarshal.Cast<T, ushort>(destination));
                return;

            case 4:
                BinaryPrimitives.ReverseEndianness(
                    MemoryMarshal.Cast<T, uint>(source),
                    MemoryMarshal.Cast<T, uint>(destination));
                return;

            case 8:
                BinaryPrimitives.ReverseEndianness(
                    MemoryMarshal.Cast<T, ulong>(source),
                    MemoryMarshal.Cast<T, ulong>(destination));
                return;

            default:
                throw new UnreachableException();
        }
    }

    private static T ReverseEndianness(T value)
    {
        return Size switch
        {
            2 => Unsafe.BitCast<ushort, T>(
                BinaryPrimitives.ReverseEndianness(
                    Unsafe.BitCast<T, ushort>(value))),
            4 => Unsafe.BitCast<uint, T>(
                BinaryPrimitives.ReverseEndianness(
                    Unsafe.BitCast<T, uint>(value))),
            8 => Unsafe.BitCast<ulong, T>(
                BinaryPrimitives.ReverseEndianness(
                    Unsafe.BitCast<T, ulong>(value))),
            _ => throw new InvalidOperationException(
                $"Cannot reverse a scalar with size {Size}."),
        };
    }
}
