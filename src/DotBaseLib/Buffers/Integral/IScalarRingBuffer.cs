namespace DotBase.Buffers.Integral;


/// <summary>
/// Immediate scalar integral operations with status-returning operational failure.
/// A scalar transfer is inherently atomic, but these methods follow the partial,
/// non-waiting policy used by the other non-Exact operations.
/// </summary>
public interface IScalarRingBuffer : IByteRingBuffer
{
    /// <summary>
    /// Reads one value. Returns <see langword="false"/> and assigns
    /// <see langword="default"/> when no complete value is immediately available.
    /// </summary>
    bool Read<T>(out T value)
        where T : unmanaged;

    /// <summary>Attempts an immediate atomic scalar read.</summary>
    bool TryRead<T>(out T value)
        where T : unmanaged;

    /// <summary>
    /// Writes one value. Returns <see langword="false"/> when there is not enough
    /// space immediately available.
    /// </summary>
    bool Write<T>(T value)
        where T : unmanaged;

    /// <summary>Attempts an immediate atomic scalar write.</summary>
    bool TryWrite<T>(T value)
        where T : unmanaged;
}
