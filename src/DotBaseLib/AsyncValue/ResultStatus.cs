namespace DotBase.AsyncValue;


/// <summary>
/// 
/// Identifies the terminal status of an operation represented by <see cref="LongResult"/>.
/// 
/// </summary>
public enum ResultStatus : int
{
    SUCCESS = 0,

    FAILED = -1,

    CANCELED = -2,

    CLOSED = -3,

    DISPOSED = -4,

    EXCEPTION = -5,

    TIMEOUT = -6,

    NOT_FOUND = -7,

    BAD_STATE = -8,

    BAD_MESSAGE = -9,

    INVALID_ARGUMENT = -10,

    /// <summary>The requested target is outside the admitted range or cannot be reached within it.</summary>
    OUT_OF_RANGE = -11,
}

public static class ResultStatusMethods
{
    public static bool IsSuccess(this ResultStatus status)
    {
        return status == ResultStatus.SUCCESS;
    }

    public static bool IsError(this ResultStatus status)
    {
        return status != ResultStatus.SUCCESS;
    }

    public static ValueTask<LongResult> AsValueTask(this ResultStatus status)
    {
        return ValueTask.FromResult((LongResult)status);
    }
}
