using DotBase.Core;
using DotBase.Integral;
using DotBase.Integral.Internal;

namespace DotBase.Buffers.Integral.Internal;


internal sealed class WaitableRingBuffer<TEndian> :
    DisposableBase,
    IWaitableRingBuffer
    where TEndian : struct, IEndianCodec
{
    private readonly object _lock = new();
    private RingBufferStorage _storage;

    internal WaitableRingBuffer(int capacity)
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

    public int Read(in IntegralSpan destination)
    {
        lock (_lock)
        {
            int requiredByteCount =
                IntegralRingOperations<TEndian>.GetRequestedByteCount(
                    ref _storage,
                    destination,
                    nameof(destination));

            if (!_storage.IsOpen)
            {
                return 0;
            }

            WaitForBytes(
                requiredByteCount,
                nameof(destination));

            return _storage.IsOpen
                ? IntegralRingOperations<TEndian>.Read(
                    ref _storage,
                    destination)
                : 0;
        }
    }

    public bool TryRead(in IntegralSpan destination)
    {
        lock (_lock)
        {
            return IntegralRingOperations<TEndian>.TryRead(
                ref _storage,
                destination);
        }
    }

    public int Write(in IntegralSpan source)
    {
        lock (_lock)
        {
            int count = IntegralRingOperations<TEndian>.Write(
                ref _storage,
                source);

            if (count > 0)
            {
                Monitor.PulseAll(_lock);
            }

            return count;
        }
    }

    public bool TryWrite(in IntegralSpan source)
    {
        lock (_lock)
        {
            bool completed =
                IntegralRingOperations<TEndian>.TryWrite(
                    ref _storage,
                    source);

            if (completed && source.IntegralLength > 0)
            {
                Monitor.PulseAll(_lock);
            }

            return completed;
        }
    }

    public int Read(byte[] data, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(data);
        return ReadBytes(data.AsSpan(offset, count));
    }

    public unsafe int Read(byte* data, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(data, offset, count, nameof(data));
        return ReadBytes(data + offset, count);
    }

    public int Write(byte[] data, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(data);
        return WriteBytes(data.AsSpan(offset, count));
    }

    public unsafe int Write(byte* data, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(data, offset, count, nameof(data));
        return WriteBytes(data + offset, count);
    }

    public T Read<T>()
        where T : unmanaged
    {
        IntegralCodec<T, TEndian>.Validate();
        int requiredBytes = IntegralCodec<T, TEndian>.Size;

        lock (_lock)
        {
            WaitForBytes(requiredBytes, nameof(T));

            if (_storage.IsOpen &&
                IntegralRingOperations<TEndian>.TryReadScalar(ref _storage, out T value))
            {
                return value;
            }
        }

        throw new InvalidOperationException(
            "The ring was closed before a complete value became available.");
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
            if (!IntegralRingOperations<TEndian>.TryWriteScalar(ref _storage, value))
            {
                throw new InvalidOperationException(
                    "The ring does not have enough free capacity for the requested value.");
            }

            Monitor.PulseAll(_lock);
        }
    }

    public bool TryWrite<T>(T value)
        where T : unmanaged
    {
        lock (_lock)
        {
            bool completed = IntegralRingOperations<TEndian>.TryWriteScalar(
                ref _storage,
                value);

            if (completed)
            {
                Monitor.PulseAll(_lock);
            }

            return completed;
        }
    }

    public int Read<T>(T[] destination, int offset, int count)
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(destination);
        return ReadValues<T>(destination.AsSpan(offset, count));
    }

    public unsafe int Read<T>(T* destination, int offset, int count)
        where T : unmanaged
    {
        IntegralBufferGuards.ValidatePointer(
            destination,
            offset,
            count,
            nameof(destination));

        return ReadValues(new Span<T>(destination + offset, count));
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
            bool completed = IntegralRingOperations<TEndian>.TryWrite(
                ref _storage,
                values);

            if (completed && !values.IsEmpty)
            {
                Monitor.PulseAll(_lock);
            }

            return completed;
        }
    }

    public unsafe bool TryWrite<T>(T* source, int offset, int count)
        where T : unmanaged
    {
        IntegralBufferGuards.ValidatePointer(source, offset, count, nameof(source));
        ReadOnlySpan<T> values = new(source + offset, count);

        lock (_lock)
        {
            bool completed = IntegralRingOperations<TEndian>.TryWrite(
                ref _storage,
                values);

            if (completed && !values.IsEmpty)
            {
                Monitor.PulseAll(_lock);
            }

            return completed;
        }
    }

    public bool TryWrite<T>(ReadOnlySpan<T> source)
        where T : unmanaged
    {
        lock (_lock)
        {
            bool completed = IntegralRingOperations<TEndian>.TryWrite(
                ref _storage,
                source);

            if (completed && !source.IsEmpty)
            {
                Monitor.PulseAll(_lock);
            }

            return completed;
        }
    }

    public int Read<T>(Span<T> destination)
        where T : unmanaged
    {
        return ReadValues<T>(destination);
    }

    public int Write<T>(ReadOnlySpan<T> source)
        where T : unmanaged
    {
        lock (_lock)
        {
            int count = IntegralRingOperations<TEndian>.Write(ref _storage, source);

            if (count > 0)
            {
                Monitor.PulseAll(_lock);
            }

            return count;
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
            Monitor.PulseAll(_lock);
        }
    }

    private int ReadBytes(Span<byte> destination)
    {
        lock (_lock)
        {
            WaitForBytes(destination.Length, nameof(destination));
            return _storage.IsOpen ? _storage.Read(destination) : 0;
        }
    }

    private int WriteBytes(ReadOnlySpan<byte> source)
    {
        lock (_lock)
        {
            int count = _storage.Write(source);
            if (count > 0)
            {
                Monitor.PulseAll(_lock);
            }

            return count;
        }
    }

    private unsafe int ReadBytes(
        byte* destination,
        int byteCount)
    {
        lock (_lock)
        {
            WaitForBytes(byteCount, nameof(destination));
            return _storage.IsOpen
                ? _storage.Read(destination, byteCount)
                : 0;
        }
    }

    private unsafe int WriteBytes(
        byte* source,
        int byteCount)
    {
        lock (_lock)
        {
            int count = _storage.Write(source, byteCount);
            if (count > 0)
            {
                Monitor.PulseAll(_lock);
            }

            return count;
        }
    }

    private int ReadValues<T>(Span<T> destination)
        where T : unmanaged
    {
        IntegralCodec<T, TEndian>.Validate();
        int requiredBytes = checked(destination.Length * IntegralCodec<T, TEndian>.Size);

        lock (_lock)
        {
            WaitForBytes(requiredBytes, nameof(destination));

            return _storage.IsOpen
                ? IntegralRingOperations<TEndian>.Read(ref _storage, destination)
                : 0;
        }
    }

    private void WaitForBytes(int requiredBytes, string parameterName)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            requiredBytes,
            _storage.Capacity,
            parameterName);

        while (_storage.IsOpen && _storage.Count < requiredBytes)
        {
            Monitor.Wait(_lock);
        }
    }
}
