namespace DotBase.AsyncValue;

internal enum AsyncComparison
{
    Undefined = 0,
    EqualTo,
    NotEqualTo,
    GreaterThan,
    GreaterOrEqualTo,
    LessThan,
    LessOrEqualTo,
}

internal static class AsyncComparisonExtensions
{
    internal static bool Compare(this AsyncComparison operation, long left, long right)
    {
        switch (operation)
        {
            case AsyncComparison.Undefined:
                return false;

            case AsyncComparison.EqualTo:
                return left == right;

            case AsyncComparison.NotEqualTo:
                return left != right;

            case AsyncComparison.GreaterThan:
                return left > right;

            case AsyncComparison.GreaterOrEqualTo:
                return left >= right;

            case AsyncComparison.LessThan:
                return left < right;

            case AsyncComparison.LessOrEqualTo:
                return left <= right;

            default:
                throw new InvalidOperationException();
        }
    }
}
