using System.Runtime.CompilerServices;
using DotBase.AsyncValue;
using DotBase.Integral;
using DotBase.Tools;

namespace DotBase.Buffers.Integral.Internal;


internal abstract unsafe class WaitableRingBuffer
    : IntegralRingBufferBase
    , IWaitableRingBuffer
{
    protected readonly object _lock = new();

    protected readonly int _byteCapacity;

    private readonly WaitableHighLowMarkValue _storedByteCount;

    internal WaitableRingBuffer(int capacity)
        : base(capacity)
    {
        _byteCapacity = capacity;
        _storedByteCount = new WaitableHighLowMarkValue(0, capacity, 0);
        if (!_storage.IsOpen)
        {
            _storedByteCount.Close();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Close();
            _storedByteCount.Dispose();
        }
        else
        {
            _storage.Close();
        }

        base.Dispose(disposing);
    }

    public override int ByteCapacity
    {
        get { lock (_lock) { return _storage.ByteCapacity; } }
    }

    public override int FreeBytes
    {
        get { lock (_lock) { return _storage.FreeBytes; } }
    }

    public override int StoredBytes
    {
        get { lock (_lock) { return _storage.StoredBytes; } }
    }

    public override bool IsOpen
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

    public override int CapacityAs<T>()
    {
        lock (_lock)
        {
            return _storage.ByteCapacity / Unsafe.SizeOf<T>();
        }
    }

    public override int FreeCount<T>()
    {
        lock (_lock)
        {
            return _storage.FreeBytes / Unsafe.SizeOf<T>();
        }
    }

    public override int StoredCount<T>()
    {
        lock (_lock)
        {
            return _storage.StoredBytes / Unsafe.SizeOf<T>();
        }
    }

    public override void AdvanceBy<T>(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        lock (_lock)
        {
            _storage.Advance(
                checked((int)(
                    (long)count * Unsafe.SizeOf<T>())));
            PublishStoredLocked();
        }
    }

    public override void Advance(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        lock (_lock)
        {
            _storage.Advance(count);
            PublishStoredLocked();
        }
    }

    public override void ClearBuffer()
    {
        lock (_lock)
        {
            _storage.Clear();
            PublishStoredLocked();
        }
    }

    public override void Close()
    {
        lock (_lock)
        {
            _storage.Close();
            _storedByteCount.Close();
        }
    }

    public override int Read(in IntegralSpan destination)
    {
        return ReadSpan(destination, validate: false);
    }

    public override int ReadChecked(in IntegralSpan destination)
    {
        return ReadSpan(destination, validate: true);
    }

    public override bool TryRead(in IntegralSpan destination)
    {
        lock (_lock)
        {
            bool completed = TryReadIntegralSpan(destination);
            if (completed)
            {
                PublishStoredLocked();
            }

            return completed;
        }
    }

    public override bool TryReadChecked(in IntegralSpan destination)
    {
        lock (_lock)
        {
            bool completed = TryReadIntegralSpanChecked(destination);
            if (completed)
            {
                PublishStoredLocked();
            }

            return completed;
        }
    }

    public override int Write(in IntegralSpan source)
    {
        return WriteSpan(source, validate: false);
    }

    public override int WriteChecked(in IntegralSpan source)
    {
        return WriteSpan(source, validate: true);
    }

    public override bool TryWrite(in IntegralSpan source)
    {
        lock (_lock)
        {
            bool completed = TryWriteIntegralSpan(source);
            if (completed)
            {
                PublishStoredLocked();
            }

            return completed;
        }
    }

    public override bool TryWriteChecked(in IntegralSpan source)
    {
        lock (_lock)
        {
            bool completed = TryWriteIntegralSpanChecked(source);
            if (completed)
            {
                PublishStoredLocked();
            }

            return completed;
        }
    }

    public override int Read(byte[] data, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(data);
        return ReadBytes(data.AsSpan(offset, count));
    }

    public override int Read(byte* data, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(data, offset, count, nameof(data));
        return ReadBytes(data + offset, count);
    }

    public override int Write(byte[] data, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(data);
        return WriteBytes(data.AsSpan(offset, count));
    }

    public override int Write(byte* data, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(data, offset, count, nameof(data));
        return WriteBytes(data + offset, count);
    }

    public override bool Read<T>(out T value)
    {
        int n = Unsafe.SizeOf<T>();

        if (n > _byteCapacity)
        {
            value = default;
            return false;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!_storage.IsOpen)
                {
                    value = default;
                    return false;
                }

                if (_storage.StoredBytes >= n && TryReadScalar(out value))
                {
                    PublishStoredLocked();
                    return true;
                }
            }

            if (!ContinueAfterWait(WaitForStoredBytes(n)))
            {
                value = default;
                return false;
            }
        }
    }

    public override bool TryRead<T>(out T value)
    {
        lock (_lock)
        {
            bool completed = TryReadScalar(out value);
            if (completed)
            {
                PublishStoredLocked();
            }

            return completed;
        }
    }

    public override bool Write<T>(T value)
    {
        int n = Unsafe.SizeOf<T>();

        if (n > _byteCapacity)
        {
            return false;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!_storage.IsOpen)
                {
                    return false;
                }

                if (_storage.FreeBytes >= n && TryWriteScalar(value))
                {
                    PublishStoredLocked();
                    return true;
                }
            }

            if (!ContinueAfterWait(WaitForFreeBytes(n)))
            {
                return false;
            }
        }
    }

    public override bool TryWrite<T>(T value)
    {
        lock (_lock)
        {
            bool completed = TryWriteScalar(value);
            if (completed)
            {
                PublishStoredLocked();
            }

            return completed;
        }
    }

    public override int Read<T>(T[] destination, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(destination);
        return ReadValues(destination.AsSpan(offset, count));
    }

    public override int Read<T>(T* destination, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(
            destination,
            offset,
            count,
            nameof(destination));

        return ReadValues(new Span<T>(destination + offset, count));
    }

    public override int Write<T>(T[] source, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        return Write((ReadOnlySpan<T>)source.AsSpan(offset, count));
    }

    public override int Write<T>(T* source, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(source, offset, count, nameof(source));
        return Write(new ReadOnlySpan<T>(source + offset, count));
    }

    public override bool TryRead<T>(T[] destination, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(destination);
        return TryRead(destination.AsSpan(offset, count));
    }

    public override bool TryRead<T>(T* destination, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(
            destination,
            offset,
            count,
            nameof(destination));

        if (!CanFit((long)count * Unsafe.SizeOf<T>()))
        {
            return false;
        }

        lock (_lock)
        {
            bool completed = TryReadCore(destination + offset, count);
            if (completed)
            {
                PublishStoredLocked();
            }

            return completed;
        }
    }

    public override bool TryRead<T>(Span<T> destination)
    {
        if (destination.IsEmpty)
        {
            lock (_lock)
            {
                return _storage.IsOpen;
            }
        }

        if (!CanFit((long)destination.Length * Unsafe.SizeOf<T>()))
        {
            return false;
        }

        fixed (T* dst = destination)
        {
            lock (_lock)
            {
                bool completed = TryReadCore(dst, destination.Length);
                if (completed)
                {
                    PublishStoredLocked();
                }

                return completed;
            }
        }
    }

    public override bool TryWrite<T>(T[] source, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        return TryWrite((ReadOnlySpan<T>)source.AsSpan(offset, count));
    }

    public override bool TryWrite<T>(T* source, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(source, offset, count, nameof(source));

        if (!CanFit((long)count * Unsafe.SizeOf<T>()))
        {
            return false;
        }

        lock (_lock)
        {
            bool completed = TryWriteCore(source + offset, count);
            if (completed)
            {
                PublishStoredLocked();
            }

            return completed;
        }
    }

    public override bool TryWrite<T>(ReadOnlySpan<T> source)
    {
        if (source.IsEmpty)
        {
            lock (_lock)
            {
                return _storage.IsOpen;
            }
        }

        if (!CanFit((long)source.Length * Unsafe.SizeOf<T>()))
        {
            return false;
        }

        fixed (T* src = source)
        {
            lock (_lock)
            {
                bool completed = TryWriteCore(src, source.Length);
                if (completed)
                {
                    PublishStoredLocked();
                }

                return completed;
            }
        }
    }

    public override int Read<T>(Span<T> destination)
    {
        return ReadValues(destination);
    }

    public override int Write<T>(ReadOnlySpan<T> source)
    {
        return WriteValues(source);
    }

    protected abstract int ReadIntegralSpan(in IntegralSpan destination);

    protected abstract bool TryReadIntegralSpan(in IntegralSpan destination);

    protected abstract bool TryReadIntegralSpanChecked(in IntegralSpan destination);

    protected abstract int WriteIntegralSpan(in IntegralSpan source);

    protected abstract bool TryWriteIntegralSpan(in IntegralSpan source);

    protected abstract bool TryWriteIntegralSpanChecked(in IntegralSpan source);

    protected abstract bool TryReadScalar<T>(out T value) where T : unmanaged;

    protected abstract bool TryWriteScalar<T>(T value) where T : unmanaged;

    protected abstract int ReadCore<T>(T* destination, int count) where T : unmanaged;

    protected abstract bool TryReadCore<T>(T* destination, int count) where T : unmanaged;

    protected abstract int WriteCore<T>(T* source, int count) where T : unmanaged;

    protected abstract bool TryWriteCore<T>(T* source, int count) where T : unmanaged;

    private void PublishStoredLocked()
    {
        if (_storedByteCount.IsOpen)
        {
            _storedByteCount.SetValue(_storage.StoredBytes);
        }
    }

    private LongResult WaitForStoredBytes(long required)
    {
        return _storedByteCount.WaitHighMarkValue(required);
    }

    private LongResult WaitForFreeBytes(long required)
    {
        return _storedByteCount.WaitLowMarkValue(_byteCapacity - required);
    }

    private bool CanFit(long required) => required <= _byteCapacity;

    private static bool ContinueAfterWait(LongResult result) =>
        result.Status == ResultStatus.SUCCESS;

    private int ReadSpan(in IntegralSpan destination, bool validate)
    {
        if (validate)
        {
            IntegralRingSpanOps.ValidateSpan(
                destination,
                nameof(destination));
        }

        long required = IntegralRingSpanOps.BlockCompleteByteCount(destination);
        if (!CanFit(required))
        {
            return 0;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!_storage.IsOpen)
                {
                    return 0;
                }

                if (required == 0 || _storage.StoredBytes >= required)
                {
                    int count = ReadIntegralSpan(destination);
                    PublishStoredLocked();
                    return count;
                }
            }

            if (!ContinueAfterWait(WaitForStoredBytes(required)))
            {
                return 0;
            }
        }
    }

    private int WriteSpan(in IntegralSpan source, bool validate)
    {
        if (validate)
        {
            IntegralRingSpanOps.ValidateSpan(
                source,
                nameof(source));
        }

        long required = IntegralRingSpanOps.BlockCompleteByteCount(source);
        if (!CanFit(required))
        {
            return 0;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!_storage.IsOpen)
                {
                    return 0;
                }

                if (required == 0 || _storage.FreeBytes >= required)
                {
                    int count = WriteIntegralSpan(source);
                    PublishStoredLocked();
                    return count;
                }
            }

            if (!ContinueAfterWait(WaitForFreeBytes(required)))
            {
                return 0;
            }
        }
    }

    private int ReadBytes(Span<byte> destination)
    {
        int required = destination.Length;
        if (required == 0)
        {
            return 0;
        }

        if (!CanFit(required))
        {
            return 0;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!_storage.IsOpen)
                {
                    return 0;
                }

                if (_storage.StoredBytes >= required)
                {
                    int count = _storage.Read(destination);
                    PublishStoredLocked();
                    return count;
                }
            }

            if (!ContinueAfterWait(WaitForStoredBytes(required)))
            {
                return 0;
            }
        }
    }

    private int ReadBytes(byte* destination, int required)
    {
        if (required == 0)
        {
            return 0;
        }

        if (!CanFit(required))
        {
            return 0;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!_storage.IsOpen)
                {
                    return 0;
                }

                if (_storage.StoredBytes >= required)
                {
                    int count = _storage.Read(destination, required);
                    PublishStoredLocked();
                    return count;
                }
            }

            if (!ContinueAfterWait(WaitForStoredBytes(required)))
            {
                return 0;
            }
        }
    }

    private int WriteBytes(ReadOnlySpan<byte> source)
    {
        int required = source.Length;
        if (required == 0)
        {
            return 0;
        }

        if (!CanFit(required))
        {
            return 0;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!_storage.IsOpen)
                {
                    return 0;
                }

                if (_storage.FreeBytes >= required)
                {
                    int count = _storage.Write(source);
                    PublishStoredLocked();
                    return count;
                }
            }

            if (!ContinueAfterWait(WaitForFreeBytes(required)))
            {
                return 0;
            }
        }
    }

    private int WriteBytes(byte* source, int required)
    {
        if (required == 0)
        {
            return 0;
        }

        if (!CanFit(required))
        {
            return 0;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!_storage.IsOpen)
                {
                    return 0;
                }

                if (_storage.FreeBytes >= required)
                {
                    int count = _storage.Write(source, required);
                    PublishStoredLocked();
                    return count;
                }
            }

            if (!ContinueAfterWait(WaitForFreeBytes(required)))
            {
                return 0;
            }
        }
    }

    private int ReadValues<T>(Span<T> destination)
        where T : unmanaged
    {
        if (destination.IsEmpty)
        {
            return 0;
        }

        long requiredBytes = (long)destination.Length * Unsafe.SizeOf<T>();
        if (!CanFit(requiredBytes))
        {
            return 0;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!_storage.IsOpen)
                {
                    return 0;
                }

                if (_storage.StoredBytes >= requiredBytes)
                {
                    int count;
                    fixed (T* dst = destination)
                    {
                        count = ReadCore(dst, destination.Length);
                    }

                    PublishStoredLocked();
                    return count;
                }
            }

            if (!ContinueAfterWait(WaitForStoredBytes(requiredBytes)))
            {
                return 0;
            }
        }
    }

    private int WriteValues<T>(ReadOnlySpan<T> source)
        where T : unmanaged
    {
        if (source.IsEmpty)
        {
            return 0;
        }

        long requiredBytes = (long)source.Length * Unsafe.SizeOf<T>();
        if (!CanFit(requiredBytes))
        {
            return 0;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!_storage.IsOpen)
                {
                    return 0;
                }

                if (_storage.FreeBytes >= requiredBytes)
                {
                    int count;
                    fixed (T* src = source)
                    {
                        count = WriteCore(src, source.Length);
                    }

                    PublishStoredLocked();
                    return count;
                }
            }

            if (!ContinueAfterWait(WaitForFreeBytes(requiredBytes)))
            {
                return 0;
            }
        }
    }
}
