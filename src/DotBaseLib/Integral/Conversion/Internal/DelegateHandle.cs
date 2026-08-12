using System.Runtime.InteropServices;

namespace DotBase.Integral.Conversion.Internal;


/// <summary>
/// Allocates and resolves opaque process-lifetime handles for managed delegates.
/// The owner of a published token must remain process-lifetime and must never free
/// the winning handle while an <see cref="IntegralConversionHandle"/> can reference it.
/// </summary>
internal static class DelegateHandle
{
    internal static nint Allocate(Delegate? target)
    {
        return target is null
            ? 0
            : GCHandle.ToIntPtr(GCHandle.Alloc(target, GCHandleType.Normal));
    }

    internal static nint GetOrAllocate(ref nint location, Delegate target)
    {
        ArgumentNullException.ThrowIfNull(target);

        nint existing = Volatile.Read(ref location);
        if (existing != 0)
        {
            return existing;
        }

        GCHandle handle = GCHandle.Alloc(target, GCHandleType.Normal);
        nint created = GCHandle.ToIntPtr(handle);
        nint winner = Interlocked.CompareExchange(
            ref location,
            created,
            comparand: 0);

        if (winner == 0)
        {
            return created;
        }

        handle.Free();
        return winner;
    }

    internal static TDelegate? Resolve<TDelegate>(nint token)
        where TDelegate : Delegate
    {
        return token == 0
            ? null
            : GCHandle.FromIntPtr(token).Target as TDelegate;
    }
}
