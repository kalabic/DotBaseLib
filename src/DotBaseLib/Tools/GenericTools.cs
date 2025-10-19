using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace DotBase.Tools;


public static class GenericTools
{
    public static TMessage? TryCreateDefaultInstance<TMessage>()
    {
        try
        {
            var ctor = CreateDefaultInstance(typeof(TMessage));
            return (TMessage?)ctor();
        }
        catch
        {
            return default;
        }
    }

    private static readonly ConcurrentDictionary<Type, Func<object>> _ctorCache = new();

    public static Func<object> CreateDefaultInstance(Type type)
    {
        if (_ctorCache.TryGetValue(type, out var cached))
        {
            return cached;
        }

        NewExpression newExp = Expression.New(type);
        var lambda = Expression.Lambda<Func<object>>(newExp);
        var compiled = lambda.Compile();
        _ctorCache[type] = compiled;
        return compiled;
    }

    public static long ToLong<T>(T value)
    {
        if (GenericType<T>.IsUnmanaged)
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            return (long)(dynamic)value;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }

        throw new InvalidOperationException();
    }
}
