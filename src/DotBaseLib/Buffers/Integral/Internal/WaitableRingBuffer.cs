using DotBase.Core;
using DotBase.Integral;
using DotBase.Integral.Internal;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotBase.Buffers.Integral.Internal;


internal sealed class WaitableRingBufferLE : DisposableBase, IWaitableRingBuffer
{
    private const int ScratchByteCount = 512;
    private readonly object _lock = new();
    private RingBufferStorage _storage;

    internal WaitableRingBufferLE(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        _storage = new RingBufferStorage(capacity);
    }

    public ByteOrder ByteOrder => ByteOrder.LittleEndian;

    public int ByteCapacity
    {
        get { lock (_lock) { return _storage.ByteCapacity; } }
    }

    public int FreeBytes
    {
        get { lock (_lock) { return _storage.FreeBytes; } }
    }

    public int StoredBytes
    {
        get { lock (_lock) { return _storage.StoredBytes; } }
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

    public int CapacityAs<T>()
        where T : unmanaged
    {
        lock (_lock)
        {
            return _storage.ByteCapacity / Unsafe.SizeOf<T>();
        }
    }

    public int FreeCount<T>()
        where T : unmanaged
    {
        lock (_lock)
        {
            return _storage.FreeBytes / Unsafe.SizeOf<T>();
        }
    }

    public int StoredCount<T>()
        where T : unmanaged
    {
        lock (_lock)
        {
            return _storage.StoredBytes / Unsafe.SizeOf<T>();
        }
    }

    public void AdvanceBy<T>(int count)
        where T : unmanaged
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        lock (_lock)
        {
            _storage.Advance(count * Unsafe.SizeOf<T>());
        }
    }

    public int Read(in IntegralSpan destination)
    {
        lock (_lock)
        {
            int requiredByteCount = IntegralRingOperationsLE.ValidateSpan(
                ref _storage,
                destination,
                nameof(destination));

            if (!_storage.IsOpen)
            {
                return 0;
            }

            WaitForBytes(requiredByteCount, nameof(destination));

            return _storage.IsOpen
                ? IntegralRingOperationsLE.Read(ref _storage, destination)
                : 0;
        }
    }

    public bool TryRead(in IntegralSpan destination)
    {
        lock (_lock)
        {
            return IntegralRingOperationsLE.TryRead(ref _storage, destination);
        }
    }

