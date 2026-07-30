using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DotBase.Buffers.Integral.Internal;


/// <summary>
/// Bare native-backed byte ring FIFO. No argument validation, callers must pass legal
/// pointers, non-negative sizes, and (for read/write) sizes within stored and free bytes.
/// </summary>
/// <remarks>
/// <c>ReadLE*</c> / <c>WriteLE*</c> reverse external buffer bytes relative to ring
/// stream order. <c>ReadBE*</c> / <c>WriteBE*</c> copy stream order as-is.
/// Contiguous transfers use unaligned word load/store; wrap uses case tables.
/// </remarks>
internal unsafe struct RingBufferStorage
{
    private byte* _ptr;
    private int _byteCapacity;
    private int _readPosition;
    private int _writePosition;
    private int _storedBytes;
    private bool _isOpen;
    private long _totalRead;
    private long _totalWritten;

    internal RingBufferStorage(int byteCapacity)
    {
        Debug.Assert(byteCapacity >= 0);

        _byteCapacity = byteCapacity;
        _isOpen = byteCapacity > 0;
        _ptr = _isOpen
            ? (byte*)NativeMemory.Alloc((nuint)byteCapacity)
            : null;
        _readPosition = 0;
        _writePosition = 0;
        _storedBytes = 0;
        _totalRead = 0;
        _totalWritten = 0;
    }

    internal readonly int ByteCapacity => _byteCapacity;

    internal readonly int StoredBytes => _storedBytes;

    internal readonly int FreeBytes => _byteCapacity - _storedBytes;

    internal readonly bool IsOpen => _isOpen;

    internal readonly long TotalRead => _totalRead;

    internal readonly long TotalWritten => _totalWritten;

    /// <summary>
    /// Exactly <paramref name="byteCount"/> bytes into <paramref name="destination"/>.
    /// Requires 0 ≤ byteCount ≤ Count and a valid destination when byteCount &gt; 0.
    /// </summary>
    internal int Read(byte* destination, int byteCount)
    {
        Debug.Assert(byteCount >= 0);
        Debug.Assert(byteCount <= _storedBytes);
        Debug.Assert(byteCount == 0 || destination is not null);

        if (byteCount == 0)
        {
            return 0;
        }

        int firstCount = Math.Min(byteCount, _byteCapacity - _readPosition);
        int secondCount = byteCount - firstCount;

        Buffer.MemoryCopy(
            _ptr + _readPosition,
            destination,
            (ulong)firstCount,
            (ulong)firstCount);

        if (secondCount > 0)
        {
            Buffer.MemoryCopy(
                _ptr,
                destination + firstCount,
                (ulong)secondCount,
                (ulong)secondCount);
        }

        AdvanceReadHead(byteCount);
        return byteCount;
    }

    /// <summary>
    /// Exactly <paramref name="byteCount"/> bytes from <paramref name="source"/>.
    /// Requires 0 ≤ byteCount ≤ FreeCount and a valid source when byteCount &gt; 0.
    /// </summary>
    internal int Write(byte* source, int byteCount)
    {
        Debug.Assert(byteCount >= 0);
        Debug.Assert(byteCount <= FreeBytes);
        Debug.Assert(byteCount == 0 || source is not null);

        if (byteCount == 0)
        {
            return 0;
        }

        int firstCount = Math.Min(byteCount, _byteCapacity - _writePosition);
        int secondCount = byteCount - firstCount;

        Buffer.MemoryCopy(
            source,
            _ptr + _writePosition,
            (ulong)firstCount,
            (ulong)firstCount);

        if (secondCount > 0)
        {
            Buffer.MemoryCopy(
                source + firstCount,
                _ptr,
                (ulong)secondCount,
                (ulong)secondCount);
        }

        AdvanceWriteHead(byteCount);
        return byteCount;
    }

    internal int ReadBE2(byte* destination)
    {
        Debug.Assert(_storedBytes >= 2);
        Debug.Assert(destination is not null);

        if (_readPosition + 2 <= _byteCapacity)
        {
            Unsafe.WriteUnaligned(
                destination,
                Unsafe.ReadUnaligned<ushort>(_ptr + _readPosition));
            AdvanceReadHead(2);
            return 2;
        }

        // Wrap: stream [rp], [0]
        destination[0] = _ptr[_readPosition];
        destination[1] = _ptr[0];
        AdvanceReadHead(2);
        return 2;
    }

