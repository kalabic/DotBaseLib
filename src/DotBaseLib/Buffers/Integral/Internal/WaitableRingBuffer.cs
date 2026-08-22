using System.Runtime.CompilerServices;
using DotBase.AsyncValue;
using DotBase.Integral;

namespace DotBase.Buffers.Integral.Internal;


internal abstract class WaitableRingBuffer
    : IntegralRingBufferBase
    , IWaitableRingBuffer
{
    protected readonly object _lock = new();

    protected readonly int _byteCapacity;

    private readonly AsyncWaitableValue _storedByteCount;

    private bool _isWritingCompleted;

    private bool _isReadingCompleted;

    private bool _isAborted;

    private Exception? _abortError;

    internal WaitableRingBuffer(int capacity)
        : base(capacity)
    {
        _byteCapacity = capacity;
        _storedByteCount = new AsyncWaitableValue();
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

    public void CompleteWriting()
    {
        lock (_lock)
        {
            if (!_storage.IsOpen || _isAborted || _isWritingCompleted)
            {
                return;
            }

            _isWritingCompleted = true;
            WakeLifecycleWaitersLocked();
        }
    }

    public void CompleteReading()
    {
        lock (_lock)
        {
            if (!_storage.IsOpen || _isAborted || _isReadingCompleted)
            {
                return;
            }

            _isReadingCompleted = true;
            _storage.Clear();
            WakeLifecycleWaitersLocked();
        }
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
        return ReadBytes(data.AsSpan(offset, count));
    }

    public override unsafe int Read(byte* data, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(data, offset, count, nameof(data));
        return ReadBytes(data + offset, count);
    }

    public override int Write(byte[] data, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(data);
        return WriteBytes(data.AsSpan(offset, count));
    }

    public override unsafe int Write(byte* data, int offset, int count)
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
        return ReadValues(destination.AsSpan(offset, count));
    }

    public override unsafe int Read<T>(T* destination, int offset, int count)
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

    public override unsafe int Write<T>(T* source, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(source, offset, count, nameof(source));
        return Write(new ReadOnlySpan<T>(source + offset, count));
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

    private LongResult WaitForStoredBytes(long required)
    {
        return _storedByteCount.WaitGreaterOrEqualTo(required);
    }

    private LongResult WaitForFreeBytes(long required)
    {
        return _storedByteCount.WaitLessOrEqualTo(_byteCapacity - required);
    }

    private bool CanFit(long required) => required <= _byteCapacity;

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
                if (!IsReadingAllowedLocked())
                {
                    return 0;
                }

                if (required == 0 ||
                    _storage.StoredBytes >= required ||
                    _isWritingCompleted)
                {
                    int count = ReadIntegralSpan(destination);
                    PublishStoredLocked();
                    return count;
                }
            }

            _ = WaitForStoredBytes(required);
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
                if (!IsReadingAllowedLocked())
                {
                    return 0;
                }

                if (_storage.StoredBytes >= required || _isWritingCompleted)
                {
                    int available = Math.Min(required, _storage.StoredBytes);
                    int count = _storage.Read(destination[..available]);
                    PublishStoredLocked();
                    return count;
                }
            }

            _ = WaitForStoredBytes(required);
        }
    }

    private unsafe int ReadBytes(byte* destination, int required)
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

                if (_storage.StoredBytes >= required || _isWritingCompleted)
                {
                    int available = Math.Min(required, _storage.StoredBytes);
                    int count = _storage.Read(destination, available);
                    PublishStoredLocked();
                    return count;
                }
            }

            _ = WaitForStoredBytes(required);
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

    private unsafe int WriteBytes(byte* source, int required)
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

    private unsafe int ReadValues<T>(Span<T> destination)
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

                if (_storage.StoredBytes >= requiredBytes || _isWritingCompleted)
                {
                    int availableValues =
                        _storage.StoredBytes / Unsafe.SizeOf<T>();
                    int finalCount = Math.Min(
                        destination.Length,
                        availableValues);
                    int count;
                    fixed (T* dst = destination)
                    {
                        count = ReadCore(dst, finalCount);
                    }

                    PublishStoredLocked();
                    return count;
                }
            }

            _ = WaitForStoredBytes(requiredBytes);
        }
    }

    private unsafe int WriteValues<T>(ReadOnlySpan<T> source)
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


    // IBulkRingBufferAsync >>

    public async ValueTask<LongResult> ReadAsync<T>(T[] destination, int offset, int count) 
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(checked(offset + count), destination.Length);

        if (count == 0)
        {
            return LongResult.SUCCESS;
        }

        long requiredBytes = (long)count * Unsafe.SizeOf<T>();
        if (!CanFit(requiredBytes))
        {
            return LongResult.OUT_OF_RANGE;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!IsReadingAllowedLocked())
                {
                    return LongResult.CLOSED;
                }

                if (_storage.StoredBytes >= requiredBytes || _isWritingCompleted)
                {
                    int countRead = ReadAsyncFinalLocked(destination, offset, count);
                    return LongResult.Success(countRead);
                }
            }

            var waitResult = await WaitForStoredBytesAsync(requiredBytes);
            if (waitResult.Status == ResultStatus.CLOSED)
            {
                // Allow pending read to do a partial read after writing is completed.
                continue;
            }

            if (!waitResult)
            {
                return waitResult;
            }
        }
    }

    public unsafe ValueTask<LongResult> ReadAsync<T>(T* destination, int offset, int count)
        where T : unmanaged
    {
        IntegralBufferGuards.ValidatePointer(destination, offset, count, nameof(destination));
        return ReadAsyncInternal(new UnsafePtrSpan<T>(destination, offset, count));
    }

    private async ValueTask<LongResult> ReadAsyncInternal<T>(UnsafePtrSpan<T> ptrSpan) 
        where T : unmanaged
    {
        if (ptrSpan.Count == 0)
        {
            return LongResult.SUCCESS;
        }

        long requiredBytes = (long)ptrSpan.Count * Unsafe.SizeOf<T>();
        if (!CanFit(requiredBytes))
        {
            return LongResult.OUT_OF_RANGE;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!IsReadingAllowedLocked())
                {
                    return LongResult.CLOSED;
                }

                if (_storage.StoredBytes >= requiredBytes || _isWritingCompleted)
                {
                    int countRead = ReadAsyncFinalLocked(ptrSpan);
                    return LongResult.Success(countRead);
                }
            }

            var waitResult = await WaitForStoredBytesAsync(requiredBytes);
            if (waitResult.Status == ResultStatus.CLOSED)
            {
                // Allow pending read to do a partial read after writing is completed.
                continue;
            }

            if (!waitResult)
            {
                return waitResult;
            }
        }
    }

    public async ValueTask<LongResult> WriteAsync<T>(T[] source, int offset, int count)
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(checked(offset + count), source.Length);

        if (count == 0)
        {
            return LongResult.SUCCESS;
        }

        long requiredBytes = (long)count * Unsafe.SizeOf<T>();
        if (!CanFit(requiredBytes))
        {
            return LongResult.OUT_OF_RANGE;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!IsWritingAllowedLocked())
                {
                    return LongResult.CLOSED;
                }

                if (_storage.FreeBytes >= requiredBytes)
                {
                    int countWriten = WriteAsyncFinalLocked(source, offset, count);
                    return LongResult.Success(countWriten);
                }
            }

            var waitResult = await WaitForFreeBytesAsync(requiredBytes);
            if (!waitResult)
            {
                return waitResult;
            }
        }
    }

    public unsafe ValueTask<LongResult> WriteAsync<T>(T* source, int offset, int count)
        where T : unmanaged
    {
        IntegralBufferGuards.ValidatePointer(source, offset, count, nameof(source));
        return WriteAsync(new UnsafePtrSpan<T>(source, offset, count));
    }

    private async ValueTask<LongResult> WriteAsync<T>(UnsafePtrSpan<T> ptrSpan)
        where T : unmanaged
    {
        if (ptrSpan.Count == 0)
        {
            return LongResult.SUCCESS;
        }

        long requiredBytes = (long)ptrSpan.Count * Unsafe.SizeOf<T>();
        if (!CanFit(requiredBytes))
        {
            return LongResult.OUT_OF_RANGE;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!IsWritingAllowedLocked())
                {
                    return LongResult.CLOSED;
                }

                if (_storage.FreeBytes >= requiredBytes)
                {
                    int countWriten = WriteAsyncFinalLocked(ptrSpan);
                    return LongResult.Success(countWriten);
                }
            }

            var waitResult = await WaitForFreeBytesAsync(requiredBytes);
            if (!waitResult)
            {
                return waitResult;
            }
        }
    }

    private unsafe int ReadAsyncFinalLocked<T>(T[] destination, int offset, int count)
        where T : unmanaged
    {
        int availableValues = _storage.StoredBytes / Unsafe.SizeOf<T>();
        int finalCount = Math.Min(count, availableValues);
        int readCount = 0;

        fixed (T* dst = &destination[offset])
        {
            readCount = ReadCore(dst, finalCount);
        }

        PublishStoredLocked();
        return readCount;
    }

    private unsafe int ReadAsyncFinalLocked<T>(UnsafePtrSpan<T> spanPtr)
        where T : unmanaged
    {
        int availableValues = _storage.StoredBytes / Unsafe.SizeOf<T>();
        int finalCount = Math.Min(spanPtr.Count, availableValues);
        int readCount = ReadCore(spanPtr.OffsetPtr, finalCount);
        PublishStoredLocked();
        return readCount;
    }

    private unsafe int WriteAsyncFinalLocked<T>(T[] source, int offset, int count) 
        where T : unmanaged
    {
        int countWriten = 0;
        fixed (T* src = &source[offset])
        {
            countWriten = WriteCore(src, count);
        }

        PublishStoredLocked();
        return countWriten;
    }

    private unsafe int WriteAsyncFinalLocked<T>(UnsafePtrSpan<T> spanPtr) 
        where T : unmanaged
    {
        int countWriten = WriteCore(spanPtr.OffsetPtr, spanPtr.Count);
        PublishStoredLocked();
        return countWriten;
    }

    private ValueTask<LongResult> WaitForStoredBytesAsync(long required)
    {
        return _storedByteCount.WaitGreaterOrEqualToAsync(required);
    }

    private ValueTask<LongResult> WaitForFreeBytesAsync(long required)
    {
        return _storedByteCount.WaitLessOrEqualToAsync(_byteCapacity - required);
    }
}
