namespace DotBase.Buffers.Integral;


/// <summary>
/// Asynchronous Exact bulk operations. Each method waits until the complete request
/// can be transferred and otherwise reports an operational failure.
/// </summary>
public interface IBulkRingBufferAsync
{
    // T[] >>

    ValueTask<LongResult> ReadAsync<T>(T[] destination, int offset, int count)
        where T : unmanaged;

    ValueTask<LongResult> ReadExactAsync<T>(T[] destination, int offset, int count)
        where T : unmanaged;

    ValueTask<LongResult> WriteExactAsync<T>(T[] source, int offset, int count)
        where T : unmanaged;


    // T* >>

    unsafe ValueTask<LongResult> ReadAsync<T>(T* destination, int offset, int count)
        where T : unmanaged;

    unsafe ValueTask<LongResult> ReadExactAsync<T>(T* destination, int offset, int count)
        where T : unmanaged;

    unsafe ValueTask<LongResult> WriteExactAsync<T>(T* source, int offset, int count)
        where T : unmanaged;
}
