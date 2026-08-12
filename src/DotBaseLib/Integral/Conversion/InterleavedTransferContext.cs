using System.Diagnostics;

namespace DotBase.Integral.Conversion;


/// <summary>
/// Layout context for interleaved <b>transfer</b> conversion: gather one lane from
/// each complete multi-value input block and scatter it into one lane of each
/// complete multi-value output block.
/// Call <see cref="ConversionContext.Convert"/> on this context.
/// </summary>
public sealed class InterleavedTransferContext
    : NumericConversionContext
{
    public int InputBlockCapacity { get; }

    public int InputValueIndex { get; }

    public int OutputBlockCapacity { get; }

    public int OutputValueIndex { get; }

    public InterleavedTransferContext(
        IntegralConversionHandle handle,
        int inputBlockCapacity, int inputValueIndex,
        int outputBlockCapacity, int outputValueIndex)
        : base(handle)
    {
        Debug.Assert(inputBlockCapacity > 1);
        Debug.Assert((uint)inputValueIndex < (uint)inputBlockCapacity);
        Debug.Assert(outputBlockCapacity > 1);
        Debug.Assert((uint)outputValueIndex < (uint)outputBlockCapacity);

        InputBlockCapacity = inputBlockCapacity;
        InputValueIndex = inputValueIndex;
        OutputBlockCapacity = outputBlockCapacity;
        OutputValueIndex = outputValueIndex;
    }
}