    internal int ReadBE4(byte* destination)
    {
        Debug.Assert(_storedBytes >= 4);
        Debug.Assert(destination is not null);

        if (_readPosition + 4 <= _byteCapacity)
        {
            Unsafe.WriteUnaligned(
                destination,
                Unsafe.ReadUnaligned<uint>(_ptr + _readPosition));
            AdvanceReadHead(4);
            return 4;
        }

        int firstCount = _byteCapacity - _readPosition;
        int secondCount = 4 - firstCount;

        if (secondCount == 1)
        {
            destination[0] = _ptr[_readPosition];
            destination[1] = _ptr[_readPosition + 1];
            destination[2] = _ptr[_readPosition + 2];
            destination[3] = _ptr[0];
        }
        else if (secondCount == 2)
        {
            destination[0] = _ptr[_readPosition];
            destination[1] = _ptr[_readPosition + 1];
            destination[2] = _ptr[0];
            destination[3] = _ptr[1];
        }
        else
        {
            // secondCount == 3
            destination[0] = _ptr[_readPosition];
            destination[1] = _ptr[0];
            destination[2] = _ptr[1];
            destination[3] = _ptr[2];
        }

        AdvanceReadHead(4);
        return 4;
    }

    internal int ReadBE8(byte* destination)
    {
        Debug.Assert(_storedBytes >= 8);
        Debug.Assert(destination is not null);

        if (_readPosition + 8 <= _byteCapacity)
        {
            Unsafe.WriteUnaligned(
                destination,
                Unsafe.ReadUnaligned<ulong>(_ptr + _readPosition));
            AdvanceReadHead(8);
            return 8;
        }

        int firstCount = _byteCapacity - _readPosition;
        int secondCount = 8 - firstCount;

        if (secondCount == 1)
        {
            destination[0] = _ptr[_readPosition];
            destination[1] = _ptr[_readPosition + 1];
            destination[2] = _ptr[_readPosition + 2];
            destination[3] = _ptr[_readPosition + 3];
            destination[4] = _ptr[_readPosition + 4];
            destination[5] = _ptr[_readPosition + 5];
            destination[6] = _ptr[_readPosition + 6];
            destination[7] = _ptr[0];
        }
        else if (secondCount == 2)
        {
            destination[0] = _ptr[_readPosition];
            destination[1] = _ptr[_readPosition + 1];
            destination[2] = _ptr[_readPosition + 2];
            destination[3] = _ptr[_readPosition + 3];
            destination[4] = _ptr[_readPosition + 4];
            destination[5] = _ptr[_readPosition + 5];
            destination[6] = _ptr[0];
            destination[7] = _ptr[1];
        }
        else if (secondCount == 3)
        {
            destination[0] = _ptr[_readPosition];
            destination[1] = _ptr[_readPosition + 1];
            destination[2] = _ptr[_readPosition + 2];
            destination[3] = _ptr[_readPosition + 3];
            destination[4] = _ptr[_readPosition + 4];
            destination[5] = _ptr[0];
            destination[6] = _ptr[1];
            destination[7] = _ptr[2];
        }
        else if (secondCount == 4)
        {
            destination[0] = _ptr[_readPosition];
            destination[1] = _ptr[_readPosition + 1];
            destination[2] = _ptr[_readPosition + 2];
            destination[3] = _ptr[_readPosition + 3];
            destination[4] = _ptr[0];
            destination[5] = _ptr[1];
            destination[6] = _ptr[2];
            destination[7] = _ptr[3];
        }
        else if (secondCount == 5)
        {
            destination[0] = _ptr[_readPosition];
            destination[1] = _ptr[_readPosition + 1];
            destination[2] = _ptr[_readPosition + 2];
            destination[3] = _ptr[0];
            destination[4] = _ptr[1];
            destination[5] = _ptr[2];
            destination[6] = _ptr[3];
            destination[7] = _ptr[4];
        }
        else if (secondCount == 6)
        {
            destination[0] = _ptr[_readPosition];
            destination[1] = _ptr[_readPosition + 1];
            destination[2] = _ptr[0];
            destination[3] = _ptr[1];
            destination[4] = _ptr[2];
            destination[5] = _ptr[3];
            destination[6] = _ptr[4];
            destination[7] = _ptr[5];
        }
        else
        {
            // secondCount == 7
            destination[0] = _ptr[_readPosition];
            destination[1] = _ptr[0];
            destination[2] = _ptr[1];
            destination[3] = _ptr[2];
            destination[4] = _ptr[3];
            destination[5] = _ptr[4];
            destination[6] = _ptr[5];
            destination[7] = _ptr[6];
        }

        AdvanceReadHead(8);
        return 8;
    }

