using DotBase.Buffers;
using DotBase.Integral.Internal;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DotBase.Integral;


/// <summary>
/// Bulk transfer between integral regions.
/// <para>
/// Short names are <b>trusted</b> (no span validation) and <b>block-complete</b>
/// by default (trailing values excluded), matching ring span I/O.
/// Use <c>*Checked</c> to validate descriptors and layout/endian contracts.
/// </para>
/// <list type="bullet">
/// <item><see cref="Copy"/> / <see cref="CopyChecked"/> - same layout and endian; raw bytes.</item>
/// <item><see cref="ReverseCopy"/> / <see cref="ReverseCopyChecked"/> - lane byte-reversal only.</item>
/// <item><see cref="Convert"/> / <see cref="ConvertChecked"/> - explicit type/endian/scale.</item>
/// </list>
/// </summary>
public static unsafe class IntegralMemory
{
    private const nuint StackAllocationByteCount = 512;

    // -------------------------------------------------------------------------
    // Copy
    // -------------------------------------------------------------------------

    /// <summary>
    /// <b>Trusted, block-complete copy.</b> No span validation. Copies complete
    /// blocks only (trailing values excluded). Caller guarantees layout/endian.
    /// </summary>
    public static void Copy(
        in IntegralSpan source,
        in IntegralSpan destination)
    {
        Copy(source, destination, CountBlockCompleteValues(source, destination));
    }

    /// <summary>
    /// <b>Trusted copy</b> of <paramref name="valueCount"/> scalars. No span validation.
    /// </summary>
    public static void Copy(
        in IntegralSpan source,
        in IntegralSpan destination,
        long valueCount)
    {
        if (valueCount == 0)
        {
            return;
        }

        nuint byteCount = GetByteCount(
            valueCount,
            source.Capacity.ValueByteCount);

        Buffer.MemoryCopy(
            source.DataPtr,
            destination.DataPtr,
            (ulong)byteCount,
            (ulong)byteCount);
    }

    /// <summary>
    /// <b>Trusted raw memcpy</b> of <paramref name="byteCount"/> bytes.
    /// No validation, no overlap check, no endian/lane processing.
    /// For same-endian bulk transfer when bases and extents are already known.
    /// </summary>
    public static void Copy(
        byte* source,
        byte* destination,
        long byteCount)
    {
        if (byteCount == 0)
        {
            return;
        }

        nuint n = checked((nuint)byteCount);
        Buffer.MemoryCopy(
            source,
            destination,
            (ulong)n,
            (ulong)n);
    }

    /// <summary>
    /// <b>Trusted raw memcpy</b> of a prefix of <paramref name="sourceRange"/>
    /// from <paramref name="sourceBase"/> into <paramref name="destination"/>.
    /// <para>
    /// Copies <paramref name="byteCount"/> bytes starting at
    /// <c>sourceBase + sourceRange.ByteOffset</c>. Caller guarantees
    /// <paramref name="byteCount"/> ≤ <see cref="IntegralRange.ByteLength"/>
    /// (or 0). No span construction, no validation.
    /// </para>
    /// </summary>
    public static void Copy(
        byte* sourceBase,
        in IntegralRange sourceRange,
        byte* destination,
        long byteCount)
    {
        if (byteCount == 0 || sourceRange.IsEmpty)
        {
            return;
        }

        Copy(
            sourceBase + sourceRange.ByteOffset,
            destination,
            byteCount);
    }

    /// <summary>
    /// <b>Trusted raw memcpy</b> of a prefix of <paramref name="destinationRange"/>
    /// from <paramref name="source"/> into <paramref name="destinationBase"/>.
    /// Copies <paramref name="byteCount"/> bytes starting at
    /// <c>destinationBase + destinationRange.ByteOffset</c>.
    /// </summary>
    public static void Copy(
        byte* source,
        byte* destinationBase,
        in IntegralRange destinationRange,
        long byteCount)
    {
        if (byteCount == 0 || destinationRange.IsEmpty)
        {
            return;
        }

        Copy(
            source,
            destinationBase + destinationRange.ByteOffset,
            byteCount);
    }

