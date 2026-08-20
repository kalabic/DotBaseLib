namespace DotBase.Buffers;


/// <summary>
/// Byte-oriented ring-buffer operations. Operational conditions such as closure,
/// insufficient data or space, and a valid request that cannot fit are reported by
/// the returned byte count rather than by an exception. Malformed arguments still throw.
/// </summary>
public interface IByteRingBuffer
{
    int ByteCapacity { get; }

    int FreeBytes { get; }

    int StoredBytes { get; }

    bool IsOpen { get; }

    void Advance(int count);

    void ClearBuffer();

    void Close();

    int Read(byte[] data, int offset, int count);

    unsafe int Read(byte* dataPtr, int offset, int count);

    int Write(byte[] data, int offset, int count);

    unsafe int Write(byte* data, int offset, int count);
}
