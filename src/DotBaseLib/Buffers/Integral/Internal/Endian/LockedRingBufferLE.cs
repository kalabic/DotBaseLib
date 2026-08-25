using DotBase.Integral;
using DotBase.Integral.Internal;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotBase.Buffers.Integral.Internal.Endian;


internal sealed class LockedRingBufferLE 
    : LockedRingBuffer
{
    private const int ScratchByteCount = 512;

    internal LockedRingBufferLE(int capacity)
        : base(capacity, IntegralFormat.LittleEndianStream)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
    }

    internal LockedRingBufferLE(int capacity, IntegralFormat format)
        : base(capacity, format)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
    }

    public override ByteOrder ByteOrder => ByteOrder.LittleEndian;

    public override void AdvanceBy<T>(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        lock (_lock)
        {
            _storage.Advance(
                checked((int)(
                    (long)count * Unsafe.SizeOf<T>())));
        }
    }

    public override int Read(in IntegralSpan destination)
    {
        lock (_lock)
        {
            return IntegralRingOperationsLE.Read(ref _storage, destination);
        }
    }

    public override bool TryRead(in IntegralSpan destination)
    {
        lock (_lock)
        {
            return IntegralRingOperationsLE.TryRead(ref _storage, destination);
        }
    }

    public override int Write(in IntegralSpan source)
    {
        lock (_lock)
        {
            return IntegralRingOperationsLE.Write(ref _storage, source);
        }
    }

    public override bool TryWrite(in IntegralSpan source)
    {
        lock (_lock)
        {
            return IntegralRingOperationsLE.TryWrite(ref _storage, source);
        }
    }

    public override int ReadChecked(in IntegralSpan destination)
    {
        lock (_lock)
        {
            return IntegralRingOperationsLE.ReadChecked(ref _storage, destination);
        }
    }

    public override bool TryReadChecked(in IntegralSpan destination)
    {
        lock (_lock)
        {
            return IntegralRingOperationsLE.TryReadChecked(ref _storage, destination);
        }
    }

    public override int WriteChecked(in IntegralSpan source)
    {
        lock (_lock)
        {
            return IntegralRingOperationsLE.WriteChecked(ref _storage, source);
        }
    }

    public override bool TryWriteChecked(in IntegralSpan source)
    {
        lock (_lock)
        {
            return IntegralRingOperationsLE.TryWriteChecked(ref _storage, source);
        }
    }

    public override int Read(byte[] data, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(data);
        Span<byte> destination = data.AsSpan(offset, count);

        lock (_lock)
        {
            int n = Math.Min(destination.Length, _storage.StoredBytes);
            return n == 0 ? 0 : _storage.Read(destination[..n]);
        }
    }

    public override unsafe int Read(byte* data, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(data, offset, count, nameof(data));

        lock (_lock)
        {
            int n = Math.Min(count, _storage.StoredBytes);
            return n == 0 ? 0 : _storage.Read(data + offset, n);
        }
    }

    public override int Write(byte[] data, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(data);
        ReadOnlySpan<byte> source = data.AsSpan(offset, count);

        lock (_lock)
        {
            int n = Math.Min(source.Length, _storage.FreeBytes);
            return n == 0 ? 0 : _storage.Write(source[..n]);
        }
    }

    public override unsafe int Write(byte* data, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(data, offset, count, nameof(data));

        lock (_lock)
        {
            int n = Math.Min(count, _storage.FreeBytes);
            return n == 0 ? 0 : _storage.Write(data + offset, n);
        }
    }

    public override unsafe bool Read<T>(out T value)
    {
        lock (_lock)
        {
            return TryReadScalar(out value);
        }
    }

    public override unsafe bool TryRead<T>(out T value)
    {
        lock (_lock)
        {
            return TryReadScalar(out value);
        }
    }

    public override unsafe bool Write<T>(T value)
    {
        lock (_lock)
        {
            return TryWriteScalar(value);
        }
    }

    public override unsafe bool TryWrite<T>(T value)
    {
        lock (_lock)
        {
            return TryWriteScalar(value);
        }
    }

    public override int Read<T>(T[] destination, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(destination);
        return Read(destination.AsSpan(offset, count));
    }

    public override unsafe int Read<T>(T* destination, int offset, int count)
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

    public override int Write<T>(T[] source, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        return Write((ReadOnlySpan<T>)source.AsSpan(offset, count));
    }

    public override unsafe int Write<T>(T* source, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(source, offset, count, nameof(source));

        lock (_lock)
        {
            return WriteCore(source + offset, count);
        }
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

        lock (_lock)
        {
            return TryReadCore(destination + offset, count);
        }
    }

    public override unsafe bool TryRead<T>(Span<T> destination)
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

    public override bool TryWrite<T>(T[] source, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        return TryWrite((ReadOnlySpan<T>)source.AsSpan(offset, count));
    }

    public override unsafe bool TryWrite<T>(T* source, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(source, offset, count, nameof(source));

        lock (_lock)
        {
            return TryWriteCore(source + offset, count);
        }
    }

    public override unsafe bool TryWrite<T>(ReadOnlySpan<T> source)
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

    public override unsafe int Read<T>(Span<T> destination)
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

    public override unsafe int Write<T>(ReadOnlySpan<T> source)
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

    public override void Advance(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        lock (_lock)
        {
            _storage.Advance(count);
        }
    }

    public override void ClearBuffer()
    {
        lock (_lock)
        {
            _storage.Clear();
        }
    }

    public override void Close()
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
        if (BitConverter.IsLittleEndian)
        {
            switch (n)
            {
                case 1:
                    _storage.Read(p, 1);
                    break;
                case 2:
                    _storage.ReadBE2(p);
                    break;
                case 4:
                    _storage.ReadBE4(p);
                    break;
                case 8:
                    _storage.ReadBE8(p);
                    break;
                default:
                    _storage.Read(p, n);
                    break;
            }
        }
        else
        {
            switch (n)
            {
                case 1:
                    _storage.Read(p, 1);
                    break;
                case 2:
                    _storage.ReadLE2(p);
                    break;
                case 4:
                    _storage.ReadLE4(p);
                    break;
                case 8:
                    _storage.ReadLE8(p);
                    break;
                default:
                    _storage.Read(p, n);
                    tmp = IntegralEndianness.ReverseValue(tmp);
                    break;
            }
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

        if (BitConverter.IsLittleEndian)
        {
            switch (n)
            {
                case 1:
                    _storage.Write((byte*)&value, 1);
                    break;
                case 2:
                    _storage.WriteBE2((byte*)&value);
                    break;
                case 4:
                    _storage.WriteBE4((byte*)&value);
                    break;
                case 8:
                    _storage.WriteBE8((byte*)&value);
                    break;
                default:
                    _storage.Write((byte*)&value, n);
                    break;
            }
        }
        else
        {
            switch (n)
            {
                case 1:
                    _storage.Write((byte*)&value, 1);
                    break;
                case 2:
                    _storage.WriteLE2((byte*)&value);
                    break;
                case 4:
                    _storage.WriteLE4((byte*)&value);
                    break;
                case 8:
                    _storage.WriteLE8((byte*)&value);
                    break;
                default:
                    value = IntegralEndianness.ReverseValue(value);
                    _storage.Write((byte*)&value, n);
                    break;
            }
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

        int bytes = _storage.Read(
            (byte*)destination,
            checked((int)((long)elementCount * n)));
        Debug.Assert(bytes == elementCount * n);

        if (!BitConverter.IsLittleEndian && n > 1)
        {
            IntegralPrimitives.ReverseLanesInPlace((byte*)destination, elementCount, n);
        }

        return elementCount;
    }

    private unsafe bool TryReadCore<T>(T* destination, int count)
        where T : unmanaged
    {
        long requiredBytes = (long)count * Unsafe.SizeOf<T>();
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

        if (BitConverter.IsLittleEndian || n <= 1)
        {
            int bytes = _storage.Write(
                (byte*)source,
                checked((int)((long)elementCount * n)));
            Debug.Assert(bytes == elementCount * n);
            return elementCount;
        }

        WriteReversed(source, elementCount);
        return elementCount;
    }

    private unsafe bool TryWriteCore<T>(T* source, int count)
        where T : unmanaged
    {
        long requiredBytes = (long)count * Unsafe.SizeOf<T>();
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
            IntegralPrimitives.ReverseCopyLanes(
                (byte*)(source + position),
                scratch,
                chunk,
                n);

            int bytes = _storage.Write(
                scratch,
                checked((int)((long)chunk * n)));
            Debug.Assert(bytes == chunk * n);
            position += chunk;
        }
    }
}
