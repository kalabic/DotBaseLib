using DotBase.Integral;

namespace DotBase.Buffers.Integral;


public interface IIntegralRingBufferAsync 
    : IDisposable
    , IByteRingBufferAsync
    , IBulkRingBufferAsync
{
    IntegralFormat Format { get; }

    ValueTask<LongResult> ReadAsync(IntegralSpan destination);

    ValueTask<LongResult> ReadExactAsync(IntegralSpan destination);

    ValueTask<LongResult> WriteExactAsync(IntegralSpan source);


    // Public wait APIs:

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
