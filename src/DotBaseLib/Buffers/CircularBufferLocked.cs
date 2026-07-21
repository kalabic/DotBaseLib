namespace DotBase.Buffers;


public class CircularBufferLocked : CircularBufferUnlocked
{
    private readonly object _lock = new object();

    public CircularBufferLocked(int size)
        : base(size)
    {
    }

    //
    // Group: Write
    //

    public override int Write(byte[] data, int offset, int length)
    {
        lock (_lock)
        {
            return base.Write(data, offset, length);
        }
    }

    //
    // Group: Read, Advance, ClearBuffer
    //

    public override int Read(byte[] data, int offset, int length)
    {
        lock (_lock)
        {
            return base.Read(data, offset, length);
        }
    }

    public override void Advance(int count)
    {
        lock (_lock)
        {
            base.Advance(count);
        }
    }

    public override void ClearBuffer()
    {
        lock (_lock)
        {
            base.ClearBuffer();
        }
    }
}
