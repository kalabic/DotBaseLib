namespace DotBase.Buffers.Integral;


/// <summary>Scalar integral operations with status-returning operational failure.</summary>
public interface IScalarRingBuffer : IByteRingBuffer
{
    /// <summary>
    /// Reads one value. Returns <see langword="false"/> and assigns
    /// <see langword="default"/> when the operation cannot complete.
    /// Waitable implementations may wait when the value can fit.
    /// </summary>
    bool Read<T>(out T value)
        where T : unmanaged;

    /// <summary>Attempts an immediate atomic scalar read.</summary>
    bool TryRead<T>(out T value)
        where T : unmanaged;

    /// <summary>
    /// Writes one value. Returns <see langword="false"/> when the operation cannot
    /// complete. Waitable implementations may wait when the value can fit.
    /// </summary>
    bool Write<T>(T value)
        where T : unmanaged;

    /// <summary>Attempts an immediate atomic scalar write.</summary>
    bool TryWrite<T>(T value)
        where T : unmanaged;
}
