using DotBase.Integral.Conversion.Internal;

namespace DotBase.Integral.Conversion;


/// <summary>
/// Layout context for planar <b>transfer</b> conversion: convert one complete
/// input plane into one complete output plane of multi-plane buffers.
/// <para>
/// Span value capacity must be a multiple of <see cref="PlaneCapacity"/>.
/// Each input and output plane must contain complete blocks for its respective
/// format. Only complete planes (channels) can be converted.
/// Call <see cref="Convert"/> on this context so the selected planes are sliced
/// before the standard conversion kernel runs.
/// </para>
/// </summary>
public sealed class PlanarTransferContext
    : NumericConversionContext
{
    /// <summary>
    /// Number of scalar values in one plane. Must be positive.
    /// </summary>
    public long PlaneCapacity { get; }

    /// <summary>
    /// Informational block framing within a plane; conversion uses
    /// <see cref="PlaneCapacity"/>.
    /// </summary>
    public int BlockCapacity { get; }

    /// <summary>
    /// Index of the plane to read from the input <see cref="IntegralSpan"/>.
    /// </summary>
    public int InputPlaneIndex { get; }

    /// <summary>
    /// Index of the plane to write in the output <see cref="IntegralSpan"/>.
    /// </summary>
    public int OutputPlaneIndex { get; }

    public PlanarTransferContext(
        IntegralConversionHandle handle,
        long planeCapacity,
        int blockCapacity,
        int inputPlaneIndex,
        int outputPlaneIndex)
        : base(handle)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(planeCapacity, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(blockCapacity, 0);
        ArgumentOutOfRangeException.ThrowIfNegative(inputPlaneIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(outputPlaneIndex);
        if (planeCapacity % blockCapacity != 0)
        {
            throw new ArgumentException(
                "The plane capacity must be divisible by the declared block capacity.",
                nameof(planeCapacity));
        }

        PlaneCapacity = planeCapacity;
        BlockCapacity = blockCapacity;
        InputPlaneIndex = inputPlaneIndex;
        OutputPlaneIndex = outputPlaneIndex;
    }

    public override long Convert(
        in IntegralSpan input,
        in IntegralSpan output,
        long count)
    {
        if (input.Format.BlockCapacity <= 0 ||
            PlaneCapacity % input.Format.BlockCapacity != 0)
        {
            throw new ArgumentException(
                "The plane capacity must contain only complete input-format blocks.",
                nameof(input));
        }

        if (output.Format.BlockCapacity <= 0 ||
            PlaneCapacity % output.Format.BlockCapacity != 0)
        {
            throw new ArgumentException(
                "The plane capacity must contain only complete output-format blocks.",
                nameof(output));
        }

        IntegralSpan inputPlane = PlanarAccess.SlicePlane(
            input,
            PlaneCapacity,
            InputPlaneIndex,
            nameof(input));
        IntegralSpan outputPlane = PlanarAccess.SlicePlane(
            output,
            PlaneCapacity,
            OutputPlaneIndex,
            nameof(output));
        return base.Convert(inputPlane, outputPlane, count);
    }
}
