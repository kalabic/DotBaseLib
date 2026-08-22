namespace DotBase.Buffers.Integral;


/// <summary>
/// Identifies an integral ring buffer whose non-Try read and write operations wait until
/// the complete request is available (stored bytes for reads, free bytes for writes).
/// A request that cannot fit returns <c>0</c> or <see langword="false"/> immediately;
/// closure, reader completion, or abort before fulfillment returns the same failure
/// status. Writer completion permits final partial non-Try reads. Try operations never
/// wait or become partial.
/// </summary>
public interface IWaitableRingBuffer 
    : IIntegralRingBuffer
    , IBulkRingBufferAsync
    , IUnsafeBulkRingBufferAsync
{
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
    /// Idempotently completes writing. Buffered data remains readable, final non-Try
    /// reads may be partial, and subsequent writes fail with their normal status result.
    /// </summary>
    void CompleteWriting();

    /// <summary>
    /// Idempotently completes reading, discards buffered data, and prevents further
    /// reads and writes without closing native storage.
    /// </summary>
    void CompleteReading();

    /// <summary>
    /// Idempotently aborts the ring, discards buffered data, and retains the error from
    /// the first abort without closing native storage.
    /// </summary>
    void Abort(Exception? error = null);
}
