namespace DotBase.Buffers.Await;


public interface IAwaitByteStream
{
    LongResult WaitForStoredBytes(long byteCount = 1);

    LongResult WaitForFreeBytes(long byteCount = 1);

    ValueTask<LongResult> WaitForStoredBytesAsync(long byteCount = 1);

    ValueTask<LongResult> WaitForFreeBytesAsync(long byteCount = 1);
}
