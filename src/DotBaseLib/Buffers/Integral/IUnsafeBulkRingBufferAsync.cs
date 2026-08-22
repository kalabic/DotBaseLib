namespace DotBase.Buffers.Integral;

public interface IUnsafeBulkRingBufferAsync
{
    unsafe ValueTask<LongResult> ReadAsync<T>(T* destination, int offset, int count)
        where T : unmanaged;

    unsafe ValueTask<LongResult> WriteAsync<T>(T* source, int offset, int count)
        where T : unmanaged;
}