    public int Write(in IntegralSpan source)
    {
        lock (_lock)
        {
            int count = IntegralRingOperationsLE.Write(ref _storage, source);
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
            bool completed = IntegralRingOperationsLE.TryWrite(ref _storage, source);
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

    public unsafe T Read<T>()
        where T : unmanaged
    {
        int n = Unsafe.SizeOf<T>();

        lock (_lock)
        {
            WaitForBytes(n, nameof(T));

            if (_storage.IsOpen && TryReadScalar(out T value))
            {
                return value;
            }
        }

        throw new InvalidOperationException(
            "The ring was closed before a complete value became available.");
    }

    public unsafe bool TryRead<T>(out T value)
        where T : unmanaged
    {
        lock (_lock)
        {
            return TryReadScalar(out value);
        }
    }

    public unsafe void Write<T>(T value)
        where T : unmanaged
    {
        lock (_lock)
        {
            if (!TryWriteScalar(value))
            {
                throw new InvalidOperationException(
                    "The ring does not have enough free capacity for the requested value.");
            }

            Monitor.PulseAll(_lock);
        }
    }

    public unsafe bool TryWrite<T>(T value)
        where T : unmanaged
    {
        lock (_lock)
        {
            bool completed = TryWriteScalar(value);
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
        return ReadValues(destination.AsSpan(offset, count));
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
        return Write((ReadOnlySpan<T>)source.AsSpan(offset, count));
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
        return TryRead(destination.AsSpan(offset, count));
    }

    public unsafe bool TryRead<T>(T* destination, int offset, int count)
        where T : unmanaged
    {
        IntegralBufferGuards.ValidatePointer(
            destination,
            offset,
            count,
            nameof(destination));

        lock (_lock)
        {
            return TryReadCore(destination + offset, count);
        }
    }

    public unsafe bool TryRead<T>(Span<T> destination)
        where T : unmanaged
    {
        if (destination.IsEmpty)
        {
            lock (_lock)
            {
                return _storage.IsOpen;
            }
        }

        fixed (T* dst = destination)
        {
            lock (_lock)
            {
                return TryReadCore(dst, destination.Length);
            }
        }
    }

    public bool TryWrite<T>(T[] source, int offset, int count)
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(source);
        return TryWrite((ReadOnlySpan<T>)source.AsSpan(offset, count));
    }

    public unsafe bool TryWrite<T>(T* source, int offset, int count)
        where T : unmanaged
    {
        IntegralBufferGuards.ValidatePointer(source, offset, count, nameof(source));

        lock (_lock)
        {
            bool completed = TryWriteCore(source + offset, count);
            if (completed && count > 0)
            {
                Monitor.PulseAll(_lock);
            }

            return completed;
        }
    }

    public unsafe bool TryWrite<T>(ReadOnlySpan<T> source)
        where T : unmanaged
    {
        if (source.IsEmpty)
        {
            lock (_lock)
            {
                return _storage.IsOpen;
            }
        }

        fixed (T* src = source)
        {
            lock (_lock)
            {
                bool completed = TryWriteCore(src, source.Length);
                if (completed)
                {
                    Monitor.PulseAll(_lock);
                }

                return completed;
            }
        }
    }

    public int Read<T>(Span<T> destination)
        where T : unmanaged
    {
        return ReadValues(destination);
    }

    public unsafe int Write<T>(ReadOnlySpan<T> source)
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
                int count = WriteCore(src, source.Length);
                if (count > 0)
                {
                    Monitor.PulseAll(_lock);
                }

                return count;
            }
        }
    }

    public void Advance(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
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
            if (!_storage.IsOpen)
            {
                return 0;
            }

            int n = Math.Min(destination.Length, _storage.StoredBytes);
            return n == 0 ? 0 : _storage.Read(destination[..n]);
        }
    }

    private int WriteBytes(ReadOnlySpan<byte> source)
    {
        lock (_lock)
        {
            int n = Math.Min(source.Length, _storage.FreeBytes);
            if (n == 0)
            {
                return 0;
            }

            int count = _storage.Write(source[..n]);
            if (count > 0)
            {
                Monitor.PulseAll(_lock);
            }

            return count;
        }
    }

    private unsafe int ReadBytes(byte* destination, int byteCount)
    {
        lock (_lock)
        {
            WaitForBytes(byteCount, nameof(destination));
            if (!_storage.IsOpen)
            {
                return 0;
            }

            int n = Math.Min(byteCount, _storage.StoredBytes);
            return n == 0 ? 0 : _storage.Read(destination, n);
        }
    }

    private unsafe int WriteBytes(byte* source, int byteCount)
    {
        lock (_lock)
        {
            int n = Math.Min(byteCount, _storage.FreeBytes);
            if (n == 0)
            {
                return 0;
            }

            int count = _storage.Write(source, n);
            if (count > 0)
            {
                Monitor.PulseAll(_lock);
            }

            return count;
        }
    }

    private unsafe int ReadValues<T>(Span<T> destination)
        where T : unmanaged
    {
        int requiredBytes = checked(destination.Length * Unsafe.SizeOf<T>());

        if (destination.IsEmpty)
        {
            lock (_lock)
            {
                WaitForBytes(0, nameof(destination));
                return 0;
            }
        }

        fixed (T* dst = destination)
        {
            lock (_lock)
            {
                WaitForBytes(requiredBytes, nameof(destination));
                return _storage.IsOpen
                    ? ReadCore(dst, destination.Length)
                    : 0;
            }
        }
    }

    private void WaitForBytes(int requiredBytes, string parameterName)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            requiredBytes,
            _storage.ByteCapacity,
            parameterName);

        while (_storage.IsOpen && _storage.StoredBytes < requiredBytes)
        {
            Monitor.Wait(_lock);
        }
    }

    private unsafe bool TryReadScalar<T>(out T value)
        where T : unmanaged
    {
        int n = Unsafe.SizeOf<T>();
        if (!_storage.IsOpen || _storage.StoredBytes < n)
        {
            value = default;
            return false;
        }

        T tmp = default;
        byte* p = (byte*)&tmp;
        switch (n)
        {
            case 1:
                _storage.Read(p, 1);
                break;
            case 2:
                if (IntegralWire.NeedsSwapForLeWire(Unsafe.SizeOf<T>()))
                {
                    _storage.ReadLE2(p);
                }
                else
                {
                    _storage.ReadBE2(p);
                }
                break;
            case 4:
                if (IntegralWire.NeedsSwapForLeWire(Unsafe.SizeOf<T>()))
                {
                    _storage.ReadLE4(p);
                }
                else
                {
                    _storage.ReadBE4(p);
                }
                break;
            case 8:
                if (IntegralWire.NeedsSwapForLeWire(Unsafe.SizeOf<T>()))
                {
                    _storage.ReadLE8(p);
                }
                else
                {
                    _storage.ReadBE8(p);
                }
                break;
            default:
                _storage.Read(p, n);
                if (IntegralWire.NeedsSwapForLeWire(Unsafe.SizeOf<T>()))
                {
                    tmp = IntegralEndianness.ReverseValue(tmp);
                }
                break;
        }

        value = tmp;
        return true;
    }

    private unsafe bool TryWriteScalar<T>(T value)
        where T : unmanaged
    {
        int n = Unsafe.SizeOf<T>();
        if (!_storage.IsOpen || _storage.FreeBytes < n)
        {
            return false;
        }

        switch (n)
        {
            case 1:
                _storage.Write((byte*)&value, 1);
                break;
            case 2:
                if (IntegralWire.NeedsSwapForLeWire(Unsafe.SizeOf<T>()))
                {
                    _storage.WriteLE2((byte*)&value);
                }
                else
                {
                    _storage.WriteBE2((byte*)&value);
                }
                break;
            case 4:
                if (IntegralWire.NeedsSwapForLeWire(Unsafe.SizeOf<T>()))
                {
                    _storage.WriteLE4((byte*)&value);
                }
                else
                {
                    _storage.WriteBE4((byte*)&value);
                }
                break;
            case 8:
                if (IntegralWire.NeedsSwapForLeWire(Unsafe.SizeOf<T>()))
                {
                    _storage.WriteLE8((byte*)&value);
                }
                else
                {
                    _storage.WriteBE8((byte*)&value);
                }
                break;
            default:
                if (IntegralWire.NeedsSwapForLeWire(Unsafe.SizeOf<T>()))
                {
                    value = IntegralEndianness.ReverseValue(value);
                }

                _storage.Write((byte*)&value, n);
                break;
        }

        return true;
    }

    private unsafe int ReadCore<T>(T* destination, int count)
        where T : unmanaged
    {
        if (count <= 0)
        {
            return 0;
        }

        int n = Unsafe.SizeOf<T>();
        int elementCount = Math.Min(count, _storage.StoredBytes / n);
        if (elementCount == 0)
        {
            return 0;
        }

        int bytes = _storage.Read((byte*)destination, elementCount * n);
        Debug.Assert(bytes == elementCount * n);

        if (IntegralWire.NeedsSwapForLeWire(n))
        {
            IntegralWire.ReverseLanesInPlace((byte*)destination, elementCount, n);
        }

        return elementCount;
    }

    private unsafe bool TryReadCore<T>(T* destination, int count)
        where T : unmanaged
    {
        int requiredBytes = checked(count * Unsafe.SizeOf<T>());
        if (!_storage.IsOpen || _storage.StoredBytes < requiredBytes)
        {
            return false;
        }

        int elementCount = ReadCore(destination, count);
        Debug.Assert(elementCount == count);
        return true;
    }

    private unsafe int WriteCore<T>(T* source, int count)
        where T : unmanaged
    {
        if (count <= 0)
        {
            return 0;
        }

        int n = Unsafe.SizeOf<T>();
        int elementCount = Math.Min(count, _storage.FreeBytes / n);
        if (elementCount == 0)
        {
            return 0;
        }

        if (!IntegralWire.NeedsSwapForLeWire(Unsafe.SizeOf<T>()))
        {
            int bytes = _storage.Write((byte*)source, elementCount * n);
            Debug.Assert(bytes == elementCount * n);
            return elementCount;
        }

        WriteReversed(source, elementCount);
        return elementCount;
    }

    private unsafe bool TryWriteCore<T>(T* source, int count)
        where T : unmanaged
    {
        int requiredBytes = checked(count * Unsafe.SizeOf<T>());
        if (!_storage.IsOpen || _storage.FreeBytes < requiredBytes)
        {
            return false;
        }

        int elementCount = WriteCore(source, count);
        Debug.Assert(elementCount == count);
        return true;
    }

    private unsafe void WriteReversed<T>(T* source, int count)
        where T : unmanaged
    {
        int n = Unsafe.SizeOf<T>();
        int scratchCount = Math.Max(1, ScratchByteCount / n);
        byte* scratch = stackalloc byte[scratchCount * n];

        int position = 0;
        while (position < count)
        {
            int chunk = Math.Min(scratchCount, count - position);
            IntegralWire.ReverseCopyLanes(
                (byte*)(source + position),
                scratch,
                chunk,
                n);

            int bytes = _storage.Write(scratch, chunk * n);
            Debug.Assert(bytes == chunk * n);
            position += chunk;
        }
    }
}


