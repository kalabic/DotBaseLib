using DotBase.Buffers;
using DotBase.Integral.Internal;
using System.Runtime.CompilerServices;
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
            source.Capacity.ValueByteCount);
        nuint destinationByteCount = GetByteCount(
            valueCount,
            destination.Capacity.ValueByteCount);

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

        if (conversion.IsIdentity &&
            source.IntegralValueType == destination.IntegralValueType &&
            source.Capacity.ValueByteCount == destination.Capacity.ValueByteCount)
        {
            ByteOrder srcOrder = ResolveByteOrder(source.Format.ByteOrder);
            ByteOrder dstOrder = ResolveByteOrder(destination.Format.ByteOrder);
            if (srcOrder != dstOrder)
            {
                // Same scalar layout, opposite endian: one-pass reverse-copy.
                IntegralWire.ReverseCopyLanes(
                    source.DataPtr,
                    destination.DataPtr,
                    valueCount,
                    source.Capacity.ValueByteCount);
                return;
            }
        }

        Dispatch(
            source.DataPtr,
            source.Capacity.ValueByteCount,
            source.IntegralValueType,
            ResolveByteOrder(source.Format.ByteOrder),
            destination.DataPtr,
            destination.Capacity.ValueByteCount,
            destination.IntegralValueType,
            ResolveByteOrder(destination.Format.ByteOrder),
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
            source.Capacity.ValueByteCount);
        nuint destinationByteCount = GetByteCount(
            valueCount,
            destination.Capacity.ValueByteCount);

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
                source.Capacity.ValueByteCount,
                source.IntegralValueType,
                ResolveByteOrder(source.Format.ByteOrder),
                destination.DataPtr,
                destination.Capacity.ValueByteCount,
                destination.IntegralValueType,
                ResolveByteOrder(destination.Format.ByteOrder),
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
                source.Capacity.ValueByteCount,
                source.IntegralValueType,
                ResolveByteOrder(source.Format.ByteOrder),
                destination.DataPtr,
                destination.Capacity.ValueByteCount,
                destination.IntegralValueType,
                ResolveByteOrder(destination.Format.ByteOrder),
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
            sourceValueOffset * source.Capacity.ValueByteCount);
        long sourceByteStride = valueCount > 1
            ? checked(sourceValueStride * source.Capacity.ValueByteCount)
            : 0;
        long destinationByteOffset = checked(
            destinationValueOffset * destination.Capacity.ValueByteCount);
        long destinationByteStride = valueCount > 1
            ? checked(destinationValueStride * destination.Capacity.ValueByteCount)
            : 0;

        byte* sourcePtr = source.DataPtr + sourceByteOffset;
        byte* destinationPtr = destination.DataPtr + destinationByteOffset;

        if (StridedRangesOverlap(
            sourcePtr,
            sourceByteStride,
            source.Capacity.ValueByteCount,
            destinationPtr,
            destinationByteStride,
            destination.Capacity.ValueByteCount,
            valueCount))
        {
            throw new ArgumentException(
                "CopyStrided does not permit overlapping touched ranges.");
        }

        Dispatch(
            sourcePtr,
            sourceByteStride,
            source.IntegralValueType,
            ResolveByteOrder(source.Format.ByteOrder),
            destinationPtr,
            destinationByteStride,
            destination.IntegralValueType,
            ResolveByteOrder(destination.Format.ByteOrder),
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

    /// <summary>
    /// Byte orders must already be resolved (Native folded to LE/BE).
    /// </summary>
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
        switch (sourceType)
        {
            case IntegralType.UInt8:
                DispatchDestinationType<byte>(
                    source, sourceByteStride, sourceByteOrder,
                    destination, destinationByteStride,
                    destinationType, destinationByteOrder, valueCount, conversion);
                return;
            case IntegralType.Int8:
                DispatchDestinationType<sbyte>(
                    source, sourceByteStride, sourceByteOrder,
                    destination, destinationByteStride,
                    destinationType, destinationByteOrder, valueCount, conversion);
                return;
            case IntegralType.UInt16:
                DispatchDestinationType<ushort>(
                    source, sourceByteStride, sourceByteOrder,
                    destination, destinationByteStride,
                    destinationType, destinationByteOrder, valueCount, conversion);
                return;
            case IntegralType.Int16:
                DispatchDestinationType<short>(
                    source, sourceByteStride, sourceByteOrder,
                    destination, destinationByteStride,
                    destinationType, destinationByteOrder, valueCount, conversion);
                return;
            case IntegralType.UInt32:
                DispatchDestinationType<uint>(
                    source, sourceByteStride, sourceByteOrder,
                    destination, destinationByteStride,
                    destinationType, destinationByteOrder, valueCount, conversion);
                return;
            case IntegralType.Int32:
                DispatchDestinationType<int>(
                    source, sourceByteStride, sourceByteOrder,
                    destination, destinationByteStride,
                    destinationType, destinationByteOrder, valueCount, conversion);
                return;
            case IntegralType.UInt64:
                DispatchDestinationType<ulong>(
                    source, sourceByteStride, sourceByteOrder,
                    destination, destinationByteStride,
                    destinationType, destinationByteOrder, valueCount, conversion);
                return;
            case IntegralType.Int64:
                DispatchDestinationType<long>(
                    source, sourceByteStride, sourceByteOrder,
                    destination, destinationByteStride,
                    destinationType, destinationByteOrder, valueCount, conversion);
                return;
            case IntegralType.Float:
                DispatchDestinationType<float>(
                    source, sourceByteStride, sourceByteOrder,
                    destination, destinationByteStride,
                    destinationType, destinationByteOrder, valueCount, conversion);
                return;
            case IntegralType.Double:
                DispatchDestinationType<double>(
                    source, sourceByteStride, sourceByteOrder,
                    destination, destinationByteStride,
                    destinationType, destinationByteOrder, valueCount, conversion);
                return;
            default:
                throw new NotSupportedException(
                    $"Integral type '{sourceType}' is not a supported source type.");
        }
    }

    private static void DispatchDestinationType<TSource>(
        byte* source,
        long sourceByteStride,
        ByteOrder sourceByteOrder,
        byte* destination,
        long destinationByteStride,
        IntegralType destinationType,
        ByteOrder destinationByteOrder,
        long valueCount,
        in IntegralConversion conversion)
        where TSource : unmanaged
    {
        switch (destinationType)
        {
            case IntegralType.UInt8:
                DispatchEndianPair<TSource, byte>(
                    source, sourceByteStride, sourceByteOrder,
                    destination, destinationByteStride, destinationByteOrder,
                    valueCount, conversion);
                return;
            case IntegralType.Int8:
                DispatchEndianPair<TSource, sbyte>(
                    source, sourceByteStride, sourceByteOrder,
                    destination, destinationByteStride, destinationByteOrder,
                    valueCount, conversion);
                return;
            case IntegralType.UInt16:
                DispatchEndianPair<TSource, ushort>(
                    source, sourceByteStride, sourceByteOrder,
                    destination, destinationByteStride, destinationByteOrder,
                    valueCount, conversion);
                return;
            case IntegralType.Int16:
                DispatchEndianPair<TSource, short>(
                    source, sourceByteStride, sourceByteOrder,
                    destination, destinationByteStride, destinationByteOrder,
                    valueCount, conversion);
                return;
            case IntegralType.UInt32:
                DispatchEndianPair<TSource, uint>(
                    source, sourceByteStride, sourceByteOrder,
                    destination, destinationByteStride, destinationByteOrder,
                    valueCount, conversion);
                return;
            case IntegralType.Int32:
                DispatchEndianPair<TSource, int>(
                    source, sourceByteStride, sourceByteOrder,
                    destination, destinationByteStride, destinationByteOrder,
                    valueCount, conversion);
                return;
            case IntegralType.UInt64:
                DispatchEndianPair<TSource, ulong>(
                    source, sourceByteStride, sourceByteOrder,
                    destination, destinationByteStride, destinationByteOrder,
                    valueCount, conversion);
                return;
            case IntegralType.Int64:
                DispatchEndianPair<TSource, long>(
                    source, sourceByteStride, sourceByteOrder,
                    destination, destinationByteStride, destinationByteOrder,
                    valueCount, conversion);
                return;
            case IntegralType.Float:
                DispatchEndianPair<TSource, float>(
                    source, sourceByteStride, sourceByteOrder,
                    destination, destinationByteStride, destinationByteOrder,
                    valueCount, conversion);
                return;
            case IntegralType.Double:
                DispatchEndianPair<TSource, double>(
                    source, sourceByteStride, sourceByteOrder,
                    destination, destinationByteStride, destinationByteOrder,
                    valueCount, conversion);
                return;
            default:
                throw new NotSupportedException(
                    $"Integral type '{destinationType}' is not a supported destination type.");
        }
    }

    /// <summary>
    /// Endian pair chosen once; convert loop has no ByteOrder switch.
    /// Orders are LE or BE only (Native already resolved).
    /// </summary>
    private static void DispatchEndianPair<TSource, TDestination>(
        byte* source,
        long sourceByteStride,
        ByteOrder sourceByteOrder,
        byte* destination,
        long destinationByteStride,
        ByteOrder destinationByteOrder,
        long valueCount,
        in IntegralConversion conversion)
        where TSource : unmanaged
        where TDestination : unmanaged
    {
        if (sourceByteOrder == ByteOrder.LittleEndian)
        {
            if (destinationByteOrder == ByteOrder.LittleEndian)
            {
                CopyCoreLE_LE<TSource, TDestination>(
                    source, sourceByteStride, destination, destinationByteStride,
                    valueCount, conversion);
                return;
            }

            CopyCoreLE_BE<TSource, TDestination>(
                source, sourceByteStride, destination, destinationByteStride,
                valueCount, conversion);
            return;
        }

        if (destinationByteOrder == ByteOrder.LittleEndian)
        {
            CopyCoreBE_LE<TSource, TDestination>(
                source, sourceByteStride, destination, destinationByteStride,
                valueCount, conversion);
            return;
        }

        CopyCoreBE_BE<TSource, TDestination>(
            source, sourceByteStride, destination, destinationByteStride,
            valueCount, conversion);
    }

    private static void CopyCoreLE_LE<TSource, TDestination>(
        byte* source,
        long sourceByteStride,
        byte* destination,
        long destinationByteStride,
        long valueCount,
        in IntegralConversion conversion)
        where TSource : unmanaged
        where TDestination : unmanaged
    {
        for (long index = 0; index < valueCount; ++index)
        {
            TSource sourceValue = LoadLE<TSource>(source);
            TDestination destinationValue =
                IntegralNumericConversion<TSource, TDestination>.Convert(
                    sourceValue,
                    conversion);
            StoreLE(destination, destinationValue);

            if (index + 1 < valueCount)
            {
                source += sourceByteStride;
                destination += destinationByteStride;
            }
        }
    }

    private static void CopyCoreLE_BE<TSource, TDestination>(
        byte* source,
        long sourceByteStride,
        byte* destination,
        long destinationByteStride,
        long valueCount,
        in IntegralConversion conversion)
        where TSource : unmanaged
        where TDestination : unmanaged
    {
        for (long index = 0; index < valueCount; ++index)
        {
            TSource sourceValue = LoadLE<TSource>(source);
            TDestination destinationValue =
                IntegralNumericConversion<TSource, TDestination>.Convert(
                    sourceValue,
                    conversion);
            StoreBE(destination, destinationValue);

            if (index + 1 < valueCount)
            {
                source += sourceByteStride;
                destination += destinationByteStride;
            }
        }
    }

    private static void CopyCoreBE_LE<TSource, TDestination>(
        byte* source,
        long sourceByteStride,
        byte* destination,
        long destinationByteStride,
        long valueCount,
        in IntegralConversion conversion)
        where TSource : unmanaged
        where TDestination : unmanaged
    {
        for (long index = 0; index < valueCount; ++index)
        {
            TSource sourceValue = LoadBE<TSource>(source);
            TDestination destinationValue =
                IntegralNumericConversion<TSource, TDestination>.Convert(
                    sourceValue,
                    conversion);
            StoreLE(destination, destinationValue);

            if (index + 1 < valueCount)
            {
                source += sourceByteStride;
                destination += destinationByteStride;
            }
        }
    }

    private static void CopyCoreBE_BE<TSource, TDestination>(
        byte* source,
        long sourceByteStride,
        byte* destination,
        long destinationByteStride,
        long valueCount,
        in IntegralConversion conversion)
        where TSource : unmanaged
        where TDestination : unmanaged
    {
        for (long index = 0; index < valueCount; ++index)
        {
            TSource sourceValue = LoadBE<TSource>(source);
            TDestination destinationValue =
                IntegralNumericConversion<TSource, TDestination>.Convert(
                    sourceValue,
                    conversion);
            StoreBE(destination, destinationValue);

            if (index + 1 < valueCount)
            {
                source += sourceByteStride;
                destination += destinationByteStride;
            }
        }
    }

    /// <summary>
    /// LE wire → host. Compatible host is a single aligned load; else Swap*.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T LoadLE<T>(byte* source)
        where T : unmanaged
    {
        if (BitConverter.IsLittleEndian || Unsafe.SizeOf<T>() == 1)
        {
            return *(T*)source;
        }

        T host = default;
        byte* hostPtr = (byte*)&host;
        switch (Unsafe.SizeOf<T>())
        {
            case 2:
                IntegralWire.Swap2(hostPtr, source);
                break;
            case 4:
                IntegralWire.Swap4(hostPtr, source);
                break;
            case 8:
                IntegralWire.Swap8(hostPtr, source);
                break;
            default:
                throw new NotSupportedException(
                    $"Scalar size {Unsafe.SizeOf<T>()} is not supported.");
        }

        return host;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T LoadBE<T>(byte* source)
        where T : unmanaged
    {
        if (!BitConverter.IsLittleEndian || Unsafe.SizeOf<T>() == 1)
        {
            return *(T*)source;
        }

        T host = default;
        byte* hostPtr = (byte*)&host;
        switch (Unsafe.SizeOf<T>())
        {
            case 2:
                IntegralWire.Swap2(hostPtr, source);
                break;
            case 4:
                IntegralWire.Swap4(hostPtr, source);
                break;
            case 8:
                IntegralWire.Swap8(hostPtr, source);
                break;
            default:
                throw new NotSupportedException(
                    $"Scalar size {Unsafe.SizeOf<T>()} is not supported.");
        }

        return host;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void StoreLE<T>(byte* destination, T value)
        where T : unmanaged
    {
        if (BitConverter.IsLittleEndian || Unsafe.SizeOf<T>() == 1)
        {
            *(T*)destination = value;
            return;
        }

        byte* hostPtr = (byte*)&value;
        switch (Unsafe.SizeOf<T>())
        {
            case 2:
                IntegralWire.Swap2(destination, hostPtr);
                return;
            case 4:
                IntegralWire.Swap4(destination, hostPtr);
                return;
            case 8:
                IntegralWire.Swap8(destination, hostPtr);
                return;
            default:
                throw new NotSupportedException(
                    $"Scalar size {Unsafe.SizeOf<T>()} is not supported.");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void StoreBE<T>(byte* destination, T value)
        where T : unmanaged
    {
        if (!BitConverter.IsLittleEndian || Unsafe.SizeOf<T>() == 1)
        {
            *(T*)destination = value;
            return;
        }

        byte* hostPtr = (byte*)&value;
        switch (Unsafe.SizeOf<T>())
        {
            case 2:
                IntegralWire.Swap2(destination, hostPtr);
                return;
            case 4:
                IntegralWire.Swap4(destination, hostPtr);
                return;
            case 8:
                IntegralWire.Swap8(destination, hostPtr);
                return;
            default:
                throw new NotSupportedException(
                    $"Scalar size {Unsafe.SizeOf<T>()} is not supported.");
        }
    }

}
