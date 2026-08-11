using System.Diagnostics;

namespace DotBase.Integral.Conversion;


/// <summary>
/// Layout context for interleaved <b>writer</b> conversion: scatter dense serial
/// input into one lane of each complete multi-value output block.
/// Pass into <see cref="IntegralConversionHandle.Convert"/>.
/// </summary>
public sealed class InterleavedWriterContext
    : NumericConversionContext
{
    public int OutputBlockCapacity { get; }

    public int ValueIndex { get; }

    public InterleavedWriterContext(IntegralConversionHandle handle, int outputBlockCapacity, int valueIndex)
        : base(handle)
    {
        Debug.Assert(outputBlockCapacity > 1);
        Debug.Assert((uint)valueIndex < (uint)outputBlockCapacity);

        OutputBlockCapacity = outputBlockCapacity;
        ValueIndex = valueIndex;
    }
}
