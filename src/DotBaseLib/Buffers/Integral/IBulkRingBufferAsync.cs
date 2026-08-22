namespace DotBase.Buffers.Integral;

/// <summary>
/// Generic bulk operations. Unlocked and locked implementations transfer as many
/// complete values as immediately fit. Waitable implementations require the whole
/// request, waiting only when that request can fit. Operational failure returns <c>0</c>.
/// </summary>
public interface IBulkRingBufferAsync
{
    ValueTask<LongResult> ReadAsync<T>(T[] destination, int offset, int count)
        where T : unmanaged;

    ValueTask<LongResult> WriteAsync<T>(T[] source, int offset, int count)
        where T : unmanaged;
}
