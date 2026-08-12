namespace DotBase.Integral.Conversion.Internal;


/// <summary>
/// Checked plane slicing for planar conversion contexts.
/// Callers must provide a positive <c>planeCapacity</c> and validate that it is
/// aligned to the applicable input/output format block capacity before slicing.
/// Parent range validation is delegated to <see cref="IntegralSpan.GetValueSpan(long, long)"/>.
/// </summary>
internal static class PlanarAccess
{
    internal static IntegralSpan SlicePlane(
        in IntegralSpan parent,
        long planeCapacity,
        int planeIndex,
        string parentParameterName)
    {
        long parentValueCount = parent.ValueCount;
        if (parentValueCount % planeCapacity != 0)
        {
            throw new ArgumentException(
                "The parent value count must be divisible by the plane capacity.",
                parentParameterName);
        }

        long valueOffset = checked((long)planeIndex * planeCapacity);
        return parent.GetValueSpan(valueOffset, planeCapacity);
    }
}