    /// <summary>
    /// Validates both spans, layout, and endian; then block-complete <see cref="Copy"/>.
    /// </summary>
    public static void CopyChecked(
        in IntegralSpan source,
        in IntegralSpan destination)
    {
        source.Validate();
        destination.Validate();
        RequireSameLayout(source, destination);
        RequireSameEndian(source, destination);
        long valueCount = CountBlockCompleteValues(source, destination);
        if (valueCount == 0)
        {
            return;
        }

        nuint byteCount = GetByteCount(
            valueCount,
            source.Capacity.ValueByteCount);
        ThrowIfRangesOverlap(
            source.DataPtr,
            byteCount,
            destination.DataPtr,
            byteCount,
            "Copy");
        Copy(source, destination, valueCount);
    }

    /// <summary>
    /// Validates both spans, layout, endian, and count; then <see cref="Copy"/>.
    /// </summary>
    public static void CopyChecked(
        in IntegralSpan source,
        in IntegralSpan destination,
        long valueCount)
    {
        source.Validate();
        destination.Validate();
        ValidateValueCount(source, destination, valueCount);
        RequireSameLayout(source, destination);
        RequireSameEndian(source, destination);
        if (valueCount == 0)
        {
            return;
        }

        nuint byteCount = GetByteCount(
            valueCount,
            source.Capacity.ValueByteCount);
        ThrowIfRangesOverlap(
            source.DataPtr,
            byteCount,
            destination.DataPtr,
            byteCount,
            "Copy");
        Copy(source, destination, valueCount);
    }

    // -------------------------------------------------------------------------
    // ReverseCopy
    // -------------------------------------------------------------------------

    /// <summary>
    /// <b>Trusted, block-complete reverse-copy.</b> No span validation. Lane endian flip only.
    /// </summary>
    public static void ReverseCopy(
        in IntegralSpan source,
        in IntegralSpan destination)
    {
        ReverseCopy(
            source,
            destination,
            CountBlockCompleteValues(source, destination));
    }

    /// <summary>
    /// <b>Trusted reverse-copy</b> of <paramref name="valueCount"/> scalars.
    /// </summary>
    public static void ReverseCopy(
        in IntegralSpan source,
        in IntegralSpan destination,
        long valueCount)
    {
        if (valueCount == 0)
        {
            return;
        }

        int valueByteCount = source.Capacity.ValueByteCount;
        if (source.DataPtr == destination.DataPtr)
        {
            IntegralPrimitives.ReverseLanesInPlace(
                destination.DataPtr,
                valueCount,
                valueByteCount);
            return;
        }

        IntegralPrimitives.ReverseCopyLanes(
            source.DataPtr,
            destination.DataPtr,
            valueCount,
            valueByteCount);
    }

    public static void ReverseCopyChecked(
        in IntegralSpan source,
        in IntegralSpan destination)
    {
        source.Validate();
        destination.Validate();
        RequireSameLayout(source, destination);
        RequireOppositeEndian(source, destination);
        long valueCount = CountBlockCompleteValues(source, destination);
        if (valueCount == 0)
        {
            return;
        }

        nuint byteCount = GetByteCount(
            valueCount,
            source.Capacity.ValueByteCount);
        if (source.DataPtr != destination.DataPtr)
        {
            ThrowIfRangesOverlap(
                source.DataPtr,
                byteCount,
                destination.DataPtr,
                byteCount,
                "ReverseCopy");
        }

        ReverseCopy(source, destination, valueCount);
    }

