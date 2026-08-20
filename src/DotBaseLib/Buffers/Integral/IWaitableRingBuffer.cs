namespace DotBase.Buffers.Integral;


/// <summary>
/// Identifies an integral ring buffer whose non-Try read and write operations wait until
/// the complete request is available (stored bytes for reads, free bytes for writes).
/// A request that cannot fit returns <c>0</c> or <see langword="false"/> immediately;
/// closure before fulfillment returns the same failure status. Try operations never wait.
/// </summary>
public interface IWaitableRingBuffer : IIntegralRingBuffer
{
}
