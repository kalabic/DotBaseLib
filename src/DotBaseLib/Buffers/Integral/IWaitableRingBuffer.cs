using DotBase.Integral;

namespace DotBase.Buffers.Integral;


/// <summary>
/// Identifies an integral ring buffer that supports waiting Exact operations.
/// Ordinary reads and writes are partial and never wait. Try operations are immediate
/// and atomic. Exact operations wait until the complete request can be transferred.
/// An Exact request that cannot fit returns <c>0</c> or <see langword="false"/>
/// immediately; closure, endpoint completion, or abort before fulfillment returns the
/// same failure status without a partial transfer.
/// </summary>
public interface IWaitableRingBuffer 
    : IIntegralRingBuffer
    , IIntegralRingBufferAsync
{
    event EventHandler<BufferReadingCompleted>? ReadingCompleted;

    event EventHandler<BufferWritingCompleted>? WritingCompleted;

    /// <summary>Gets whether the producer has completed writing normally.</summary>
    bool IsWritingCompleted { get; }

    /// <summary>Gets whether the consumer has completed reading normally.</summary>
    bool IsReadingCompleted { get; }

    /// <summary>
    /// Gets whether writing completed normally and no buffered bytes remain.
    /// </summary>
    bool IsDrained { get; }

    /// <summary>Gets whether the ring was aborted.</summary>
    bool IsAborted { get; }

    /// <summary>Gets the error supplied to the first abort, if any.</summary>
    Exception? AbortError { get; }

    /// <summary>
    /// Idempotently completes writing. Buffered data remains available to partial reads,
    /// and subsequent writes fail with their normal status result.
    /// </summary>
    /// <returns> Total number of bytes written into buffer. </returns>
    long CompleteWriting();

    /// <summary>
    /// Idempotently completes reading, discards buffered data, and prevents further
    /// reads and writes without closing native storage.
    /// </summary>
    /// <returns> Total number of bytes read from buffer. </returns>
    long CompleteReading();

    /// <summary>
    /// Idempotently aborts the ring, discards buffered data, and retains the error from
    /// the first abort without closing native storage.
    /// </summary>
    void Abort(Exception? error = null);

    ValueTask<LongResult> WaitForStoredBytesAsync(long required);

    ValueTask<LongResult> WaitForFreeBytesAsync(long required);

    // byte[] >>

    int ReadExact(byte[] destination, int offset, int count);

    int WriteExact(byte[] source, int offset, int count);


    // byte* >>

    unsafe int ReadExact(byte* destination, int offset, int count);

    unsafe int WriteExact(byte* source, int offset, int count);


    // Scalar value >>

    bool ReadExact<T>(out T value)
        where T : unmanaged;

    bool WriteExact<T>(T value)
        where T : unmanaged;


    // Array[] >>

    int ReadExact<T>(T[] destination, int offset, int count)
        where T : unmanaged;

    int WriteExact<T>(T[] source, int offset, int count)
        where T : unmanaged;


    // T* >>

    unsafe int ReadExact<T>(T* destination, int offset, int count)
        where T : unmanaged;

    unsafe int WriteExact<T>(T* source, int offset, int count)
        where T : unmanaged;


    // Span >>

    int ReadExact<T>(Span<T> destination)
        where T : unmanaged;

    int WriteExact<T>(ReadOnlySpan<T> source)
        where T : unmanaged;


    // IntegralSpan >>

    int ReadExact(in IntegralSpan destination);

    int ReadExactChecked(in IntegralSpan destination);

    int WriteExact(in IntegralSpan source);

    int WriteExactChecked(in IntegralSpan source);
}
