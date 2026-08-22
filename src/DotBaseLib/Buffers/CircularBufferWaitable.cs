using DotBase.AsyncValue;

namespace DotBase.Buffers;


/// <summary>
/// One producer and one consumer may operate concurrently, but overlapping Write calls
/// or overlapping Read calls are not supported. Fitting requests wait for the complete
/// byte count. Valid requests larger than the current capacity, or requests terminated
/// by closure, return <c>0</c>.
/// </summary>
public class CircularBufferWaitable : CircularBufferUnlocked
{
    private readonly object _lock = new object();

    private readonly SimpleWaitableLongValue _storedByteCount = new();

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
        ArgumentNullException.ThrowIfNull(data);
        _ = data.AsSpan(offset, length);
        if (length > ByteCapacity)
        {
            return 0;
        }

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
        ValidatePointer(data, offset, length, nameof(data));
        if (length > ByteCapacity)
        {
            return 0;
        }

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
        return _storedByteCount.WaitLessOrEqualTo(ByteCapacity - length);
    }

    //
    // Group: Read, Advance, ClearBuffer
    //

    private bool WaitForStoredData(int length)
    {
        return _storedByteCount.WaitGreaterOrEqualTo(length);
    }

    public override int Read(byte[] data, int offset, int length)
    {
        ArgumentNullException.ThrowIfNull(data);
        _ = data.AsSpan(offset, length);
        if (length > ByteCapacity)
        {
            return 0;
        }

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
        ValidatePointer(data, offset, length, nameof(data));
        if (length > ByteCapacity)
        {
            return 0;
        }

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

    private static unsafe void ValidatePointer(
        byte* data,
        int offset,
        int length,
        string parameterName)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        if (data is null && (offset != 0 || length != 0))
        {
            throw new ArgumentNullException(parameterName);
        }
    }
}
