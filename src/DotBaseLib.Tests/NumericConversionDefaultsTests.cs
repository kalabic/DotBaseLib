using DotBase.Integral;
using DotBase.Integral.Conversion.Numeric;
using DotBase.Integral.Conversion.Numeric.Defaults;
using DotBase.Integral.Internal;

namespace DotBaseLib.Tests;


/// <summary>
/// Parity: DefaultConversionsTo* scalar defaults vs IntegralNumericConversion Identity.
/// </summary>
public class NumericConversionDefaultsTests
{
    [Fact]
    public void IntegerAndFloatDefaultsMatchIntegralNumericConversion()
    {
        // UInt8 → Float
        foreach (byte v in new byte[] { 0, 1, 127, 128, 255 })
        {
            Assert.Equal(
                IntegralNumericConversion<byte, float>.Convert(v, NumericScaleBias.Identity),
                DefaultConversionsToFloat.ConvertUInt8ToFloat_Default(v));
        }

        // Int16 → Float
        foreach (short v in new short[] { short.MinValue, -1, 0, 1, short.MaxValue })
        {
            Assert.Equal(
                IntegralNumericConversion<short, float>.Convert(v, NumericScaleBias.Identity),
                DefaultConversionsToFloat.ConvertInt16ToFloat_Default(v));
        }

        // Float → UInt8 (saturation)
        foreach (float v in new float[] { float.NaN, -10f, 0f, 42.9f, 255f, 255.1f, 1000f })
        {
            Assert.Equal(
                IntegralNumericConversion<float, byte>.Convert(v, NumericScaleBias.Identity),
                DefaultConversionsToUInt8.ConvertFloatToUInt8_Default(v));
        }

        // UInt16 → Int8 (unsigned → signed clamp)
        foreach (ushort v in new ushort[] { 0, 1, 127, 128, 255, ushort.MaxValue })
        {
            Assert.Equal(
                IntegralNumericConversion<ushort, sbyte>.Convert(v, NumericScaleBias.Identity),
                DefaultConversionsToInt8.ConvertUInt16ToInt8_Default(v));
        }

        // Int32 → UInt16 (signed → unsigned)
        foreach (int v in new int[] { int.MinValue, -1, 0, 1, 65535, 65536, int.MaxValue })
        {
            Assert.Equal(
                IntegralNumericConversion<int, ushort>.Convert(v, NumericScaleBias.Identity),
                DefaultConversionsToUInt16.ConvertInt32ToUInt16_Default(v));
        }

        // Double → Float (NaN / inf / range)
        foreach (double v in new double[]
                 {
                     double.NaN,
                     double.PositiveInfinity,
                     double.NegativeInfinity,
                     0d,
                     1.5d,
                     float.MaxValue * 2d,
                     -float.MaxValue * 2d,
                 })
        {
            Assert.Equal(
                IntegralNumericConversion<double, float>.Convert(v, NumericScaleBias.Identity),
                DefaultConversionsToFloat.ConvertDoubleToFloat_Default(v));
        }

        // Float → Double (NaN / ±Inf saturators must remain)
        foreach (float v in new float[]
                 {
                     float.NaN,
                     float.PositiveInfinity,
                     float.NegativeInfinity,
                     0f,
                     -1.5f,
                     float.MaxValue,
                 })
        {
            Assert.Equal(
                IntegralNumericConversion<float, double>.Convert(v, NumericScaleBias.Identity),
                DefaultConversionsToDouble.ConvertFloatToDouble_Default(v));
        }

        // Integer → Double simple cast
        Assert.Equal(
            IntegralNumericConversion<ulong, double>.Convert(ulong.MaxValue, NumericScaleBias.Identity),
            DefaultConversionsToDouble.ConvertUInt64ToDouble_Default(ulong.MaxValue));

        // Identity same-type
        Assert.Equal(42, DefaultConversionsToInt32.ConvertInt32ToInt32_Default(42));
        Assert.Equal(3.25f, DefaultConversionsToFloat.ConvertFloatToFloat_Default(3.25f));
    }
}
