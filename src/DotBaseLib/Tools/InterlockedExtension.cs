namespace DotBase.Tools;


public class InterlockedExtension
{
    public static bool AssignIfNewValueSmaller(ref int target, int newValue)
    {
        int snapshot;
        bool stillLess;
        do
        {
            snapshot = target;
            stillLess = newValue < snapshot;
        } while (stillLess && Interlocked.CompareExchange(ref target, newValue, snapshot) != snapshot);

        return stillLess;
    }

    public static bool AssignIfNewValueSmaller(ref long target, long newValue)
    {
        long snapshot;
        bool stillLess;
        do
        {
            snapshot = target;
            stillLess = newValue < snapshot;
        } while (stillLess && Interlocked.CompareExchange(ref target, newValue, snapshot) != snapshot);

        return stillLess;
    }

    public static bool AssignIfNewValueSmaller(ref ulong target, ulong newValue)
    {
        ulong snapshot;
        bool stillLess;
        do
        {
            snapshot = target;
            stillLess = newValue < snapshot;
        } while (stillLess && Interlocked.CompareExchange(ref target, newValue, snapshot) != snapshot);

        return stillLess;
    }

    public static bool AssignIfNewValueBigger(ref int target, int newValue)
    {
        int snapshot;
        bool stillMore;
        do
        {
            snapshot = target;
            stillMore = newValue > snapshot;
        } while (stillMore && Interlocked.CompareExchange(ref target, newValue, snapshot) != snapshot);

        return stillMore;
    }

    public static bool AssignIfNewValueBigger(ref long target, long newValue)
    {
        long snapshot;
        bool stillMore;
        do
        {
            snapshot = target;
            stillMore = newValue > snapshot;
        } while (stillMore && Interlocked.CompareExchange(ref target, newValue, snapshot) != snapshot);

        return stillMore;
    }

    public static bool AssignIfNewValueBigger(ref ulong target, ulong newValue)
    {
        ulong snapshot;
        bool stillMore;
        do
        {
            snapshot = target;
            stillMore = newValue > snapshot;
        } while (stillMore && Interlocked.CompareExchange(ref target, newValue, snapshot) != snapshot);

        return stillMore;
    }
}
