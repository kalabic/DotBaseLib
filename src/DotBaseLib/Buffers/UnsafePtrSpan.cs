namespace DotBase.Buffers;


public readonly unsafe struct UnsafePtrSpan<T>
    where T : unmanaged
{
    public readonly T* OffsetPtr;

    public readonly int Count;

    internal UnsafePtrSpan(T* ptr, int offset, int count)
    {
        OffsetPtr = ptr + offset;
        Count = count;
    }
}
