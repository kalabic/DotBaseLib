namespace DotBase.Buffers.Integral;


/// <summary>
/// Generic bulk operations. Transfer as many complete values as immediately fit.
/// Operational failure returns <c>0</c>.
/// </summary>
public interface IBulkRingBuffer : IByteRingBuffer
{
    int Read<T>(T[] destination, int offset, int count)
        where T : unmanaged;

    unsafe int Read<T>(T* destination, int offset, int count)
        where T : unmanaged;

    int Write<T>(T[] source, int offset, int count)
        where T : unmanaged;

    unsafe int Write<T>(T* source, int offset, int count)
        where T : unmanaged;
}
