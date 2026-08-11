using System.Diagnostics;

namespace DotBase.Integral.Conversion;


/// <summary>
/// Layout context for interleaved <b>reader</b> conversion: gather one lane from
/// each complete multi-value input block into dense serial output.
/// Pass into <see cref="IntegralConversionHandle.Convert"/>.
/// </summary>
public sealed class InterleavedReaderContext 
    : NumericConversionContext
{
    public int InputBlockCapacity { get; }

    public int ValueIndex { get; }

    public InterleavedReaderContext(IntegralConversionHandle handle, int inputBlockCapacity, int valueIndex)
        : base(handle)
    {
        Debug.Assert(inputBlockCapacity > 1);
        Debug.Assert((uint)valueIndex < (uint)inputBlockCapacity);

        InputBlockCapacity = inputBlockCapacity;
        ValueIndex = valueIndex;
    }
}
