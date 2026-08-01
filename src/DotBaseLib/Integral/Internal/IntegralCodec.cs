using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DotBase.Integral.Internal;


/// <summary>
/// LE-wire scalar codec. Aligned pointer path: same-endian is a single load/store;
/// opposite endian size-switches into <see cref="IntegralPrimitives"/> Swap*.
/// <see cref="RequiresReversal"/> is also used by ring bulk paths.
/// </summary>
internal static class IntegralCodecLE<T>
    where T : unmanaged
{
    internal static bool RequiresReversal =>
        !BitConverter.IsLittleEndian && Unsafe.SizeOf<T>() > 1;

    internal static unsafe T Read(byte* source)
    {
        if (!RequiresReversal)
        {
            return *(T*)source;
        }

        T host = default;
        byte* hostPtr = (byte*)&host;
        switch (Unsafe.SizeOf<T>())
        {
            case 2:
                IntegralPrimitives.Swap2(hostPtr, source);
                break;
            case 4:
                IntegralPrimitives.Swap4(hostPtr, source);
                break;
            case 8:
                IntegralPrimitives.Swap8(hostPtr, source);
                break;
            default:
                throw new NotSupportedException(
                    $"Scalar size {Unsafe.SizeOf<T>()} is not supported.");
        }

        return host;
    }

    internal static unsafe void Write(byte* destination, T value)
    {
        if (!RequiresReversal)
        {
            *(T*)destination = value;
            return;
        }

        byte* hostPtr = (byte*)&value;
        switch (Unsafe.SizeOf<T>())
        {
            case 2:
                IntegralPrimitives.Swap2(destination, hostPtr);
                return;
            case 4:
                IntegralPrimitives.Swap4(destination, hostPtr);
                return;
            case 8:
                IntegralPrimitives.Swap8(destination, hostPtr);
                return;
            default:
                throw new NotSupportedException(
                    $"Scalar size {Unsafe.SizeOf<T>()} is not supported.");
        }
    }

    internal static unsafe T Read(ReadOnlySpan<byte> source)
    {
        Debug.Assert(source.Length >= Unsafe.SizeOf<T>());
        fixed (byte* sourcePtr = source)
        {
            return Read(sourcePtr);
        }
    }

    internal static unsafe void Write(Span<byte> destination, T value)
    {
        Debug.Assert(destination.Length >= Unsafe.SizeOf<T>());
        fixed (byte* destinationPtr = destination)
        {
            Write(destinationPtr, value);
        }
    }

    internal static void ReverseEndianness(
        ReadOnlySpan<T> source,
        Span<T> destination)
    {
        IntegralEndianness.ReverseSpan(source, destination);
    }
}


/// <summary>
/// BE-wire scalar codec. Same aligned pointer discipline as LE.
/// </summary>
internal static class IntegralCodecBE<T>
    where T : unmanaged
{
    internal static bool RequiresReversal =>
        BitConverter.IsLittleEndian && Unsafe.SizeOf<T>() > 1;

    internal static unsafe T Read(byte* source)
    {
        if (!RequiresReversal)
        {
            return *(T*)source;
        }

        T host = default;
        byte* hostPtr = (byte*)&host;
        switch (Unsafe.SizeOf<T>())
        {
            case 2:
                IntegralPrimitives.Swap2(hostPtr, source);
                break;
            case 4:
                IntegralPrimitives.Swap4(hostPtr, source);
                break;
            case 8:
                IntegralPrimitives.Swap8(hostPtr, source);
                break;
            default:
                throw new NotSupportedException(
                    $"Scalar size {Unsafe.SizeOf<T>()} is not supported.");
        }

        return host;
    }

    internal static unsafe void Write(byte* destination, T value)
    {
        if (!RequiresReversal)
        {
            *(T*)destination = value;
            return;
        }

        byte* hostPtr = (byte*)&value;
        switch (Unsafe.SizeOf<T>())
        {
            case 2:
                IntegralPrimitives.Swap2(destination, hostPtr);
                return;
            case 4:
                IntegralPrimitives.Swap4(destination, hostPtr);
                return;
            case 8:
                IntegralPrimitives.Swap8(destination, hostPtr);
                return;
            default:
                throw new NotSupportedException(
                    $"Scalar size {Unsafe.SizeOf<T>()} is not supported.");
        }
    }

    internal static unsafe T Read(ReadOnlySpan<byte> source)
    {
        Debug.Assert(source.Length >= Unsafe.SizeOf<T>());
        fixed (byte* sourcePtr = source)
        {
            return Read(sourcePtr);
        }
    }

    internal static unsafe void Write(Span<byte> destination, T value)
    {
        Debug.Assert(destination.Length >= Unsafe.SizeOf<T>());
        fixed (byte* destinationPtr = destination)
        {
            Write(destinationPtr, value);
        }
    }

    internal static void ReverseEndianness(
        ReadOnlySpan<T> source,
        Span<T> destination)
    {
        IntegralEndianness.ReverseSpan(source, destination);
    }
}


internal static class IntegralEndianness
{
    internal static void ReverseSpan<T>(
        ReadOnlySpan<T> source,
        Span<T> destination)
        where T : unmanaged
    {
        switch (Unsafe.SizeOf<T>())
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

    internal static T ReverseValue<T>(T value)
        where T : unmanaged
    {
        return Unsafe.SizeOf<T>() switch
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
                $"Cannot reverse a scalar with size {Unsafe.SizeOf<T>()}."),
        };
    }
}
