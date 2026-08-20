using DotBase.AsyncValue;
using DotBase.AsyncValue.Internal;

namespace DotBaseLib.Tests;


public class AsyncWaitableValueTests
{
    [Fact]
    public async Task SupportsEntireLongRange()
    {
        var value = new AsyncWaitableValue();
        ValueTask<LongResult> maximumWait = value.WaitEqualToAsync(long.MaxValue);

        value.SetValue(long.MaxValue);

        AssertSuccess(long.MaxValue, await maximumWait);

        value.SetValue(long.MinValue);

        AssertSuccess(long.MinValue, await value.WaitEqualToAsync(long.MinValue));
    }

    [Fact]
    public async Task InvalidTimeoutIsReportedBeforeOtherImmediateOutcomes()
    {
        var value = new AsyncWaitableValue();
        value.SetValue(5);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        ValueTask<LongResult> wait = value.WaitEqualToAsync(
            5,
            Timeout.Infinite - 1,
            cancellation.Token);

        LongResult result = await wait;

        Assert.Equal(ResultStatus.INVALID_ARGUMENT, result.Status);
    }

    [Fact]
    public async Task PreCancelledTokenWinsOverSatisfiedValueAndZeroTimeout()
    {
        var value = new AsyncWaitableValue();
        value.SetValue(5);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        ValueTask<LongResult> wait = value.WaitEqualToAsync(5, 0, cancellation.Token);

        LongResult result = await wait;

        Assert.Equal(ResultStatus.CANCELED, result.Status);
    }

    [Fact]
    public async Task SatisfiedValueWinsOverZeroTimeout()
    {
        var value = new AsyncWaitableValue();
        value.SetValue(5);

        ValueTask<LongResult> wait = value.WaitEqualToAsync(5, 0, CancellationToken.None);

        AssertSuccess(5, await wait);
    }

    [Fact]
    public async Task ZeroTimeoutDisablesTimerWhenCancellationIsUnavailable()
    {
        var value = new AsyncWaitableValue();

        ValueTask<LongResult> wait = value.WaitEqualToAsync(5, 0, CancellationToken.None);

        Assert.False(wait.IsCompleted);

        value.SetValue(5);

        AssertSuccess(5, await wait);
    }

    [Fact]
    public async Task PendingCancellationReturnsStatusAndFinishedAwaiterCanBeRemoved()
    {
        var list = new AsyncValueAwaiterList();
        using var cancellation = new CancellationTokenSource();
        ValueTaskSource<LongResult> source = list.AddTarget(
            AsyncComparison.EqualTo,
            1,
            Timeout.Infinite,
            cancellation.Token);
        var wait = new ValueTask<LongResult>(source, source.Version);
        Assert.Equal(1, list.ActiveCount);

        cancellation.Cancel();

        LongResult result = await wait.AsTask().WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal(ResultStatus.CANCELED, result.Status);
        Assert.True(source.IsSucceeded);
        Assert.Equal(1, list.RemoveFinishedTargets());
        Assert.Equal(0, list.ActiveCount);
    }

    [Fact]
    public async Task PendingTimeoutReturnsStatusAndFinishedAwaiterCanBeRemoved()
    {
        var list = new AsyncValueAwaiterList();
        ValueTaskSource<LongResult> source = list.AddTarget(
            AsyncComparison.EqualTo,
            1,
            20,
            CancellationToken.None);
        var wait = new ValueTask<LongResult>(source, source.Version);
        Assert.Equal(1, list.ActiveCount);

        LongResult result = await wait.AsTask().WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal(ResultStatus.TIMEOUT, result.Status);
        Assert.True(source.IsSucceeded);
        Assert.Equal(1, list.RemoveFinishedTargets());
        Assert.Equal(0, list.ActiveCount);
    }

