using DotBase.Buffers;
using DotBase.Integral.Internal;
using System.Runtime.InteropServices;

namespace DotBase.Integral;


public static unsafe class IntegralMemory
{
    private const nuint StackAllocationByteCount = 512;

    public static void Copy(
        in IntegralSpan source,
        in IntegralSpan destination)
    {
        Copy(
            source,
            destination,
            source.IntegralLength,
            IntegralConversion.Identity);
    }

    public static void Copy(
        in IntegralSpan source,
        in IntegralSpan destination,
        long valueCount)
    {
        Copy(
            source,
            destination,
            valueCount,
            IntegralConversion.Identity);
    }

    public static void Copy(
        in IntegralSpan source,
        in IntegralSpan destination,
        long valueCount,
        in IntegralConversion conversion)
    {
        ValidateValueCount(source, destination, valueCount);
        if (valueCount == 0)
        {
            return;
        }

        nuint sourceByteCount = GetByteCount(
            valueCount,
            source.CountOf.ValueByteCount);
        nuint destinationByteCount = GetByteCount(
            valueCount,
            destination.CountOf.ValueByteCount);

        if (RangesOverlap(
            source.DataPtr,
            sourceByteCount,
            destination.DataPtr,
            destinationByteCount))
        {
            throw new ArgumentException(
                "Copy does not permit overlapping source and destination ranges.");
        }

        if (conversion.IsIdentity &&
            HasSameRepresentation(source, destination))
        {
            IntegralByteMemory.Copy(
                source.DataPtr,
                destination.DataPtr,
                sourceByteCount);
            return;
        }

        Dispatch(
            source.DataPtr,
            source.CountOf.ValueByteCount,
            source.IntegralValueType,
            source.Format.ByteOrder,
            destination.DataPtr,
            destination.CountOf.ValueByteCount,
            destination.IntegralValueType,
            destination.Format.ByteOrder,
            valueCount,
            conversion);
    }

    public static void Move(
        in IntegralSpan source,
        in IntegralSpan destination)
    {
        Move(
            source,
            destination,
            source.IntegralLength,
            IntegralConversion.Identity);
    }

    public static void Move(
        in IntegralSpan source,
        in IntegralSpan destination,
        long valueCount)
    {
        Move(
            source,
            destination,
            valueCount,
            IntegralConversion.Identity);
    }

    public static void Move(
        in IntegralSpan source,
        in IntegralSpan destination,
        long valueCount,
        in IntegralConversion conversion)
    {
        ValidateValueCount(source, destination, valueCount);
        if (valueCount == 0)
        {
            return;
        }

        nuint sourceByteCount = GetByteCount(
            valueCount,
            source.CountOf.ValueByteCount);
        nuint destinationByteCount = GetByteCount(
            valueCount,
            destination.CountOf.ValueByteCount);

        bool overlaps = RangesOverlap(
            source.DataPtr,
            sourceByteCount,
            destination.DataPtr,
            destinationByteCount);

        if (!overlaps)
        {
            Copy(
                source,
                destination,
                valueCount,
                conversion);
            return;
        }

        if (conversion.IsIdentity &&
            HasSameRepresentation(source, destination))
        {
            IntegralByteMemory.Move(
                source.DataPtr,
                destination.DataPtr,
                sourceByteCount);
            return;
        }

        if (conversion.IsIdentity &&
            source.IntegralValueType == destination.IntegralValueType &&
            source.DataPtr == destination.DataPtr)
        {
            Dispatch(
                source.DataPtr,
                source.CountOf.ValueByteCount,
                source.IntegralValueType,
                source.Format.ByteOrder,
                destination.DataPtr,
                destination.CountOf.ValueByteCount,
                destination.IntegralValueType,
                destination.Format.ByteOrder,
                valueCount,
                conversion);
            return;
        }

        int stackByteCount = sourceByteCount <= StackAllocationByteCount
            ? (int)sourceByteCount
            : 1;
        byte* stackBuffer = stackalloc byte[stackByteCount];
        byte* allocated = null;
        byte* preserved = stackBuffer;

        if (sourceByteCount > StackAllocationByteCount)
        {
            allocated = (byte*)NativeMemory.Alloc(sourceByteCount);
            preserved = allocated;
        }

        try
        {
            IntegralByteMemory.Copy(
                source.DataPtr,
                preserved,
                sourceByteCount);

            Dispatch(
                preserved,
                source.CountOf.ValueByteCount,
                source.IntegralValueType,
                source.Format.ByteOrder,
                destination.DataPtr,
                destination.CountOf.ValueByteCount,
                destination.IntegralValueType,
                destination.Format.ByteOrder,
                valueCount,
                conversion);
        }
        finally
        {
            if (allocated is not null)
            {
                NativeMemory.Free(allocated);
            }
        }
    }

