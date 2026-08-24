namespace DotBase.Buffers.Integral;

public readonly struct BufferWritingCompleted
{
    public readonly long TotalWritten;

    public BufferWritingCompleted(long totalWritten)
    {
        TotalWritten = totalWritten;
    }
}

public readonly struct BufferReadingCompleted
{
    public readonly long TotalRead;

    public BufferReadingCompleted(long totalRead)
    {
        TotalRead = totalRead;
    }
}