    [Fact]
    public async Task MultipleWaitersHaveIndependentSignals()
    {
        var value = new AsyncWaitableValue();
        using var cancellation = new CancellationTokenSource();
        ValueTask<LongResult> cancelledWait = value.WaitEqualToAsync(
            1,
            Timeout.Infinite,
            cancellation.Token);
        ValueTask<LongResult> successfulWait = value.WaitEqualToAsync(
            2,
            5_000,
            CancellationToken.None);

        cancellation.Cancel();
        value.SetValue(2);

        LongResult cancelledResult = await cancelledWait.AsTask().WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal(ResultStatus.CANCELED, cancelledResult.Status);
        AssertSuccess(2, await successfulWait.AsTask().WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task ValueAndCancellationRaceHasOneTerminalOutcome()
    {
        for (int iteration = 0; iteration < 100; iteration++)
        {
            var list = new AsyncValueAwaiterList();
            using var cancellation = new CancellationTokenSource();
            ValueTaskSource<LongResult> source = list.AddTarget(
                AsyncComparison.EqualTo,
                1,
                Timeout.Infinite,
                cancellation.Token);
            var wait = new ValueTask<LongResult>(source, source.Version);

            Task<int> valueCompletion = Task.Run(
                () => list.CheckValueTargets(1, new LongValueRange()));
            Task cancellationCompletion = Task.Run(cancellation.Cancel);
            await Task.WhenAll(valueCompletion, cancellationCompletion);
            int valueCompletionCount = await valueCompletion;

            LongResult result = await wait.AsTask().WaitAsync(TimeSpan.FromSeconds(5));

            if (result.Status == ResultStatus.SUCCESS)
            {
                AssertSuccess(1, result);
            }
            else
            {
                Assert.Equal(ResultStatus.CANCELED, result.Status);
            }

            Assert.True(source.IsSucceeded);
            Assert.InRange(valueCompletionCount, 0, 1);
            Assert.Equal(0, list.ActiveCount);
        }
    }

    [Fact]
    public async Task ValueAndTimeoutRaceHasOneTerminalOutcome()
    {
        for (int iteration = 0; iteration < 50; iteration++)
        {
            var list = new AsyncValueAwaiterList();
            ValueTaskSource<LongResult> source = list.AddTarget(
                AsyncComparison.EqualTo,
                1,
                1,
                CancellationToken.None);
            var wait = new ValueTask<LongResult>(source, source.Version);

            Task<int> valueCompletion = Task.Run(
                () => list.CheckValueTargets(1, new LongValueRange()));
            int valueCompletionCount = await valueCompletion;

            LongResult result = await wait.AsTask().WaitAsync(TimeSpan.FromSeconds(5));

            if (result.Status == ResultStatus.SUCCESS)
            {
                AssertSuccess(1, result);
            }
            else
            {
                Assert.Equal(ResultStatus.TIMEOUT, result.Status);
            }

            Assert.True(source.IsSucceeded);
            Assert.InRange(valueCompletionCount, 0, 1);
            Assert.Equal(0, list.ActiveCount);
        }
    }

    private static ValueTask<LongResult> Wait(
        AsyncWaitableValue value,
        AsyncComparison comparison,
        long target)
    {
        return comparison switch
        {
            AsyncComparison.EqualTo => value.WaitEqualToAsync(target),
            AsyncComparison.GreaterThan => value.WaitGreaterThanAsync(target),
            AsyncComparison.GreaterOrEqualTo => value.WaitGreaterOrEqualToAsync(target),
            AsyncComparison.LessThan => value.WaitLessThanAsync(target),
            AsyncComparison.LessOrEqualTo => value.WaitLessOrEqualToAsync(target),
            _ => throw new ArgumentOutOfRangeException(nameof(comparison)),
        };
    }

    private static ValueTask<LongResult> Wait(
        AsyncWaitableValue value,
        AsyncComparison comparison,
        long target,
        int timeout,
        CancellationToken cancellationToken)
    {
        return comparison switch
        {
            AsyncComparison.EqualTo => value.WaitEqualToAsync(target, timeout, cancellationToken),
            AsyncComparison.GreaterThan => value.WaitGreaterThanAsync(target, timeout, cancellationToken),
            AsyncComparison.GreaterOrEqualTo => value.WaitGreaterOrEqualToAsync(target, timeout, cancellationToken),
            AsyncComparison.LessThan => value.WaitLessThanAsync(target, timeout, cancellationToken),
            AsyncComparison.LessOrEqualTo => value.WaitLessOrEqualToAsync(target, timeout, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(comparison)),
        };
    }

    private static void AssertSuccess(long expectedValue, LongResult result)
    {
        Assert.Equal(ResultStatus.SUCCESS, result.Status);
        Assert.Equal(expectedValue, result.Value);
    }
}