    public static void CopyStrided(
        in IntegralSpan source,
        long sourceValueOffset,
        long sourceValueStride,
        in IntegralSpan destination,
        long destinationValueOffset,
        long destinationValueStride,
        long valueCount)
    {
        CopyStrided(
            source,
            sourceValueOffset,
            sourceValueStride,
            destination,
            destinationValueOffset,
            destinationValueStride,
            valueCount,
            IntegralConversion.Identity);
    }

    public static void CopyStrided(
        in IntegralSpan source,
        long sourceValueOffset,
        long sourceValueStride,
        in IntegralSpan destination,
        long destinationValueOffset,
        long destinationValueStride,
        long valueCount,
        in IntegralConversion conversion)
    {
        ValidateStridedRange(
            sourceValueOffset,
            sourceValueStride,
            valueCount,
            source.IntegralLength,
            nameof(sourceValueOffset),
            nameof(sourceValueStride));
        ValidateStridedRange(
            destinationValueOffset,
            destinationValueStride,
            valueCount,
            destination.IntegralLength,
            nameof(destinationValueOffset),
            nameof(destinationValueStride));

        if (valueCount == 0)
        {
            return;
        }

        long sourceByteOffset = checked(
            sourceValueOffset * source.CountOf.ValueByteCount);
        long sourceByteStride = valueCount > 1
            ? checked(sourceValueStride * source.CountOf.ValueByteCount)
            : 0;
        long destinationByteOffset = checked(
            destinationValueOffset * destination.CountOf.ValueByteCount);
        long destinationByteStride = valueCount > 1
            ? checked(destinationValueStride * destination.CountOf.ValueByteCount)
            : 0;

        byte* sourcePtr = source.DataPtr + sourceByteOffset;
        byte* destinationPtr = destination.DataPtr + destinationByteOffset;

        if (StridedRangesOverlap(
            sourcePtr,
            sourceByteStride,
            source.CountOf.ValueByteCount,
            destinationPtr,
            destinationByteStride,
            destination.CountOf.ValueByteCount,
            valueCount))
        {
            throw new ArgumentException(
                "CopyStrided does not permit overlapping touched ranges.");
        }

        Dispatch(
            sourcePtr,
            sourceByteStride,
            source.IntegralValueType,
            source.Format.ByteOrder,
            destinationPtr,
            destinationByteStride,
            destination.IntegralValueType,
            destination.Format.ByteOrder,
            valueCount,
            conversion);
    }

    public static void Clear(in IntegralSpan destination)
    {
        IntegralByteMemory.Clear(
            destination.DataPtr,
            checked((nuint)destination.Length));
    }

    private static void ValidateValueCount(
        in IntegralSpan source,
        in IntegralSpan destination,
        long valueCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(valueCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            valueCount,
            source.IntegralLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            valueCount,
            destination.IntegralLength);
    }

    private static void ValidateStridedRange(
        long valueOffset,
        long valueStride,
        long valueCount,
        long availableValueCount,
        string offsetName,
        string strideName)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(
            valueOffset,
            offsetName);
        ArgumentOutOfRangeException.ThrowIfNegative(valueCount);

