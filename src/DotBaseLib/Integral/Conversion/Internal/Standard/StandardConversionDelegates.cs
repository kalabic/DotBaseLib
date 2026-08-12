namespace DotBase.Integral.Conversion.Internal.Standard;


/// <summary>
/// Atomically published custom/default delegate-handle pair for one standard
/// conversion table slot. Published instances are retained for process lifetime.
/// </summary>
internal sealed class StandardConversionDelegates
{
    internal nint Custom { get; }

    internal nint Default { get; }

    internal StandardConversionDelegates(
        IntegralSpanConversionFunc custom,
        IntegralSpanConversionFunc @default)
    {
        Custom = DelegateHandle.Allocate(custom);
        Default = DelegateHandle.Allocate(@default);
    }
}
