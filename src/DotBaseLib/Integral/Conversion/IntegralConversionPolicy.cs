using DotBase.Integral.Conversion.Internal;

namespace DotBase.Integral.Conversion;


/// <summary>
/// Unmanaged process-local conversion policy attached to <see cref="IntegralFormat"/>.
/// Zero selects built-in behavior, minus one refuses every path, and positive
/// values identify immutable managed entries retained by the policy registry.
/// </summary>
public readonly struct IntegralConversionPolicy
    : IEquatable<IntegralConversionPolicy>
{
    /// <summary>All paths use built-in conversion tables.</summary>
    public static IntegralConversionPolicy None => default;

    private readonly int _registryIndex;

    private IntegralConversionPolicy(int registryIndex)
    {
        _registryIndex = registryIndex;
    }

    internal int RegistryIndex => _registryIndex;

    public bool IsEmpty => _registryIndex == ConversionPolicyRegistry.Default;

    /// <summary>All paths refuse conversion.</summary>
    public static IntegralConversionPolicy RefuseAll()
    {
        return new IntegralConversionPolicy(ConversionPolicyRegistry.Refuse);
    }

    /// <summary>
    /// Uses <paramref name="converters"/> for span, planar, and interleaved
    /// scalar conversion paths.
    /// </summary>
    public static IntegralConversionPolicy FromValueConverters(
        NumericValueConverters converters)
    {
        ArgumentNullException.ThrowIfNull(converters);
        return new IntegralConversionPolicy(
            converters.GetOrRegisterPolicyIndex());
    }

    /// <summary>
    /// Uses a single-cast static structural function for the contiguous span path.
    /// Planar and interleaved paths continue using built-in tables. When supplied,
    /// <paramref name="valueConverters"/> provides the scalar converter for the
    /// static span function.
    /// </summary>
    public static IntegralConversionPolicy FromFunc(
        IntegralSpanConversionFunc func,
        NumericValueConverters? valueConverters = null)
    {
        ArgumentNullException.ThrowIfNull(func);
        if (func.Target is not null ||
            !func.Method.IsStatic ||
            func.GetInvocationList().Length != 1)
        {
            throw new ArgumentException(
                "The conversion function must be a single-cast static delegate.",
                nameof(func));
        }

        valueConverters?.EnsureFrozenForPolicy();
        int registryIndex =
            ConversionPolicyRegistry.RegisterStaticSpanFunction(
                func,
                valueConverters);
        return new IntegralConversionPolicy(registryIndex);
    }

    public bool Equals(IntegralConversionPolicy other)
    {
        return _registryIndex == other._registryIndex;
    }

    public override bool Equals(object? obj)
    {
        return obj is IntegralConversionPolicy other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _registryIndex;
    }

    public static bool operator ==(
        IntegralConversionPolicy left,
        IntegralConversionPolicy right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(
        IntegralConversionPolicy left,
        IntegralConversionPolicy right)
    {
        return !left.Equals(right);
    }
}
