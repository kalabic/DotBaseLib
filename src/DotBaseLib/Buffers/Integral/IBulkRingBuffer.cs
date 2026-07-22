namespace DotBase.Buffers.Integral;


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
