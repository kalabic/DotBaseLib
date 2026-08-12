namespace DotBase.Integral.Conversion.Internal.Interleaved;


/// <summary>
/// Atomically published custom/default delegate-handle pair for one interleaved
/// conversion table slot. Published instances are retained for process lifetime.
/// </summary>
internal sealed class InterleavedConversionDelegates
{
    internal nint Custom { get; }

    internal nint Default { get; }

    internal InterleavedConversionDelegates(
        IntegralSpanConversionFunc custom,
        IntegralSpanConversionFunc @default)
    {
        Custom = DelegateHandle.Allocate(custom);
        Default = DelegateHandle.Allocate(@default);
    }
}