    internal int WriteBE2(byte* source)
    {
        Debug.Assert(FreeBytes >= 2);
        Debug.Assert(source is not null);

        if (_writePosition + 2 <= _byteCapacity)
        {
            Unsafe.WriteUnaligned(
                _ptr + _writePosition,
                Unsafe.ReadUnaligned<ushort>(source));
            AdvanceWriteHead(2);
            return 2;
        }

        _ptr[_writePosition] = source[0];
        _ptr[0] = source[1];
        AdvanceWriteHead(2);
        return 2;
    }

    internal int WriteBE4(byte* source)
    {
        Debug.Assert(FreeBytes >= 4);
        Debug.Assert(source is not null);

        if (_writePosition + 4 <= _byteCapacity)
        {
            Unsafe.WriteUnaligned(
                _ptr + _writePosition,
                Unsafe.ReadUnaligned<uint>(source));
            AdvanceWriteHead(4);
            return 4;
        }

        int firstCount = _byteCapacity - _writePosition;
        int secondCount = 4 - firstCount;

        if (secondCount == 1)
        {
            _ptr[_writePosition] = source[0];
            _ptr[_writePosition + 1] = source[1];
            _ptr[_writePosition + 2] = source[2];
            _ptr[0] = source[3];
        }
        else if (secondCount == 2)
        {
            _ptr[_writePosition] = source[0];
            _ptr[_writePosition + 1] = source[1];
            _ptr[0] = source[2];
            _ptr[1] = source[3];
        }
        else
        {
            _ptr[_writePosition] = source[0];
            _ptr[0] = source[1];
            _ptr[1] = source[2];
            _ptr[2] = source[3];
        }

        AdvanceWriteHead(4);
        return 4;
    }

    internal int WriteBE8(byte* source)
    {
        Debug.Assert(FreeBytes >= 8);
        Debug.Assert(source is not null);

        if (_writePosition + 8 <= _byteCapacity)
        {
            Unsafe.WriteUnaligned(
                _ptr + _writePosition,
                Unsafe.ReadUnaligned<ulong>(source));
            AdvanceWriteHead(8);
            return 8;
        }

        int firstCount = _byteCapacity - _writePosition;
        int secondCount = 8 - firstCount;

        if (secondCount == 1)
        {
            _ptr[_writePosition] = source[0];
            _ptr[_writePosition + 1] = source[1];
            _ptr[_writePosition + 2] = source[2];
            _ptr[_writePosition + 3] = source[3];
            _ptr[_writePosition + 4] = source[4];
            _ptr[_writePosition + 5] = source[5];
            _ptr[_writePosition + 6] = source[6];
            _ptr[0] = source[7];
        }
        else if (secondCount == 2)
        {
            _ptr[_writePosition] = source[0];
            _ptr[_writePosition + 1] = source[1];
            _ptr[_writePosition + 2] = source[2];
            _ptr[_writePosition + 3] = source[3];
            _ptr[_writePosition + 4] = source[4];
            _ptr[_writePosition + 5] = source[5];
            _ptr[0] = source[6];
            _ptr[1] = source[7];
        }
        else if (secondCount == 3)
        {
            _ptr[_writePosition] = source[0];
            _ptr[_writePosition + 1] = source[1];
            _ptr[_writePosition + 2] = source[2];
            _ptr[_writePosition + 3] = source[3];
            _ptr[_writePosition + 4] = source[4];
            _ptr[0] = source[5];
            _ptr[1] = source[6];
            _ptr[2] = source[7];
        }
        else if (secondCount == 4)
        {
            _ptr[_writePosition] = source[0];
            _ptr[_writePosition + 1] = source[1];
            _ptr[_writePosition + 2] = source[2];
            _ptr[_writePosition + 3] = source[3];
            _ptr[0] = source[4];
            _ptr[1] = source[5];
            _ptr[2] = source[6];
            _ptr[3] = source[7];
        }
        else if (secondCount == 5)
        {
            _ptr[_writePosition] = source[0];
            _ptr[_writePosition + 1] = source[1];
            _ptr[_writePosition + 2] = source[2];
            _ptr[0] = source[3];
            _ptr[1] = source[4];
            _ptr[2] = source[5];
            _ptr[3] = source[6];
            _ptr[4] = source[7];
        }
        else if (secondCount == 6)
        {
            _ptr[_writePosition] = source[0];
            _ptr[_writePosition + 1] = source[1];
            _ptr[0] = source[2];
            _ptr[1] = source[3];
            _ptr[2] = source[4];
            _ptr[3] = source[5];
            _ptr[4] = source[6];
            _ptr[5] = source[7];
        }
        else
        {
            _ptr[_writePosition] = source[0];
            _ptr[0] = source[1];
            _ptr[1] = source[2];
            _ptr[2] = source[3];
            _ptr[3] = source[4];
            _ptr[4] = source[5];
            _ptr[5] = source[6];
            _ptr[6] = source[7];
        }

        AdvanceWriteHead(8);
        return 8;
    }