internal sealed class WaitableRingBufferBE : DisposableBase, IWaitableRingBuffer
{
    private const int ScratchByteCount = 512;
    private readonly object _lock = new();
    private RingBufferStorage _storage;

    internal WaitableRingBufferBE(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        _storage = new RingBufferStorage(capacity);
    }

    public ByteOrder ByteOrder => ByteOrder.BigEndian;

    public int ByteCapacity
    {
        get { lock (_lock) { return _storage.ByteCapacity; } }
    }

    public int FreeBytes
    {
        get { lock (_lock) { return _storage.FreeBytes; } }
    }

    public int StoredBytes
    {
        get { lock (_lock) { return _storage.StoredBytes; } }
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

    public int CapacityAs<T>()
        where T : unmanaged
    {
        lock (_lock)
        {
            return _storage.ByteCapacity / Unsafe.SizeOf<T>();
        }
    }

    public int FreeCount<T>()
        where T : unmanaged
    {
        lock (_lock)
        {
            return _storage.FreeBytes / Unsafe.SizeOf<T>();
        }
    }

    public int StoredCount<T>()
        where T : unmanaged
    {
        lock (_lock)
        {
            return _storage.StoredBytes / Unsafe.SizeOf<T>();
        }
    }

    public void AdvanceBy<T>(int count)
        where T : unmanaged
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        lock (_lock)
        {
            _storage.Advance(count * Unsafe.SizeOf<T>());
        }
    }

