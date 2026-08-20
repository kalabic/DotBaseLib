using System.Diagnostics.CodeAnalysis;

namespace DotBase.AsyncValue;


/// <summary>
///
/// Represents a numeric operation result together with its terminal <see cref="ResultStatus"/>.
/// Async value waits return the current value for both successful and out-of-range outcomes.
///<para>
/// Conversion to and from <see langword="long"/> type internal value is done using <see cref="System.Convert"/> API.
///</para>
/// </summary>
public readonly struct LongResult
    : IEquatable<LongResult>
{
    /// <summary> Creates a result with given <paramref name="status"/> and containing the optional <paramref name="value"/>.</summary>
    public static LongResult FromStatus(ResultStatus status, long value = 0) { return new LongResult(value, status); }

    /// <summary> Creates a successful result containing the optional <paramref name="value"/>.</summary>
    public static LongResult Success(long value = 0) { return new LongResult(value); }

    /// <summary> Creates a cancelled result containing the optional <paramref name="value"/>.</summary>
    public static LongResult Canceled(long value = 0) { return new LongResult(value, ResultStatus.CANCELED); }

    /// <summary> Creates a closed result containing the optional <paramref name="value"/>.</summary>
    public static LongResult Closed(long value = 0) { return new LongResult(value, ResultStatus.CLOSED); }

    /// <summary> Creates a disposed result containing the optional <paramref name="value"/>.</summary>
    public static LongResult Disposed(long value = 0) { return new LongResult(value, ResultStatus.DISPOSED); }

    /// <summary> Creates an invalid argument result containing the optional <paramref name="value"/>.</summary>
    public static LongResult InvalidArgument(long value = 0) { return new LongResult(value, ResultStatus.INVALID_ARGUMENT); }

    /// <summary> Creates an out-of-range result containing the optional <paramref name="value"/>.</summary>
    public static LongResult OutOfRange(long value = 0) { return new LongResult(value, ResultStatus.OUT_OF_RANGE); }

    //-------------------------------------------------------------------------
    //
    // Public properties.
    //
    //-------------------------------------------------------------------------

    /// <summary>Gets the numeric value associated with the result.</summary>
    public readonly long Value;

    /// <summary>Gets the terminal status.</summary>
    public readonly ResultStatus Status;


    //-------------------------------------------------------------------------
    //
    // Implementation.
    //
    //-------------------------------------------------------------------------

    public LongResult(bool value)
    {
        Value = Convert.ToInt64(value);
        Status = ResultStatus.SUCCESS;
    }

    public LongResult(short value)
    {
        Value = Convert.ToInt64(value);
        Status = ResultStatus.SUCCESS;
    }

    public LongResult(int value)
    {
        Value = Convert.ToInt64(value);
        Status = ResultStatus.SUCCESS;
    }

    public LongResult(long value)
    {
        Value = value;
        Status = ResultStatus.SUCCESS;
    }

    public LongResult(ulong value)
    {
        Value = Convert.ToInt64(value);
        Status = ResultStatus.SUCCESS;
    }

    public LongResult(ResultStatus status)
    {
        Value = 0;
        Status = status;
    }

    public LongResult(long value, ResultStatus status)
    {
        Value = value;
        Status = status;
    }

    public bool ValueAsBool()
    {
        return Convert.ToBoolean(Value);
    }

    public short ValueAsShort()
    {
        return Convert.ToInt16(Value);
    }

    public int ValueAsInt()
    {
        return Convert.ToInt32(Value);
    }

    public long ValueAsLong()
    {
        return Value;
    }

    public ulong ValueAsULong()
    {
        return Convert.ToUInt64(Value);
    }

    public ValueTask<LongResult> AsValueTask()
    {
        return ValueTask.FromResult(this);
    }

    public bool Equals(LongResult other)
        => other.Status == Status && other.Value == Value;

    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is LongResult other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(Status, Value);

    public override string ToString()
        => Status == ResultStatus.SUCCESS ? Value.ToString() : Status.ToString();


    // Operators >>

    public static bool operator ==(LongResult first, LongResult second)
    {
        return (first.Status == second.Status) && (first.Value == second.Value);
    }

    public static bool operator !=(LongResult first, LongResult second)
    {
        return (first.Status != second.Status) || (first.Value != second.Value);
    }


    //-------------------------------------------------------------------------
    //
    // Static properties.
    //
    //-------------------------------------------------------------------------

    public static readonly LongResult SUCCESS = new(ResultStatus.SUCCESS);

    public static readonly LongResult FAILED = new(ResultStatus.FAILED);

    public static readonly LongResult CANCELED = new(ResultStatus.CANCELED);

    public static readonly LongResult CLOSED = new(ResultStatus.CLOSED);

    public static readonly LongResult DISPOSED = new(ResultStatus.DISPOSED);

    public static readonly LongResult EXCEPTION = new(ResultStatus.EXCEPTION);

    public static readonly LongResult TIMEOUT = new(ResultStatus.TIMEOUT);

    public static readonly LongResult NOT_FOUND = new(ResultStatus.NOT_FOUND);

    public static readonly LongResult BAD_STATE = new(ResultStatus.BAD_STATE);

    public static readonly LongResult BAD_MESSAGE = new(ResultStatus.BAD_MESSAGE);

    public static readonly LongResult INVALID_ARGUMENT = new(ResultStatus.INVALID_ARGUMENT);

    public static readonly LongResult OUT_OF_RANGE = new(ResultStatus.OUT_OF_RANGE);


    /// <summary>
    /// Note how implicit <see langword="bool"/> operator ignores the value and works only with status codes.
    /// </summary>
    /// <param name="other"></param>
    public static implicit operator bool(LongResult other)
    {
        return other.Status == ResultStatus.SUCCESS;
    }

    //
    // Not implemented to eliminate the inconsistency:
    // - 'bool' operator interprets false as FAILED
    // - LongResult(false) interprets it as a successful numeric value of zero
    //
/*  public static implicit operator LongResult(bool other)
    { 
        return other ? SUCCESS : FAILED;
    } */

    public static implicit operator LongResult(ResultStatus other)
    {
        switch (other)
        {
            case ResultStatus.SUCCESS:
                return SUCCESS;

            case ResultStatus.FAILED:
                return FAILED;

            case ResultStatus.CANCELED:
                return CANCELED;

            case ResultStatus.CLOSED:
                return CLOSED;

            case ResultStatus.DISPOSED:
                return DISPOSED;

            case ResultStatus.EXCEPTION:
                return EXCEPTION;

            case ResultStatus.TIMEOUT:
                return TIMEOUT;

            case ResultStatus.NOT_FOUND:
                return NOT_FOUND;

            case ResultStatus.BAD_STATE:
                return BAD_STATE;

            case ResultStatus.BAD_MESSAGE:
                return BAD_MESSAGE;

            case ResultStatus.INVALID_ARGUMENT:
                return INVALID_ARGUMENT;

            case ResultStatus.OUT_OF_RANGE:
                return OUT_OF_RANGE;

            default:
                return FAILED;
        }
    }
}
