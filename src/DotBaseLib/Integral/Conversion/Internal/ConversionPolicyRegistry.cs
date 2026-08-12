using System.Runtime.CompilerServices;

namespace DotBase.Integral.Conversion.Internal;


internal enum ConversionPolicyKind
{
    ValueConverters,
    StaticSpanFunction,
}


/// <summary>
/// Immutable managed policy payload retained by <see cref="ConversionPolicyRegistry"/>
/// for process lifetime.
/// </summary>
internal sealed class ConversionPolicyEntry
{
    private nint _spanFunctionHandle;

    internal ConversionPolicyKind Kind { get; }

    internal NumericValueConverters? ValueConverters { get; }

    internal IntegralSpanConversionFunc? SpanFunction { get; }

    internal nint SpanFunctionHandle
    {
        get
        {
            IntegralSpanConversionFunc? function = SpanFunction;
            return function is null
                ? 0
                : DelegateHandle.GetOrAllocate(
                    ref _spanFunctionHandle,
                    function);
        }
    }

    internal ConversionPolicyEntry(
        ConversionPolicyKind kind,
        NumericValueConverters? valueConverters,
        IntegralSpanConversionFunc? spanFunction)
    {
        Kind = kind;
        ValueConverters = valueConverters;
        SpanFunction = spanFunction;
    }
}


/// <summary>
/// Append-only process-lifetime registry for managed conversion policy state.
/// Positive indexes address immutable entries; zero and minus one are reserved
/// for built-in and refuse-all behavior.
/// </summary>
internal static class ConversionPolicyRegistry
{
    internal const int Default = 0;
    internal const int Refuse = -1;

    private static readonly object Sync = new();

    private static ConversionPolicyEntry[] _entries = [];

    // Accessed only while holding Sync. Readers resolve through _entries.
    private static Dictionary<PolicyKey, int> _indexes = new();

    internal static int RegisterValueConverters(NumericValueConverters converters)
    {
        ArgumentNullException.ThrowIfNull(converters);
        PolicyKey key = PolicyKey.ForValueConverters(converters);
        return Register(
            key,
            new ConversionPolicyEntry(
                ConversionPolicyKind.ValueConverters,
                converters,
                spanFunction: null));
    }

    internal static int RegisterStaticSpanFunction(
        IntegralSpanConversionFunc function,
        NumericValueConverters? converters)
    {
        ArgumentNullException.ThrowIfNull(function);
        PolicyKey key = PolicyKey.ForStaticSpanFunction(function, converters);
        return Register(
            key,
            new ConversionPolicyEntry(
                ConversionPolicyKind.StaticSpanFunction,
                converters,
                function));
    }

    internal static ConversionPolicyEntry Resolve(int registryIndex)
    {
        if (registryIndex <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(registryIndex));
        }

        ConversionPolicyEntry[] entries = Volatile.Read(ref _entries);
        int entryIndex = registryIndex - 1;
        if ((uint)entryIndex >= (uint)entries.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(registryIndex));
        }

        return entries[entryIndex];
    }

    private static int Register(PolicyKey key, ConversionPolicyEntry entry)
    {
        lock (Sync)
        {
            if (_indexes.TryGetValue(key, out int existingIndex))
            {
                return existingIndex;
            }

            ConversionPolicyEntry[] currentEntries = _entries;
            int registryIndex = checked(currentEntries.Length + 1);

            // Build both replacements privately. If any allocation or insertion
            // fails, the published registry remains unchanged.
            ConversionPolicyEntry[] nextEntries =
                new ConversionPolicyEntry[registryIndex];
            Array.Copy(currentEntries, nextEntries, currentEntries.Length);
            nextEntries[^1] = entry;

            Dictionary<PolicyKey, int> nextIndexes = new(_indexes);
            nextIndexes.Add(key, registryIndex);

            _indexes = nextIndexes;
            Volatile.Write(ref _entries, nextEntries);
            return registryIndex;
        }
    }

    private readonly struct PolicyKey : IEquatable<PolicyKey>
    {
        private readonly ConversionPolicyKind _kind;
        private readonly IntegralSpanConversionFunc? _spanFunction;
        private readonly NumericValueConverters? _valueConverters;

        private PolicyKey(
            ConversionPolicyKind kind,
            IntegralSpanConversionFunc? spanFunction,
            NumericValueConverters? valueConverters)
        {
            _kind = kind;
            _spanFunction = spanFunction;
            _valueConverters = valueConverters;
        }

        internal static PolicyKey ForValueConverters(
            NumericValueConverters converters)
        {
            return new PolicyKey(
                ConversionPolicyKind.ValueConverters,
                spanFunction: null,
                converters);
        }

        internal static PolicyKey ForStaticSpanFunction(
            IntegralSpanConversionFunc function,
            NumericValueConverters? converters)
        {
            return new PolicyKey(
                ConversionPolicyKind.StaticSpanFunction,
                function,
                converters);
        }

        public bool Equals(PolicyKey other)
        {
            return _kind == other._kind
                && Equals(_spanFunction, other._spanFunction)
                && ReferenceEquals(_valueConverters, other._valueConverters);
        }

        public override bool Equals(object? obj)
        {
            return obj is PolicyKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            int convertersHash = _valueConverters is null
                ? 0
                : RuntimeHelpers.GetHashCode(_valueConverters);
            return HashCode.Combine(
                (int)_kind,
                _spanFunction?.GetHashCode() ?? 0,
                convertersHash);
        }
    }
}
