using DotBase.Buffers.Integral;

namespace DotBase.Buffers.Await;


public interface IAwaitBlockStream
{
    /// <summary>
    /// Block size is defined by <see cref="IIntegralRingBuffer.Format"/>
    /// </summary>
    /// <param name="blockCount"> Count of complete blocks of data. </param>
    /// <returns></returns>
    LongResult WaitForStoredBlock(long blockCount = 1);

    /// <summary>
    /// Block size is defined by <see cref="IIntegralRingBuffer.Format"/>
    /// </summary>
    /// <param name="blockCount"> Count of complete blocks of data. </param>
    /// <returns></returns>
    LongResult WaitForFreeBlock(long blockCount = 1);

    /// <summary>
    /// Block size is defined by <see cref="IIntegralRingBuffer.Format"/>
    /// </summary>
    /// <param name="blockCount"> Count of complete blocks of data. </param>
    /// <returns></returns>
    ValueTask<LongResult> WaitForStoredBlockAsync(long blockCount = 1);

    /// <summary>
    /// Block size is defined by <see cref="IIntegralRingBuffer.Format"/>
    /// </summary>
    /// <param name="blockCount"> Count of complete blocks of data. </param>
    /// <returns></returns>
    ValueTask<LongResult> WaitForFreeBlockAsync(long blockCount = 1);
}