    internal int ReadLE2(byte* destination)
    {
        Debug.Assert(_storedBytes >= 2);
        Debug.Assert(destination is not null);

        if (_readPosition + 2 <= _byteCapacity)
        {
            ushort raw = Unsafe.ReadUnaligned<ushort>(_ptr + _readPosition);
            Unsafe.WriteUnaligned(
                destination,
                BinaryPrimitives.ReverseEndianness(raw));
            AdvanceReadHead(2);
            return 2;
        }

        // Wrap: stream [rp], [0] → reverse to dest
        destination[0] = _ptr[0];
        destination[1] = _ptr[_readPosition];
        AdvanceReadHead(2);
        return 2;
    }

    internal int ReadLE4(byte* destination)
    {
        Debug.Assert(_storedBytes >= 4);
        Debug.Assert(destination is not null);

        if (_readPosition + 4 <= _byteCapacity)
        {
            uint raw = Unsafe.ReadUnaligned<uint>(_ptr + _readPosition);
            Unsafe.WriteUnaligned(
                destination,
                BinaryPrimitives.ReverseEndianness(raw));
            AdvanceReadHead(4);
            return 4;
        }

        int firstCount = _byteCapacity - _readPosition;
        int secondCount = 4 - firstCount;

        if (secondCount == 1)
        {
            destination[0] = _ptr[0];
            destination[1] = _ptr[_readPosition + 2];
            destination[2] = _ptr[_readPosition + 1];
            destination[3] = _ptr[_readPosition];
        }
        else if (secondCount == 2)
        {
            destination[0] = _ptr[1];
            destination[1] = _ptr[0];
            destination[2] = _ptr[_readPosition + 1];
            destination[3] = _ptr[_readPosition];
        }
        else
        {
            destination[0] = _ptr[2];
            destination[1] = _ptr[1];
            destination[2] = _ptr[0];
            destination[3] = _ptr[_readPosition];
        }

        AdvanceReadHead(4);
        return 4;
    }

