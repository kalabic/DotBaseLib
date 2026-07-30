using DotBase.Cancellation;

namespace DotBase.Buffers;


/// <summary>
///
/// Concurrent producers are supported but overlapping Read calls from multiple consumers are not.
///
/// </summary>
public class CircularBufferWaitable : CircularBufferUnlocked
{
    private readonly object _lock = new object();

    private readonly CancellableEventSlim _storedByteCountEvent;

    private int _waitingStoredByteCount = 0;

    public CircularBufferWaitable(int size)
        : base(size)
    {
        _storedByteCountEvent = new CancellableEventSlim();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Close();
            _storedByteCountEvent.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary> Will be invoked by base.Dispose(). </summary>
    public override void Close()
    {
        lock (_lock)
        {
            base.Close();
            _storedByteCountEvent.Cancel();
        }
    }

    //
    // Group: Write
    //


    public override int Write(byte[] data, int offset, int length)
    {
        // An array Write wrapper that acquires _lock before calling base.Write(byte[], ...); let the pointer override perform signalling.
        lock (_lock)
        {
            return base.Write(data, offset, length);
        }
    }

    public override unsafe int Write(byte* data, int offset, int length)
    {
        int bytesWritten;
        lock (_lock)
        {
            bytesWritten = base.Write(data, offset, length);
            if (!IsOpen || ((_waitingStoredByteCount > 0) && (Count >= _waitingStoredByteCount)))
            {
                _storedByteCountEvent.Set();
            }
        }
        return bytesWritten;
    }

    private bool ResetStoredByteCountEvent(int length)
    {
        lock (_lock)
        {
            if (!IsOpen || (Count >= length))
            {
                _storedByteCountEvent.Set();
                _waitingStoredByteCount = 0;
                return false;
            }
            else
            {
                _storedByteCountEvent.Reset();
                _waitingStoredByteCount = length;
                return true;
            }
        }
    }

    //
    // Group: Read, Advance, ClearBuffer
    //

    private bool WaitForStoredData(int length)
    {
        if (ResetStoredByteCountEvent(length))
        {
            bool result = _storedByteCountEvent.Wait();
            lock (_lock) { _waitingStoredByteCount = 0; }
            if (!result)
            {
                return false;
            }
        }
        return IsOpen;
    }

    public override int Read(byte[] data, int offset, int length)
    {
        // Cannot force read if requested length is larger than allocated buffer size.
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, Capacity, nameof(length));

        while (true)
        {
            lock (_lock)
            {
                if (Count >= length)
                {
                    unsafe { fixed (byte* dataPtr = data) {
                            return base.Read(dataPtr, offset, length);
                    } }
                }
            }

            if (!WaitForStoredData(length))
            {
                break;
            }
        }

        return 0;
    }

    public override unsafe int Read(byte* data, int offset, int length)
    {
        // Cannot force read if requested length is larger than allocated buffer size.
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, Capacity, nameof(length));

        while (true)
        {
            lock (_lock)
            {
                if (Count >= length)
                {
                    return base.Read(data, offset, length);
                }
            }

            if (!WaitForStoredData(length))
            {
                break;
            }
        }

        return 0;
    }

    public override void Advance(int count)
    {
        lock (_lock)
        {
            base.Advance(count);
        }
    }

    public override void ClearBuffer()
    {
        lock (_lock)
        {
            base.ClearBuffer();
        }
    }
}
