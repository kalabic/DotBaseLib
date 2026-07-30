using DotBase.Integral;

namespace DotBase.Buffers.Integral;


public interface IIntegralRingBuffer :
    IDisposable,
    IByteRingBuffer,
    IAtomicBulkRingBuffer,
    IBulkRingBuffer,
    IScalarRingBuffer,
    ISpanRingBuffer
{
    ByteOrder ByteOrder { get; }

    int CapacityAs<T>()
        where T : unmanaged;

    int FreeCount<T>()
        where T : unmanaged;

    int StoredCount<T>()
        where T : unmanaged;

    void AdvanceBy<T>(int count)
        where T : unmanaged;

    int Read(in IntegralSpan destination);

    bool TryRead(in IntegralSpan destination);

    int Write(in IntegralSpan source);

    bool TryWrite(in IntegralSpan source);
}
