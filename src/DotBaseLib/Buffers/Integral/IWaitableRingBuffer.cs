namespace DotBase.Buffers.Integral;


/// <summary>Identifies an integral ring buffer whose non-Try read operations wait until the complete request is available or the ring is closed.</summary>
public interface IWaitableRingBuffer : IIntegralRingBuffer
{
}
