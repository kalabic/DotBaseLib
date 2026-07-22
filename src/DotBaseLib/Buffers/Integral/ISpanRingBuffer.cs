namespace DotBase.Buffers.Integral;


public interface ISpanRingBuffer : IByteRingBuffer
{
    int Read<T>(Span<T> destination)
        where T : unmanaged;

    int Write<T>(ReadOnlySpan<T> source)
        where T : unmanaged;
}
