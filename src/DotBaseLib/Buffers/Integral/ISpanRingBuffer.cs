namespace DotBase.Buffers.Integral;


/// <summary>
/// Partial, non-waiting generic span operations. Each method transfers as many
/// complete values as immediately fit and returns <c>0</c> on operational failure.
/// </summary>
public interface ISpanRingBuffer : IByteRingBuffer
{
    int Read<T>(Span<T> destination)
        where T : unmanaged;

    int Write<T>(ReadOnlySpan<T> source)
        where T : unmanaged;
}