    public static void ReverseCopyChecked(
        in IntegralSpan source,
        in IntegralSpan destination,
        long valueCount)
    {
        source.Validate();
        destination.Validate();
        ValidateValueCount(source, destination, valueCount);
        RequireSameLayout(source, destination);
        RequireOppositeEndian(source, destination);
        if (valueCount == 0)
        {
            return;
        }

        nuint byteCount = GetByteCount(
            valueCount,
            source.Capacity.ValueByteCount);
        if (source.DataPtr != destination.DataPtr)
        {
            ThrowIfRangesOverlap(
                source.DataPtr,
                byteCount,
                destination.DataPtr,
                byteCount,
                "ReverseCopy");
        }

        ReverseCopy(source, destination, valueCount);
    }

    // -------------------------------------------------------------------------
    // Convert
    // -------------------------------------------------------------------------

    /// <summary>
    /// <b>Trusted, block-complete convert.</b> No span validation.
    /// </summary>
    public static void Convert(
        in IntegralSpan source,
        in IntegralSpan destination,
        in IntegralConversion conversion)
    {
        Convert(
            source,
            destination,
            CountBlockCompleteValues(source, destination),
            conversion);
    }

    /// <summary>
    /// <b>Trusted convert</b> of <paramref name="valueCount"/> scalars.
    /// </summary>
    public static void Convert(
        in IntegralSpan source,
        in IntegralSpan destination,
        long valueCount,
        in IntegralConversion conversion)
    {
        if (valueCount == 0)
        {
            return;
        }

        nuint sourceByteCount = GetByteCount(
            valueCount,
            source.Capacity.ValueByteCount);

        if (conversion.IsIdentity &&
            HasSameLayout(source, destination) &&
            HasSameEndian(source, destination))
        {
            Buffer.MemoryCopy(
                source.DataPtr,
                destination.DataPtr,
                (ulong)sourceByteCount,
                (ulong)sourceByteCount);
            return;
        }

        if (conversion.IsIdentity &&
            HasSameLayout(source, destination) &&
            HasOppositeEndian(source, destination))
        {
            IntegralPrimitives.ReverseCopyLanes(
                source.DataPtr,
                destination.DataPtr,
                valueCount,
                source.Capacity.ValueByteCount);
            return;
        }

        Dispatch(
            source.DataPtr,
            source.Capacity.ValueByteCount,
            source.IntegralValueType,
            source.Format.ByteOrder.Resolve(),
            destination.DataPtr,
            destination.Capacity.ValueByteCount,
            destination.IntegralValueType,
            destination.Format.ByteOrder.Resolve(),
            valueCount,
            conversion);
    }

    public static void ConvertChecked(
        in IntegralSpan source,
        in IntegralSpan destination,
        in IntegralConversion conversion)
    {
        source.Validate();
        destination.Validate();
        long valueCount = CountBlockCompleteValues(source, destination);
        ConvertCheckedCore(source, destination, valueCount, conversion);
    }

    public static void ConvertChecked(
        in IntegralSpan source,
        in IntegralSpan destination,
        long valueCount,
        in IntegralConversion conversion)
    {
        source.Validate();
        destination.Validate();
        ValidateValueCount(source, destination, valueCount);
        ConvertCheckedCore(source, destination, valueCount, conversion);
    }

    private static void ConvertCheckedCore(
        in IntegralSpan source,
        in IntegralSpan destination,
        long valueCount,
        in IntegralConversion conversion)
    {
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
        ThrowIfRangesOverlap(
            source.DataPtr,
            sourceByteCount,
            destination.DataPtr,
            destinationByteCount,
            "Convert");
        Convert(source, destination, valueCount, conversion);
    }

    // -------------------------------------------------------------------------
    // Move / ReverseMove / ConvertMove
    // -------------------------------------------------------------------------

    /// <summary>
    /// <b>Trusted, block-complete move</b> (memmove-class). No span validation.
    /// </summary>
    public static void Move(
        in IntegralSpan source,
        in IntegralSpan destination)
    {
        Move(source, destination, CountBlockCompleteValues(source, destination));
    }

