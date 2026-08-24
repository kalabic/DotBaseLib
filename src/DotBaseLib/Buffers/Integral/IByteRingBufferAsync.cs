namespace DotBase.Buffers.Integral;


/// <summary>
/// Partial reads return immediately. Waits asynchronously if buffer is empty.
/// </summary>
public interface IByteRingBufferAsync
{
    // byte[] >>

    ValueTask<LongResult> ReadAsync(byte[] data, int offset, int count);

    ValueTask<LongResult> ReadExactAsync(byte[] destination, int offset, int count);

    ValueTask<LongResult> WriteExactAsync(byte[] source, int offset, int count);


    // byte* >>

    unsafe ValueTask<LongResult> ReadAsync(byte* dataPtr, int offset, int count);

    unsafe ValueTask<LongResult> ReadExactAsync(byte* destination, int offset, int count);

    unsafe ValueTask<LongResult> WriteExactAsync(byte* source, int offset, int count);
}
