namespace DotBase.Tools;


public static class InvalidOperationExtension
{
    public static void ThrowIfNotNull(object? other, string? message = null)
    {
        if (other is not null)
        {
            throw new InvalidOperationException(message);
        }
    }

    public static void ThrowIfFalse(bool value, string? message = null)
    {
        if (!value)
        {
            throw new InvalidOperationException(message);
        }
    }

    public static void ThrowIfTrue(bool value, string? message = null)
    {
        if (value)
        {
            throw new InvalidOperationException(message);
        }
    }
}
