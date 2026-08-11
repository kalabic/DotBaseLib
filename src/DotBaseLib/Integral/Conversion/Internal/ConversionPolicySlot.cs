using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DotBase.Integral.Conversion.Internal;


/// <summary>
/// Sentinel encoding for <see cref="IntegralConversionPolicy"/> factory slots.
/// </summary>
internal static class ConversionPolicySlot
{
    /// <summary>Use Standard / Interleaved table for this path.</summary>
    public const nint Default = 0;

    /// <summary>Path explicitly unsupported; do not fall back to table.</summary>
    public const nint Refuse = -1;

    public static bool IsDefault(nint slot) => slot == Default;

    public static bool IsRefuse(nint slot) => slot == Refuse;

    public static bool IsFactory(nint slot) => slot != Default && slot != Refuse;

    public static T ResolveFactory<T>(nint slot)
        where T : class
    {
        Debug.Assert(IsFactory(slot));
        object? target = GCHandle.FromIntPtr(slot).Target;
        Debug.Assert(target is T);
        return (T)target!;
    }

    /// <summary>
    /// Roots <paramref name="factory"/> for process lifetime and returns its GCHandle address.
    /// </summary>
    public static nint AllocFactory(Delegate factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        return GCHandle.ToIntPtr(GCHandle.Alloc(factory, GCHandleType.Normal));
    }
}
