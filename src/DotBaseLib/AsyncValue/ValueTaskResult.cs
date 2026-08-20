namespace DotBase.AsyncValue;


/// <summary>Creates completed, canceled, or faulted <see cref="ValueTask{TResult}"/> instances for <see cref="LongResult"/>.</summary>
public static class ValueTaskResult
{
    public static readonly ValueTask<LongResult> SUCCESS = ValueTask.FromResult(LongResult.SUCCESS);

    public static readonly ValueTask<LongResult> FAILED = ValueTask.FromResult(LongResult.FAILED);

    public static readonly ValueTask<LongResult> CANCELED = ValueTask.FromResult(LongResult.CANCELED);

    public static readonly ValueTask<LongResult> EXCEPTION = ValueTask.FromResult(LongResult.EXCEPTION);

    public static readonly ValueTask<LongResult> TIMEOUT = ValueTask.FromResult(LongResult.TIMEOUT);

    public static readonly ValueTask<LongResult> DISPOSED = ValueTask.FromResult(LongResult.DISPOSED);

    public static readonly ValueTask<LongResult> NOT_FOUND = ValueTask.FromResult(LongResult.NOT_FOUND);

    public static readonly ValueTask<LongResult> BAD_STATE = ValueTask.FromResult(LongResult.BAD_STATE);

    public static readonly ValueTask<LongResult> BAD_MESSAGE = ValueTask.FromResult(LongResult.BAD_MESSAGE);

    public static readonly ValueTask<LongResult> INVALID_ARGUMENT = ValueTask.FromResult(LongResult.INVALID_ARGUMENT);

    public static readonly ValueTask<LongResult> OUT_OF_RANGE = ValueTask.FromResult(LongResult.OUT_OF_RANGE);



    /// <summary> Creates a completed result: <paramref name="status"/> with optional <paramref name="value"/>.</summary>
    public static ValueTask<LongResult> FromStatus(ResultStatus status, long value = 0)
    {
        return ValueTask.FromResult(LongResult.FromStatus(status, value));
    }

    /// <summary> Creates a completed result: <see cref="LongResult.SUCCESS"/> with optional value.</summary>
    public static ValueTask<LongResult> Success(long value = 0)
    {
        return ValueTask.FromResult(LongResult.Success(value));
    }

    /// <summary> Creates a completed result: <see cref="LongResult.CANCELED"/> with optional value.</summary>
    public static ValueTask<LongResult> Canceled(long value = 0)
    {
        return ValueTask.FromResult(LongResult.Canceled(value));
    }

    /// <summary> Creates a completed result: <see cref="LongResult.INVALID_ARGUMENT"/> with optional value.</summary>
    public static ValueTask<LongResult> InvalidArgument(long value = 0)
    {
        return ValueTask.FromResult(LongResult.InvalidArgument(value));
    }

    /// <summary> Creates a completed result: <see cref="LongResult.OUT_OF_RANGE"/> with optional value.</summary>
    public static ValueTask<LongResult> OutOfRange(long value = 0)
    {
        return ValueTask.FromResult(LongResult.OutOfRange(value));
    }

    /// <summary> Creates a faulted task: associated with exception <paramref name="ex"/>.</summary>
    public static ValueTask<LongResult> CompletedByException(Exception ex)
    {
        return ValueTask.FromException<LongResult>(ex);
    }

    /// <summary> Creates a canceled task: associated with <paramref name="cancellation"/>.</summary>
    public static ValueTask<LongResult> CompletedByCancellation(CancellationToken cancellation)
    {
        return ValueTask.FromCanceled<LongResult>(cancellation);
    }
}
