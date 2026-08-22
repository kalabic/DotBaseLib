using System.Runtime.InteropServices;
using DotBase.Buffers;
using DotBase.Buffers.Integral;
using DotBase.Integral;

namespace DotBaseLib.Tests;


public class IntegralRingBufferLifecycleTests
{
    public enum ReaderTermination
    {
        CompleteReading,
        Abort,
        Close,
        Dispose,
    }

    public enum WriterTermination
    {
        CompleteWriting,
        CompleteReading,
        Abort,
        Close,
        Dispose,
    }

    public static TheoryData<ByteOrder> BothOrders() =>
        new()
        {
            ByteOrder.LittleEndian,
            ByteOrder.BigEndian,
        };

    [Theory]
    [MemberData(nameof(BothOrders))]
    public void LifecycleFlagsAreIndependentAndAbortRetainsFirstError(
        ByteOrder byteOrder)
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            2 * sizeof(int),
            byteOrder);

        Assert.False(ring.IsWritingCompleted);
        Assert.False(ring.IsReadingCompleted);
        Assert.False(ring.IsDrained);
        Assert.False(ring.IsAborted);
        Assert.Null(ring.AbortError);

        Assert.True(ring.TryWrite(123));
        ring.CompleteWriting();
        ring.CompleteWriting();

        Assert.True(ring.IsWritingCompleted);
        Assert.False(ring.IsReadingCompleted);
        Assert.False(ring.IsDrained);
        Assert.False(ring.IsAborted);

        long totalRead = Counter(ring, "TotalRead");
        ring.CompleteReading();
        ring.CompleteReading();

        Assert.True(ring.IsWritingCompleted);
        Assert.True(ring.IsReadingCompleted);
        Assert.True(ring.IsDrained);
        Assert.Equal(0, ring.StoredBytes);
        Assert.Equal(totalRead, Counter(ring, "TotalRead"));

        InvalidOperationException first = new("first");
        ring.Abort(first);
        ring.Abort(new ApplicationException("second"));
        ring.CompleteWriting();
        ring.CompleteReading();

        Assert.True(ring.IsWritingCompleted);
        Assert.True(ring.IsReadingCompleted);
        Assert.True(ring.IsAborted);
        Assert.Same(first, ring.AbortError);
    }

    [Theory]
    [MemberData(nameof(BothOrders))]
    public void ReadingMayCompleteBeforeWriting(ByteOrder byteOrder)
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            sizeof(int),
            byteOrder);

        ring.CompleteReading();
        Assert.True(ring.IsReadingCompleted);
        Assert.False(ring.IsWritingCompleted);

        ring.CompleteWriting();
        Assert.True(ring.IsReadingCompleted);
        Assert.True(ring.IsWritingCompleted);
        Assert.True(ring.IsDrained);
    }

    [Fact]
    public void AbortWithNullStillWinsOverLaterErrors()
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            sizeof(int),
            ByteOrder.Native);

        ring.Abort();
        ring.Abort(new InvalidOperationException("ignored"));

        Assert.True(ring.IsAborted);
        Assert.Null(ring.AbortError);
    }

    [Fact]
    public void LifecycleCallsAfterCloseDisposeAndZeroCapacityAreNoOps()
    {
        IWaitableRingBuffer closed = IntegralRingBuffer.CreateWaitable(
            sizeof(int),
            ByteOrder.Native);
        closed.Close();
        ExerciseLifecycle(closed);
        AssertLifecycleUnchanged(closed);
        closed.Dispose();

        IWaitableRingBuffer disposed = IntegralRingBuffer.CreateWaitable(
            sizeof(int),
            ByteOrder.Native);
        disposed.Dispose();
        ExerciseLifecycle(disposed);
        AssertLifecycleUnchanged(disposed);

        using IWaitableRingBuffer zero = IntegralRingBuffer.CreateWaitable(
            0,
            ByteOrder.Native);
        ExerciseLifecycle(zero);
        AssertLifecycleUnchanged(zero);
    }

    [Theory]
    [MemberData(nameof(BothOrders))]
    public void ClosePreservesLifecycleStateAndAbortError(ByteOrder byteOrder)
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            sizeof(int),
            byteOrder);
        Exception error = new InvalidDataException("aborted");

        ring.CompleteWriting();
        ring.Abort(error);
        ring.Close();

        Assert.False(ring.IsOpen);
        Assert.True(ring.IsWritingCompleted);
        Assert.True(ring.IsAborted);
        Assert.True(ring.IsDrained);
        Assert.Same(error, ring.AbortError);
    }

    [Theory]
    [MemberData(nameof(BothOrders))]
    public async Task WriterCompletionUnblocksFinalPartialByteRead(
        ByteOrder byteOrder)
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            8,
            byteOrder);
        byte[] source = [1, 2];
        byte[] destination = new byte[4];
        Assert.Equal(2, ring.Write(source, 0, source.Length));

        using ManualResetEventSlim started = new();
        Task<int> readTask = Task.Run(() =>
        {
            started.Set();
            return ring.Read(destination, 0, destination.Length);
        });

        Assert.True(started.Wait(TimeSpan.FromSeconds(2)));
        await AssertBlocked(readTask);
        ring.CompleteWriting();

        Assert.Equal(2, await readTask.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.Equal(source, destination[..source.Length]);
        Assert.True(ring.IsDrained);
        Assert.Equal(0, ring.Read(destination, 0, destination.Length));
    }

    [Theory]
    [MemberData(nameof(BothOrders))]
    public async Task WriterCompletionUnblocksFinalCompleteValueRead(
        ByteOrder byteOrder)
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            3 * sizeof(int),
            byteOrder);
        Assert.True(ring.TryWrite(11));
        Assert.True(ring.TryWrite(22));

        int[] destination = new int[3];
        using ManualResetEventSlim started = new();
        Task<int> readTask = Task.Run(() =>
        {
            started.Set();
            return ring.Read(destination.AsSpan());
        });

        Assert.True(started.Wait(TimeSpan.FromSeconds(2)));
        await AssertBlocked(readTask);
        ring.CompleteWriting();

        Assert.Equal(2, await readTask.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.Equal([11, 22, 0], destination);
        Assert.True(ring.IsDrained);
    }

    [Theory]
    [MemberData(nameof(BothOrders))]
    public async Task WriterCompletionReadsOnlyCompleteIntegralSpanBlocks(
        ByteOrder byteOrder)
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            4 * sizeof(int),
            byteOrder);
        Assert.Equal(3, ring.Write((ReadOnlySpan<int>)[10, 20, 30]));

        int[] destination = new int[4];
        GCHandle pin = GCHandle.Alloc(destination, GCHandleType.Pinned);
        try
        {
            IntegralSpan span;
            unsafe
            {
                span = IntegralTestData.CreateSpan(
                    (byte*)pin.AddrOfPinnedObject(),
                    destination.Length,
                    IntegralType.Int32,
                    ByteOrder.Native,
                    blockCapacity: 2);
            }

            using ManualResetEventSlim started = new();
            Task<int> readTask = Task.Run(() =>
            {
                started.Set();
                return ring.Read(span);
            });

            Assert.True(started.Wait(TimeSpan.FromSeconds(2)));
            await AssertBlocked(readTask);
            ring.CompleteWriting();

            Assert.Equal(2, await readTask.WaitAsync(TimeSpan.FromSeconds(5)));
            Assert.Equal([10, 20, 0, 0], destination);
            Assert.Equal(sizeof(int), ring.StoredBytes);

            byte[] trailing = new byte[sizeof(int)];
            Assert.Equal(
                trailing.Length,
                ring.Read(trailing, 0, trailing.Length));
            Assert.True(ring.IsDrained);
        }
        finally
        {
            pin.Free();
        }
    }

    [Theory]
    [MemberData(nameof(BothOrders))]
    public async Task WriterCompletionLeavesScalarTrailingBytesReadable(
        ByteOrder byteOrder)
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            sizeof(int),
            byteOrder);
        byte[] trailing = [0xA1, 0xB2];
        Assert.Equal(2, ring.Write(trailing, 0, trailing.Length));

        using ManualResetEventSlim started = new();
        Task<(bool Success, int Value)> readTask = Task.Run(() =>
        {
            started.Set();
            bool success = ring.Read(out int value);
            return (success, value);
        });

        Assert.True(started.Wait(TimeSpan.FromSeconds(2)));
        await AssertBlocked(readTask);
        ring.CompleteWriting();

        (bool success, int value) = await readTask.WaitAsync(
            TimeSpan.FromSeconds(5));
        Assert.False(success);
        Assert.Equal(default, value);
        Assert.Equal(2, ring.StoredBytes);

        byte[] destination = new byte[2];
        Assert.Equal(2, ring.Read(destination, 0, destination.Length));
        Assert.Equal(trailing, destination);
        Assert.True(ring.IsDrained);
    }

    [Theory]
    [InlineData(ReaderTermination.CompleteReading)]
    [InlineData(ReaderTermination.Abort)]
    [InlineData(ReaderTermination.Close)]
    [InlineData(ReaderTermination.Dispose)]
    public async Task ReaderTerminationUnblocksReader(ReaderTermination transition)
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            sizeof(int),
            ByteOrder.Native);
        byte[] destination = new byte[sizeof(int)];
        using ManualResetEventSlim started = new();
        Task<int> readTask = Task.Run(() =>
        {
            started.Set();
            return ring.Read(destination, 0, destination.Length);
        });

        Assert.True(started.Wait(TimeSpan.FromSeconds(2)));
        await AssertBlocked(readTask);
        Apply(ring, transition);

        Assert.Equal(0, await readTask.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Theory]
    [InlineData(WriterTermination.CompleteWriting)]
    [InlineData(WriterTermination.CompleteReading)]
    [InlineData(WriterTermination.Abort)]
    [InlineData(WriterTermination.Close)]
    [InlineData(WriterTermination.Dispose)]
    public async Task LifecycleTerminationUnblocksAndRejectsWriter(
        WriterTermination transition)
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            sizeof(int),
            ByteOrder.Native);
        Assert.True(ring.TryWrite(1));

        byte[] source = [2, 0, 0, 0];
        using ManualResetEventSlim started = new();
        Task<int> writeTask = Task.Run(() =>
        {
            started.Set();
            return ring.Write(source, 0, source.Length);
        });

        Assert.True(started.Wait(TimeSpan.FromSeconds(2)));
        await AssertBlocked(writeTask);
        Apply(ring, transition);

        Assert.Equal(0, await writeTask.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.False(ring.TryWrite(3));

        if (transition == WriterTermination.CompleteWriting)
        {
            Assert.Equal(sizeof(int), ring.StoredBytes);
        }
        else
        {
            Assert.Equal(0, ring.StoredBytes);
        }
    }

    [Theory]
    [MemberData(nameof(BothOrders))]
    public void TryOperationsRetainAtomicLifecycleSemantics(ByteOrder byteOrder)
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            2 * sizeof(int),
            byteOrder);
        Assert.True(ring.TryWrite(10));
        Assert.True(ring.TryWrite(20));
        ring.CompleteWriting();

        int[] one = new int[1];
        Assert.True(ring.TryRead(one.AsSpan()));
        Assert.Equal(10, one[0]);

        Assert.False(ring.TryRead(new int[2].AsSpan()));
        Assert.True(ring.TryRead(Span<int>.Empty));
        Assert.False(ring.TryWrite(ReadOnlySpan<int>.Empty));
        Assert.True(ring.TryRead(out int last));
        Assert.Equal(20, last);
        Assert.True(ring.IsDrained);
        Assert.False(ring.Read(out int eof));
        Assert.Equal(default, eof);

        ring.CompleteReading();
        Assert.False(ring.TryRead(Span<int>.Empty));
    }

    [Theory]
    [MemberData(nameof(BothOrders))]
    public void CompletionAndAbortPreserveCapacityAndCounters(ByteOrder byteOrder)
    {
        using IWaitableRingBuffer completed = IntegralRingBuffer.CreateWaitable(
            2 * sizeof(int),
            byteOrder);
        Assert.True(completed.TryWrite(1));
        long readBefore = Counter(completed, "TotalRead");
        long writtenBefore = Counter(completed, "TotalWritten");

        completed.CompleteReading();
        Assert.True(completed.IsOpen);
        Assert.Equal(2 * sizeof(int), completed.ByteCapacity);
        Assert.Equal(0, completed.StoredBytes);
        Assert.Equal(readBefore, Counter(completed, "TotalRead"));
        Assert.Equal(writtenBefore, Counter(completed, "TotalWritten"));

        using IWaitableRingBuffer aborted = IntegralRingBuffer.CreateWaitable(
            2 * sizeof(int),
            byteOrder);
        Assert.True(aborted.TryWrite(2));
        readBefore = Counter(aborted, "TotalRead");
        writtenBefore = Counter(aborted, "TotalWritten");

        aborted.Abort();
        Assert.True(aborted.IsOpen);
        Assert.Equal(2 * sizeof(int), aborted.ByteCapacity);
        Assert.Equal(0, aborted.StoredBytes);
        Assert.Equal(readBefore, Counter(aborted, "TotalRead"));
        Assert.Equal(writtenBefore, Counter(aborted, "TotalWritten"));
    }

    [Fact]
    public unsafe void MalformedArgumentsStillThrowAfterTermination()
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            sizeof(int),
            ByteOrder.Native);
        ring.Abort();

        byte[] bytes = new byte[sizeof(int)];
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ring.Read(bytes, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ring.Write(bytes, 0, -1));
        Assert.Throws<ArgumentNullException>(
            () => ring.TryRead<int>((int*)null, 0, 1));

        int value = 0;
        IntegralSpan invalid = new(
            (byte*)&value,
            0,
            sizeof(int),
            new IntegralFormat(
                IntegralType.Int32,
                1,
                ByteOrder.Undefined));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ring.TryReadChecked(invalid));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ring.TryWriteChecked(invalid));
    }

    [Theory]
    [MemberData(nameof(BothOrders))]
    public unsafe void AllWriteFamiliesRejectWritingCompletion(ByteOrder byteOrder)
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            4 * sizeof(int),
            byteOrder);
        ring.CompleteWriting();

        byte[] bytes = new byte[sizeof(int)];
        int[] values = [1];
        fixed (byte* bytePtr = bytes)
        fixed (int* valuePtr = values)
        {
            IntegralSpan span = IntegralTestData.CreateSpan(
                (byte*)valuePtr,
                values.Length,
                IntegralType.Int32,
                ByteOrder.Native);

            Assert.Equal(0, ring.Write(bytes, 0, bytes.Length));
            Assert.Equal(0, ring.Write(bytePtr, 0, bytes.Length));
            Assert.False(ring.Write(1));
            Assert.Equal(0, ring.Write(values, 0, values.Length));
            Assert.Equal(0, ring.Write(valuePtr, 0, values.Length));
            Assert.Equal(0, ring.Write((ReadOnlySpan<int>)values));
            Assert.Equal(0, ring.Write(span));
            Assert.Equal(0, ring.WriteChecked(span));
            Assert.False(ring.TryWrite(1));
            Assert.False(ring.TryWrite(values, 0, values.Length));
            Assert.False(ring.TryWrite(valuePtr, 0, values.Length));
            Assert.False(ring.TryWrite((ReadOnlySpan<int>)values));
            Assert.False(ring.TryWrite(span));
            Assert.False(ring.TryWriteChecked(span));
        }

        Assert.Equal(0, ring.StoredBytes);
        Assert.Equal(0, Counter(ring, "TotalWritten"));
    }

    [Theory]
    [InlineData(ReaderTermination.CompleteReading)]
    [InlineData(ReaderTermination.Abort)]
    public unsafe void AllReadFamiliesRejectConsumerTermination(
        ReaderTermination transition)
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            4 * sizeof(int),
            ByteOrder.Native);
        Assert.True(ring.TryWrite(123));
        Apply(ring, transition);
        long totalRead = Counter(ring, "TotalRead");

        byte[] bytes = new byte[sizeof(int)];
        int[] values = new int[1];
        fixed (byte* bytePtr = bytes)
        fixed (int* valuePtr = values)
        {
            IntegralSpan span = IntegralTestData.CreateSpan(
                (byte*)valuePtr,
                values.Length,
                IntegralType.Int32,
                ByteOrder.Native);

            Assert.Equal(0, ring.Read(bytes, 0, bytes.Length));
            Assert.Equal(0, ring.Read(bytePtr, 0, bytes.Length));
            Assert.False(ring.Read(out int scalar));
            Assert.Equal(default, scalar);
            Assert.Equal(0, ring.Read(values, 0, values.Length));
            Assert.Equal(0, ring.Read(valuePtr, 0, values.Length));
            Assert.Equal(0, ring.Read(values.AsSpan()));
            Assert.Equal(0, ring.Read(span));
            Assert.Equal(0, ring.ReadChecked(span));
            Assert.False(ring.TryRead(out scalar));
            Assert.Equal(default, scalar);
            Assert.False(ring.TryRead(values, 0, values.Length));
            Assert.False(ring.TryRead(valuePtr, 0, values.Length));
            Assert.False(ring.TryRead(values.AsSpan()));
            Assert.False(ring.TryRead(span));
            Assert.False(ring.TryReadChecked(span));
        }

        Assert.Equal(0, ring.StoredBytes);
        Assert.Equal(totalRead, Counter(ring, "TotalRead"));
    }

    [Theory]
    [MemberData(nameof(BothOrders))]
    public void AdvanceAndClearRemainAvailableAfterWritingCompletion(
        ByteOrder byteOrder)
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            2 * sizeof(int),
            byteOrder);
        Assert.True(ring.TryWrite(1));
        Assert.True(ring.TryWrite(2));
        long totalRead = Counter(ring, "TotalRead");
        ring.CompleteWriting();

        ring.Advance(1);
        Assert.Equal(2 * sizeof(int) - 1, ring.StoredBytes);
        Assert.Equal(totalRead, Counter(ring, "TotalRead"));
        Assert.False(ring.IsDrained);

        ring.ClearBuffer();
        Assert.Equal(0, ring.StoredBytes);
        Assert.Equal(totalRead, Counter(ring, "TotalRead"));
        Assert.True(ring.IsDrained);

        ring.AdvanceBy<int>(1);
        Assert.Equal(0, ring.StoredBytes);
        Assert.Equal(totalRead, Counter(ring, "TotalRead"));
    }

    [Theory]
    [MemberData(nameof(BothOrders))]
    public async Task FinalPartialByteReadSupportsWrapAround(ByteOrder byteOrder)
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            8,
            byteOrder);
        byte[] first = [1, 2, 3, 4, 5, 6];
        Assert.Equal(first.Length, ring.Write(first, 0, first.Length));

        byte[] prefix = new byte[4];
        Assert.Equal(prefix.Length, ring.Read(prefix, 0, prefix.Length));
        Assert.Equal([1, 2, 3, 4], prefix);

        byte[] wrapped = [7, 8, 9, 10];
        Assert.Equal(wrapped.Length, ring.Write(wrapped, 0, wrapped.Length));

        byte[] destination = new byte[8];
        using ManualResetEventSlim started = new();
        Task<int> readTask = Task.Run(() =>
        {
            started.Set();
            return ring.Read(destination, 0, destination.Length);
        });

        Assert.True(started.Wait(TimeSpan.FromSeconds(2)));
        await AssertBlocked(readTask);
        ring.CompleteWriting();

        Assert.Equal(6, await readTask.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.Equal([5, 6, 7, 8, 9, 10], destination[..6]);
        Assert.True(ring.IsDrained);
    }

    [Fact]
    public async Task FinalWriteRaceIsSerializedByTheRingLock()
    {
        for (int iteration = 0; iteration < 50; ++iteration)
        {
            using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
                sizeof(int),
                ByteOrder.Native);
            using Barrier barrier = new(3);

            Task<bool> write = Task.Run(() =>
            {
                barrier.SignalAndWait();
                return ring.Write(iteration);
            });
            Task complete = Task.Run(() =>
            {
                barrier.SignalAndWait();
                ring.CompleteWriting();
            });
            barrier.SignalAndWait();

            await Task.WhenAll(write, complete).WaitAsync(
                TimeSpan.FromSeconds(5));
            bool writeCompleted = await write;
            Assert.True(ring.IsWritingCompleted);

            if (writeCompleted)
            {
                Assert.True(ring.Read(out int value));
                Assert.Equal(iteration, value);
            }
            else
            {
                Assert.Equal(0, ring.StoredBytes);
            }

            Assert.True(ring.IsDrained);
        }
    }

    [Theory]
    [MemberData(nameof(BothOrders))]
    public async Task BoundedFileCopyDrainsThroughProducerCompletion(
        ByteOrder byteOrder)
    {
        byte[] source = Enumerable.Range(0, 4099)
            .Select(i => (byte)(i * 31))
            .ToArray();
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            64,
            byteOrder);

        Task producer = Task.Run(() =>
        {
            int offset = 0;
            while (offset < source.Length)
            {
                int count = Math.Min(31, source.Length - offset);
                int written = ring.Write(source, offset, count);
                Assert.Equal(count, written);
                offset += written;
            }

            ring.CompleteWriting();
        });

        Task<byte[]> consumer = Task.Run(() =>
        {
            using MemoryStream output = new();
            byte[] chunk = new byte[31];
            while (true)
            {
                int count = ring.Read(chunk, 0, chunk.Length);
                if (count == 0)
                {
                    return output.ToArray();
                }

                output.Write(chunk, 0, count);
            }
        });

        await Task.WhenAll(producer, consumer).WaitAsync(
            TimeSpan.FromSeconds(10));
        byte[] copied = await consumer;

        Assert.Equal(
            System.Security.Cryptography.SHA256.HashData(source),
            System.Security.Cryptography.SHA256.HashData(copied));
        Assert.Equal(source, copied);
        Assert.True(ring.IsDrained);
    }

    private static async Task AssertBlocked(Task task)
    {
        await Task.Delay(40);
        Assert.False(task.IsCompleted);
    }

    private static void Apply(
        IWaitableRingBuffer ring,
        ReaderTermination transition)
    {
        switch (transition)
        {
            case ReaderTermination.CompleteReading:
                ring.CompleteReading();
                break;
            case ReaderTermination.Abort:
                ring.Abort();
                break;
            case ReaderTermination.Close:
                ring.Close();
                break;
            case ReaderTermination.Dispose:
                ring.Dispose();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(transition));
        }
    }

    private static void Apply(
        IWaitableRingBuffer ring,
        WriterTermination transition)
    {
        switch (transition)
        {
            case WriterTermination.CompleteWriting:
                ring.CompleteWriting();
                break;
            case WriterTermination.CompleteReading:
                ring.CompleteReading();
                break;
            case WriterTermination.Abort:
                ring.Abort();
                break;
            case WriterTermination.Close:
                ring.Close();
                break;
            case WriterTermination.Dispose:
                ring.Dispose();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(transition));
        }
    }

    private static void ExerciseLifecycle(IWaitableRingBuffer ring)
    {
        ring.CompleteWriting();
        ring.CompleteReading();
        ring.Abort(new InvalidOperationException("ignored"));
    }

    private static void AssertLifecycleUnchanged(IWaitableRingBuffer ring)
    {
        Assert.False(ring.IsWritingCompleted);
        Assert.False(ring.IsReadingCompleted);
        Assert.False(ring.IsDrained);
        Assert.False(ring.IsAborted);
        Assert.Null(ring.AbortError);
    }

    private static long Counter(
        IWaitableRingBuffer ring,
        string propertyName)
    {
        object? value = ring.GetType().GetProperty(propertyName)?.GetValue(ring);
        return Assert.IsType<long>(value);
    }
}
