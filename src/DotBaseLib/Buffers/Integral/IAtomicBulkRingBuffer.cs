namespace DotBase.Buffers.Integral;


/// <summary>
/// Immediate, atomic bulk operations. Each method transfers the complete request or
/// returns <see langword="false"/> without mutation.
/// </summary>
public interface IAtomicBulkRingBuffer : IBulkRingBuffer
{
    bool TryRead<T>(Span<T> destination)
        where T : unmanaged;

    bool TryRead<T>(T[] destination, int offset, int count)
        where T : unmanaged;

    unsafe bool TryRead<T>(T* destination, int offset, int count)
        where T : unmanaged;

    bool TryWrite<T>(ReadOnlySpan<T> source)
        where T : unmanaged;

    bool TryWrite<T>(T[] source, int offset, int count)
        where T : unmanaged;

    unsafe bool TryWrite<T>(T* source, int offset, int count)
        where T : unmanaged;
}
