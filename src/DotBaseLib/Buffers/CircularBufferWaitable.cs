using DotBase.Tools;

namespace DotBase.Buffers;


/// <summary>
///
/// One producer and one consumer may operate concurrently, but overlapping Write calls
/// or overlapping Read calls are not supported.
///
/// </summary>
public class CircularBufferWaitable : CircularBufferUnlocked
{
    private readonly object _lock = new object();

    private readonly SimpleWaitableValue<int> _storedByteCount = new();

    public CircularBufferWaitable(int size)
        : base(size)
    {
        if (!IsOpen)
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

        base.Dispose(disposing);
    }

    /// <summary> Will be invoked by base.Dispose(). </summary>
    public override void Close()
    {
        lock (_lock)
        {
            base.Close();
            _storedByteCount.Close();
        }
    }

    //
    // Group: Write
    //


    public override int Write(byte[] data, int offset, int length)
    {
        // Cannot force write if requested length is larger than allocated buffer size.
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, ByteCapacity, nameof(length));

        while (true)
        {
            lock (_lock)
            {
                if (FreeBytes >= length)
                {
                    unsafe { fixed (byte* dataPtr = data) {
                            int bytesWritten = base.Write(dataPtr, offset, length);
                            _storedByteCount.SetValue(StoredBytes);
                            return bytesWritten;
                    } }
                }
            }

            if (!WaitForFreeSpace(length))
            {
                break;
            }
        }

        return 0;
    }

    public override unsafe int Write(byte* data, int offset, int length)
    {
        // Cannot force write if requested length is larger than allocated buffer size.
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, ByteCapacity, nameof(length));

        while (true)
        {
            lock (_lock)
            {
                if (FreeBytes >= length)
                {
                    int bytesWritten = base.Write(data, offset, length);
                    _storedByteCount.SetValue(StoredBytes);
                    return bytesWritten;
                }
            }

            if (!WaitForFreeSpace(length))
            {
                break;
            }
        }

        return 0;
    }

    private bool WaitForFreeSpace(int length)
    {
        return _storedByteCount.WaitLowMarkValue(ByteCapacity - length);
    }

    //
    // Group: Read, Advance, ClearBuffer
    //

    private bool WaitForStoredData(int length)
    {
        return _storedByteCount.WaitHighMarkValue(length);
    }

    public override int Read(byte[] data, int offset, int length)
    {
        // Cannot force read if requested length is larger than allocated buffer size.
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, ByteCapacity, nameof(length));

        while (true)
        {
            lock (_lock)
            {
                if (StoredBytes >= length)
                {
                    unsafe { fixed (byte* dataPtr = data) {
                            int bytesRead = base.Read(dataPtr, offset, length);
                            _storedByteCount.SetValue(StoredBytes);
                            return bytesRead;
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
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, ByteCapacity, nameof(length));

        while (true)
        {
            lock (_lock)
            {
                if (StoredBytes >= length)
                {
                    int bytesRead = base.Read(data, offset, length);
                    _storedByteCount.SetValue(StoredBytes);
                    return bytesRead;
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
            _storedByteCount.SetValue(StoredBytes);
        }
    }

    public override void ClearBuffer()
    {
        lock (_lock)
        {
            base.ClearBuffer();
            _storedByteCount.SetValue(StoredBytes);
        }
    }
}
