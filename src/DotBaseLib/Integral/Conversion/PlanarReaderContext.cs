using DotBase.Integral.Conversion.Internal;

namespace DotBase.Integral.Conversion;


/// <summary>
/// Layout context for planar <b>reader</b> conversion: convert one complete plane
/// from a multi-plane input buffer into dense serial output.
/// <para>
/// Span value capacity must be a multiple of <see cref="PlaneCapacity"/>.
/// The input plane must contain complete input-format blocks. Only complete
/// planes (channels) can be converted.
/// Call <see cref="Convert"/> on this context so the selected plane is sliced
/// before the standard conversion kernel runs.
/// </para>
/// </summary>
public sealed class PlanarReaderContext
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
    public int PlaneIndex { get; }

    public PlanarReaderContext(
        IntegralConversionHandle handle,
        long planeCapacity,
        int blockCapacity,
        int planeIndex)
        : base(handle)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(planeCapacity, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(blockCapacity, 0);
        ArgumentOutOfRangeException.ThrowIfNegative(planeIndex);
        if (planeCapacity % blockCapacity != 0)
        {
            throw new ArgumentException(
                "The plane capacity must be divisible by the declared block capacity.",
                nameof(planeCapacity));
        }

        PlaneCapacity = planeCapacity;
        BlockCapacity = blockCapacity;
        PlaneIndex = planeIndex;
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

        IntegralSpan inputPlane = PlanarAccess.SlicePlane(
            input,
            PlaneCapacity,
            PlaneIndex,
            nameof(input));
        return base.Convert(inputPlane, output, count);
    }
}
