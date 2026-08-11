using System.Runtime.InteropServices;
using DotBase.Integral.Conversion.Internal;

namespace DotBase.Integral.Conversion;


/// <summary>
/// Unmanaged conversion policy attached to <see cref="IntegralFormat"/>.
/// Three path slots (span / interleaved reader / interleaved writer), each a sentinel-aware
/// <see cref="nint"/>:
/// <list type="bullet">
///   <item><c>0</c> — default (Standard / Interleaved table)</item>
///   <item><c>-1</c> — refuse (no table fallback; null handle)</item>
///   <item>other — GCHandle to the path's handle-factory delegate</item>
/// </list>
/// Numeric with/without is closed into the installed factories, not stored separately.
/// </summary>
public readonly struct IntegralConversionPolicy
    : IEquatable<IntegralConversionPolicy>
{
    /// <summary>All paths default to tables.</summary>
    public static IntegralConversionPolicy None => default;

    private readonly nint _spanHandleFactory;
    private readonly nint _readerHandleFactory;
    private readonly nint _writerHandleFactory;

    private IntegralConversionPolicy(
        nint spanHandleFactory,
        nint readerHandleFactory,
        nint writerHandleFactory)
    {
        _spanHandleFactory = spanHandleFactory;
        _readerHandleFactory = readerHandleFactory;
        _writerHandleFactory = writerHandleFactory;
    }

    internal nint SpanHandleFactorySlot => _spanHandleFactory;
    internal nint ReaderHandleFactorySlot => _readerHandleFactory;
    internal nint WriterHandleFactorySlot => _writerHandleFactory;

    public bool IsEmpty =>
        _spanHandleFactory == ConversionPolicySlot.Default
        && _readerHandleFactory == ConversionPolicySlot.Default
        && _writerHandleFactory == ConversionPolicySlot.Default;

    /// <summary>All three paths refuse conversion.</summary>
    public static IntegralConversionPolicy RefuseAll()
    {
        return new IntegralConversionPolicy(
            ConversionPolicySlot.Refuse,
            ConversionPolicySlot.Refuse,
            ConversionPolicySlot.Refuse);
    }

    /// <summary>
    /// Custom scalar converters on all three paths (closed-over handle factories).
    /// Uses cached factory handles on <paramref name="converters"/> so equal tables share slots.
    /// </summary>
    public static IntegralConversionPolicy FromValueConverters(NumericValueConverters converters)
    {
        ArgumentNullException.ThrowIfNull(converters);
        return new IntegralConversionPolicy(
            converters.GetOrCreateSpanHandleFactory(),
            converters.GetOrCreateReaderHandleFactory(),
            converters.GetOrCreateWriterHandleFactory());
    }

    /// <summary>
    /// User-owned structural bulk func for the contiguous path only.
    /// Reader/writer paths remain table defaults unless overridden.
    /// </summary>
    public static IntegralConversionPolicy FromFunc(
        IntegralSpanConversionFunc func,
        NumericValueConverters? valueConverters = null)
    {
        ArgumentNullException.ThrowIfNull(func);
        IntegralSpanConversionHandleFunc factory =
            InternalConversionDelegates.MakeSpanHandle_FromFunc(func, valueConverters);
        nint spanSlot = ConversionPolicySlot.AllocFactory(factory);
        return new IntegralConversionPolicy(
            spanSlot,
            ConversionPolicySlot.Default,
            ConversionPolicySlot.Default);
    }

    /// <summary>Explicit factories per path (<see cref="ConversionPolicySlot.Default"/> / Refuse / GCHandle).</summary>
    public static IntegralConversionPolicy Create(
        nint spanHandleFactory = ConversionPolicySlot.Default,
        nint readerHandleFactory = ConversionPolicySlot.Default,
        nint writerHandleFactory = ConversionPolicySlot.Default)
    {
        return new IntegralConversionPolicy(
            spanHandleFactory,
            readerHandleFactory,
            writerHandleFactory);
    }

    public bool Equals(IntegralConversionPolicy other)
    {
        return _spanHandleFactory == other._spanHandleFactory
            && _readerHandleFactory == other._readerHandleFactory
            && _writerHandleFactory == other._writerHandleFactory;
    }

    public override bool Equals(object? obj)
    {
        return obj is IntegralConversionPolicy other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_spanHandleFactory, _readerHandleFactory, _writerHandleFactory);
    }

    public static bool operator ==(IntegralConversionPolicy left, IntegralConversionPolicy right)
        => left.Equals(right);

    public static bool operator !=(IntegralConversionPolicy left, IntegralConversionPolicy right)
        => !left.Equals(right);
}
