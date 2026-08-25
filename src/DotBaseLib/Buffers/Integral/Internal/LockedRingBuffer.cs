using DotBase.Integral;
using System.Runtime.CompilerServices;

namespace DotBase.Buffers.Integral.Internal;


internal abstract class LockedRingBuffer 
    : IntegralRingBufferBase
{
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

    public override int CapacityAsBlockCount()
    {
        lock (_lock)
        {
            return (_format.BytesPerBlock > 0) ? (int)(_storage.ByteCapacity / _format.BytesPerBlock) : 0;
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


    protected readonly object _lock = new();

    internal LockedRingBuffer(int capacity, IntegralFormat format)
        : base(capacity, format)
    { }
}
