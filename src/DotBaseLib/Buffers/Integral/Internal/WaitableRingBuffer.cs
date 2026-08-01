using System.Runtime.CompilerServices;

namespace DotBase.Buffers.Integral.Internal;


internal abstract class WaitableRingBuffer 
    : IntegralRingBufferBase
    , IWaitableRingBuffer
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

    internal WaitableRingBuffer(int capacity)
        : base(capacity)
    { }
}
