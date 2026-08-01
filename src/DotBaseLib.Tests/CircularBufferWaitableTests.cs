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
}
