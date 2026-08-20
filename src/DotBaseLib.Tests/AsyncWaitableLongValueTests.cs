using DotBase.AsyncValue;
using DotBase.Tools;

namespace DotBaseLib.Tests;


public class AsyncWaitableLongValueTests
{
    [Fact]
    public void MutationsReturnCurrentValue()
    {
        using WaitableHighLowMarkValue value = new();

        Assert.Equal(5, value.SetValue(5));
        Assert.Equal(8, value.Increase(3));
        Assert.Equal(6, value.Decrease(2));
    }

    [Fact]
    public async Task SupportsMultipleIndependentWaiters()
    {
        using WaitableHighLowMarkValue value = new();
        using CountdownEvent started = new(3);

        Task<LongResult> firstHighWait = StartWait(
            started,
            () => value.WaitHighMarkValue(3));
        Task<LongResult> secondHighWait = StartWait(
            started,
            () => value.WaitHighMarkValue(5));
        Task<LongResult> lowWait = StartWait(
            started,
            () => value.WaitLowMarkValue(-2));

        Assert.True(started.Wait(TimeSpan.FromSeconds(2)));
        await Task.Delay(50);
        Assert.False(firstHighWait.IsCompleted);
        Assert.False(secondHighWait.IsCompleted);
        Assert.False(lowWait.IsCompleted);

        Assert.Equal(3, value.SetValue(3));
        Assert.Equal(LongResult.Success(3), await firstHighWait.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.False(secondHighWait.IsCompleted);
        Assert.False(lowWait.IsCompleted);

        Assert.Equal(5, value.SetValue(5));
        Assert.Equal(LongResult.Success(5), await secondHighWait.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.False(lowWait.IsCompleted);

        Assert.Equal(-2, value.SetValue(-2));
        Assert.Equal(LongResult.Success(-2), await lowWait.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task ClosingAndDisposingCompletePendingWaitsWithStableStatuses()
    {
        WaitableHighLowMarkValue value = new();
        using CountdownEvent closedWaitStarted = new(1);
        Task<LongResult> closedWait = StartWait(
            closedWaitStarted,
            () => value.WaitHighMarkValue(1));

        Assert.True(closedWaitStarted.Wait(TimeSpan.FromSeconds(2)));
        await Task.Delay(50);
        Assert.False(closedWait.IsCompleted);
        value.Close();
        LongResult closed = await closedWait.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(ResultStatus.CLOSED, closed.Status);

        value.Open();
        Assert.Equal(ResultStatus.CLOSED, closed.Status);

        using CountdownEvent disposedWaitStarted = new(1);
        Task<LongResult> disposedWait = StartWait(
            disposedWaitStarted,
            () => value.WaitLowMarkValue(-1));

        Assert.True(disposedWaitStarted.Wait(TimeSpan.FromSeconds(2)));
        await Task.Delay(50);
        Assert.False(disposedWait.IsCompleted);
        value.Dispose();
        LongResult disposed = await disposedWait.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(ResultStatus.DISPOSED, disposed.Status);
        Assert.Throws<ObjectDisposedException>(() => value.SetValue(1));
        Assert.Throws<ObjectDisposedException>(() => value.Increase(1));
        Assert.Throws<ObjectDisposedException>(() => value.Decrease(1));
    }

    [Fact]
    public async Task RangedConstructorRejectsTargetsOutsideRange()
    {
        using WaitableHighLowMarkValue value = new(0, 10, 0);

        Assert.Equal(0, value.Value);
        Assert.Equal(ResultStatus.OUT_OF_RANGE, value.WaitHighMarkValue(11).Status);
        Assert.Equal(ResultStatus.OUT_OF_RANGE, value.WaitLowMarkValue(-1).Status);

        using CountdownEvent started = new(1);
        Task<LongResult> highWait = StartWait(started, () => value.WaitHighMarkValue(10));
        Assert.True(started.Wait(TimeSpan.FromSeconds(2)));
        await Task.Delay(50);
        Assert.False(highWait.IsCompleted);

        Assert.Equal(10, value.SetValue(10));
        Assert.Equal(LongResult.Success(10), await highWait.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task SetRangeCompletesWaitersWhoseTargetLeavesTheRange()
    {
        using WaitableHighLowMarkValue value = new(0, 100, 0);
        using CountdownEvent started = new(1);
        Task<LongResult> highWait = StartWait(started, () => value.WaitHighMarkValue(10));

        Assert.True(started.Wait(TimeSpan.FromSeconds(2)));
        await Task.Delay(50);
        Assert.False(highWait.IsCompleted);

        value.SetRange(new LongValueRange(0, 5));
        LongResult result = await highWait.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(ResultStatus.OUT_OF_RANGE, result.Status);
        Assert.Equal(0, result.Value);
        Assert.Equal(0, value.Value);
    }

    [Fact]
    public async Task SetValueAndRangeUpdatesValueAndWakesWaiters()
    {
        using WaitableHighLowMarkValue value = new(0, 100, 0);
        using CountdownEvent started = new(1);
        Task<LongResult> highWait = StartWait(started, () => value.WaitHighMarkValue(3));

        Assert.True(started.Wait(TimeSpan.FromSeconds(2)));
        await Task.Delay(50);
        Assert.False(highWait.IsCompleted);

        Assert.Equal(4, value.SetValueAndRange(4, new LongValueRange(0, 10)));
        Assert.Equal(4, value.Value);
        Assert.Equal(LongResult.Success(4), await highWait.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public void SetRangeAndSetValueAndRangeRejectValueOutsideRange()
    {
        using WaitableHighLowMarkValue value = new(0, 10, 5);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => value.SetRange(new LongValueRange(0, 3)));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => value.SetValueAndRange(20, new LongValueRange(0, 10)));
        Assert.Equal(5, value.Value);
    }

    [Fact]
    public void RangedConstructorRequiresValueInsideRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new WaitableHighLowMarkValue(0, 10, 11));
    }

    private static Task<LongResult> StartWait(
        CountdownEvent started,
        Func<LongResult> wait)
    {
        return Task.Run(() =>
        {
            started.Signal();
            return wait();
        });
    }
}
