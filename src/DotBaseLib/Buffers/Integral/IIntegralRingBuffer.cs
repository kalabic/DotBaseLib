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

    int CapacityOf<T>()
        where T : unmanaged;

    int CountOf<T>()
        where T : unmanaged;

    void AdvanceBy<T>(int count)
        where T : unmanaged;
}