        if (valueCount == 0)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(
                valueOffset,
                availableValueCount,
                offsetName);
            return;
        }

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(
            valueStride,
            0,
            strideName);

        long lastValueIndex = checked(
            valueOffset +
            checked((valueCount - 1) * valueStride));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(
            lastValueIndex,
            availableValueCount,
            offsetName);
    }

    private static nuint GetByteCount(
        long valueCount,
        int valueByteCount)
    {
        return checked((nuint)checked(valueCount * valueByteCount));
    }

    private static bool HasSameRepresentation(
        in IntegralSpan source,
        in IntegralSpan destination)
    {
        return
            source.IntegralValueType == destination.IntegralValueType &&
            ResolveByteOrder(source.Format.ByteOrder) ==
            ResolveByteOrder(destination.Format.ByteOrder);
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

    private static bool RangesOverlap(
        byte* first,
        nuint firstByteCount,
        byte* second,
        nuint secondByteCount)
    {
        if (firstByteCount == 0 || secondByteCount == 0)
        {
            return false;
        }

        nuint firstAddress = (nuint)first;
        nuint secondAddress = (nuint)second;

        return firstAddress < secondAddress
            ? secondAddress - firstAddress < firstByteCount
            : firstAddress - secondAddress < secondByteCount;
    }

    private static bool StridedRangesOverlap(
        byte* source,
        long sourceByteStride,
        int sourceValueByteCount,
        byte* destination,
        long destinationByteStride,
        int destinationValueByteCount,
        long valueCount)
    {
        long sourceIndex = 0;
        long destinationIndex = 0;

        while (sourceIndex < valueCount &&
               destinationIndex < valueCount)
        {
            nuint sourceAddress = (nuint)source;
            nuint destinationAddress = (nuint)destination;

            if (sourceAddress < destinationAddress &&
                destinationAddress - sourceAddress >=
                (nuint)sourceValueByteCount)
            {
                ++sourceIndex;
                if (sourceIndex < valueCount)
                {
                    source += sourceByteStride;
                }
                continue;
            }

            if (destinationAddress < sourceAddress &&
                sourceAddress - destinationAddress >=
                (nuint)destinationValueByteCount)
            {
                ++destinationIndex;
                if (destinationIndex < valueCount)
                {
                    destination += destinationByteStride;
                }
                continue;
            }

            return true;
        }

        return false;
    }

    private static void Dispatch(
        byte* source,
        long sourceByteStride,
        IntegralType sourceType,
        ByteOrder sourceByteOrder,
        byte* destination,
        long destinationByteStride,
        IntegralType destinationType,
        ByteOrder destinationByteOrder,
        long valueCount,
        in IntegralConversion conversion)
    {
        switch (sourceByteOrder)
        {
            case ByteOrder.Native:
                DispatchSource<NativeEndianCodec>(
                    source,
                    sourceByteStride,
                    sourceType,
                    destination,
                    destinationByteStride,
                    destinationType,
                    destinationByteOrder,
                    valueCount,
                    conversion);
                return;

            case ByteOrder.LittleEndian:
                DispatchSource<LittleEndianCodec>(
                    source,
                    sourceByteStride,
                    sourceType,
                    destination,
                    destinationByteStride,
                    destinationType,
                    destinationByteOrder,
                    valueCount,
                    conversion);
                return;

            case ByteOrder.BigEndian:
                DispatchSource<BigEndianCodec>(
                    source,
                    sourceByteStride,
                    sourceType,
                    destination,
                    destinationByteStride,
                    destinationType,
                    destinationByteOrder,
                    valueCount,
                    conversion);
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(sourceByteOrder));
        }
    }

    private static void DispatchSource<TSourceEndian>(
        byte* source,
        long sourceByteStride,
        IntegralType sourceType,
        byte* destination,
        long destinationByteStride,
        IntegralType destinationType,
        ByteOrder destinationByteOrder,
        long valueCount,
        in IntegralConversion conversion)
        where TSourceEndian : struct, IEndianCodec
    {
        switch (sourceType)
        {
            case IntegralType.UInt8:
                DispatchDestination<byte, TSourceEndian>(
                    source, sourceByteStride, destination, destinationByteStride,
                    destinationType, destinationByteOrder, valueCount, conversion);
                return;
            case IntegralType.Int8:
                DispatchDestination<sbyte, TSourceEndian>(
                    source, sourceByteStride, destination, destinationByteStride,
                    destinationType, destinationByteOrder, valueCount, conversion);
                return;
            case IntegralType.UInt16:
                DispatchDestination<ushort, TSourceEndian>(
                    source, sourceByteStride, destination, destinationByteStride,
                    destinationType, destinationByteOrder, valueCount, conversion);
                return;
            case IntegralType.Int16:
                DispatchDestination<short, TSourceEndian>(
                    source, sourceByteStride, destination, destinationByteStride,
                    destinationType, destinationByteOrder, valueCount, conversion);
                return;
            case IntegralType.UInt32:
                DispatchDestination<uint, TSourceEndian>(
                    source, sourceByteStride, destination, destinationByteStride,
                    destinationType, destinationByteOrder, valueCount, conversion);
                return;
            case IntegralType.Int32:
                DispatchDestination<int, TSourceEndian>(
                    source, sourceByteStride, destination, destinationByteStride,
                    destinationType, destinationByteOrder, valueCount, conversion);
                return;
            case IntegralType.UInt64:
                DispatchDestination<ulong, TSourceEndian>(
                    source, sourceByteStride, destination, destinationByteStride,
                    destinationType, destinationByteOrder, valueCount, conversion);
                return;
            case IntegralType.Int64:
                DispatchDestination<long, TSourceEndian>(
                    source, sourceByteStride, destination, destinationByteStride,
                    destinationType, destinationByteOrder, valueCount, conversion);
                return;
            case IntegralType.Float:
                DispatchDestination<float, TSourceEndian>(
                    source, sourceByteStride, destination, destinationByteStride,
                    destinationType, destinationByteOrder, valueCount, conversion);
                return;
            case IntegralType.Double:
                DispatchDestination<double, TSourceEndian>(
                    source, sourceByteStride, destination, destinationByteStride,
                    destinationType, destinationByteOrder, valueCount, conversion);
                return;
            default:
                throw new NotSupportedException(
                    $"Integral type '{sourceType}' is not a supported source type.");
        }
    }

    private static void DispatchDestination<TSource, TSourceEndian>(
        byte* source,
        long sourceByteStride,
        byte* destination,
        long destinationByteStride,
        IntegralType destinationType,
        ByteOrder destinationByteOrder,
        long valueCount,
        in IntegralConversion conversion)
        where TSource : unmanaged
        where TSourceEndian : struct, IEndianCodec
    {
        switch (destinationByteOrder)
        {
            case ByteOrder.Native:
                DispatchDestinationType<
                    TSource,
                    TSourceEndian,
                    NativeEndianCodec>(
                    source, sourceByteStride, destination, destinationByteStride,
                    destinationType, valueCount, conversion);
                return;
            case ByteOrder.LittleEndian:
                DispatchDestinationType<
                    TSource,
                    TSourceEndian,
                    LittleEndianCodec>(
                    source, sourceByteStride, destination, destinationByteStride,
                    destinationType, valueCount, conversion);
                return;
            case ByteOrder.BigEndian:
                DispatchDestinationType<
                    TSource,
                    TSourceEndian,
                    BigEndianCodec>(
                    source, sourceByteStride, destination, destinationByteStride,
                    destinationType, valueCount, conversion);
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(destinationByteOrder));
        }
    }

    private static void DispatchDestinationType<
        TSource,
        TSourceEndian,
        TDestinationEndian>(
        byte* source,
        long sourceByteStride,
        byte* destination,
        long destinationByteStride,
        IntegralType destinationType,
        long valueCount,
        in IntegralConversion conversion)
        where TSource : unmanaged
        where TSourceEndian : struct, IEndianCodec
        where TDestinationEndian : struct, IEndianCodec
    {
        switch (destinationType)
        {
            case IntegralType.UInt8:
                CopyCore<TSource, TSourceEndian, byte, TDestinationEndian>(
                    source, sourceByteStride, destination, destinationByteStride,
                    valueCount, conversion);
                return;
            case IntegralType.Int8:
                CopyCore<TSource, TSourceEndian, sbyte, TDestinationEndian>(
                    source, sourceByteStride, destination, destinationByteStride,
                    valueCount, conversion);
                return;
            case IntegralType.UInt16:
                CopyCore<TSource, TSourceEndian, ushort, TDestinationEndian>(
                    source, sourceByteStride, destination, destinationByteStride,
                    valueCount, conversion);
                return;
            case IntegralType.Int16:
                CopyCore<TSource, TSourceEndian, short, TDestinationEndian>(
                    source, sourceByteStride, destination, destinationByteStride,
                    valueCount, conversion);
                return;
            case IntegralType.UInt32:
                CopyCore<TSource, TSourceEndian, uint, TDestinationEndian>(
                    source, sourceByteStride, destination, destinationByteStride,
                    valueCount, conversion);
                return;
            case IntegralType.Int32:
                CopyCore<TSource, TSourceEndian, int, TDestinationEndian>(
                    source, sourceByteStride, destination, destinationByteStride,
                    valueCount, conversion);
                return;
            case IntegralType.UInt64:
                CopyCore<TSource, TSourceEndian, ulong, TDestinationEndian>(
                    source, sourceByteStride, destination, destinationByteStride,
                    valueCount, conversion);
                return;
            case IntegralType.Int64:
                CopyCore<TSource, TSourceEndian, long, TDestinationEndian>(
                    source, sourceByteStride, destination, destinationByteStride,
                    valueCount, conversion);
                return;
            case IntegralType.Float:
                CopyCore<TSource, TSourceEndian, float, TDestinationEndian>(
                    source, sourceByteStride, destination, destinationByteStride,
                    valueCount, conversion);
                return;
            case IntegralType.Double:
                CopyCore<TSource, TSourceEndian, double, TDestinationEndian>(
                    source, sourceByteStride, destination, destinationByteStride,
                    valueCount, conversion);
                return;
            default:
                throw new NotSupportedException(
                    $"Integral type '{destinationType}' is not a supported destination type.");
        }
    }

    private static void CopyCore<
        TSource,
        TSourceEndian,
        TDestination,
        TDestinationEndian>(
        byte* source,
        long sourceByteStride,
        byte* destination,
        long destinationByteStride,
        long valueCount,
        in IntegralConversion conversion)
        where TSource : unmanaged
        where TSourceEndian : struct, IEndianCodec
        where TDestination : unmanaged
        where TDestinationEndian : struct, IEndianCodec
    {
        for (long index = 0; index < valueCount; ++index)
        {
            TSource sourceValue =
                IntegralCodec<TSource, TSourceEndian>.Read(source);
            TDestination destinationValue =
                IntegralNumericConversion<TSource, TDestination>.Convert(
                    sourceValue,
                    conversion);
            IntegralCodec<TDestination, TDestinationEndian>.Write(
                destination,
                destinationValue);

            if (index + 1 < valueCount)
            {
                source += sourceByteStride;
                destination += destinationByteStride;
            }
        }
    }
}
