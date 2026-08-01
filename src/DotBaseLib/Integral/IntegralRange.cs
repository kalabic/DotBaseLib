namespace DotBase.Integral;


/// <summary>
/// Half-open range into a parent integral region, counted strictly in that
/// parent's <b>blocks</b> (same units as
/// <see cref="IntegralSpan.GetBlockSpan(long, long)"/>).
/// <para>
/// <b>Not</b> a byte range and <b>not</b> a scalar-value range.
/// <see cref="BlockOffset"/> / <see cref="BlockCount"/> are never measured in
/// bytes or in integral value counts, except when the parent happens to use
/// one-byte values and block capacity 1 (so one block equals one byte equals
/// one value). Retyping the region after a slice does not change how this
/// range is interpreted: units always stay the <b>parent</b> block geometry.
/// </para>
/// <para>
/// <see cref="BlockByteSize"/> is the parent's block size in bytes at range
/// creation (<see cref="IntegralCapacity.BlockByteCount"/>). Together with the
/// block indices it yields a contiguous byte interval for raw memcpy without
/// building a subspan.
/// </para>
/// </summary>
public readonly struct IntegralRange
{
    public static readonly IntegralRange Empty = default;

    /// <summary>
    /// Start index in <b>parent blocks</b> (not bytes, not values).
    /// </summary>
    public readonly long BlockOffset;

    /// <summary>
    /// Number of complete <b>parent blocks</b> (not bytes, not values).
    /// </summary>
    public readonly long BlockCount;

    /// <summary>
    /// Size in bytes of one parent block when this range was created
    /// (<c>ValueByteCount × BlockCapacity</c> of the parent).
    /// </summary>
    public readonly long BlockByteSize;

    public IntegralRange(long blockOffset, long blockCount, long blockByteSize)
    {
        BlockOffset = blockOffset;
        BlockCount = blockCount;
        BlockByteSize = blockByteSize;
    }

    public bool IsEmpty => BlockCount <= 0;

    /// <summary>Byte offset of the range start within the parent region.</summary>
    public long ByteOffset => checked(BlockOffset * BlockByteSize);

    /// <summary>Byte length of the full range within the parent region.</summary>
    public long ByteLength => checked(BlockCount * BlockByteSize);
}
