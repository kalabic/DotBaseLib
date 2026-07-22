using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DotBase.Buffers.Integral.Internal;


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
        typeof(T) == typeof(char);

    internal static bool RequiresReversal =>
        Size > 1 &&
        ((TEndian.ByteOrder == ByteOrder.LittleEndian) != BitConverter.IsLittleEndian);

    internal static void Validate()
    {
        if (!IsSupported)
        {
            throw new NotSupportedException(
                $"Type '{typeof(T)}' is not a supported integral type.");
        }
    }

    internal static T Read(ReadOnlySpan<byte> source)
    {
        Validate();

        if (typeof(T) == typeof(sbyte))
            return From(unchecked((sbyte)source[0]));
        if (typeof(T) == typeof(byte))
            return From(source[0]);
        if (typeof(T) == typeof(short))
            return From(TEndian.ReadInt16(source));
        if (typeof(T) == typeof(ushort))
            return From(TEndian.ReadUInt16(source));
        if (typeof(T) == typeof(int))
            return From(TEndian.ReadInt32(source));
        if (typeof(T) == typeof(uint))
            return From(TEndian.ReadUInt32(source));
        if (typeof(T) == typeof(long))
            return From(TEndian.ReadInt64(source));
        if (typeof(T) == typeof(ulong))
            return From(TEndian.ReadUInt64(source));
        if (typeof(T) == typeof(nint))
            return From(TEndian.ReadIntPtr(source));
        if (typeof(T) == typeof(nuint))
            return From(TEndian.ReadUIntPtr(source));
        if (typeof(T) == typeof(char))
            return From((char)TEndian.ReadUInt16(source));

        throw new UnreachableException();
    }

    internal static void Write(Span<byte> destination, T value)
    {
        Validate();

        if (typeof(T) == typeof(sbyte))
            destination[0] = unchecked((byte)To<sbyte>(value));
        else if (typeof(T) == typeof(byte))
            destination[0] = To<byte>(value);
        else if (typeof(T) == typeof(short))
            TEndian.WriteInt16(destination, To<short>(value));
        else if (typeof(T) == typeof(ushort))
            TEndian.WriteUInt16(destination, To<ushort>(value));
        else if (typeof(T) == typeof(int))
            TEndian.WriteInt32(destination, To<int>(value));
        else if (typeof(T) == typeof(uint))
            TEndian.WriteUInt32(destination, To<uint>(value));
        else if (typeof(T) == typeof(long))
            TEndian.WriteInt64(destination, To<long>(value));
        else if (typeof(T) == typeof(ulong))
            TEndian.WriteUInt64(destination, To<ulong>(value));
        else if (typeof(T) == typeof(nint))
            TEndian.WriteIntPtr(destination, To<nint>(value));
        else if (typeof(T) == typeof(nuint))
            TEndian.WriteUIntPtr(destination, To<nuint>(value));
        else if (typeof(T) == typeof(char))
            TEndian.WriteUInt16(destination, To<char>(value));
        else
            throw new UnreachableException();
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

    private static T From<TValue>(TValue value)
        where TValue : unmanaged
    {
        return Unsafe.BitCast<TValue, T>(value);
    }

    private static TValue To<TValue>(T value)
        where TValue : unmanaged
    {
        return Unsafe.BitCast<T, TValue>(value);
    }
}
