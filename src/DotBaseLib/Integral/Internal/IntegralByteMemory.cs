using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DotBase.Integral.Internal;


internal static unsafe class IntegralByteMemory
{
    internal static void Copy(
        byte* source,
        byte* destination,
        nuint byteCount)
    {
        if (byteCount == 0)
        {
            return;
        }

        Debug.Assert(source is not null);
        Debug.Assert(destination is not null);
        Debug.Assert(!Overlaps(source, destination, byteCount));

        Buffer.MemoryCopy(
            source,
            destination,
            (ulong)byteCount,
            (ulong)byteCount);
    }

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

        if (destination < source)
        {
            for (nuint index = 0; index < byteCount; ++index)
            {
                destination[index] = source[index];
            }
        }
        else
        {
            while (byteCount > 0)
            {
                --byteCount;
                destination[byteCount] = source[byteCount];
            }
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
