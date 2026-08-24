using DotBase.Integral;
using System.Runtime.CompilerServices;

namespace DotBase.Buffers.Integral.Internal;


internal abstract partial class WaitableRingBuffer
{
    public async ValueTask<LongResult> ReadAsync(byte[] destination, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(checked(offset + count), destination.Length);

        if (count == 0)
        {
            return LongResult.SUCCESS;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!IsReadingAllowedLocked())
                {
                    return LongResult.CLOSED;
                }

                if (_storage.StoredBytes >= 1)
                {
                    int countRead = ReadAsyncFinalLocked(destination, offset, count);
                    return LongResult.Success(countRead);
                }

                if (_isWritingCompleted)
                {
                    return LongResult.CLOSED;
                }
            }

            var waitResult = await WaitForStoredBytesAsync(1);
            if (waitResult.Status == ResultStatus.CLOSED)
            {
                // Recheck under the ring lock, completion may race with fulfillment:
                // - The reader’s wait may observe CLOSED, even though the required
                //   data was written before completion.
                continue;
            }

            if (!waitResult)
            {
                return waitResult;
            }
        }
    }

    public async ValueTask<LongResult> ReadExactAsync(byte[] destination, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(checked(offset + count), destination.Length);

        if (count == 0)
        {
            return LongResult.SUCCESS;
        }

        if (!CanFit(count))
        {
            return LongResult.OUT_OF_RANGE;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!IsReadingAllowedLocked())
                {
                    return LongResult.CLOSED;
                }

                if (_storage.StoredBytes >= count)
                {
                    int countRead = ReadAsyncFinalLocked(destination, offset, count);
                    return LongResult.Success(countRead);
                }

                if (_isWritingCompleted)
                {
                    return LongResult.CLOSED;
                }
            }

            var waitResult = await WaitForStoredBytesAsync(count);
            if (waitResult.Status == ResultStatus.CLOSED)
            {
                // Recheck under the ring lock, completion may race with fulfillment:
                // - The reader’s wait may observe CLOSED, even though the required
                //   data was written before completion.
                continue;
            }

            if (!waitResult)
            {
                return waitResult;
            }
        }
    }

    public unsafe ValueTask<LongResult> ReadAsync(byte* destination, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(destination, offset, count, nameof(destination));
        return ReadAsyncInternal(new UnsafePtrSpan<byte>(destination, offset, count));
    }

    private async ValueTask<LongResult> ReadAsyncInternal(UnsafePtrSpan<byte> ptrSpan)
    {
        if (ptrSpan.Count == 0)
        {
            return LongResult.SUCCESS;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!IsReadingAllowedLocked())
                {
                    return LongResult.CLOSED;
                }

                if (_storage.StoredBytes >= 1)
                {
                    int countRead = ReadAsyncFinalLocked(ptrSpan);
                    return LongResult.Success(countRead);
                }

                if (_isWritingCompleted)
                {
                    return LongResult.CLOSED;
                }
            }

            var waitResult = await WaitForStoredBytesAsync(1);
            if (waitResult.Status == ResultStatus.CLOSED)
            {
                // Recheck under the ring lock, completion may race with fulfillment:
                // - The reader’s wait may observe CLOSED, even though the required
                //   data was written before completion.
                continue;
            }

            if (!waitResult)
            {
                return waitResult;
            }
        }
    }

    public unsafe ValueTask<LongResult> ReadExactAsync(byte* destination, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(destination, offset, count, nameof(destination));
        return ReadExactAsyncInternal(new UnsafePtrSpan<byte>(destination, offset, count));
    }

    private async ValueTask<LongResult> ReadExactAsyncInternal(UnsafePtrSpan<byte> ptrSpan)
    {
        if (ptrSpan.Count == 0)
        {
            return LongResult.SUCCESS;
        }

        if (!CanFit(ptrSpan.Count))
        {
            return LongResult.OUT_OF_RANGE;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!IsReadingAllowedLocked())
                {
                    return LongResult.CLOSED;
                }

                if (_storage.StoredBytes >= ptrSpan.Count)
                {
                    int countRead = ReadAsyncFinalLocked(ptrSpan);
                    return LongResult.Success(countRead);
                }

                if (_isWritingCompleted)
                {
                    return LongResult.CLOSED;
                }
            }

            var waitResult = await WaitForStoredBytesAsync(ptrSpan.Count);
            if (waitResult.Status == ResultStatus.CLOSED)
            {
                // Recheck under the ring lock, completion may race with fulfillment:
                // - The reader’s wait may observe CLOSED, even though the required
                //   data was written before completion.
                continue;
            }

            if (!waitResult)
            {
                return waitResult;
            }
        }
    }

    public async ValueTask<LongResult> WriteExactAsync(byte[] source, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(checked(offset + count), source.Length);

        if (count == 0)
        {
            return LongResult.SUCCESS;
        }

        if (!CanFit(count))
        {
            return LongResult.OUT_OF_RANGE;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!IsWritingAllowedLocked())
                {
                    return LongResult.CLOSED;
                }

                if (_storage.FreeBytes >= count)
                {
                    int countWritten = WriteAsyncFinalLocked(source, offset, count);
                    return LongResult.Success(countWritten);
                }
            }

            var waitResult = await WaitForFreeBytesAsync(count);
            if (!waitResult)
            {
                return waitResult;
            }
        }
    }

    public unsafe ValueTask<LongResult> WriteExactAsync(byte* source, int offset, int count)
    {
        IntegralBufferGuards.ValidatePointer(source, offset, count, nameof(source));
        return WriteExactAsync(new UnsafePtrSpan<byte>(source, offset, count));
    }


    public async ValueTask<LongResult> ReadAsync<T>(T[] destination, int offset, int count)
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(checked(offset + count), destination.Length);

        if (count == 0)
        {
            return LongResult.SUCCESS;
        }

        int requiredBytes = Unsafe.SizeOf<T>();
        if (!CanFit(requiredBytes))
        {
            return LongResult.OUT_OF_RANGE;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!IsReadingAllowedLocked())
                {
                    return LongResult.CLOSED;
                }

                if (_storage.StoredBytes >= requiredBytes)
                {
                    int countRead = ReadAsyncFinalLocked(destination, offset, count);
                    return LongResult.Success(countRead);
                }

                if (_isWritingCompleted)
                {
                    return LongResult.CLOSED;
                }
            }

            var waitResult = await WaitForStoredBytesAsync(requiredBytes);
            if (waitResult.Status == ResultStatus.CLOSED)
            {
                // Recheck under the ring lock, completion may race with fulfillment:
                // - The reader’s wait may observe CLOSED, even though the required
                //   data was written before completion.
                continue;
            }

            if (!waitResult)
            {
                return waitResult;
            }
        }
    }

    public async ValueTask<LongResult> ReadExactAsync<T>(T[] destination, int offset, int count)
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(checked(offset + count), destination.Length);

        if (count == 0)
        {
            return LongResult.SUCCESS;
        }

        long requiredBytes = (long)count * Unsafe.SizeOf<T>();
        if (!CanFit(requiredBytes))
        {
            return LongResult.OUT_OF_RANGE;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!IsReadingAllowedLocked())
                {
                    return LongResult.CLOSED;
                }

                if (_storage.StoredBytes >= requiredBytes)
                {
                    int countRead = ReadAsyncFinalLocked(destination, offset, count);
                    return LongResult.Success(countRead);
                }

                if (_isWritingCompleted)
                {
                    return LongResult.CLOSED;
                }
            }

            var waitResult = await WaitForStoredBytesAsync(requiredBytes);
            if (waitResult.Status == ResultStatus.CLOSED)
            {
                // Recheck under the ring lock, completion may race with fulfillment:
                // - The reader’s wait may observe CLOSED, even though the required
                //   data was written before completion.
                continue;
            }

            if (!waitResult)
            {
                return waitResult;
            }
        }
    }

    public unsafe ValueTask<LongResult> ReadAsync<T>(T* destination, int offset, int count)
        where T : unmanaged
    {
        IntegralBufferGuards.ValidatePointer(destination, offset, count, nameof(destination));
        return ReadAsyncInternal(new UnsafePtrSpan<T>(destination, offset, count));
    }

    private async ValueTask<LongResult> ReadAsyncInternal<T>(UnsafePtrSpan<T> ptrSpan)
        where T : unmanaged
    {
        if (ptrSpan.Count == 0)
        {
            return LongResult.SUCCESS;
        }

        int requiredBytes = Unsafe.SizeOf<T>();
        if (!CanFit(requiredBytes))
        {
            return LongResult.OUT_OF_RANGE;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!IsReadingAllowedLocked())
                {
                    return LongResult.CLOSED;
                }

                if (_storage.StoredBytes >= requiredBytes)
                {
                    int countRead = ReadAsyncFinalLocked(ptrSpan);
                    return LongResult.Success(countRead);
                }

                if (_isWritingCompleted)
                {
                    return LongResult.CLOSED;
                }
            }

            var waitResult = await WaitForStoredBytesAsync(requiredBytes);
            if (waitResult.Status == ResultStatus.CLOSED)
            {
                // Recheck under the ring lock, completion may race with fulfillment:
                // - The reader’s wait may observe CLOSED, even though the required
                //   data was written before completion.
                continue;
            }

            if (!waitResult)
            {
                return waitResult;
            }
        }
    }

    public unsafe ValueTask<LongResult> ReadExactAsync<T>(T* destination, int offset, int count)
        where T : unmanaged
    {
        IntegralBufferGuards.ValidatePointer(destination, offset, count, nameof(destination));
        return ReadExactAsyncInternal(new UnsafePtrSpan<T>(destination, offset, count));
    }

    private async ValueTask<LongResult> ReadExactAsyncInternal<T>(UnsafePtrSpan<T> ptrSpan)
        where T : unmanaged
    {
        if (ptrSpan.Count == 0)
        {
            return LongResult.SUCCESS;
        }

        long requiredBytes = (long)ptrSpan.Count * Unsafe.SizeOf<T>();
        if (!CanFit(requiredBytes))
        {
            return LongResult.OUT_OF_RANGE;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!IsReadingAllowedLocked())
                {
                    return LongResult.CLOSED;
                }

                if (_storage.StoredBytes >= requiredBytes)
                {
                    int countRead = ReadAsyncFinalLocked(ptrSpan);
                    return LongResult.Success(countRead);
                }

                if (_isWritingCompleted)
                {
                    return LongResult.CLOSED;
                }
            }

            var waitResult = await WaitForStoredBytesAsync(requiredBytes);
            if (waitResult.Status == ResultStatus.CLOSED)
            {
                // Recheck under the ring lock, completion may race with fulfillment:
                // - The reader’s wait may observe CLOSED, even though the required
                //   data was written before completion.
                continue;
            }

            if (!waitResult)
            {
                return waitResult;
            }
        }
    }

    public async ValueTask<LongResult> WriteExactAsync<T>(T[] source, int offset, int count)
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(checked(offset + count), source.Length);

        if (count == 0)
        {
            return LongResult.SUCCESS;
        }

        long requiredBytes = (long)count * Unsafe.SizeOf<T>();
        if (!CanFit(requiredBytes))
        {
            return LongResult.OUT_OF_RANGE;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!IsWritingAllowedLocked())
                {
                    return LongResult.CLOSED;
                }

                if (_storage.FreeBytes >= requiredBytes)
                {
                    int countWritten = WriteAsyncFinalLocked(source, offset, count);
                    return LongResult.Success(countWritten);
                }
            }

            var waitResult = await WaitForFreeBytesAsync(requiredBytes);
            if (!waitResult)
            {
                return waitResult;
            }
        }
    }

    public unsafe ValueTask<LongResult> WriteExactAsync<T>(T* source, int offset, int count)
        where T : unmanaged
    {
        IntegralBufferGuards.ValidatePointer(source, offset, count, nameof(source));
        return WriteExactAsync(new UnsafePtrSpan<T>(source, offset, count));
    }


    // IIntegralRingBufferAsync >>

    public async ValueTask<LongResult> ReadAsync(IntegralSpan destination)
    {
        if (destination.BlockCount == 0)
        {
            return LongResult.SUCCESS;
        }

        int requiredBytes = checked((int)destination.Format.BytesPerBlock);
        if (!CanFit(requiredBytes))
        {
            return LongResult.OUT_OF_RANGE;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!IsReadingAllowedLocked())
                {
                    return LongResult.CLOSED;
                }

                if (_storage.StoredBytes >= requiredBytes)
                {
                    int count = ReadIntegralSpan(destination);
                    PublishStoredLocked();
                    return LongResult.Success(count);
                }

                if (_isWritingCompleted)
                {
                    return LongResult.CLOSED;
                }
            }

            var waitResult = await WaitForStoredBytesAsync(requiredBytes);
            if (waitResult.Status == ResultStatus.CLOSED)
            {
                // Recheck under the ring lock, completion may race with fulfillment:
                // - The reader’s wait may observe CLOSED, even though the required
                //   data was written before completion.
                continue;
            }

            if (!waitResult)
            {
                return waitResult;
            }
        }
    }

    public async ValueTask<LongResult> ReadExactAsync(IntegralSpan destination)
    {
        if (destination.BlockCount == 0)
        {
            return LongResult.SUCCESS;
        }

        long required = IntegralRingSpanOps.BlockCompleteByteCount(destination);
        if (!CanFit(required))
        {
            return LongResult.OUT_OF_RANGE;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!IsReadingAllowedLocked())
                {
                    return LongResult.CLOSED;
                }

                if (required == 0 ||
                    _storage.StoredBytes >= required)
                {
                    int count = ReadIntegralSpan(destination);
                    PublishStoredLocked();
                    return LongResult.Success(count);
                }

                if (_isWritingCompleted)
                {
                    return LongResult.CLOSED;
                }
            }

            var waitResult = await WaitForStoredBytesAsync(required);
            if (waitResult.Status == ResultStatus.CLOSED)
            {
                // Recheck under the ring lock, completion may race with fulfillment:
                // - The reader’s wait may observe CLOSED, even though the required
                //   data was written before completion.
                continue;
            }

            if (!waitResult)
            {
                return waitResult;
            }
        }
    }

    public async ValueTask<LongResult> WriteExactAsync(IntegralSpan source)
    {
        if (source.BlockCount == 0)
        {
            return LongResult.SUCCESS;
        }

        long required = IntegralRingSpanOps.BlockCompleteByteCount(source);
        if (!CanFit(required))
        {
            return LongResult.OUT_OF_RANGE;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!IsWritingAllowedLocked())
                {
                    return LongResult.CLOSED;
                }

                if (required == 0 || _storage.FreeBytes >= required)
                {
                    int count = WriteIntegralSpan(source);
                    PublishStoredLocked();
                    return LongResult.Success(count);
                }
            }

            var waitResult = await WaitForFreeBytesAsync(required);
            if (!waitResult)
            {
                return waitResult;
            }
        }
    }


    // Private async helpers >>

    private async ValueTask<LongResult> WriteExactAsync(UnsafePtrSpan<byte> ptrSpan)
    {
        if (ptrSpan.Count == 0)
        {
            return LongResult.SUCCESS;
        }

        if (!CanFit(ptrSpan.Count))
        {
            return LongResult.OUT_OF_RANGE;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!IsWritingAllowedLocked())
                {
                    return LongResult.CLOSED;
                }

                if (_storage.FreeBytes >= ptrSpan.Count)
                {
                    int countWritten = WriteAsyncFinalLocked(ptrSpan);
                    return LongResult.Success(countWritten);
                }
            }

            var waitResult = await WaitForFreeBytesAsync(ptrSpan.Count);
            if (!waitResult)
            {
                return waitResult;
            }
        }
    }

    private async ValueTask<LongResult> WriteExactAsync<T>(UnsafePtrSpan<T> ptrSpan)
        where T : unmanaged
    {
        if (ptrSpan.Count == 0)
        {
            return LongResult.SUCCESS;
        }

        long requiredBytes = (long)ptrSpan.Count * Unsafe.SizeOf<T>();
        if (!CanFit(requiredBytes))
        {
            return LongResult.OUT_OF_RANGE;
        }

        while (true)
        {
            lock (_lock)
            {
                if (!IsWritingAllowedLocked())
                {
                    return LongResult.CLOSED;
                }

                if (_storage.FreeBytes >= requiredBytes)
                {
                    int countWritten = WriteAsyncFinalLocked(ptrSpan);
                    return LongResult.Success(countWritten);
                }
            }

            var waitResult = await WaitForFreeBytesAsync(requiredBytes);
            if (!waitResult)
            {
                return waitResult;
            }
        }
    }

    private unsafe int ReadAsyncFinalLocked<T>(T[] destination, int offset, int count)
        where T : unmanaged
    {
        int readCount = 0;

        fixed (T* dst = &destination[offset])
        {
            readCount = ReadCore(dst, count);
        }

        PublishStoredLocked();
        return readCount;
    }

    private unsafe int ReadAsyncFinalLocked<T>(UnsafePtrSpan<T> spanPtr)
        where T : unmanaged
    {
        int readCount = ReadCore(spanPtr.OffsetPtr, spanPtr.Count);
        PublishStoredLocked();
        return readCount;
    }

    private unsafe int WriteAsyncFinalLocked<T>(T[] source, int offset, int count)
        where T : unmanaged
    {
        int countWritten = 0;
        fixed (T* src = &source[offset])
        {
            countWritten = WriteCore(src, count);
        }

        PublishStoredLocked();
        return countWritten;
    }

    private unsafe int WriteAsyncFinalLocked<T>(UnsafePtrSpan<T> spanPtr)
        where T : unmanaged
    {
        int countWritten = WriteCore(spanPtr.OffsetPtr, spanPtr.Count);
        PublishStoredLocked();
        return countWritten;
    }

    public ValueTask<LongResult> WaitForStoredBytesAsync(long required)
    {
        return _storedByteCount.WaitGreaterOrEqualToAsync(required);
    }

    public ValueTask<LongResult> WaitForFreeBytesAsync(long required)
    {
        return _storedByteCount.WaitLessOrEqualToAsync(_byteCapacity - required);
    }
}
