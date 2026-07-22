namespace DotBase.Buffers.Integral.Internal;


internal static class IntegralBufferGuards
{
    internal static unsafe void ValidatePointer<T>(
        T* pointer,
        int offset,
        int count,
        string pointerName)
        where T : unmanaged
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        if (pointer is null && (offset != 0 || count != 0))
        {
            throw new ArgumentNullException(pointerName);
        }
    }
}
