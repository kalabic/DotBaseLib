using DotBase.Integral;

namespace DotBase.Buffers.Integral;


public interface IIntegralRingBuffer :
    IDisposable,
    IByteRingBuffer,
    IAtomicBulkRingBuffer,
    IBulkRingBuffer,
    IScalarRingBuffer,
    ISpanRingBuffer
{
    ByteOrder ByteOrder { get; }

    int CapacityAs<T>()
        where T : unmanaged;

    int FreeCount<T>()
        where T : unmanaged;

    int StoredCount<T>()
        where T : unmanaged;

    void AdvanceBy<T>(int count)
        where T : unmanaged;

    /// <summary>
    /// <para>
    /// <b>Partial, block-complete read (trusted).</b>
    /// Moves as many <b>complete blocks</b> as fit from the ring into
    /// <paramref name="destination"/> (trailing values on the span are never filled).
    /// </para>
    /// <para>
    /// No format/geometry validation — use <see cref="ReadChecked"/> for untrusted spans.
    /// Closed ring or no complete block available → <c>0</c>.
    /// </para>
    /// </summary>
    /// <returns>Count of scalar values transferred (always a multiple of block capacity).</returns>
    int Read(in IntegralSpan destination);

    /// <summary>
    /// <para>
    /// <b>Atomic, block-complete read (trusted).</b>
    /// Fills <b>all</b> complete blocks of <paramref name="destination"/>, or leaves
    /// the ring unchanged and returns <c>false</c>. Trailing values on the span are
    /// never part of the requirement or the transfer.
    /// </para>
    /// <para>
    /// No format/geometry validation — use <see cref="TryReadChecked"/> for untrusted spans.
    /// </para>
    /// </summary>
    /// <returns><c>true</c> if every complete block of the destination was filled.</returns>
    bool TryRead(in IntegralSpan destination);

    /// <summary>
    /// <para>
    /// <b>Partial, block-complete write (trusted).</b>
    /// Moves as many <b>complete blocks</b> as fit from <paramref name="source"/> into
    /// the ring (trailing values on the span are never written).
    /// </para>
    /// <para>
    /// No format/geometry validation — use <see cref="WriteChecked"/> for untrusted spans.
    /// Closed ring or no complete block free → <c>0</c>.
    /// </para>
    /// </summary>
    /// <returns>Count of scalar values transferred (always a multiple of block capacity).</returns>
    int Write(in IntegralSpan source);

    /// <summary>
    /// <para>
    /// <b>Atomic, block-complete write (trusted).</b>
    /// Writes <b>all</b> complete blocks of <paramref name="source"/>, or leaves
    /// the ring unchanged and returns <c>false</c>. Trailing values on the span are
    /// never part of the requirement or the transfer.
    /// </para>
    /// <para>
    /// No format/geometry validation — use <see cref="TryWriteChecked"/> for untrusted spans.
    /// </para>
    /// </summary>
    /// <returns><c>true</c> if every complete block of the source was written.</returns>
    bool TryWrite(in IntegralSpan source);

    /// <summary>
    /// Validates <paramref name="destination"/> (format, geometry, size vs ring capacity),
    /// then same policy as <see cref="Read"/>.
    /// </summary>
    int ReadChecked(in IntegralSpan destination);

    /// <summary>
    /// Validates <paramref name="destination"/>, then same policy as <see cref="TryRead"/>.
    /// </summary>
    bool TryReadChecked(in IntegralSpan destination);

    /// <summary>
    /// Validates <paramref name="source"/>, then same policy as <see cref="Write"/>.
    /// </summary>
    int WriteChecked(in IntegralSpan source);

    /// <summary>
    /// Validates <paramref name="source"/>, then same policy as <see cref="TryWrite"/>.
    /// </summary>
    bool TryWriteChecked(in IntegralSpan source);
}
