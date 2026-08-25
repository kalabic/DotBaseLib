using DotBase.AsyncValue;
using DotBase.Buffers.Await;
using DotBase.Integral;
using System.Runtime.CompilerServices;

namespace DotBase.Buffers.Integral.Internal;


internal abstract partial class WaitableRingBuffer
    : IntegralRingBufferBase
    , IWaitableRingBuffer
{
    // Public events >>

    public event EventHandler<BufferReadingCompleted>? ReadingCompleted;

    public event EventHandler<BufferWritingCompleted>? WritingCompleted;


    // Private members >>

    protected readonly object _lock = new();

    protected readonly int _byteCapacity;

    private readonly AsyncWaitableValue _storedByteCount;

    private bool _isWritingCompleted;

    private bool _isReadingCompleted;

    private bool _isAborted;

    private Exception? _abortError;

    internal WaitableRingBuffer(int capacity, IntegralFormat format)
        : base(capacity, format)
    {
        _byteCapacity = capacity;
        _storedByteCount = new AsyncWaitableValue(new LongValueRange(0, capacity));
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

    public bool IsWritingCompleted
    {
        get { lock (_lock) { return _isWritingCompleted; } }
    }

    public bool IsReadingCompleted
    {
        get { lock (_lock) { return _isReadingCompleted; } }
    }

    public bool IsDrained
    {
        get
        {
            lock (_lock)
            {
                return _isWritingCompleted && _storage.StoredBytes == 0;
            }
        }
    }

    public bool IsAborted
    {
        get { lock (_lock) { return _isAborted; } }
    }

    public Exception? AbortError
    {
        get { lock (_lock) { return _abortError; } }
    }

    private bool CompleteWritingInternal()
    {
        lock (_lock)
        {
            if (!_storage.IsOpen || _isAborted || _isWritingCompleted)
            {
                return false;
            }

            _isWritingCompleted = true;
            WakeLifecycleWaitersLocked();
            return true;
        }
    }

    public long CompleteWriting()
    {
        if (CompleteWritingInternal())
        {
            WritingCompleted?.Invoke(this, new BufferWritingCompleted(_storage.TotalWritten));
        }
        return _storage.TotalWritten;
    }

    private bool CompleteReadingInternal()
    {
        lock (_lock)
        {
            if (!_storage.IsOpen || _isAborted || _isReadingCompleted)
            {
                return false;
            }

            _isReadingCompleted = true;
            _storage.Clear();
            WakeLifecycleWaitersLocked();
            return true;
        }
    }

    public long CompleteReading()
    {
        if (CompleteReadingInternal())
        {
            ReadingCompleted?.Invoke(this, new BufferReadingCompleted(_storage.TotalRead));
        }
        return _storage.TotalRead;
    }

    public void Abort(Exception? error = null)
    {
        lock (_lock)
        {
            if (!_storage.IsOpen || _isAborted)
            {
                return;
            }

            _isAborted = true;
            _abortError = error;
            _storage.Clear();
            WakeLifecycleWaitersLocked();
        }
    }

    public override int CapacityAs<T>()
    {
        lock (_lock)
        {
            return _storage.ByteCapacity / Unsafe.SizeOf<T>();
        }
    }

    public override int CapacityAsBlockCount()
    {
        lock (_lock)
        {
            return (_format.BytesPerBlock > 0) ? (int)(_storage.ByteCapacity / _format.BytesPerBlock) : 0;
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
        return ReadPartialSpan(destination, validate: false);
    }

    public override int ReadChecked(in IntegralSpan destination)
    {
        return ReadPartialSpan(destination, validate: true);
    }

    public int ReadExact(in IntegralSpan destination)
    {
        return ReadExactSpan(destination, validate: false);
    }

    public int ReadExactChecked(in IntegralSpan destination)
    {
        return ReadExactSpan(destination, validate: true);
    }

    public override bool TryRead(in IntegralSpan destination)
    {
        lock (_lock)
        {
            if (!IsReadingAllowedLocked())
            {
                return false;
            }

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
        IntegralRingSpanOps.ValidateSpan(destination, nameof(destination));

        lock (_lock)
        {
            if (!IsReadingAllowedLocked())
            {
                return false;
            }

            bool completed = TryReadIntegralSpan(destination);
            if (completed)
            {
                PublishStoredLocked();
            }

            return completed;
        }
    }

    public override int Write(in IntegralSpan source)
    {
        return WritePartialSpan(source, validate: false);
    }

    public override int WriteChecked(in IntegralSpan source)
    {
        return WritePartialSpan(source, validate: true);
    }

    public int WriteExact(in IntegralSpan source)
    {
        return WriteExactSpan(source, validate: false);
    }

    public int WriteExactChecked(in IntegralSpan source)
    {
        return WriteExactSpan(source, validate: true);
    }

    public override bool TryWrite(in IntegralSpan source)
    {
        lock (_lock)
        {
            if (!IsWritingAllowedLocked())
            {
                return false;
            }

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
        IntegralRingSpanOps.ValidateSpan(source, nameof(source));

        lock (_lock)
        {
            if (!IsWritingAllowedLocked())
            {
                return false;
            }

            bool completed = TryWriteIntegralSpan(source);
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
        return ReadPartialBytes(data.AsSpan(offset, count));
    }

    public override unsafe int Read(byte* data, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(data, offset, count, nameof(data));
        return ReadPartialBytes(data + offset, count);
    }

    public int ReadExact(byte[] destination, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(destination);
        return ReadExactBytes(destination.AsSpan(offset, count));
    }

    public unsafe int ReadExact(byte* destination, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(
            destination,
            offset,
            count,
            nameof(destination));
        return ReadExactBytes(destination + offset, count);
    }

    public override int Write(byte[] data, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(data);
        return WritePartialBytes(data.AsSpan(offset, count));
    }

    public override unsafe int Write(byte* data, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(data, offset, count, nameof(data));
        return WritePartialBytes(data + offset, count);
    }

    public int WriteExact(byte[] source, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        return WriteExactBytes(source.AsSpan(offset, count));
    }

    public unsafe int WriteExact(byte* source, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(source, offset, count, nameof(source));
        return WriteExactBytes(source + offset, count);
    }

    public override bool Read<T>(out T value) => TryRead(out value);

    public bool ReadExact<T>(out T value)
        where T : unmanaged
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
                if (!IsReadingAllowedLocked())
                {
                    value = default;
                    return false;
                }

                if (_storage.StoredBytes >= n && TryReadScalar(out value))
                {
                    PublishStoredLocked();
                    return true;
                }

                if (_isWritingCompleted)
                {
                    value = default;
                    return false;
                }
            }

            _ = WaitForStoredBytes(n);
        }
    }

    public override bool TryRead<T>(out T value)
    {
        lock (_lock)
        {
            if (!IsReadingAllowedLocked())
            {
                value = default;
                return false;
            }

            bool completed = TryReadScalar(out value);
            if (completed)
            {
                PublishStoredLocked();
            }

            return completed;
        }
    }

    public override bool Write<T>(T value) => TryWrite(value);

    public bool WriteExact<T>(T value)
        where T : unmanaged
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
                if (!IsWritingAllowedLocked())
                {
                    return false;
                }

                if (_storage.FreeBytes >= n && TryWriteScalar(value))
                {
                    PublishStoredLocked();
                    return true;
                }
            }

            _ = WaitForFreeBytes(n);
        }
    }

    public override bool TryWrite<T>(T value)
    {
        lock (_lock)
        {
            if (!IsWritingAllowedLocked())
            {
                return false;
            }

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
        return ReadPartialValues(destination.AsSpan(offset, count));
    }

    public override unsafe int Read<T>(T* destination, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(
            destination,
            offset,
            count,
            nameof(destination));

        return ReadPartialValues(new Span<T>(destination + offset, count));
    }

    public int ReadExact<T>(T[] destination, int offset, int count)
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(destination);
        return ReadExactValues(destination.AsSpan(offset, count));
    }

    public unsafe int ReadExact<T>(T* destination, int offset, int count)
        where T : unmanaged
    {
        IntegralBufferGuards.ValidatePointer(
            destination,
            offset,
            count,
            nameof(destination));
        return ReadExactValues(new Span<T>(destination + offset, count));
    }

    public override int Write<T>(T[] source, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        return WritePartialValues((ReadOnlySpan<T>)source.AsSpan(offset, count));
    }

    public override unsafe int Write<T>(T* source, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(source, offset, count, nameof(source));
        return WritePartialValues(new ReadOnlySpan<T>(source + offset, count));
    }

    public int WriteExact<T>(T[] source, int offset, int count)
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(source);
        return WriteExactValues((ReadOnlySpan<T>)source.AsSpan(offset, count));
    }

    public unsafe int WriteExact<T>(T* source, int offset, int count)
        where T : unmanaged
    {
        IntegralBufferGuards.ValidatePointer(source, offset, count, nameof(source));
        return WriteExactValues(new ReadOnlySpan<T>(source + offset, count));
    }

    public override bool TryRead<T>(T[] destination, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(destination);
        return TryRead(destination.AsSpan(offset, count));
    }

    public override unsafe bool TryRead<T>(T* destination, int offset, int count)
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
            if (!IsReadingAllowedLocked())
            {
                return false;
            }

            bool completed = TryReadCore(destination + offset, count);
            if (completed)
            {
                PublishStoredLocked();
            }

            return completed;
        }
    }

    public override unsafe bool TryRead<T>(Span<T> destination)
    {
        if (destination.IsEmpty)
        {
            lock (_lock)
            {
                return IsReadingAllowedLocked();
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
                if (!IsReadingAllowedLocked())
                {
                    return false;
                }

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

    public override unsafe bool TryWrite<T>(T* source, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(source, offset, count, nameof(source));

        if (!CanFit((long)count * Unsafe.SizeOf<T>()))
        {
            return false;
        }

        lock (_lock)
        {
            if (!IsWritingAllowedLocked())
            {
                return false;
            }

            bool completed = TryWriteCore(source + offset, count);
            if (completed)
            {
                PublishStoredLocked();
            }

            return completed;
        }
    }

    public override unsafe bool TryWrite<T>(ReadOnlySpan<T> source)
    {
        if (source.IsEmpty)
        {
            lock (_lock)
            {
                return IsWritingAllowedLocked();
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
                if (!IsWritingAllowedLocked())
                {
                    return false;
                }

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
        return ReadPartialValues(destination);
    }

    public int ReadExact<T>(Span<T> destination)
        where T : unmanaged
    {
        return ReadExactValues(destination);
    }

    public override int Write<T>(ReadOnlySpan<T> source)
    {
        return WritePartialValues(source);
    }

    public int WriteExact<T>(ReadOnlySpan<T> source)
        where T : unmanaged
    {
        return WriteExactValues(source);
    }

    protected abstract int ReadIntegralSpan(in IntegralSpan destination);

    protected abstract bool TryReadIntegralSpan(in IntegralSpan destination);

    protected abstract bool TryReadIntegralSpanChecked(in IntegralSpan destination);

    protected abstract int WriteIntegralSpan(in IntegralSpan source);

    protected abstract bool TryWriteIntegralSpan(in IntegralSpan source);

    protected abstract bool TryWriteIntegralSpanChecked(in IntegralSpan source);

    protected abstract bool TryReadScalar<T>(out T value) where T : unmanaged;

    protected abstract bool TryWriteScalar<T>(T value) where T : unmanaged;

    protected abstract unsafe int ReadCore<T>(T* destination, int count) where T : unmanaged;

    protected abstract unsafe bool TryReadCore<T>(T* destination, int count) where T : unmanaged;

    protected abstract unsafe int WriteCore<T>(T* source, int count) where T : unmanaged;

    protected abstract unsafe bool TryWriteCore<T>(T* source, int count) where T : unmanaged;

    private bool IsReadingAllowedLocked() =>
        _storage.IsOpen &&
        !_isReadingCompleted &&
        !_isAborted;

    private bool IsWritingAllowedLocked() =>
        _storage.IsOpen &&
        !_isWritingCompleted &&
        !_isReadingCompleted &&
        !_isAborted;

    private void WakeLifecycleWaitersLocked()
    {
        _storedByteCount.Close();
    }

    private void PublishStoredLocked()
    {
        if (_storedByteCount.IsOpen)
        {
            _storedByteCount.SetValue(_storage.StoredBytes);
        }
    }

    private bool CanFit(long required) => required <= _byteCapacity;

    private int ReadPartialSpan(in IntegralSpan destination, bool validate)
    {
        if (validate)
        {
            IntegralRingSpanOps.ValidateSpan(
                destination,
                nameof(destination));
        }

        lock (_lock)
        {
            if (!IsReadingAllowedLocked())
            {
                return 0;
            }

            int count = ReadIntegralSpan(destination);
            if (count != 0)
            {
                PublishStoredLocked();
            }

            return count;
        }
    }

    private int ReadExactSpan(in IntegralSpan destination, bool validate)
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
                if (!IsReadingAllowedLocked())
                {
                    return 0;
                }

                if (required == 0 ||
                    _storage.StoredBytes >= required)
                {
                    int count = ReadIntegralSpan(destination);
                    PublishStoredLocked();
                    return count;
                }

                if (_isWritingCompleted)
                {
                    return 0;
                }
            }

            _ = WaitForStoredBytes(required);
        }
    }

    private int WritePartialSpan(in IntegralSpan source, bool validate)
    {
        if (validate)
        {
            IntegralRingSpanOps.ValidateSpan(
                source,
                nameof(source));
        }

        lock (_lock)
        {
            if (!IsWritingAllowedLocked())
            {
                return 0;
            }

            int count = WriteIntegralSpan(source);
            if (count != 0)
            {
                PublishStoredLocked();
            }

            return count;
        }
    }

    private int WriteExactSpan(in IntegralSpan source, bool validate)
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
                if (!IsWritingAllowedLocked())
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

            _ = WaitForFreeBytes(required);
        }
    }

    private int ReadPartialBytes(Span<byte> destination)
    {
        lock (_lock)
        {
            if (!IsReadingAllowedLocked())
            {
                return 0;
            }

            int count = _storage.Read(destination);
            if (count != 0)
            {
                PublishStoredLocked();
            }

            return count;
        }
    }

    private unsafe int ReadPartialBytes(byte* destination, int count)
    {
        lock (_lock)
        {
            if (!IsReadingAllowedLocked())
            {
                return 0;
            }

            int read = _storage.Read(destination, count);
            if (read != 0)
            {
                PublishStoredLocked();
            }

            return read;
        }
    }

    private int ReadExactBytes(Span<byte> destination)
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
                if (!IsReadingAllowedLocked())
                {
                    return 0;
                }

                if (_storage.StoredBytes >= required)
                {
                    int count = _storage.Read(destination);
                    PublishStoredLocked();
                    return count;
                }

                if (_isWritingCompleted)
                {
                    return 0;
                }
            }

            _ = WaitForStoredBytes(required);
        }
    }

    private unsafe int ReadExactBytes(byte* destination, int required)
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
                if (!IsReadingAllowedLocked())
                {
                    return 0;
                }

                if (_storage.StoredBytes >= required)
                {
                    int count = _storage.Read(destination, required);
                    PublishStoredLocked();
                    return count;
                }

                if (_isWritingCompleted)
                {
                    return 0;
                }
            }

            _ = WaitForStoredBytes(required);
        }
    }

    private int WritePartialBytes(ReadOnlySpan<byte> source)
    {
        lock (_lock)
        {
            if (!IsWritingAllowedLocked())
            {
                return 0;
            }

            int count = _storage.Write(source);
            if (count != 0)
            {
                PublishStoredLocked();
            }

            return count;
        }
    }

    private unsafe int WritePartialBytes(byte* source, int count)
    {
        lock (_lock)
        {
            if (!IsWritingAllowedLocked())
            {
                return 0;
            }

            int written = _storage.Write(source, count);
            if (written != 0)
            {
                PublishStoredLocked();
            }

            return written;
        }
    }

    private int WriteExactBytes(ReadOnlySpan<byte> source)
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
                if (!IsWritingAllowedLocked())
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

            _ = WaitForFreeBytes(required);
        }
    }

    private unsafe int WriteExactBytes(byte* source, int required)
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
                if (!IsWritingAllowedLocked())
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

            _ = WaitForFreeBytes(required);
        }
    }

    private unsafe int ReadPartialValues<T>(Span<T> destination)
        where T : unmanaged
    {
        if (destination.IsEmpty)
        {
            return 0;
        }

        fixed (T* dst = destination)
        {
            lock (_lock)
            {
                if (!IsReadingAllowedLocked())
                {
                    return 0;
                }

                int count = ReadCore(dst, destination.Length);
                if (count != 0)
                {
                    PublishStoredLocked();
                }

                return count;
            }
        }
    }

    private unsafe int ReadExactValues<T>(Span<T> destination)
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
                if (!IsReadingAllowedLocked())
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

                if (_isWritingCompleted)
                {
                    return 0;
                }
            }

            _ = WaitForStoredBytes(requiredBytes);
        }
    }

    private unsafe int WritePartialValues<T>(ReadOnlySpan<T> source)
        where T : unmanaged
    {
        if (source.IsEmpty)
        {
            return 0;
        }

        fixed (T* src = source)
        {
            lock (_lock)
            {
                if (!IsWritingAllowedLocked())
                {
                    return 0;
                }

                int count = WriteCore(src, source.Length);
                if (count != 0)
                {
                    PublishStoredLocked();
                }

                return count;
            }
        }
    }

    private unsafe int WriteExactValues<T>(ReadOnlySpan<T> source)
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
                if (!IsWritingAllowedLocked())
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

            _ = WaitForFreeBytes(requiredBytes);
        }
    }


    // Public Wait implementation  >>

    public LongResult WaitForStoredBytes(long byteCount = 1)
    {
        if (byteCount < 0)
        {
            return LongResult.OUT_OF_RANGE;
        }

        return _storedByteCount.WaitGreaterOrEqualTo(byteCount);
    }

    public LongResult WaitForFreeBytes(long byteCount = 1)
    {
        if (byteCount < 0)
        {
            return LongResult.OUT_OF_RANGE;
        }

        return _storedByteCount.WaitLessOrEqualTo(_byteCapacity - byteCount);
    }

    public LongResult WaitForStoredValues<T>(long valueCount = 1)
        where T : unmanaged
    {
        if ((valueCount < 0) ||
            (valueCount > CapacityAs<T>()))
        {
            return LongResult.OUT_OF_RANGE;
        }

        return _storedByteCount.WaitGreaterOrEqualTo(valueCount * Unsafe.SizeOf<T>());
    }

    public LongResult WaitForFreeValues<T>(long valueCount = 1)
        where T : unmanaged
    {
        if ((valueCount < 0) ||
            (valueCount > CapacityAs<T>()))
        {
            return LongResult.OUT_OF_RANGE;
        }

        return _storedByteCount.WaitLessOrEqualTo(_byteCapacity - checked(valueCount * Unsafe.SizeOf<T>()));
    }

    public LongResult WaitForStoredBlock(long blockCount = 1)
    {
        if ((blockCount < 0) ||
            (blockCount > CapacityAsBlockCount()))
        {
            return LongResult.OUT_OF_RANGE;
        }

        return _storedByteCount.WaitGreaterOrEqualTo(blockCount * _format.BytesPerBlock);
    }

    public LongResult WaitForFreeBlock(long blockCount = 1)
    {
        if ((blockCount < 0) ||
            (blockCount > CapacityAsBlockCount()))
        {
            return LongResult.OUT_OF_RANGE;
        }

        return _storedByteCount.WaitLessOrEqualTo(_byteCapacity - checked(blockCount * _format.BytesPerBlock));
    }
}
