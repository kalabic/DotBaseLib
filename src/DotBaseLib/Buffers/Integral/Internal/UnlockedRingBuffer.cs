using System.Diagnostics;
using System.Runtime.CompilerServices;
using DotBase.Integral;
using DotBase.Integral.Internal;

namespace DotBase.Buffers.Integral.Internal;


/// <summary>
/// Endian-aware integral ring buffer over a native byte slab.
/// Holds a permanent <see cref="IntegralSpan"/> view of the full storage; span
/// read/write builds free/stored sub-views and moves data with
/// <see cref="IntegralMemory"/>.
/// </summary>
internal abstract unsafe class UnlockedRingBuffer 
    : IntegralRingBufferBase
{
    private const int ScratchByteCount = 512;

    /// <summary>
    /// Permanent view of the entire allocated memory block as bytes in ring wire order.
    /// Free/stored regions are sub-spans of this view (then retyped for value I/O).
    /// </summary>
    private IntegralSpan _slab;

    internal UnlockedRingBuffer(int capacity)
        : base(capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        _slab = CreateSlabView();
    }

    /// <summary> A view of the entire allocated memory block (empty when closed or zero capacity). </summary>
    public IntegralSpan Slab => _slab;

    public override int ByteCapacity => _storage.ByteCapacity;

    public override int FreeBytes => _storage.FreeBytes;

    public override int StoredBytes => _storage.StoredBytes;

    public override bool IsOpen => _storage.IsOpen;

    public long TotalRead => _storage.TotalRead;

    public long TotalWritten => _storage.TotalWritten;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Close();
        }
        else
        {
            _storage.Close(); // Only minimal unmanaged release.
        }

        base.Dispose(disposing);
    }

    public override int CapacityAs<T>()
    {
        return _storage.ByteCapacity / Unsafe.SizeOf<T>();
    }

    public override int FreeCount<T>()
    {
        return _storage.FreeBytes / Unsafe.SizeOf<T>();
    }

    public override int StoredCount<T>()
    {
        return _storage.StoredBytes / Unsafe.SizeOf<T>();
    }

    public override void AdvanceBy<T>(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        _storage.Advance(
            checked((int)(
                (long)count * Unsafe.SizeOf<T>())));
    }

    /// <summary>
    /// <para>
    /// <b>Partial, block-complete read (trusted).</b>
    /// Fills as many complete blocks of <paramref name="destination"/> as stored data allows.
    /// Trailing values on the span are never filled.
    /// </para>
    /// <para>
    /// No format/geometry validation — use <see cref="ReadChecked"/> for untrusted spans.
    /// Closed ring or no complete block available → <c>0</c>.
    /// </para>
    /// </summary>
    /// <returns>Scalar values transferred (multiple of block capacity).</returns>
    public override int Read(in IntegralSpan destination)
    {
        if (!_storage.IsOpen || destination.Length == 0)
        {
            return 0;
        }

        long valueCount = IntegralRingSpanOps.CountBlockCompleteValues(destination, _storage.StoredBytes);
        return checked((int)TransferFromRing(destination, valueCount));
    }

    /// <summary>
    /// <para>
    /// <b>Atomic, block-complete read (trusted).</b>
    /// Fills <b>all</b> complete blocks of <paramref name="destination"/>, or fails with
    /// <see langword="false"/> and no ring mutation. Trailing values are never required or filled.
    /// </para>
    /// <para>
    /// No format/geometry validation — use <see cref="TryReadChecked"/> for untrusted spans.
    /// </para>
    /// </summary>
    /// <returns><see langword="true"/> if every complete block of the destination was filled.</returns>
    public override bool TryRead(in IntegralSpan destination)
    {
        long requiredByteCount = IntegralRingSpanOps.BlockCompleteByteCount(destination);
        if (!_storage.IsOpen ||
            _storage.StoredBytes < requiredByteCount)
        {
            return false;
        }

        long valueCount = IntegralRingSpanOps.CountBlockCompleteValues(destination, _storage.StoredBytes);
        long readCount = TransferFromRing(destination, valueCount);
        Debug.Assert(readCount == valueCount);
        return true;
    }

    /// <summary>
    /// <para>
    /// <b>Partial, block-complete write (trusted).</b>
    /// Writes as many complete blocks of <paramref name="source"/> as free space allows.
    /// Trailing values on the span are never written.
    /// </para>
    /// <para>
    /// No format/geometry validation — use <see cref="WriteChecked"/> for untrusted spans.
    /// Closed ring or no complete block free → <c>0</c>.
    /// </para>
    /// </summary>
    /// <returns>Scalar values transferred (multiple of block capacity).</returns>
    public override int Write(in IntegralSpan source)
    {
        if (!_storage.IsOpen || source.Length == 0)
        {
            return 0;
        }

        long valueCount = IntegralRingSpanOps.CountBlockCompleteValues(source, _storage.FreeBytes);
        return checked((int)TransferToRing(source, valueCount));
    }

    /// <summary>
    /// <para>
    /// <b>Atomic, block-complete write (trusted).</b>
    /// Writes <b>all</b> complete blocks of <paramref name="source"/>, or fails with
    /// <see langword="false"/> and no ring mutation. Trailing values are never required or written.
    /// </para>
    /// <para>
    /// No format/geometry validation — use <see cref="TryWriteChecked"/> for untrusted spans.
    /// </para>
    /// </summary>
    /// <returns><see langword="true"/> if every complete block of the source was written.</returns>
    public override bool TryWrite(in IntegralSpan source)
    {
        long requiredByteCount = IntegralRingSpanOps.BlockCompleteByteCount(source);
        if (!_storage.IsOpen ||
            _storage.FreeBytes < requiredByteCount)
        {
            return false;
        }

        long valueCount = IntegralRingSpanOps.CountBlockCompleteValues(source, _storage.FreeBytes);
        long writtenCount = TransferToRing(source, valueCount);
        Debug.Assert(writtenCount == valueCount);
        return true;
    }

    /// <summary>
    /// Validates <paramref name="destination"/>, then <see cref="Read"/>.
    /// </summary>
    public override int ReadChecked(in IntegralSpan destination)
    {
        IntegralRingSpanOps.ValidateSpan(
            destination,
            nameof(destination));
        return Read(destination);
    }

    /// <summary>
    /// Validates <paramref name="destination"/>, then <see cref="TryRead"/>.
    /// </summary>
    public override bool TryReadChecked(in IntegralSpan destination)
    {
        IntegralRingSpanOps.ValidateSpan(
            destination,
            nameof(destination));
        return TryRead(destination);
    }

    /// <summary>
    /// Validates <paramref name="source"/>, then <see cref="Write"/>.
    /// </summary>
    public override int WriteChecked(in IntegralSpan source)
    {
        IntegralRingSpanOps.ValidateSpan(
            source,
            nameof(source));
        return Write(source);
    }

    /// <summary>
    /// Validates <paramref name="source"/>, then <see cref="TryWrite"/>.
    /// </summary>
    public override bool TryWriteChecked(in IntegralSpan source)
    {
        IntegralRingSpanOps.ValidateSpan(
            source,
            nameof(source));
        return TryWrite(source);
    }

    public override int Read(byte[] data, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(data);
        Span<byte> destination = data.AsSpan(offset, count);
        int n = Math.Min(destination.Length, _storage.StoredBytes);
        return n == 0 ? 0 : _storage.Read(destination[..n]);
    }

    public override int Read(byte* data, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(data, offset, count, nameof(data));
        int n = Math.Min(count, _storage.StoredBytes);
        return n == 0 ? 0 : _storage.Read(data + offset, n);
    }

    public override int Write(byte[] data, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(data);
        ReadOnlySpan<byte> source = data.AsSpan(offset, count);
        int n = Math.Min(source.Length, _storage.FreeBytes);
        return n == 0 ? 0 : _storage.Write(source[..n]);
    }

    public override int Write(byte* data, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(data, offset, count, nameof(data));
        int n = Math.Min(count, _storage.FreeBytes);
        return n == 0 ? 0 : _storage.Write(data + offset, n);
    }

    public override bool Read<T>(out T value) => TryRead(out value);

    public override bool TryRead<T>(out T value)
    {
        int n = Unsafe.SizeOf<T>();
        if (!_storage.IsOpen || _storage.StoredBytes < n)
        {
            value = default;
            return false;
        }

        T tmp = default;
        byte* p = (byte*)&tmp;
        if (HostMatchesRing || n <= 1)
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

    public override bool Write<T>(T value) => TryWrite(value);

    public override bool TryWrite<T>(T value)
    {
        int n = Unsafe.SizeOf<T>();
        if (!_storage.IsOpen || _storage.FreeBytes < n)
        {
            return false;
        }

        if (HostMatchesRing || n <= 1)
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

    public override int Read<T>(T[] destination, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(destination);
        return Read(destination.AsSpan(offset, count));
    }

    public override int Read<T>(T* destination, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(
            destination,
            offset,
            count,
            nameof(destination));

        return ReadCore(destination + offset, count);
    }

    public override int Write<T>(T[] source, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        return Write((ReadOnlySpan<T>)source.AsSpan(offset, count));
    }

    public override int Write<T>(T* source, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(source, offset, count, nameof(source));
        return WriteCore(source + offset, count);
    }

    public override bool TryRead<T>(T[] destination, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(destination);
        return TryRead(destination.AsSpan(offset, count));
    }

    public override bool TryRead<T>(T* destination, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(
            destination,
            offset,
            count,
            nameof(destination));

        return TryReadCore(destination + offset, count);
    }

    public override bool TryRead<T>(Span<T> destination)
    {
        if (destination.IsEmpty)
        {
            return _storage.IsOpen;
        }

        fixed (T* dst = destination)
        {
            return TryReadCore(dst, destination.Length);
        }
    }

    public override bool TryWrite<T>(T[] source, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        return TryWrite((ReadOnlySpan<T>)source.AsSpan(offset, count));
    }

    public override bool TryWrite<T>(T* source, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(source, offset, count, nameof(source));
        return TryWriteCore(source + offset, count);
    }

    public override bool TryWrite<T>(ReadOnlySpan<T> source)
    {
        if (source.IsEmpty)
        {
            return _storage.IsOpen;
        }

        fixed (T* src = source)
        {
            return TryWriteCore(src, source.Length);
        }
    }

    public override int Read<T>(Span<T> destination)
    {
        if (destination.IsEmpty)
        {
            return 0;
        }

        fixed (T* dst = destination)
        {
            return ReadCore(dst, destination.Length);
        }
    }

    public override int Write<T>(ReadOnlySpan<T> source)
    {
        if (source.IsEmpty)
        {
            return 0;
        }

        fixed (T* src = source)
        {
            return WriteCore(src, source.Length);
        }
    }

    public override void Advance(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        _storage.Advance(count);
    }

    public override void ClearBuffer()
    {
        _storage.Clear();
    }

    public override void Close()
    {
        _storage.Close();
        _slab = IntegralSpan.Empty;
    }

    /// <summary>
    /// True when host memory order matches ring wire order (no lane reverse for host <c>T</c>).
    /// </summary>
    private bool HostMatchesRing =>
        ByteOrder.Native.Resolve() == ByteOrder;

    private IntegralSpan CreateSlabView()
    {
        int capacity = _storage.ByteCapacity;
        if (capacity == 0 || _storage.Ptr is null)
        {
            return IntegralSpan.Empty;
        }

        return new IntegralSpan(
            _storage.Ptr,
            0,
            capacity,
            new IntegralFormat(IntegralType.UInt8, 1, ByteOrder));
    }

    /// <summary>
    /// Free space as at most two ranges of <see cref="_slab"/> in <b>parent
    /// block</b> units (slab is UInt8 BC=1, so one block = one byte). Stream
    /// order: high run at write head, then wrap to block 0. Empty ranges have
    /// <see cref="IntegralRange.BlockCount"/> 0.
    /// </summary>
    private void GetFreeRanges(out IntegralRange first, out IntegralRange second)
    {
        _storage.GetFreeSegments(
            out int firstOffset,
            out int firstByteCount,
            out int secondByteCount);

        long blockByteSize = _slab.Capacity.BlockByteCount;
        // Parent block units: UInt8 BC=1 ⇒ block offset/count == byte offset/count.
        first = firstByteCount == 0
            ? IntegralRange.Empty
            : new IntegralRange(firstOffset, firstByteCount, blockByteSize);
        second = secondByteCount == 0
            ? IntegralRange.Empty
            : new IntegralRange(0, secondByteCount, blockByteSize);
    }

    /// <summary>
    /// Stored data as at most two ranges of <see cref="_slab"/> in <b>parent
    /// block</b> units (slab is UInt8 BC=1, so one block = one byte). Stream
    /// order: high run at read head, then wrap to block 0. Empty ranges have
    /// <see cref="IntegralRange.BlockCount"/> 0.
    /// </summary>
    private void GetStoredRanges(out IntegralRange first, out IntegralRange second)
    {
        _storage.GetStoredSegments(
            out int firstOffset,
            out int firstByteCount,
            out int secondByteCount);

        long blockByteSize = _slab.Capacity.BlockByteCount;
        // Parent block units: UInt8 BC=1 ⇒ block offset/count == byte offset/count.
        first = firstByteCount == 0
            ? IntegralRange.Empty
            : new IntegralRange(firstOffset, firstByteCount, blockByteSize);
        second = secondByteCount == 0
            ? IntegralRange.Empty
            : new IntegralRange(0, secondByteCount, blockByteSize);
    }

    /// <returns>Scalar values transferred.</returns>
    private long TransferToRing(in IntegralSpan source, long valueCount)
    {
        if (valueCount == 0)
        {
            return 0;
        }

        int valueByteCount = source.Capacity.ValueByteCount;
        long bytesLeft = valueCount * valueByteCount;
        long sourceValueOffset = 0;
        bool sameEndian = source.IsEqual(ByteOrder);

        GetFreeRanges(out IntegralRange first, out IntegralRange second);

        long firstTaken = CopyWholeValuesToRing(
            source,
            sourceValueOffset,
            bytesLeft,
            first,
            valueByteCount,
            sameEndian);
        sourceValueOffset += firstTaken / valueByteCount;
        bytesLeft -= firstTaken;

        if (bytesLeft == 0)
        {
            return sourceValueOffset;
        }

        // Unused tail on the first run → stream (values may straddle the seam).
        // firstTaken is bytes; first.ByteLength is the free run size in bytes.
        if (firstTaken < first.ByteLength)
        {
            return sourceValueOffset + WriteStreamRemainder(
                source,
                sourceValueOffset,
                bytesLeft / valueByteCount,
                valueByteCount,
                sameEndian);
        }

        long secondTaken = CopyWholeValuesToRing(
            source,
            sourceValueOffset,
            bytesLeft,
            second,
            valueByteCount,
            sameEndian);
        sourceValueOffset += secondTaken / valueByteCount;
        bytesLeft -= secondTaken;

        if (bytesLeft > 0)
        {
            return sourceValueOffset + WriteStreamRemainder(
                source,
                sourceValueOffset,
                bytesLeft / valueByteCount,
                valueByteCount,
                sameEndian);
        }

        return sourceValueOffset;
    }

    /// <returns>Scalar values transferred.</returns>
    private long TransferFromRing(in IntegralSpan destination, long valueCount)
    {
        if (valueCount == 0)
        {
            return 0;
        }

        int valueByteCount = destination.Capacity.ValueByteCount;
        long bytesLeft = valueCount * valueByteCount;
        long destinationValueOffset = 0;
        bool sameEndian = destination.IsEqual(ByteOrder);

        GetStoredRanges(out IntegralRange first, out IntegralRange second);

        long firstTaken = CopyWholeValuesFromRing(
            destination,
            destinationValueOffset,
            bytesLeft,
            first,
            valueByteCount,
            sameEndian);
        destinationValueOffset += firstTaken / valueByteCount;
        bytesLeft -= firstTaken;

        if (bytesLeft == 0)
        {
            return destinationValueOffset;
        }

        // firstTaken is bytes; first.ByteLength is the stored run size in bytes.
        if (firstTaken < first.ByteLength)
        {
            return destinationValueOffset + ReadStreamRemainder(
                destination,
                destinationValueOffset,
                bytesLeft / valueByteCount,
                valueByteCount,
                sameEndian);
        }

        long secondTaken = CopyWholeValuesFromRing(
            destination,
            destinationValueOffset,
            bytesLeft,
            second,
            valueByteCount,
            sameEndian);
        destinationValueOffset += secondTaken / valueByteCount;
        bytesLeft -= secondTaken;

        if (bytesLeft > 0)
        {
            return destinationValueOffset + ReadStreamRemainder(
                destination,
                destinationValueOffset,
                bytesLeft / valueByteCount,
                valueByteCount,
                sameEndian);
        }

        return destinationValueOffset;
    }

    /// <summary>
    /// Whole values from <paramref name="source"/> into one free slab range.
    /// Returns bytes written (multiple of <paramref name="valueByteCount"/>).
    /// </summary>
    private long CopyWholeValuesToRing(
        in IntegralSpan source,
        long sourceValueOffset,
        long bytesLeft,
        in IntegralRange freeRange,
        int valueByteCount,
        bool sameEndian)
    {
        if (freeRange.IsEmpty)
        {
            return 0;
        }

        long freeBytes = freeRange.ByteLength;
        long chunkBytes = Math.Min(freeBytes, bytesLeft);
        chunkBytes -= chunkBytes % valueByteCount;
        if (chunkBytes == 0)
        {
            return 0;
        }

        long chunkValues = chunkBytes / valueByteCount;
        int chunkBytesInt = checked((int)chunkBytes);

        if (sameEndian)
        {
            // Raw memcpy into slab at freeRange; no subspan / ChangeFormat.
            IntegralMemory.Copy(
                source.DataPtr + checked(sourceValueOffset * valueByteCount),
                _slab.DataPtr,
                freeRange,
                chunkBytes);
        }
        else
        {
            IntegralSpan ringView = _slab.GetBlockSpan(
                freeRange,
                source.IntegralValueType,
                source.Format.BlockCapacity);
            IntegralSpan sourceChunk = source.GetValueSpan(
                sourceValueOffset,
                chunkValues);
            IntegralMemory.ReverseCopy(sourceChunk, ringView, chunkValues);
        }

        _storage.AdvanceWriteHead(chunkBytesInt);
        return chunkBytes;
    }

    /// <summary>
    /// Whole values from one stored slab range into <paramref name="destination"/>.
    /// Returns bytes read (multiple of <paramref name="valueByteCount"/>).
    /// </summary>
    private long CopyWholeValuesFromRing(
        in IntegralSpan destination,
        long destinationValueOffset,
        long bytesLeft,
        in IntegralRange storedRange,
        int valueByteCount,
        bool sameEndian)
    {
        if (storedRange.IsEmpty)
        {
            return 0;
        }

        long storedBytes = storedRange.ByteLength;
        long chunkBytes = Math.Min(storedBytes, bytesLeft);
        chunkBytes -= chunkBytes % valueByteCount;
        if (chunkBytes == 0)
        {
            return 0;
        }

        long chunkValues = chunkBytes / valueByteCount;
        int chunkBytesInt = checked((int)chunkBytes);
        long destinationByteOffset = checked(
            destinationValueOffset * valueByteCount);

        if (sameEndian)
        {
            // Raw memcpy from slab at storedRange; no subspan / ChangeFormat.
            IntegralMemory.Copy(
                _slab.DataPtr,
                storedRange,
                destination.DataPtr + destinationByteOffset,
                chunkBytes);
        }
        else
        {
            IntegralSpan ringView = _slab.GetBlockSpan(
                storedRange,
                destination.IntegralValueType,
                destination.Format.BlockCapacity);
            IntegralSpan destinationChunk = destination.GetValueSpan(
                destinationValueOffset,
                chunkValues);
            IntegralMemory.ReverseCopy(ringView, destinationChunk, chunkValues);
        }

        _storage.AdvanceReadHead(chunkBytesInt);
        return chunkBytes;
    }

    private long WriteStreamRemainder(
        in IntegralSpan source,
        long sourceValueOffset,
        long valueCount,
        int valueByteCount,
        bool sameEndian)
    {
        if (valueCount <= 0)
        {
            return 0;
        }

        IntegralSpan chunk = source.GetValueSpan(sourceValueOffset, valueCount);
        if (sameEndian)
        {
            int bytes = checked((int)(
                valueCount * valueByteCount));
            int written = _storage.Write(chunk.DataPtr, bytes);
            Debug.Assert(written == bytes);
            return valueCount;
        }

        return IntegralRingSpanOps.WriteEndianFlip(
            ref _storage,
            chunk.DataPtr,
            valueCount,
            valueByteCount);
    }

    private long ReadStreamRemainder(
        in IntegralSpan destination,
        long destinationValueOffset,
        long valueCount,
        int valueByteCount,
        bool sameEndian)
    {
        if (valueCount <= 0)
        {
            return 0;
        }

        IntegralSpan chunk = destination.GetValueSpan(
            destinationValueOffset,
            valueCount);
        if (sameEndian)
        {
            int bytes = checked((int)(
                valueCount * valueByteCount));
            int read = _storage.Read(chunk.DataPtr, bytes);
            Debug.Assert(read == bytes);
            return valueCount;
        }

        return IntegralRingSpanOps.ReadEndianFlip(
            ref _storage,
            chunk.DataPtr,
            valueCount,
            valueByteCount);
    }

    private int ReadCore<T>(T* destination, int count)
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

        if (!HostMatchesRing && n > 1)
        {
            IntegralPrimitives.ReverseLanesInPlace(
                (byte*)destination,
                elementCount,
                n);
        }

        return elementCount;
    }

    private bool TryReadCore<T>(T* destination, int count)
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

    private int WriteCore<T>(T* source, int count)
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

        if (HostMatchesRing || n <= 1)
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

    private bool TryWriteCore<T>(T* source, int count)
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

    private void WriteReversed<T>(T* source, int count)
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
