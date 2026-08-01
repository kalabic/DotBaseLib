using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DotBase.Integral.Internal;


/// <summary>
/// Bare pointer bulk ops. Callers own validity of ranges.
/// </summary>
internal static unsafe class IntegralByteMemory
{
    private const nuint StackAllocationByteCount = 512;

    internal static void Move(
        byte* source,
        byte* destination,
        nuint byteCount)
    {
        if (byteCount == 0 || source == destination)
        {
            return;
        }

        Debug.Assert(source is not null);
        Debug.Assert(destination is not null);

        if (!Overlaps(source, destination, byteCount))
        {
            Buffer.MemoryCopy(
                source,
                destination,
                (ulong)byteCount,
                (ulong)byteCount);
            return;
        }

        // MemoryCopy is memcpy-class (undefined on overlap). Stage through temp.
        if (byteCount <= StackAllocationByteCount)
        {
            byte* stackTemp = stackalloc byte[(int)byteCount];
            Buffer.MemoryCopy(
                source,
                stackTemp,
                (ulong)byteCount,
                (ulong)byteCount);
            Buffer.MemoryCopy(
                stackTemp,
                destination,
                (ulong)byteCount,
                (ulong)byteCount);
            return;
        }

        byte* heapTemp = (byte*)NativeMemory.Alloc(byteCount);
        try
        {
            Buffer.MemoryCopy(
                source,
                heapTemp,
                (ulong)byteCount,
                (ulong)byteCount);
            Buffer.MemoryCopy(
                heapTemp,
                destination,
                (ulong)byteCount,
                (ulong)byteCount);
        }
        finally
        {
            NativeMemory.Free(heapTemp);
        }
    }

    internal static void Clear(
        byte* destination,
        nuint byteCount)
    {
        if (byteCount == 0)
        {
            return;
        }

        Debug.Assert(destination is not null);
        NativeMemory.Clear(destination, byteCount);
    }

    private static bool Overlaps(
        byte* source,
        byte* destination,
        nuint byteCount)
    {
        nuint sourceAddress = (nuint)source;
        nuint destinationAddress = (nuint)destination;

        return sourceAddress < destinationAddress
            ? destinationAddress - sourceAddress < byteCount
            : sourceAddress - destinationAddress < byteCount;
    }
}