    public static void Move(
        in IntegralSpan source,
        in IntegralSpan destination,
        long valueCount)
    {
        if (valueCount == 0)
        {
            return;
        }

        nuint byteCount = GetByteCount(
            valueCount,
            source.Capacity.ValueByteCount);
        IntegralByteMemory.Move(
            source.DataPtr,
            destination.DataPtr,
            byteCount);
    }

    public static void MoveChecked(
        in IntegralSpan source,
        in IntegralSpan destination)
    {
        source.Validate();
        destination.Validate();
        RequireSameLayout(source, destination);
        RequireSameEndian(source, destination);
        Move(source, destination, CountBlockCompleteValues(source, destination));
    }

    public static void MoveChecked(
        in IntegralSpan source,
        in IntegralSpan destination,
        long valueCount)
    {
        source.Validate();
        destination.Validate();
        ValidateValueCount(source, destination, valueCount);
        RequireSameLayout(source, destination);
        RequireSameEndian(source, destination);
        Move(source, destination, valueCount);
    }

    public static void ReverseMove(
        in IntegralSpan source,
        in IntegralSpan destination)
    {
        ReverseMove(
            source,
            destination,
            CountBlockCompleteValues(source, destination));
    }

    public static void ReverseMove(
        in IntegralSpan source,
        in IntegralSpan destination,
        long valueCount)
    {
        if (valueCount == 0)
        {
            return;
        }

        int valueByteCount = source.Capacity.ValueByteCount;
        if (source.DataPtr == destination.DataPtr)
        {
            IntegralPrimitives.ReverseLanesInPlace(
                destination.DataPtr,
                valueCount,
                valueByteCount);
            return;
        }

        IntegralPrimitives.ReverseCopyLanes(
            source.DataPtr,
            destination.DataPtr,
            valueCount,
            valueByteCount);
    }

    public static void ReverseMoveChecked(
        in IntegralSpan source,
        in IntegralSpan destination)
    {
        source.Validate();
        destination.Validate();
        RequireSameLayout(source, destination);
        RequireOppositeEndian(source, destination);
        long valueCount = CountBlockCompleteValues(source, destination);
        ReverseMoveCheckedCore(source, destination, valueCount);
    }

    public static void ReverseMoveChecked(
        in IntegralSpan source,
        in IntegralSpan destination,
        long valueCount)
    {
        source.Validate();
        destination.Validate();
        ValidateValueCount(source, destination, valueCount);
        RequireSameLayout(source, destination);
        RequireOppositeEndian(source, destination);
        ReverseMoveCheckedCore(source, destination, valueCount);
    }

    private static void ReverseMoveCheckedCore(
        in IntegralSpan source,
        in IntegralSpan destination,
        long valueCount)
    {
        if (valueCount == 0)
        {
            return;
        }

        nuint byteCount = GetByteCount(
            valueCount,
            source.Capacity.ValueByteCount);
        if (source.DataPtr != destination.DataPtr &&
            RangesOverlap(
                source.DataPtr,
                byteCount,
                destination.DataPtr,
                byteCount))
        {
            throw new ArgumentException(
                "ReverseMove does not permit partially overlapping ranges " +
                "(same pointer in-place reverse is allowed).");
        }

        ReverseMove(source, destination, valueCount);
    }

    public static void ConvertMove(
        in IntegralSpan source,
        in IntegralSpan destination,
        in IntegralConversion conversion)
    {
        ConvertMove(
            source,
            destination,
            CountBlockCompleteValues(source, destination),
            conversion);
    }

    public static void ConvertMove(
        in IntegralSpan source,
        in IntegralSpan destination,
        long valueCount,
        in IntegralConversion conversion)
    {
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
            Convert(source, destination, valueCount, conversion);
            return;
        }

        if (conversion.IsIdentity &&
            HasSameLayout(source, destination) &&
            HasSameEndian(source, destination))
        {
            IntegralByteMemory.Move(
                source.DataPtr,
                destination.DataPtr,
                sourceByteCount);
            return;
        }

