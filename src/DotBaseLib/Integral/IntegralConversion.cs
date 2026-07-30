namespace DotBase.Integral;


public readonly struct IntegralConversion
{
    public static readonly IntegralConversion Identity = new(1);

    public double Scale { get; }

    public double Bias { get; }

    internal bool IsIdentity => Scale == 1 && Bias == 0;

    public IntegralConversion(
        double scale,
        double bias = 0)
    {
        if (!double.IsFinite(scale))
        {
            throw new ArgumentOutOfRangeException(
                nameof(scale),
                scale,
                "Scale must be finite.");
        }

        if (!double.IsFinite(bias))
        {
            throw new ArgumentOutOfRangeException(
                nameof(bias),
                bias,
                "Bias must be finite.");
        }

        Scale = scale;
        Bias = bias;
    }
}
