namespace DotBase.Buffers.Await;


public interface IAwaitValueStream
{
    LongResult WaitForStoredValues<T>(long valueCount = 1)
        where T : unmanaged;

    LongResult WaitForFreeValues<T>(long valueCount = 1)
        where T : unmanaged;

    ValueTask<LongResult> WaitForStoredValuesAsync<T>(long valueCount = 1)
        where T : unmanaged;

    ValueTask<LongResult> WaitForFreeValuesAsync<T>(long valueCount = 1)
        where T : unmanaged;
}