        if (conversion.IsIdentity &&
            HasSameLayout(source, destination) &&
            HasOppositeEndian(source, destination) &&
            source.DataPtr == destination.DataPtr)
        {
            IntegralPrimitives.ReverseLanesInPlace(
                destination.DataPtr,
                valueCount,
                source.Capacity.ValueByteCount);
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
            Buffer.MemoryCopy(
                source.DataPtr,
                preserved,
                (ulong)sourceByteCount,
                (ulong)sourceByteCount);

            Dispatch(
                preserved,
                source.Capacity.ValueByteCount,
                source.IntegralValueType,
                source.Format.ByteOrder.Resolve(),
                destination.DataPtr,
                destination.Capacity.ValueByteCount,
                destination.IntegralValueType,
                destination.Format.ByteOrder.Resolve(),
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

    public static void ConvertMoveChecked(
        in IntegralSpan source,
        in IntegralSpan destination,
        in IntegralConversion conversion)
    {
        source.Validate();
        destination.Validate();
        ConvertMove(
            source,
            destination,
            CountBlockCompleteValues(source, destination),
            conversion);
    }

    public static void ConvertMoveChecked(
        in IntegralSpan source,
        in IntegralSpan destination,
        long valueCount,
        in IntegralConversion conversion)
    {
        source.Validate();
        destination.Validate();
        ValidateValueCount(source, destination, valueCount);
        ConvertMove(source, destination, valueCount, conversion);
    }

    // -------------------------------------------------------------------------
    // Strided (value-granular; block framing is the caller's offset/stride)
    // -------------------------------------------------------------------------

    /// <summary>
    /// <b>Trusted</b> strided copy. No span validation. Caller guarantees ranges.
    /// </summary>
    public static void CopyStrided(
        in IntegralSpan source,
        long sourceValueOffset,
        long sourceValueStride,
        in IntegralSpan destination,
        long destinationValueOffset,
        long destinationValueStride,
        long valueCount)
    {
        if (valueCount == 0)
        {
            return;
        }

        PrepareStridedTrusted(
            source,
            sourceValueOffset,
            sourceValueStride,
            destination,
            destinationValueOffset,
            destinationValueStride,
            valueCount,
            out byte* sourcePtr,
            out long sourceByteStride,
            out byte* destinationPtr,
            out long destinationByteStride);

        Dispatch(
            sourcePtr,
            sourceByteStride,
            source.IntegralValueType,
            source.Format.ByteOrder.Resolve(),
            destinationPtr,
            destinationByteStride,
            destination.IntegralValueType,
            destination.Format.ByteOrder.Resolve(),
            valueCount,
            IntegralConversion.Identity);
    }

    public static void CopyStridedChecked(
        in IntegralSpan source,
        long sourceValueOffset,
        long sourceValueStride,
        in IntegralSpan destination,
        long destinationValueOffset,
        long destinationValueStride,
        long valueCount)
    {
        source.Validate();
        destination.Validate();
        RequireSameLayout(source, destination);
        RequireSameEndian(source, destination);
        PrepareStridedChecked(
            source,
            sourceValueOffset,
            sourceValueStride,
            destination,
            destinationValueOffset,
            destinationValueStride,
            valueCount,
            out byte* sourcePtr,
            out long sourceByteStride,
            out byte* destinationPtr,
            out long destinationByteStride);

        if (valueCount == 0)
        {
            return;
        }

        Dispatch(
            sourcePtr,
            sourceByteStride,
            source.IntegralValueType,
            source.Format.ByteOrder.Resolve(),
            destinationPtr,
            destinationByteStride,
            destination.IntegralValueType,
            destination.Format.ByteOrder.Resolve(),
            valueCount,
            IntegralConversion.Identity);
    }

    public static void ConvertStrided(
        in IntegralSpan source,
        long sourceValueOffset,
        long sourceValueStride,
        in IntegralSpan destination,
        long destinationValueOffset,
        long destinationValueStride,
        long valueCount,
        in IntegralConversion conversion)
    {
        if (valueCount == 0)
        {
            return;
        }

        PrepareStridedTrusted(
            source,
            sourceValueOffset,
            sourceValueStride,
            destination,
            destinationValueOffset,
            destinationValueStride,
            valueCount,
            out byte* sourcePtr,
            out long sourceByteStride,
            out byte* destinationPtr,
            out long destinationByteStride);

        Dispatch(
            sourcePtr,
            sourceByteStride,
            source.IntegralValueType,
            source.Format.ByteOrder.Resolve(),
            destinationPtr,
            destinationByteStride,
            destination.IntegralValueType,
            destination.Format.ByteOrder.Resolve(),
            valueCount,
            conversion);
    }

    public static void ConvertStridedChecked(
        in IntegralSpan source,
        long sourceValueOffset,
        long sourceValueStride,
        in IntegralSpan destination,
        long destinationValueOffset,
        long destinationValueStride,
        long valueCount,
        in IntegralConversion conversion)
    {
        source.Validate();
        destination.Validate();
        PrepareStridedChecked(
            source,
            sourceValueOffset,
            sourceValueStride,
            destination,
            destinationValueOffset,
            destinationValueStride,
            valueCount,
            out byte* sourcePtr,
            out long sourceByteStride,
            out byte* destinationPtr,
            out long destinationByteStride);

        if (valueCount == 0)
        {
            return;
        }

        Dispatch(
            sourcePtr,
            sourceByteStride,
            source.IntegralValueType,
            source.Format.ByteOrder.Resolve(),
            destinationPtr,
            destinationByteStride,
            destination.IntegralValueType,
            destination.Format.ByteOrder.Resolve(),
            valueCount,
            conversion);
    }

    /// <summary>
    /// <b>Trusted clear</b> of the full span length (bytes). No validation.
    /// </summary>
    public static void Clear(in IntegralSpan destination)
    {
        IntegralByteMemory.Clear(
            destination.DataPtr,
            checked((nuint)destination.Length));
    }

    public static void ClearChecked(in IntegralSpan destination)
    {
        destination.Validate();
        Clear(destination);
    }

    /// <summary>
    /// Scalar values in complete blocks shared by both spans. Trailing values excluded.
    /// When block capacities differ, one must be multiple of other. The calculation rounds
    /// the smaller value count _down_ to the multiple of larger capacity.
    /// </summary>
    public static long CountBlockCompleteValues(
        in IntegralSpan source,
        in IntegralSpan destination)
    {
        long sourceValues =
            source.BlockCount * source.Capacity.BlockCapacity;
        long destinationValues =
            destination.BlockCount * destination.Capacity.BlockCapacity;

        long valuesLow = Math.Min(sourceValues, destinationValues);
        if (valuesLow <= 0)
        {
            return 0;
        }

        long capHigh = Math.Max(source.Capacity.BlockCapacity, destination.Capacity.BlockCapacity);
        long countHigh = valuesLow / capHigh;
        return countHigh * capHigh;
    }

    private static void PrepareStridedTrusted(
        in IntegralSpan source,
        long sourceValueOffset,
        long sourceValueStride,
        in IntegralSpan destination,
        long destinationValueOffset,
        long destinationValueStride,
        long valueCount,
        out byte* sourcePtr,
        out long sourceByteStride,
        out byte* destinationPtr,
        out long destinationByteStride)
    {
        long sourceByteOffset = checked(
            sourceValueOffset * source.Capacity.ValueByteCount);
        sourceByteStride = valueCount > 1
            ? checked(sourceValueStride * source.Capacity.ValueByteCount)
            : 0;
        long destinationByteOffset = checked(
            destinationValueOffset * destination.Capacity.ValueByteCount);
        destinationByteStride = valueCount > 1
            ? checked(destinationValueStride * destination.Capacity.ValueByteCount)
            : 0;

        sourcePtr = source.DataPtr + sourceByteOffset;
        destinationPtr = destination.DataPtr + destinationByteOffset;
    }

    private static void PrepareStridedChecked(
        in IntegralSpan source,
        long sourceValueOffset,
        long sourceValueStride,
        in IntegralSpan destination,
        long destinationValueOffset,
        long destinationValueStride,
        long valueCount,
        out byte* sourcePtr,
        out long sourceByteStride,
        out byte* destinationPtr,
        out long destinationByteStride)
    {
        ValidateStridedRange(
            sourceValueOffset,
            sourceValueStride,
            valueCount,
            source.ValueCount,
            nameof(sourceValueOffset),
            nameof(sourceValueStride));
        ValidateStridedRange(
            destinationValueOffset,
            destinationValueStride,
            valueCount,
            destination.ValueCount,
            nameof(destinationValueOffset),
            nameof(destinationValueStride));

        PrepareStridedTrusted(
            source,
            sourceValueOffset,
            sourceValueStride,
            destination,
            destinationValueOffset,
            destinationValueStride,
            valueCount,
            out sourcePtr,
            out sourceByteStride,
            out destinationPtr,
            out destinationByteStride);

        if (valueCount > 0 &&
            StridedRangesOverlap(
                sourcePtr,
                sourceByteStride,
                source.Capacity.ValueByteCount,
                destinationPtr,
                destinationByteStride,
                destination.Capacity.ValueByteCount,
                valueCount))
        {
            throw new ArgumentException(
                "Strided transfer does not permit overlapping touched ranges.");
        }
    }

    private static void ValidateValueCount(
        in IntegralSpan source,
        in IntegralSpan destination,
        long valueCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(valueCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            valueCount,
            source.ValueCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            valueCount,
            destination.ValueCount);
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

    private static bool HasSameLayout(
        in IntegralSpan source,
        in IntegralSpan destination)
    {
        return
            source.IntegralValueType == destination.IntegralValueType &&
            source.Capacity.BlockCapacity == destination.Capacity.BlockCapacity &&
            source.Capacity.ValueByteCount == destination.Capacity.ValueByteCount;
    }

    private static bool HasSameEndian(
        in IntegralSpan source,
        in IntegralSpan destination)
    {
        return source.Format.ByteOrder.Resolve() ==
               destination.Format.ByteOrder.Resolve();
    }

    private static bool HasOppositeEndian(
        in IntegralSpan source,
        in IntegralSpan destination)
    {
        return !HasSameEndian(source, destination);
    }

    private static void RequireSameLayout(
        in IntegralSpan source,
        in IntegralSpan destination)
    {
        if (!HasSameLayout(source, destination))
        {
            throw new ArgumentException(
                "Source and destination must share the same integral type and block capacity.");
        }
    }

    private static void RequireSameEndian(
        in IntegralSpan source,
        in IntegralSpan destination)
    {
        if (!HasSameEndian(source, destination))
        {
            throw new ArgumentException(
                "Source and destination must use the same resolved byte order. " +
                "Use ReverseCopy for endian-only transfer, or Convert for mixed formats.");
        }
    }

    private static void RequireOppositeEndian(
        in IntegralSpan source,
        in IntegralSpan destination)
    {
        if (!HasOppositeEndian(source, destination))
        {
            throw new ArgumentException(
                "Source and destination must use opposite resolved byte orders for reverse copy.");
        }
    }

    private static void ThrowIfRangesOverlap(
        byte* first,
        nuint firstByteCount,
        byte* second,
        nuint secondByteCount,
        string operationName)
    {
        if (RangesOverlap(first, firstByteCount, second, secondByteCount))
        {
            throw new ArgumentException(
                $"{operationName} does not permit overlapping source and destination ranges.");
        }
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

}
