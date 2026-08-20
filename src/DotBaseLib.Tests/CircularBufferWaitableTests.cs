using DotBase.Buffers;

namespace DotBaseLib.Tests;


public class CircularBufferWaitableTests
{
    [Fact]
    public async Task WriteWaitsUntilRequestedLengthFits()
    {
        using CircularBufferWaitable buffer = new(3);
        Assert.Equal(3, buffer.Write([1, 2, 3], 0, 3));
        using ManualResetEventSlim started = new();

        Task<int> writeTask = Task.Run(() =>
        {
            started.Set();
            return buffer.Write([4, 5], 0, 2);
        });

        Assert.True(started.Wait(TimeSpan.FromSeconds(2)));
        await Task.Delay(50);
        Assert.False(writeTask.IsCompleted);

        byte[] destination = new byte[2];
        Assert.Equal(2, buffer.Read(destination, 0, destination.Length));
        Assert.Equal([1, 2], destination);

        Assert.Equal(2, await writeTask.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.Equal(3, buffer.StoredBytes);
    }

    [Fact]
    public async Task CloseReleasesBlockedWrite()
    {
        using CircularBufferWaitable buffer = new(1);
        Assert.Equal(1, buffer.Write([1], 0, 1));
        using ManualResetEventSlim started = new();

        Task<int> writeTask = Task.Run(() =>
        {
            started.Set();
            return buffer.Write([2], 0, 1);
        });

        Assert.True(started.Wait(TimeSpan.FromSeconds(2)));
        await Task.Delay(50);
        Assert.False(writeTask.IsCompleted);

        buffer.Close();

        Assert.Equal(0, await writeTask.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task ReadWaitsUntilRequestedLengthIsStored()
    {
        using CircularBufferWaitable buffer = new(4);
        byte[] destination = new byte[3];
        using ManualResetEventSlim started = new();

        Task<int> readTask = Task.Run(() =>
        {
            started.Set();
            return buffer.Read(destination, 0, destination.Length);
        });

        Assert.True(started.Wait(TimeSpan.FromSeconds(2)));
        Assert.Equal(2, buffer.Write([1, 2], 0, 2));
        await Task.Delay(50);
        Assert.False(readTask.IsCompleted);

        Assert.Equal(1, buffer.Write([3], 0, 1));
        Assert.Equal(3, await readTask.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.Equal([1, 2, 3], destination);
    }

    [Fact]
    public async Task CloseReleasesBlockedRead()
    {
        using CircularBufferWaitable buffer = new(4);
        byte[] destination = new byte[1];
        using ManualResetEventSlim started = new();

        Task<int> readTask = Task.Run(() =>
        {
            started.Set();
            return buffer.Read(destination, 0, destination.Length);
        });

        Assert.True(started.Wait(TimeSpan.FromSeconds(2)));
        await Task.Delay(50);
        Assert.False(readTask.IsCompleted);

        buffer.Close();

        Assert.Equal(0, await readTask.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task AdvanceSynchronizesWaitableStoredCountWithClampedBufferCount()
    {
        using CircularBufferWaitable buffer = new(4);
        Assert.Equal(2, buffer.Write([1, 2], 0, 2));
        buffer.Advance(10);
        Assert.Equal(0, buffer.StoredBytes);

        byte[] destination = new byte[1];
        using ManualResetEventSlim started = new();
        Task<int> readTask = Task.Run(() =>
        {
            started.Set();
            return buffer.Read(destination, 0, destination.Length);
        });

        Assert.True(started.Wait(TimeSpan.FromSeconds(2)));
        await Task.Delay(50);
        Assert.False(readTask.IsCompleted);

        Assert.Equal(1, buffer.Write([3], 0, 1));
        Assert.Equal(1, await readTask.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.Equal(3, destination[0]);
    }

    [Fact]
    public unsafe void OversizedArrayAndPointerRequestsReturnZeroWithoutMutation()
    {
        using CircularBufferWaitable buffer = new(3);
        byte[] oversized = [1, 2, 3, 4];

        fixed (byte* pointer = oversized)
        {
            Assert.Equal(0, buffer.Write(oversized, 0, oversized.Length));
            Assert.Equal(0, buffer.Write(pointer, 0, oversized.Length));
            Assert.Equal(0, buffer.StoredBytes);
            Assert.Equal(3, buffer.FreeBytes);
            Assert.Equal(0, buffer.TotalWritten);

            Assert.Equal(3, buffer.Write(oversized, 0, 3));
            Assert.Equal(0, buffer.Read(oversized, 0, oversized.Length));
            Assert.Equal(0, buffer.Read(pointer, 0, oversized.Length));
            Assert.Equal(3, buffer.StoredBytes);
            Assert.Equal(0, buffer.FreeBytes);
            Assert.Equal(0, buffer.TotalRead);
        }
    }

    [Fact]
    public unsafe void OversizedRequestsAfterCloseReturnZero()
    {
        using CircularBufferWaitable buffer = new(3);
        byte[] data = [1];
        buffer.Close();

        fixed (byte* pointer = data)
        {
            Assert.Equal(0, buffer.Write(data, 0, data.Length));
            Assert.Equal(0, buffer.Write(pointer, 0, data.Length));
            Assert.Equal(0, buffer.Read(data, 0, data.Length));
            Assert.Equal(0, buffer.Read(pointer, 0, data.Length));
        }
    }

    [Fact]
    public unsafe void MalformedArgumentsStillThrow()
    {
        using CircularBufferWaitable buffer = new(3);
        byte[] data = [1, 2, 3, 4];

        Assert.Throws<ArgumentNullException>(
            () => buffer.Write((byte[])null!, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => buffer.Write(data, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => buffer.Write(data, 0, -1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => buffer.Read(data, 3, 2));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => buffer.Write((byte*)null, -1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => buffer.Read((byte*)null, 0, -1));
        Assert.Throws<ArgumentNullException>(
            () => buffer.Write((byte*)null, 0, 1));
        Assert.Throws<ArgumentNullException>(
            () => buffer.Read((byte*)null, 1, 0));
    }
}
