using DotBase.Core;

namespace DotBase.Buffers.Integral.Internal;


internal sealed class LockedRingBuffer<TEndian> : DisposableBase, IIntegralRingBuffer
    where TEndian : struct, IEndianCodec
{
    private readonly object _lock = new();
    private RingBufferStorage _storage;

    internal LockedRingBuffer(int capacity)
    {
        _storage = new RingBufferStorage(capacity);
    }

    public ByteOrder ByteOrder => IntegralRingOperations<TEndian>.ByteOrder;

    public int Capacity
    {
        get { lock (_lock) { return _storage.Capacity; } }
    }

    public int Count
    {
        get { lock (_lock) { return _storage.Count; } }
    }

    public bool IsOpen
    {
        get { lock (_lock) { return _storage.IsOpen; } }
    }

    public long TotalRead
    {
        get { lock (_lock) { return _storage.TotalRead; } }
    }

    public long TotalWritten
    {
        get { lock (_lock) { return _storage.TotalWritten; } }
    }

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
        lock (_lock)
        {
            return IntegralRingOperations<TEndian>.CapacityOf<T>(ref _storage);
        }
    }

    public int CountOf<T>()
        where T : unmanaged
    {
        lock (_lock)
        {
            return IntegralRingOperations<TEndian>.CountOf<T>(ref _storage);
        }
    }

    public void AdvanceBy<T>(int count)
        where T : unmanaged
    {
        lock (_lock)
        {
            IntegralRingOperations<TEndian>.AdvanceBy<T>(ref _storage, count);
        }
    }

    public int Read(byte[] data, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(data);
        Span<byte> destination = data.AsSpan(offset, count);

        lock (_lock)
        {
            return _storage.Read(destination);
        }
    }

    public unsafe int Read(byte* data, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(data, offset, count, nameof(data));
        Span<byte> destination = new(data + offset, count);

        lock (_lock)
        {
            return _storage.Read(destination);
        }
    }

    public int Write(byte[] data, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(data);
        ReadOnlySpan<byte> source = data.AsSpan(offset, count);

        lock (_lock)
        {
            return _storage.Write(source);
        }
    }

    public unsafe int Write(byte* data, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(data, offset, count, nameof(data));
        ReadOnlySpan<byte> source = new(data + offset, count);

        lock (_lock)
        {
            return _storage.Write(source);
        }
    }

    public T Read<T>()
        where T : unmanaged
    {
        lock (_lock)
        {
            if (IntegralRingOperations<TEndian>.TryReadScalar(ref _storage, out T value))
            {
                return value;
            }
        }

        throw new InvalidOperationException(
            "The ring does not contain a complete value of the requested type.");
    }

    public bool TryRead<T>(out T value)
        where T : unmanaged
    {
        lock (_lock)
        {
            return IntegralRingOperations<TEndian>.TryReadScalar(
                ref _storage,
                out value);
        }
    }

    public void Write<T>(T value)
        where T : unmanaged
    {
        lock (_lock)
        {
            if (IntegralRingOperations<TEndian>.TryWriteScalar(ref _storage, value))
            {
                return;
            }
        }

        throw new InvalidOperationException(
            "The ring does not have enough free capacity for the requested value.");
    }

    public bool TryWrite<T>(T value)
        where T : unmanaged
    {
        lock (_lock)
        {
            return IntegralRingOperations<TEndian>.TryWriteScalar(ref _storage, value);
        }
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
        Span<T> values = destination.AsSpan(offset, count);

        lock (_lock)
        {
            return IntegralRingOperations<TEndian>.TryRead(ref _storage, values);
        }
    }

    public unsafe bool TryRead<T>(T* destination, int offset, int count)
        where T : unmanaged
    {
        IntegralBufferGuards.ValidatePointer(
            destination,
            offset,
            count,
            nameof(destination));

        Span<T> values = new(destination + offset, count);
        lock (_lock)
        {
            return IntegralRingOperations<TEndian>.TryRead(ref _storage, values);
        }
    }

    public bool TryRead<T>(Span<T> destination)
        where T : unmanaged
    {
        lock (_lock)
        {
            return IntegralRingOperations<TEndian>.TryRead(ref _storage, destination);
        }
    }

    public bool TryWrite<T>(T[] source, int offset, int count)
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(source);
        ReadOnlySpan<T> values = source.AsSpan(offset, count);

        lock (_lock)
        {
            return IntegralRingOperations<TEndian>.TryWrite(ref _storage, values);
        }
    }

    public unsafe bool TryWrite<T>(T* source, int offset, int count)
        where T : unmanaged
    {
        IntegralBufferGuards.ValidatePointer(source, offset, count, nameof(source));
        ReadOnlySpan<T> values = new(source + offset, count);

        lock (_lock)
        {
            return IntegralRingOperations<TEndian>.TryWrite(ref _storage, values);
        }
    }

    public bool TryWrite<T>(ReadOnlySpan<T> source)
        where T : unmanaged
    {
        lock (_lock)
        {
            return IntegralRingOperations<TEndian>.TryWrite(ref _storage, source);
        }
    }

    public int Read<T>(Span<T> destination)
        where T : unmanaged
    {
        lock (_lock)
        {
            return IntegralRingOperations<TEndian>.Read(ref _storage, destination);
        }
    }

    public int Write<T>(ReadOnlySpan<T> source)
        where T : unmanaged
    {
        lock (_lock)
        {
            return IntegralRingOperations<TEndian>.Write(ref _storage, source);
        }
    }

    public void Advance(int count)
    {
        lock (_lock)
        {
            _storage.Advance(count);
        }
    }

    public void ClearBuffer()
    {
        lock (_lock)
        {
            _storage.Clear();
        }
    }

    public void Close()
    {
        lock (_lock)
        {
            _storage.Close();
        }
    }
}
