namespace DotBase.Buffers.Integral.Internal;


internal struct RingBufferStorage
{
    private byte[] _buffer;
    private int _readPosition;
    private int _writePosition;
    private int _byteCount;
    private bool _isOpen;
    private long _totalRead;
    private long _totalWritten;

    internal RingBufferStorage(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);

        _isOpen = capacity > 0;
        _buffer = _isOpen ? new byte[capacity] : Array.Empty<byte>();
    }

    internal readonly int Capacity => _buffer.Length;

    internal readonly int Count => _byteCount;

    internal readonly int FreeCount => _buffer.Length - _byteCount;

    internal readonly bool IsOpen => _isOpen;

    internal readonly long TotalRead => _totalRead;

    internal readonly long TotalWritten => _totalWritten;

    internal int Read(Span<byte> destination)
    {
        if (!_isOpen || destination.IsEmpty)
        {
            return 0;
        }

        int byteCount = Math.Min(destination.Length, _byteCount);
        int firstCount = Math.Min(byteCount, _buffer.Length - _readPosition);

        _buffer.AsSpan(_readPosition, firstCount).CopyTo(destination);

        int secondCount = byteCount - firstCount;
        if (secondCount > 0)
        {
            _buffer.AsSpan(0, secondCount).CopyTo(destination[firstCount..]);
        }

        _readPosition = (_readPosition + byteCount) % _buffer.Length;
        _byteCount -= byteCount;
        _totalRead += byteCount;
        return byteCount;
    }

    internal int Write(ReadOnlySpan<byte> source)
    {
        if (!_isOpen || source.IsEmpty)
        {
            return 0;
        }

        int byteCount = Math.Min(source.Length, FreeCount);
        int firstCount = Math.Min(byteCount, _buffer.Length - _writePosition);

        source[..firstCount].CopyTo(_buffer.AsSpan(_writePosition));

        int secondCount = byteCount - firstCount;
        if (secondCount > 0)
        {
            source.Slice(firstCount, secondCount).CopyTo(_buffer);
        }

        _writePosition = (_writePosition + byteCount) % _buffer.Length;
        _byteCount += byteCount;
        _totalWritten += byteCount;
        return byteCount;
    }

    internal void Advance(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        if (!_isOpen)
        {
            return;
        }

        if (count >= _byteCount)
        {
            Clear();
            return;
        }

        _byteCount -= count;
        _readPosition = (_readPosition + count) % _buffer.Length;
    }

    internal void Clear()
    {
        _byteCount = 0;
        _readPosition = 0;
        _writePosition = 0;
    }

    internal void Close()
    {
        Clear();
        _isOpen = false;
        _buffer = Array.Empty<byte>();
    }
}