    public int Read(in IntegralSpan destination)
    {
        lock (_lock)
        {
            int requiredByteCount = IntegralRingOperationsBE.ValidateSpan(
                ref _storage,
                destination,
                nameof(destination));

            if (!_storage.IsOpen)
            {
                return 0;
            }

            WaitForBytes(requiredByteCount, nameof(destination));

            return _storage.IsOpen
                ? IntegralRingOperationsBE.Read(ref _storage, destination)
                : 0;
        }
    }

    public bool TryRead(in IntegralSpan destination)
    {
        lock (_lock)
        {
            return IntegralRingOperationsBE.TryRead(ref _storage, destination);
        }
    }

    public int Write(in IntegralSpan source)
    {
        lock (_lock)
        {
            int count = IntegralRingOperationsBE.Write(ref _storage, source);
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
            bool completed = IntegralRingOperationsBE.TryWrite(ref _storage, source);
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

    public unsafe T Read<T>()
        where T : unmanaged
    {
        int n = Unsafe.SizeOf<T>();

        lock (_lock)
        {
            WaitForBytes(n, nameof(T));

            if (_storage.IsOpen && TryReadScalar(out T value))
            {
                return value;
            }
        }

        throw new InvalidOperationException(
            "The ring was closed before a complete value became available.");
    }

    public unsafe bool TryRead<T>(out T value)
        where T : unmanaged
    {
        lock (_lock)
        {
            return TryReadScalar(out value);
        }
    }

    public unsafe void Write<T>(T value)
        where T : unmanaged
    {
        lock (_lock)
        {
            if (!TryWriteScalar(value))
            {
                throw new InvalidOperationException(
                    "The ring does not have enough free capacity for the requested value.");
            }

            Monitor.PulseAll(_lock);
        }
    }

    public unsafe bool TryWrite<T>(T value)
        where T : unmanaged
    {
        lock (_lock)
        {
            bool completed = TryWriteScalar(value);
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
        return ReadValues(destination.AsSpan(offset, count));
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
        return Write((ReadOnlySpan<T>)source.AsSpan(offset, count));
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
        return TryRead(destination.AsSpan(offset, count));
    }

    public unsafe bool TryRead<T>(T* destination, int offset, int count)
        where T : unmanaged
    {
        IntegralBufferGuards.ValidatePointer(
            destination,
            offset,
            count,
            nameof(destination));

        lock (_lock)
        {
            return TryReadCore(destination + offset, count);
        }
    }

    public unsafe bool TryRead<T>(Span<T> destination)
        where T : unmanaged
    {
        if (destination.IsEmpty)
        {
            lock (_lock)
            {
                return _storage.IsOpen;
            }
        }

        fixed (T* dst = destination)
        {
            lock (_lock)
            {
                return TryReadCore(dst, destination.Length);
            }
        }
    }

    public bool TryWrite<T>(T[] source, int offset, int count)
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(source);
        return TryWrite((ReadOnlySpan<T>)source.AsSpan(offset, count));
    }

    public unsafe bool TryWrite<T>(T* source, int offset, int count)
        where T : unmanaged
    {
        IntegralBufferGuards.ValidatePointer(source, offset, count, nameof(source));

        lock (_lock)
        {
            bool completed = TryWriteCore(source + offset, count);
            if (completed && count > 0)
            {
                Monitor.PulseAll(_lock);
            }

            return completed;
        }
    }

    public unsafe bool TryWrite<T>(ReadOnlySpan<T> source)
        where T : unmanaged
    {
        if (source.IsEmpty)
        {
            lock (_lock)
            {
                return _storage.IsOpen;
            }
        }

        fixed (T* src = source)
        {
            lock (_lock)
            {
                bool completed = TryWriteCore(src, source.Length);
                if (completed)
                {
                    Monitor.PulseAll(_lock);
                }

                return completed;
            }
        }
    }

    public int Read<T>(Span<T> destination)
        where T : unmanaged
    {
        return ReadValues(destination);
    }

    public unsafe int Write<T>(ReadOnlySpan<T> source)
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
                int count = WriteCore(src, source.Length);
                if (count > 0)
                {
                    Monitor.PulseAll(_lock);
                }

                return count;
            }
        }
    }

    public void Advance(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
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
            if (!_storage.IsOpen)
            {
                return 0;
            }

            int n = Math.Min(destination.Length, _storage.StoredBytes);
            return n == 0 ? 0 : _storage.Read(destination[..n]);
        }
    }

    private int WriteBytes(ReadOnlySpan<byte> source)
    {
        lock (_lock)
        {
            int n = Math.Min(source.Length, _storage.FreeBytes);
            if (n == 0)
            {
                return 0;
            }

            int count = _storage.Write(source[..n]);
            if (count > 0)
            {
                Monitor.PulseAll(_lock);
            }

            return count;
        }
    }

    private unsafe int ReadBytes(byte* destination, int byteCount)
    {
        lock (_lock)
        {
            WaitForBytes(byteCount, nameof(destination));
            if (!_storage.IsOpen)
            {
                return 0;
            }

            int n = Math.Min(byteCount, _storage.StoredBytes);
            return n == 0 ? 0 : _storage.Read(destination, n);
        }
    }

    private unsafe int WriteBytes(byte* source, int byteCount)
    {
        lock (_lock)
        {
            int n = Math.Min(byteCount, _storage.FreeBytes);
            if (n == 0)
            {
                return 0;
            }

            int count = _storage.Write(source, n);
            if (count > 0)
            {
                Monitor.PulseAll(_lock);
            }

            return count;
        }
    }

    private unsafe int ReadValues<T>(Span<T> destination)
        where T : unmanaged
    {
        int requiredBytes = checked(destination.Length * Unsafe.SizeOf<T>());

        if (destination.IsEmpty)
        {
            lock (_lock)
            {
                WaitForBytes(0, nameof(destination));
                return 0;
            }
        }

        fixed (T* dst = destination)
        {
            lock (_lock)
            {
                WaitForBytes(requiredBytes, nameof(destination));
                return _storage.IsOpen
                    ? ReadCore(dst, destination.Length)
                    : 0;
            }
        }
    }

    private void WaitForBytes(int requiredBytes, string parameterName)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            requiredBytes,
            _storage.ByteCapacity,
            parameterName);

        while (_storage.IsOpen && _storage.StoredBytes < requiredBytes)
        {
            Monitor.Wait(_lock);
        }
    }

    private unsafe bool TryReadScalar<T>(out T value)
        where T : unmanaged
    {
        int n = Unsafe.SizeOf<T>();
        if (!_storage.IsOpen || _storage.StoredBytes < n)
        {
            value = default;
            return false;
        }

        T tmp = default;
        byte* p = (byte*)&tmp;
        switch (n)
        {
            case 1:
                _storage.Read(p, 1);
                break;
            case 2:
                if (IntegralWire.NeedsSwapForBeWire(Unsafe.SizeOf<T>()))
                {
                    _storage.ReadLE2(p);
                }
                else
                {
                    _storage.ReadBE2(p);
                }
                break;
            case 4:
                if (IntegralWire.NeedsSwapForBeWire(Unsafe.SizeOf<T>()))
                {
                    _storage.ReadLE4(p);
                }
                else
                {
                    _storage.ReadBE4(p);
                }
                break;
            case 8:
                if (IntegralWire.NeedsSwapForBeWire(Unsafe.SizeOf<T>()))
                {
                    _storage.ReadLE8(p);
                }
                else
                {
                    _storage.ReadBE8(p);
                }
                break;
            default:
                _storage.Read(p, n);
                if (IntegralWire.NeedsSwapForBeWire(Unsafe.SizeOf<T>()))
                {
                    tmp = IntegralEndianness.ReverseValue(tmp);
                }
                break;
        }

        value = tmp;
        return true;
    }

    private unsafe bool TryWriteScalar<T>(T value)
        where T : unmanaged
    {
        int n = Unsafe.SizeOf<T>();
        if (!_storage.IsOpen || _storage.FreeBytes < n)
        {
            return false;
        }

        switch (n)
        {
            case 1:
                _storage.Write((byte*)&value, 1);
                break;
            case 2:
                if (IntegralWire.NeedsSwapForBeWire(Unsafe.SizeOf<T>()))
                {
                    _storage.WriteLE2((byte*)&value);
                }
                else
                {
                    _storage.WriteBE2((byte*)&value);
                }
                break;
            case 4:
                if (IntegralWire.NeedsSwapForBeWire(Unsafe.SizeOf<T>()))
                {
                    _storage.WriteLE4((byte*)&value);
                }
                else
                {
                    _storage.WriteBE4((byte*)&value);
                }
                break;
            case 8:
                if (IntegralWire.NeedsSwapForBeWire(Unsafe.SizeOf<T>()))
                {
                    _storage.WriteLE8((byte*)&value);
                }
                else
                {
                    _storage.WriteBE8((byte*)&value);
                }
                break;
            default:
                if (IntegralWire.NeedsSwapForBeWire(Unsafe.SizeOf<T>()))
                {
                    value = IntegralEndianness.ReverseValue(value);
                }

                _storage.Write((byte*)&value, n);
                break;
        }

        return true;
    }

    private unsafe int ReadCore<T>(T* destination, int count)
        where T : unmanaged
    {
        if (count <= 0)
        {
            return 0;
        }

        int n = Unsafe.SizeOf<T>();
        int elementCount = Math.Min(count, _storage.StoredBytes / n);
        if (elementCount == 0)
        {
            return 0;
        }

        int bytes = _storage.Read((byte*)destination, elementCount * n);
        Debug.Assert(bytes == elementCount * n);

        if (IntegralWire.NeedsSwapForBeWire(n))
        {
            IntegralWire.ReverseLanesInPlace((byte*)destination, elementCount, n);
        }

        return elementCount;
    }

    private unsafe bool TryReadCore<T>(T* destination, int count)
        where T : unmanaged
    {
        int requiredBytes = checked(count * Unsafe.SizeOf<T>());
        if (!_storage.IsOpen || _storage.StoredBytes < requiredBytes)
        {
            return false;
        }

        int elementCount = ReadCore(destination, count);
        Debug.Assert(elementCount == count);
        return true;
    }

    private unsafe int WriteCore<T>(T* source, int count)
        where T : unmanaged
    {
        if (count <= 0)
        {
            return 0;
        }

        int n = Unsafe.SizeOf<T>();
        int elementCount = Math.Min(count, _storage.FreeBytes / n);
        if (elementCount == 0)
        {
            return 0;
        }

        if (!IntegralWire.NeedsSwapForBeWire(Unsafe.SizeOf<T>()))
        {
            int bytes = _storage.Write((byte*)source, elementCount * n);
            Debug.Assert(bytes == elementCount * n);
            return elementCount;
        }

        WriteReversed(source, elementCount);
        return elementCount;
    }

    private unsafe bool TryWriteCore<T>(T* source, int count)
        where T : unmanaged
    {
        int requiredBytes = checked(count * Unsafe.SizeOf<T>());
        if (!_storage.IsOpen || _storage.FreeBytes < requiredBytes)
        {
            return false;
        }

        int elementCount = WriteCore(source, count);
        Debug.Assert(elementCount == count);
        return true;
    }

    private unsafe void WriteReversed<T>(T* source, int count)
        where T : unmanaged
    {
        int n = Unsafe.SizeOf<T>();
        int scratchCount = Math.Max(1, ScratchByteCount / n);
        byte* scratch = stackalloc byte[scratchCount * n];

        int position = 0;
        while (position < count)
        {
            int chunk = Math.Min(scratchCount, count - position);
            IntegralWire.ReverseCopyLanes(
                (byte*)(source + position),
                scratch,
                chunk,
                n);

            int bytes = _storage.Write(scratch, chunk * n);
            Debug.Assert(bytes == chunk * n);
            position += chunk;
        }
    }
}
