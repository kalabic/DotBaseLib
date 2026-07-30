using DotBase.Integral.Internal;

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

    internal unsafe int Read(
        byte* destination,
        int byteCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(byteCount);

        if (!_isOpen || byteCount == 0)
        {
            return 0;
        }

        if (destination is null)
        {
            throw new ArgumentNullException(nameof(destination));
        }

        byteCount = Math.Min(byteCount, _byteCount);
        if (byteCount == 0)
        {
            return 0;
        }

        int firstCount = Math.Min(byteCount, _buffer.Length - _readPosition);
        int secondCount = byteCount - firstCount;

        fixed (byte* bufferPtr = _buffer)
        {
            IntegralByteMemory.Copy(
                bufferPtr + _readPosition,
                destination,
                (nuint)firstCount);

            IntegralByteMemory.Copy(
                bufferPtr,
                destination + firstCount,
                (nuint)secondCount);
        }

        _readPosition = (_readPosition + byteCount) % _buffer.Length;
        _byteCount -= byteCount;
        _totalRead += byteCount;
        return byteCount;
    }

    internal unsafe int Write(
        byte* source,
        int byteCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(byteCount);

        if (!_isOpen || byteCount == 0)
        {
            return 0;
        }

        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        byteCount = Math.Min(byteCount, FreeCount);
        if (byteCount == 0)
        {
            return 0;
        }

        int firstCount = Math.Min(byteCount, _buffer.Length - _writePosition);
        int secondCount = byteCount - firstCount;

        fixed (byte* bufferPtr = _buffer)
        {
            IntegralByteMemory.Copy(
                source,
                bufferPtr + _writePosition,
                (nuint)firstCount);

            IntegralByteMemory.Copy(
                source + firstCount,
                bufferPtr,
                (nuint)secondCount);
        }

        _writePosition = (_writePosition + byteCount) % _buffer.Length;
        _byteCount += byteCount;
        _totalWritten += byteCount;
        return byteCount;
    }

    internal unsafe int Read(Span<byte> destination)
    {
        fixed (byte* destinationPtr = destination)
        {
            return Read(destinationPtr, destination.Length);
        }
    }

    internal unsafe int Write(ReadOnlySpan<byte> source)
    {
        fixed (byte* sourcePtr = source)
        {
            return Write(sourcePtr, source.Length);
        }
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
