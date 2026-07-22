namespace DotBase.Buffers.Integral;


public interface IScalarRingBuffer : IByteRingBuffer
{
    T Read<T>()
        where T : unmanaged;

    bool TryRead<T>(out T value)
        where T : unmanaged;

    void Write<T>(T value)
        where T : unmanaged;

    bool TryWrite<T>(T value)
        where T : unmanaged;
}
