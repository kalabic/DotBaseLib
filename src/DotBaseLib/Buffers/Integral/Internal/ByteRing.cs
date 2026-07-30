using DotBase.Core;
using DotBase.Integral;
using DotBase.Integral.Internal;

namespace DotBase.Buffers.Integral.Internal;


internal sealed class ByteRing<TEndian> : DisposableBase, IIntegralRingBuffer
    where TEndian : struct, IEndianCodec
{
    private RingBufferStorage _storage;

    internal ByteRing(int capacity)
    {
        _storage = new RingBufferStorage(capacity);
    }

    public ByteOrder ByteOrder => IntegralRingOperations<TEndian>.ByteOrder;

    public int Capacity => _storage.Capacity;

    public int Count => _storage.Count;

    public bool IsOpen => _storage.IsOpen;

    public long TotalRead => _storage.TotalRead;

    public long TotalWritten => _storage.TotalWritten;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Close();
        }

        base.Dispose(disposing);
    }

    public int CapacityOf<T>()
        where T : unmanaged
    {
        return IntegralRingOperations<TEndian>.CapacityOf<T>(ref _storage);
    }

    public int CountOf<T>()
        where T : unmanaged
    {
        return IntegralRingOperations<TEndian>.CountOf<T>(ref _storage);
    }

    public void AdvanceBy<T>(int count)
        where T : unmanaged
    {
        IntegralRingOperations<TEndian>.AdvanceBy<T>(ref _storage, count);
    }

    public int Read(in IntegralSpan destination)
    {
        return IntegralRingOperations<TEndian>.Read(
            ref _storage,
            destination);
    }

    public bool TryRead(in IntegralSpan destination)
    {
        return IntegralRingOperations<TEndian>.TryRead(
            ref _storage,
            destination);
    }

    public int Write(in IntegralSpan source)
    {
        return IntegralRingOperations<TEndian>.Write(
            ref _storage,
            source);
    }

    public bool TryWrite(in IntegralSpan source)
    {
        return IntegralRingOperations<TEndian>.TryWrite(
            ref _storage,
            source);
    }

    public int Read(byte[] data, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(data);
        return _storage.Read(data.AsSpan(offset, count));
    }

    public unsafe int Read(byte* data, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(data, offset, count, nameof(data));
        return _storage.Read(data + offset, count);
    }

    public int Write(byte[] data, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(data);
        return _storage.Write(data.AsSpan(offset, count));
    }

    public unsafe int Write(byte* data, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(data, offset, count, nameof(data));
        return _storage.Write(data + offset, count);
    }

    public T Read<T>()
        where T : unmanaged
    {
        if (IntegralRingOperations<TEndian>.TryReadScalar(ref _storage, out T value))
        {
            return value;
        }

        throw new InvalidOperationException(
            "The ring does not contain a complete value of the requested type.");
    }

    public bool TryRead<T>(out T value)
        where T : unmanaged
    {
        return IntegralRingOperations<TEndian>.TryReadScalar(ref _storage, out value);
    }

    public void Write<T>(T value)
        where T : unmanaged
    {
        if (!IntegralRingOperations<TEndian>.TryWriteScalar(ref _storage, value))
        {
            throw new InvalidOperationException(
                "The ring does not have enough free capacity for the requested value.");
        }
    }

    public bool TryWrite<T>(T value)
        where T : unmanaged
    {
        return IntegralRingOperations<TEndian>.TryWriteScalar(ref _storage, value);
    }

    public int Read<T>(T[] destination, int offset, int count)
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(destination);
        return Read(destination.AsSpan(offset, count));
    }

    public unsafe int Read<T>(T* destination, int offset, int count)
        where T : unmanaged
    {
        IntegralBufferGuards.ValidatePointer(
            destination,
            offset,
            count,
            nameof(destination));

        return Read(new Span<T>(destination + offset, count));
    }

    public int Write<T>(T[] source, int offset, int count)
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(source);
        return Write<T>((ReadOnlySpan<T>)source.AsSpan(offset, count));
    }

    public unsafe int Write<T>(T* source, int offset, int count)
        where T : unmanaged
    {
        IntegralBufferGuards.ValidatePointer(source, offset, count, nameof(source));
        return Write(new ReadOnlySpan<T>(source + offset, count));
    }

    public bool TryRead<T>(T[] destination, int offset, int count)
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(destination);
        return IntegralRingOperations<TEndian>.TryRead(
            ref _storage,
            destination.AsSpan(offset, count));
    }

    public unsafe bool TryRead<T>(T* destination, int offset, int count)
        where T : unmanaged
    {
        IntegralBufferGuards.ValidatePointer(
            destination,
            offset,
            count,
            nameof(destination));

        return IntegralRingOperations<TEndian>.TryRead(
            ref _storage,
            new Span<T>(destination + offset, count));
    }

    public bool TryRead<T>(Span<T> destination)
        where T : unmanaged
    {
        return IntegralRingOperations<TEndian>.TryRead(ref _storage, destination);
    }

    public bool TryWrite<T>(T[] source, int offset, int count)
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(source);
        return IntegralRingOperations<TEndian>.TryWrite<T>(
            ref _storage,
            (ReadOnlySpan<T>)source.AsSpan(offset, count));
    }

    public unsafe bool TryWrite<T>(T* source, int offset, int count)
        where T : unmanaged
    {
        IntegralBufferGuards.ValidatePointer(source, offset, count, nameof(source));
        return IntegralRingOperations<TEndian>.TryWrite(
            ref _storage,
            new ReadOnlySpan<T>(source + offset, count));
    }

    public bool TryWrite<T>(ReadOnlySpan<T> source)
        where T : unmanaged
    {
        return IntegralRingOperations<TEndian>.TryWrite(ref _storage, source);
    }

    public int Read<T>(Span<T> destination)
        where T : unmanaged
    {
        return IntegralRingOperations<TEndian>.Read(ref _storage, destination);
    }

    public int Write<T>(ReadOnlySpan<T> source)
        where T : unmanaged
    {
        return IntegralRingOperations<TEndian>.Write(ref _storage, source);
    }

    public void Advance(int count)
    {
        _storage.Advance(count);
    }

    public void ClearBuffer()
    {
        _storage.Clear();
    }

    public void Close()
    {
        _storage.Close();
    }
}
