using DotBase.Core;
using DotBase.Integral;
using DotBase.Integral.Internal;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotBase.Buffers.Integral.Internal;


internal sealed class LockedRingBufferLE : DisposableBase, IIntegralRingBuffer
{
    private const int ScratchByteCount = 512;
    private readonly object _lock = new();
    private RingBufferStorage _storage;

    internal LockedRingBufferLE(int capacity)
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
            return IntegralRingOperationsLE.Read(ref _storage, destination);
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
            return IntegralRingOperationsLE.Write(ref _storage, source);
        }
    }

    public bool TryWrite(in IntegralSpan source)
    {
        lock (_lock)
        {
            return IntegralRingOperationsLE.TryWrite(ref _storage, source);
        }
    }

    public int Read(byte[] data, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(data);
        Span<byte> destination = data.AsSpan(offset, count);

        lock (_lock)
        {
            int n = Math.Min(destination.Length, _storage.StoredBytes);
            return n == 0 ? 0 : _storage.Read(destination[..n]);
        }
    }

    public unsafe int Read(byte* data, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(data, offset, count, nameof(data));

        lock (_lock)
        {
            int n = Math.Min(count, _storage.StoredBytes);
            return n == 0 ? 0 : _storage.Read(data + offset, n);
        }
    }

    public int Write(byte[] data, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(data);
        ReadOnlySpan<byte> source = data.AsSpan(offset, count);

        lock (_lock)
        {
            int n = Math.Min(source.Length, _storage.FreeBytes);
            return n == 0 ? 0 : _storage.Write(source[..n]);
        }
    }

    public unsafe int Write(byte* data, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(data, offset, count, nameof(data));

        lock (_lock)
        {
            int n = Math.Min(count, _storage.FreeBytes);
            return n == 0 ? 0 : _storage.Write(data + offset, n);
        }
    }

    public unsafe T Read<T>()
        where T : unmanaged
    {
        lock (_lock)
        {
            if (TryReadScalar(out T value))
            {
                return value;
            }
        }

        throw new InvalidOperationException(
            "The ring does not contain a complete value of the requested type.");
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
        }
    }

    public unsafe bool TryWrite<T>(T value)
        where T : unmanaged
    {
        lock (_lock)
        {
            return TryWriteScalar(value);
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

        lock (_lock)
        {
            return ReadCore(destination + offset, count);
        }
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

        lock (_lock)
        {
            return WriteCore(source + offset, count);
        }
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
            return TryWriteCore(source + offset, count);
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
                return TryWriteCore(src, source.Length);
            }
        }
    }

    public unsafe int Read<T>(Span<T> destination)
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
                return ReadCore(dst, destination.Length);
            }
        }
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
                return WriteCore(src, source.Length);
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


internal sealed class LockedRingBufferBE : DisposableBase, IIntegralRingBuffer
{
    private const int ScratchByteCount = 512;
    private readonly object _lock = new();
    private RingBufferStorage _storage;

    internal LockedRingBufferBE(int capacity)
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
            return IntegralRingOperationsBE.Read(ref _storage, destination);
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
            return IntegralRingOperationsBE.Write(ref _storage, source);
        }
    }

    public bool TryWrite(in IntegralSpan source)
    {
        lock (_lock)
        {
            return IntegralRingOperationsBE.TryWrite(ref _storage, source);
        }
    }

    public int Read(byte[] data, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(data);
        Span<byte> destination = data.AsSpan(offset, count);

        lock (_lock)
        {
            int n = Math.Min(destination.Length, _storage.StoredBytes);
            return n == 0 ? 0 : _storage.Read(destination[..n]);
        }
    }

    public unsafe int Read(byte* data, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(data, offset, count, nameof(data));

        lock (_lock)
        {
            int n = Math.Min(count, _storage.StoredBytes);
            return n == 0 ? 0 : _storage.Read(data + offset, n);
        }
    }

    public int Write(byte[] data, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(data);
        ReadOnlySpan<byte> source = data.AsSpan(offset, count);

        lock (_lock)
        {
            int n = Math.Min(source.Length, _storage.FreeBytes);
            return n == 0 ? 0 : _storage.Write(source[..n]);
        }
    }

    public unsafe int Write(byte* data, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(data, offset, count, nameof(data));

        lock (_lock)
        {
            int n = Math.Min(count, _storage.FreeBytes);
            return n == 0 ? 0 : _storage.Write(data + offset, n);
        }
    }

    public unsafe T Read<T>()
        where T : unmanaged
    {
        lock (_lock)
        {
            if (TryReadScalar(out T value))
            {
                return value;
            }
        }

        throw new InvalidOperationException(
            "The ring does not contain a complete value of the requested type.");
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
        }
    }

    public unsafe bool TryWrite<T>(T value)
        where T : unmanaged
    {
        lock (_lock)
        {
            return TryWriteScalar(value);
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

        lock (_lock)
        {
            return ReadCore(destination + offset, count);
        }
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

        lock (_lock)
        {
            return WriteCore(source + offset, count);
        }
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
            return TryWriteCore(source + offset, count);
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
                return TryWriteCore(src, source.Length);
            }
        }
    }

    public unsafe int Read<T>(Span<T> destination)
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
                return ReadCore(dst, destination.Length);
            }
        }
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
                return WriteCore(src, source.Length);
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
