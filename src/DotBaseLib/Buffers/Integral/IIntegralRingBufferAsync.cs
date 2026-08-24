using DotBase.Integral;

namespace DotBase.Buffers.Integral;


public interface IIntegralRingBufferAsync 
    : IDisposable
    , IByteRingBufferAsync
    , IBulkRingBufferAsync
{
    ValueTask<LongResult> ReadAsync(IntegralSpan destination);

    ValueTask<LongResult> ReadExactAsync(IntegralSpan destination);

    ValueTask<LongResult> WriteExactAsync(IntegralSpan source);
}