    internal int ReadLE8(byte* destination)
    {
        Debug.Assert(_storedBytes >= 8);
        Debug.Assert(destination is not null);

        if (_readPosition + 8 <= _byteCapacity)
        {
            ulong raw = Unsafe.ReadUnaligned<ulong>(_ptr + _readPosition);
            Unsafe.WriteUnaligned(
                destination,
                BinaryPrimitives.ReverseEndianness(raw));
            AdvanceReadHead(8);
            return 8;
        }

        int firstCount = _byteCapacity - _readPosition;
        int secondCount = 8 - firstCount;

        if (secondCount == 1)
        {
            destination[0] = _ptr[0];
            destination[1] = _ptr[_readPosition + 6];
            destination[2] = _ptr[_readPosition + 5];
            destination[3] = _ptr[_readPosition + 4];
            destination[4] = _ptr[_readPosition + 3];
            destination[5] = _ptr[_readPosition + 2];
            destination[6] = _ptr[_readPosition + 1];
            destination[7] = _ptr[_readPosition];
        }
        else if (secondCount == 2)
        {
            destination[0] = _ptr[1];
            destination[1] = _ptr[0];
            destination[2] = _ptr[_readPosition + 5];
            destination[3] = _ptr[_readPosition + 4];
            destination[4] = _ptr[_readPosition + 3];
            destination[5] = _ptr[_readPosition + 2];
            destination[6] = _ptr[_readPosition + 1];
            destination[7] = _ptr[_readPosition];
        }
        else if (secondCount == 3)
        {
            destination[0] = _ptr[2];
            destination[1] = _ptr[1];
            destination[2] = _ptr[0];
            destination[3] = _ptr[_readPosition + 4];
            destination[4] = _ptr[_readPosition + 3];
            destination[5] = _ptr[_readPosition + 2];
            destination[6] = _ptr[_readPosition + 1];
            destination[7] = _ptr[_readPosition];
        }
        else if (secondCount == 4)
        {
            destination[0] = _ptr[3];
            destination[1] = _ptr[2];
            destination[2] = _ptr[1];
            destination[3] = _ptr[0];
            destination[4] = _ptr[_readPosition + 3];
            destination[5] = _ptr[_readPosition + 2];
            destination[6] = _ptr[_readPosition + 1];
            destination[7] = _ptr[_readPosition];
        }
        else if (secondCount == 5)
        {
            destination[0] = _ptr[4];
            destination[1] = _ptr[3];
            destination[2] = _ptr[2];
            destination[3] = _ptr[1];
            destination[4] = _ptr[0];
            destination[5] = _ptr[_readPosition + 2];
            destination[6] = _ptr[_readPosition + 1];
            destination[7] = _ptr[_readPosition];
        }
        else if (secondCount == 6)
        {
            destination[0] = _ptr[5];
            destination[1] = _ptr[4];
            destination[2] = _ptr[3];
            destination[3] = _ptr[2];
            destination[4] = _ptr[1];
            destination[5] = _ptr[0];
            destination[6] = _ptr[_readPosition + 1];
            destination[7] = _ptr[_readPosition];
        }
        else
        {
            destination[0] = _ptr[6];
            destination[1] = _ptr[5];
            destination[2] = _ptr[4];
            destination[3] = _ptr[3];
            destination[4] = _ptr[2];
            destination[5] = _ptr[1];
            destination[6] = _ptr[0];
            destination[7] = _ptr[_readPosition];
        }

        AdvanceReadHead(8);
        return 8;
    }

    internal int WriteLE2(byte* source)
    {
        Debug.Assert(FreeBytes >= 2);
        Debug.Assert(source is not null);

        if (_writePosition + 2 <= _byteCapacity)
        {
            ushort raw = Unsafe.ReadUnaligned<ushort>(source);
            Unsafe.WriteUnaligned(
                _ptr + _writePosition,
                BinaryPrimitives.ReverseEndianness(raw));
            AdvanceWriteHead(2);
            return 2;
        }

        _ptr[_writePosition] = source[1];
        _ptr[0] = source[0];
        AdvanceWriteHead(2);
        return 2;
    }

    internal int WriteLE4(byte* source)
    {
        Debug.Assert(FreeBytes >= 4);
        Debug.Assert(source is not null);

        if (_writePosition + 4 <= _byteCapacity)
        {
            uint raw = Unsafe.ReadUnaligned<uint>(source);
            Unsafe.WriteUnaligned(
                _ptr + _writePosition,
                BinaryPrimitives.ReverseEndianness(raw));
            AdvanceWriteHead(4);
            return 4;
        }

        int firstCount = _byteCapacity - _writePosition;
        int secondCount = 4 - firstCount;

        if (secondCount == 1)
        {
            _ptr[_writePosition] = source[3];
            _ptr[_writePosition + 1] = source[2];
            _ptr[_writePosition + 2] = source[1];
            _ptr[0] = source[0];
        }
        else if (secondCount == 2)
        {
            _ptr[_writePosition] = source[3];
            _ptr[_writePosition + 1] = source[2];
            _ptr[0] = source[1];
            _ptr[1] = source[0];
        }
        else
        {
            _ptr[_writePosition] = source[3];
            _ptr[0] = source[2];
            _ptr[1] = source[1];
            _ptr[2] = source[0];
        }

        AdvanceWriteHead(4);
        return 4;
    }

