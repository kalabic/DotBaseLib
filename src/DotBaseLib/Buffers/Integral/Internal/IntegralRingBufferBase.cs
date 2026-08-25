using DotBase.Core;
using DotBase.Integral;

namespace DotBase.Buffers.Integral.Internal;


internal abstract class IntegralRingBufferBase 
    : DisposableBase
    , IIntegralRingBuffer
{
    public IntegralFormat Format { get { return _format; } }

    protected readonly IntegralFormat _format;

    protected RingBufferStorage _storage;

    internal IntegralRingBufferBase(int capacity, IntegralFormat format)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        if (capacity > 0 && (!format.IsValid() || format.IsEmptyType()))
        {
            throw new ArgumentException("Invalid format.", nameof(format));
        }
        else if (capacity == 0 && !format.IsValid())
        {
            throw new ArgumentException("Invalid format.", nameof(format));
        }

        _format = format;
        _storage = new RingBufferStorage(capacity);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Close();
        }
        else
        {
            _storage.Close(); // Only minimal unmanaged release.
        }

        base.Dispose(disposing);
    }

    public abstract int ByteCapacity { get; }
    public abstract int FreeBytes { get; }
    public abstract int StoredBytes { get; }
    public abstract bool IsOpen { get; }
    public abstract ByteOrder ByteOrder { get; }
    public abstract void Advance(int count);
    public abstract void AdvanceBy<T>(int count) where T : unmanaged;
    public abstract int CapacityAsBlockCount();
    public abstract int CapacityAs<T>() where T : unmanaged;
    public abstract void ClearBuffer();
    public abstract void Close();
    public abstract int FreeCount<T>() where T : unmanaged;
    public abstract int Read(byte[] data, int offset, int count);
    public abstract unsafe int Read(byte* dataPtr, int offset, int count);
    public abstract int Read(in IntegralSpan destination);
    public abstract int Read<T>(T[] destination, int offset, int count) where T : unmanaged;
    public abstract unsafe int Read<T>(T* destination, int offset, int count) where T : unmanaged;
    public abstract bool Read<T>(out T value) where T : unmanaged;
    public abstract int Read<T>(Span<T> destination) where T : unmanaged;
    public abstract int ReadChecked(in IntegralSpan destination);
    public abstract int StoredCount<T>() where T : unmanaged;
    public abstract bool TryRead(in IntegralSpan destination);
    public abstract bool TryRead<T>(Span<T> destination) where T : unmanaged;
    public abstract bool TryRead<T>(T[] destination, int offset, int count) where T : unmanaged;
    public abstract unsafe bool TryRead<T>(T* destination, int offset, int count) where T : unmanaged;
    public abstract bool TryRead<T>(out T value) where T : unmanaged;
    public abstract bool TryReadChecked(in IntegralSpan destination);
    public abstract bool TryWrite(in IntegralSpan source);
    public abstract bool TryWrite<T>(ReadOnlySpan<T> source) where T : unmanaged;
    public abstract bool TryWrite<T>(T[] source, int offset, int count) where T : unmanaged;
    public abstract unsafe bool TryWrite<T>(T* source, int offset, int count) where T : unmanaged;
    public abstract bool TryWrite<T>(T value) where T : unmanaged;
    public abstract bool TryWriteChecked(in IntegralSpan source);
    public abstract int Write(byte[] data, int offset, int count);
    public abstract unsafe int Write(byte* data, int offset, int count);
    public abstract int Write(in IntegralSpan source);
    public abstract int Write<T>(T[] source, int offset, int count) where T : unmanaged;
    public abstract unsafe int Write<T>(T* source, int offset, int count) where T : unmanaged;
    public abstract bool Write<T>(T value) where T : unmanaged;
    public abstract int Write<T>(ReadOnlySpan<T> source) where T : unmanaged;
    public abstract int WriteChecked(in IntegralSpan source);
}
