namespace DotBase.Buffers.Integral;


/// <summary>
/// Generic span operations. Unlocked and locked implementations are partial and
/// nonblocking. Waitable implementations wait for the entire request only when it can
/// fit; otherwise they return <c>0</c>.
/// </summary>
public interface ISpanRingBuffer : IByteRingBuffer
{
    int Read<T>(Span<T> destination)
        where T : unmanaged;

    int Write<T>(ReadOnlySpan<T> source)
        where T : unmanaged;
}
