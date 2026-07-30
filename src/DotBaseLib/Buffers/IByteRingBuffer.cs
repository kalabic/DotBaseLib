namespace DotBase.Buffers;


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