    internal int WriteLE8(byte* source)
    {
        Debug.Assert(FreeBytes >= 8);
        Debug.Assert(source is not null);

        if (_writePosition + 8 <= _byteCapacity)
        {
            ulong raw = Unsafe.ReadUnaligned<ulong>(source);
            Unsafe.WriteUnaligned(
                _ptr + _writePosition,
                BinaryPrimitives.ReverseEndianness(raw));
            AdvanceWriteHead(8);
            return 8;
        }

        int firstCount = _byteCapacity - _writePosition;
        int secondCount = 8 - firstCount;

        if (secondCount == 1)
        {
            _ptr[_writePosition] = source[7];
            _ptr[_writePosition + 1] = source[6];
            _ptr[_writePosition + 2] = source[5];
            _ptr[_writePosition + 3] = source[4];
            _ptr[_writePosition + 4] = source[3];
            _ptr[_writePosition + 5] = source[2];
            _ptr[_writePosition + 6] = source[1];
            _ptr[0] = source[0];
        }
        else if (secondCount == 2)
        {
            _ptr[_writePosition] = source[7];
            _ptr[_writePosition + 1] = source[6];
            _ptr[_writePosition + 2] = source[5];
            _ptr[_writePosition + 3] = source[4];
            _ptr[_writePosition + 4] = source[3];
            _ptr[_writePosition + 5] = source[2];
            _ptr[0] = source[1];
            _ptr[1] = source[0];
        }
        else if (secondCount == 3)
        {
            _ptr[_writePosition] = source[7];
            _ptr[_writePosition + 1] = source[6];
            _ptr[_writePosition + 2] = source[5];
            _ptr[_writePosition + 3] = source[4];
            _ptr[_writePosition + 4] = source[3];
            _ptr[0] = source[2];
            _ptr[1] = source[1];
            _ptr[2] = source[0];
        }
        else if (secondCount == 4)
        {
            _ptr[_writePosition] = source[7];
            _ptr[_writePosition + 1] = source[6];
            _ptr[_writePosition + 2] = source[5];
            _ptr[_writePosition + 3] = source[4];
            _ptr[0] = source[3];
            _ptr[1] = source[2];
            _ptr[2] = source[1];
            _ptr[3] = source[0];
        }
        else if (secondCount == 5)
        {
            _ptr[_writePosition] = source[7];
            _ptr[_writePosition + 1] = source[6];
            _ptr[_writePosition + 2] = source[5];
            _ptr[0] = source[4];
            _ptr[1] = source[3];
            _ptr[2] = source[2];
            _ptr[3] = source[1];
            _ptr[4] = source[0];
        }
        else if (secondCount == 6)
        {
            _ptr[_writePosition] = source[7];
            _ptr[_writePosition + 1] = source[6];
            _ptr[0] = source[5];
            _ptr[1] = source[4];
            _ptr[2] = source[3];
            _ptr[3] = source[2];
            _ptr[4] = source[1];
            _ptr[5] = source[0];
        }
        else
        {
            _ptr[_writePosition] = source[7];
            _ptr[0] = source[6];
            _ptr[1] = source[5];
            _ptr[2] = source[4];
            _ptr[3] = source[3];
            _ptr[4] = source[2];
            _ptr[5] = source[1];
            _ptr[6] = source[0];
        }

        AdvanceWriteHead(8);
        return 8;
    }

    internal int Read(Span<byte> destination)
    {
        if (destination.IsEmpty)
        {
            return 0;
        }

        fixed (byte* destinationPtr = destination)
        {
            return Read(destinationPtr, destination.Length);
        }
    }

    internal int Write(ReadOnlySpan<byte> source)
    {
        if (source.IsEmpty)
        {
            return 0;
        }

        fixed (byte* sourcePtr = source)
        {
            return Write(sourcePtr, source.Length);
        }
    }

    internal void Advance(int count)
    {
        Debug.Assert(count >= 0);

        if (count == 0)
        {
            return;
        }

        if (count >= _storedBytes)
        {
            Clear();
            return;
        }

        _storedBytes -= count;
        _readPosition += count;
        if (_readPosition >= _byteCapacity)
        {
            _readPosition -= _byteCapacity;
        }
    }

    internal void Clear()
    {
        _storedBytes = 0;
        _readPosition = 0;
        _writePosition = 0;
    }

    internal void Close()
    {
        Clear();
        _isOpen = false;
        if (_ptr is not null)
        {
            NativeMemory.Free(_ptr);
            _ptr = null;
        }

        _byteCapacity = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AdvanceReadHead(int byteCount)
    {
        _readPosition += byteCount;
        if (_readPosition >= _byteCapacity)
        {
            _readPosition -= _byteCapacity;
        }

        _storedBytes -= byteCount;
        _totalRead += byteCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AdvanceWriteHead(int byteCount)
    {
        _writePosition += byteCount;
        if (_writePosition >= _byteCapacity)
        {
            _writePosition -= _byteCapacity;
        }

        _storedBytes += byteCount;
        _totalWritten += byteCount;
    }
}
