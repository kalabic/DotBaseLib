using DotBase.Integral.Conversion.Numeric;

namespace DotBase.Integral.Conversion.Numeric.Defaults;


/// <summary>
/// Registers default scalar converters into a <see cref="INumericValueDelegateTable"/>.
/// Each slot uses the concrete <c>Convert{Src}To{Dst}_Delegate</c> type so structural
/// kernels can cast the general <see cref="System.Delegate"/> successfully.
/// </summary>
internal static class DefaultNumericValueConvertersReg
{
    internal static void AddToTable(INumericValueDelegateTable table)
    {
        // → UInt8
        {
            ConvertUInt8ToUInt8_Delegate d = DefaultConversionsToUInt8.ConvertUInt8ToUInt8_Default;
            table.SetConverter(d, IntegralType.UInt8, IntegralType.UInt8);
        }
        {
            ConvertInt8ToUInt8_Delegate d = DefaultConversionsToUInt8.ConvertInt8ToUInt8_Default;
            table.SetConverter(d, IntegralType.Int8, IntegralType.UInt8);
        }
        {
            ConvertUInt16ToUInt8_Delegate d = DefaultConversionsToUInt8.ConvertUInt16ToUInt8_Default;
            table.SetConverter(d, IntegralType.UInt16, IntegralType.UInt8);
        }
        {
            ConvertInt16ToUInt8_Delegate d = DefaultConversionsToUInt8.ConvertInt16ToUInt8_Default;
            table.SetConverter(d, IntegralType.Int16, IntegralType.UInt8);
        }
        {
            ConvertUInt32ToUInt8_Delegate d = DefaultConversionsToUInt8.ConvertUInt32ToUInt8_Default;
            table.SetConverter(d, IntegralType.UInt32, IntegralType.UInt8);
        }
        {
            ConvertInt32ToUInt8_Delegate d = DefaultConversionsToUInt8.ConvertInt32ToUInt8_Default;
            table.SetConverter(d, IntegralType.Int32, IntegralType.UInt8);
        }
        {
            ConvertUInt64ToUInt8_Delegate d = DefaultConversionsToUInt8.ConvertUInt64ToUInt8_Default;
            table.SetConverter(d, IntegralType.UInt64, IntegralType.UInt8);
        }
        {
            ConvertInt64ToUInt8_Delegate d = DefaultConversionsToUInt8.ConvertInt64ToUInt8_Default;
            table.SetConverter(d, IntegralType.Int64, IntegralType.UInt8);
        }
        {
            ConvertFloatToUInt8_Delegate d = DefaultConversionsToUInt8.ConvertFloatToUInt8_Default;
            table.SetConverter(d, IntegralType.Float, IntegralType.UInt8);
        }
        {
            ConvertDoubleToUInt8_Delegate d = DefaultConversionsToUInt8.ConvertDoubleToUInt8_Default;
            table.SetConverter(d, IntegralType.Double, IntegralType.UInt8);
        }

        // → Int8
        {
            ConvertUInt8ToInt8_Delegate d = DefaultConversionsToInt8.ConvertUInt8ToInt8_Default;
            table.SetConverter(d, IntegralType.UInt8, IntegralType.Int8);
        }
        {
            ConvertInt8ToInt8_Delegate d = DefaultConversionsToInt8.ConvertInt8ToInt8_Default;
            table.SetConverter(d, IntegralType.Int8, IntegralType.Int8);
        }
        {
            ConvertUInt16ToInt8_Delegate d = DefaultConversionsToInt8.ConvertUInt16ToInt8_Default;
            table.SetConverter(d, IntegralType.UInt16, IntegralType.Int8);
        }
        {
            ConvertInt16ToInt8_Delegate d = DefaultConversionsToInt8.ConvertInt16ToInt8_Default;
            table.SetConverter(d, IntegralType.Int16, IntegralType.Int8);
        }
        {
            ConvertUInt32ToInt8_Delegate d = DefaultConversionsToInt8.ConvertUInt32ToInt8_Default;
            table.SetConverter(d, IntegralType.UInt32, IntegralType.Int8);
        }
        {
            ConvertInt32ToInt8_Delegate d = DefaultConversionsToInt8.ConvertInt32ToInt8_Default;
            table.SetConverter(d, IntegralType.Int32, IntegralType.Int8);
        }
        {
            ConvertUInt64ToInt8_Delegate d = DefaultConversionsToInt8.ConvertUInt64ToInt8_Default;
            table.SetConverter(d, IntegralType.UInt64, IntegralType.Int8);
        }
        {
            ConvertInt64ToInt8_Delegate d = DefaultConversionsToInt8.ConvertInt64ToInt8_Default;
            table.SetConverter(d, IntegralType.Int64, IntegralType.Int8);
        }
        {
            ConvertFloatToInt8_Delegate d = DefaultConversionsToInt8.ConvertFloatToInt8_Default;
            table.SetConverter(d, IntegralType.Float, IntegralType.Int8);
        }
        {
            ConvertDoubleToInt8_Delegate d = DefaultConversionsToInt8.ConvertDoubleToInt8_Default;
            table.SetConverter(d, IntegralType.Double, IntegralType.Int8);
        }

        // → UInt16
        {
            ConvertUInt8ToUInt16_Delegate d = DefaultConversionsToUInt16.ConvertUInt8ToUInt16_Default;
            table.SetConverter(d, IntegralType.UInt8, IntegralType.UInt16);
        }
        {
            ConvertInt8ToUInt16_Delegate d = DefaultConversionsToUInt16.ConvertInt8ToUInt16_Default;
            table.SetConverter(d, IntegralType.Int8, IntegralType.UInt16);
        }
        {
            ConvertUInt16ToUInt16_Delegate d = DefaultConversionsToUInt16.ConvertUInt16ToUInt16_Default;
            table.SetConverter(d, IntegralType.UInt16, IntegralType.UInt16);
        }
        {
            ConvertInt16ToUInt16_Delegate d = DefaultConversionsToUInt16.ConvertInt16ToUInt16_Default;
            table.SetConverter(d, IntegralType.Int16, IntegralType.UInt16);
        }
        {
            ConvertUInt32ToUInt16_Delegate d = DefaultConversionsToUInt16.ConvertUInt32ToUInt16_Default;
            table.SetConverter(d, IntegralType.UInt32, IntegralType.UInt16);
        }
        {
            ConvertInt32ToUInt16_Delegate d = DefaultConversionsToUInt16.ConvertInt32ToUInt16_Default;
            table.SetConverter(d, IntegralType.Int32, IntegralType.UInt16);
        }
        {
            ConvertUInt64ToUInt16_Delegate d = DefaultConversionsToUInt16.ConvertUInt64ToUInt16_Default;
            table.SetConverter(d, IntegralType.UInt64, IntegralType.UInt16);
        }
        {
            ConvertInt64ToUInt16_Delegate d = DefaultConversionsToUInt16.ConvertInt64ToUInt16_Default;
            table.SetConverter(d, IntegralType.Int64, IntegralType.UInt16);
        }
        {
            ConvertFloatToUInt16_Delegate d = DefaultConversionsToUInt16.ConvertFloatToUInt16_Default;
            table.SetConverter(d, IntegralType.Float, IntegralType.UInt16);
        }
        {
            ConvertDoubleToUInt16_Delegate d = DefaultConversionsToUInt16.ConvertDoubleToUInt16_Default;
            table.SetConverter(d, IntegralType.Double, IntegralType.UInt16);
        }

        // → Int16
        {
            ConvertUInt8ToInt16_Delegate d = DefaultConversionsToInt16.ConvertUInt8ToInt16_Default;
            table.SetConverter(d, IntegralType.UInt8, IntegralType.Int16);
        }
        {
            ConvertInt8ToInt16_Delegate d = DefaultConversionsToInt16.ConvertInt8ToInt16_Default;
            table.SetConverter(d, IntegralType.Int8, IntegralType.Int16);
        }
        {
            ConvertUInt16ToInt16_Delegate d = DefaultConversionsToInt16.ConvertUInt16ToInt16_Default;
            table.SetConverter(d, IntegralType.UInt16, IntegralType.Int16);
        }
        {
            ConvertInt16ToInt16_Delegate d = DefaultConversionsToInt16.ConvertInt16ToInt16_Default;
            table.SetConverter(d, IntegralType.Int16, IntegralType.Int16);
        }
        {
            ConvertUInt32ToInt16_Delegate d = DefaultConversionsToInt16.ConvertUInt32ToInt16_Default;
            table.SetConverter(d, IntegralType.UInt32, IntegralType.Int16);
        }
        {
            ConvertInt32ToInt16_Delegate d = DefaultConversionsToInt16.ConvertInt32ToInt16_Default;
            table.SetConverter(d, IntegralType.Int32, IntegralType.Int16);
        }
        {
            ConvertUInt64ToInt16_Delegate d = DefaultConversionsToInt16.ConvertUInt64ToInt16_Default;
            table.SetConverter(d, IntegralType.UInt64, IntegralType.Int16);
        }
        {
            ConvertInt64ToInt16_Delegate d = DefaultConversionsToInt16.ConvertInt64ToInt16_Default;
            table.SetConverter(d, IntegralType.Int64, IntegralType.Int16);
        }
        {
            ConvertFloatToInt16_Delegate d = DefaultConversionsToInt16.ConvertFloatToInt16_Default;
            table.SetConverter(d, IntegralType.Float, IntegralType.Int16);
        }
        {
            ConvertDoubleToInt16_Delegate d = DefaultConversionsToInt16.ConvertDoubleToInt16_Default;
            table.SetConverter(d, IntegralType.Double, IntegralType.Int16);
        }

        // → UInt32
        {
            ConvertUInt8ToUInt32_Delegate d = DefaultConversionsToUInt32.ConvertUInt8ToUInt32_Default;
            table.SetConverter(d, IntegralType.UInt8, IntegralType.UInt32);
        }
        {
            ConvertInt8ToUInt32_Delegate d = DefaultConversionsToUInt32.ConvertInt8ToUInt32_Default;
            table.SetConverter(d, IntegralType.Int8, IntegralType.UInt32);
        }
        {
            ConvertUInt16ToUInt32_Delegate d = DefaultConversionsToUInt32.ConvertUInt16ToUInt32_Default;
            table.SetConverter(d, IntegralType.UInt16, IntegralType.UInt32);
        }
        {
            ConvertInt16ToUInt32_Delegate d = DefaultConversionsToUInt32.ConvertInt16ToUInt32_Default;
            table.SetConverter(d, IntegralType.Int16, IntegralType.UInt32);
        }
        {
            ConvertUInt32ToUInt32_Delegate d = DefaultConversionsToUInt32.ConvertUInt32ToUInt32_Default;
            table.SetConverter(d, IntegralType.UInt32, IntegralType.UInt32);
        }
        {
            ConvertInt32ToUInt32_Delegate d = DefaultConversionsToUInt32.ConvertInt32ToUInt32_Default;
            table.SetConverter(d, IntegralType.Int32, IntegralType.UInt32);
        }
        {
            ConvertUInt64ToUInt32_Delegate d = DefaultConversionsToUInt32.ConvertUInt64ToUInt32_Default;
            table.SetConverter(d, IntegralType.UInt64, IntegralType.UInt32);
        }
        {
            ConvertInt64ToUInt32_Delegate d = DefaultConversionsToUInt32.ConvertInt64ToUInt32_Default;
            table.SetConverter(d, IntegralType.Int64, IntegralType.UInt32);
        }
        {
            ConvertFloatToUInt32_Delegate d = DefaultConversionsToUInt32.ConvertFloatToUInt32_Default;
            table.SetConverter(d, IntegralType.Float, IntegralType.UInt32);
        }
        {
            ConvertDoubleToUInt32_Delegate d = DefaultConversionsToUInt32.ConvertDoubleToUInt32_Default;
            table.SetConverter(d, IntegralType.Double, IntegralType.UInt32);
        }

        // → Int32
        {
            ConvertUInt8ToInt32_Delegate d = DefaultConversionsToInt32.ConvertUInt8ToInt32_Default;
            table.SetConverter(d, IntegralType.UInt8, IntegralType.Int32);
        }
        {
            ConvertInt8ToInt32_Delegate d = DefaultConversionsToInt32.ConvertInt8ToInt32_Default;
            table.SetConverter(d, IntegralType.Int8, IntegralType.Int32);
        }
        {
            ConvertUInt16ToInt32_Delegate d = DefaultConversionsToInt32.ConvertUInt16ToInt32_Default;
            table.SetConverter(d, IntegralType.UInt16, IntegralType.Int32);
        }
        {
            ConvertInt16ToInt32_Delegate d = DefaultConversionsToInt32.ConvertInt16ToInt32_Default;
            table.SetConverter(d, IntegralType.Int16, IntegralType.Int32);
        }
        {
            ConvertUInt32ToInt32_Delegate d = DefaultConversionsToInt32.ConvertUInt32ToInt32_Default;
            table.SetConverter(d, IntegralType.UInt32, IntegralType.Int32);
        }
        {
            ConvertInt32ToInt32_Delegate d = DefaultConversionsToInt32.ConvertInt32ToInt32_Default;
            table.SetConverter(d, IntegralType.Int32, IntegralType.Int32);
        }
        {
            ConvertUInt64ToInt32_Delegate d = DefaultConversionsToInt32.ConvertUInt64ToInt32_Default;
            table.SetConverter(d, IntegralType.UInt64, IntegralType.Int32);
        }
        {
            ConvertInt64ToInt32_Delegate d = DefaultConversionsToInt32.ConvertInt64ToInt32_Default;
            table.SetConverter(d, IntegralType.Int64, IntegralType.Int32);
        }
        {
            ConvertFloatToInt32_Delegate d = DefaultConversionsToInt32.ConvertFloatToInt32_Default;
            table.SetConverter(d, IntegralType.Float, IntegralType.Int32);
        }
        {
            ConvertDoubleToInt32_Delegate d = DefaultConversionsToInt32.ConvertDoubleToInt32_Default;
            table.SetConverter(d, IntegralType.Double, IntegralType.Int32);
        }

        // → UInt64
        {
            ConvertUInt8ToUInt64_Delegate d = DefaultConversionsToUInt64.ConvertUInt8ToUInt64_Default;
            table.SetConverter(d, IntegralType.UInt8, IntegralType.UInt64);
        }
        {
            ConvertInt8ToUInt64_Delegate d = DefaultConversionsToUInt64.ConvertInt8ToUInt64_Default;
            table.SetConverter(d, IntegralType.Int8, IntegralType.UInt64);
        }
        {
            ConvertUInt16ToUInt64_Delegate d = DefaultConversionsToUInt64.ConvertUInt16ToUInt64_Default;
            table.SetConverter(d, IntegralType.UInt16, IntegralType.UInt64);
        }
        {
            ConvertInt16ToUInt64_Delegate d = DefaultConversionsToUInt64.ConvertInt16ToUInt64_Default;
            table.SetConverter(d, IntegralType.Int16, IntegralType.UInt64);
        }
        {
            ConvertUInt32ToUInt64_Delegate d = DefaultConversionsToUInt64.ConvertUInt32ToUInt64_Default;
            table.SetConverter(d, IntegralType.UInt32, IntegralType.UInt64);
        }
        {
            ConvertInt32ToUInt64_Delegate d = DefaultConversionsToUInt64.ConvertInt32ToUInt64_Default;
            table.SetConverter(d, IntegralType.Int32, IntegralType.UInt64);
        }
        {
            ConvertUInt64ToUInt64_Delegate d = DefaultConversionsToUInt64.ConvertUInt64ToUInt64_Default;
            table.SetConverter(d, IntegralType.UInt64, IntegralType.UInt64);
        }
        {
            ConvertInt64ToUInt64_Delegate d = DefaultConversionsToUInt64.ConvertInt64ToUInt64_Default;
            table.SetConverter(d, IntegralType.Int64, IntegralType.UInt64);
        }
        {
            ConvertFloatToUInt64_Delegate d = DefaultConversionsToUInt64.ConvertFloatToUInt64_Default;
            table.SetConverter(d, IntegralType.Float, IntegralType.UInt64);
        }
        {
            ConvertDoubleToUInt64_Delegate d = DefaultConversionsToUInt64.ConvertDoubleToUInt64_Default;
            table.SetConverter(d, IntegralType.Double, IntegralType.UInt64);
        }

        // → Int64
        {
            ConvertUInt8ToInt64_Delegate d = DefaultConversionsToInt64.ConvertUInt8ToInt64_Default;
            table.SetConverter(d, IntegralType.UInt8, IntegralType.Int64);
        }
        {
            ConvertInt8ToInt64_Delegate d = DefaultConversionsToInt64.ConvertInt8ToInt64_Default;
            table.SetConverter(d, IntegralType.Int8, IntegralType.Int64);
        }
        {
            ConvertUInt16ToInt64_Delegate d = DefaultConversionsToInt64.ConvertUInt16ToInt64_Default;
            table.SetConverter(d, IntegralType.UInt16, IntegralType.Int64);
        }
        {
            ConvertInt16ToInt64_Delegate d = DefaultConversionsToInt64.ConvertInt16ToInt64_Default;
            table.SetConverter(d, IntegralType.Int16, IntegralType.Int64);
        }
        {
            ConvertUInt32ToInt64_Delegate d = DefaultConversionsToInt64.ConvertUInt32ToInt64_Default;
            table.SetConverter(d, IntegralType.UInt32, IntegralType.Int64);
        }
        {
            ConvertInt32ToInt64_Delegate d = DefaultConversionsToInt64.ConvertInt32ToInt64_Default;
            table.SetConverter(d, IntegralType.Int32, IntegralType.Int64);
        }
        {
            ConvertUInt64ToInt64_Delegate d = DefaultConversionsToInt64.ConvertUInt64ToInt64_Default;
            table.SetConverter(d, IntegralType.UInt64, IntegralType.Int64);
        }
        {
            ConvertInt64ToInt64_Delegate d = DefaultConversionsToInt64.ConvertInt64ToInt64_Default;
            table.SetConverter(d, IntegralType.Int64, IntegralType.Int64);
        }
        {
            ConvertFloatToInt64_Delegate d = DefaultConversionsToInt64.ConvertFloatToInt64_Default;
            table.SetConverter(d, IntegralType.Float, IntegralType.Int64);
        }
        {
            ConvertDoubleToInt64_Delegate d = DefaultConversionsToInt64.ConvertDoubleToInt64_Default;
            table.SetConverter(d, IntegralType.Double, IntegralType.Int64);
        }

        // → Float
        {
            ConvertUInt8ToFloat_Delegate d = DefaultConversionsToFloat.ConvertUInt8ToFloat_Default;
            table.SetConverter(d, IntegralType.UInt8, IntegralType.Float);
        }
        {
            ConvertInt8ToFloat_Delegate d = DefaultConversionsToFloat.ConvertInt8ToFloat_Default;
            table.SetConverter(d, IntegralType.Int8, IntegralType.Float);
        }
        {
            ConvertUInt16ToFloat_Delegate d = DefaultConversionsToFloat.ConvertUInt16ToFloat_Default;
            table.SetConverter(d, IntegralType.UInt16, IntegralType.Float);
        }
        {
            ConvertInt16ToFloat_Delegate d = DefaultConversionsToFloat.ConvertInt16ToFloat_Default;
            table.SetConverter(d, IntegralType.Int16, IntegralType.Float);
        }
        {
            ConvertUInt32ToFloat_Delegate d = DefaultConversionsToFloat.ConvertUInt32ToFloat_Default;
            table.SetConverter(d, IntegralType.UInt32, IntegralType.Float);
        }
        {
            ConvertInt32ToFloat_Delegate d = DefaultConversionsToFloat.ConvertInt32ToFloat_Default;
            table.SetConverter(d, IntegralType.Int32, IntegralType.Float);
        }
        {
            ConvertUInt64ToFloat_Delegate d = DefaultConversionsToFloat.ConvertUInt64ToFloat_Default;
            table.SetConverter(d, IntegralType.UInt64, IntegralType.Float);
        }
        {
            ConvertInt64ToFloat_Delegate d = DefaultConversionsToFloat.ConvertInt64ToFloat_Default;
            table.SetConverter(d, IntegralType.Int64, IntegralType.Float);
        }
        {
            ConvertFloatToFloat_Delegate d = DefaultConversionsToFloat.ConvertFloatToFloat_Default;
            table.SetConverter(d, IntegralType.Float, IntegralType.Float);
        }
        {
            ConvertDoubleToFloat_Delegate d = DefaultConversionsToFloat.ConvertDoubleToFloat_Default;
            table.SetConverter(d, IntegralType.Double, IntegralType.Float);
        }

        // → Double
        {
            ConvertUInt8ToDouble_Delegate d = DefaultConversionsToDouble.ConvertUInt8ToDouble_Default;
            table.SetConverter(d, IntegralType.UInt8, IntegralType.Double);
        }
        {
            ConvertInt8ToDouble_Delegate d = DefaultConversionsToDouble.ConvertInt8ToDouble_Default;
            table.SetConverter(d, IntegralType.Int8, IntegralType.Double);
        }
        {
            ConvertUInt16ToDouble_Delegate d = DefaultConversionsToDouble.ConvertUInt16ToDouble_Default;
            table.SetConverter(d, IntegralType.UInt16, IntegralType.Double);
        }
        {
            ConvertInt16ToDouble_Delegate d = DefaultConversionsToDouble.ConvertInt16ToDouble_Default;
            table.SetConverter(d, IntegralType.Int16, IntegralType.Double);
        }
        {
            ConvertUInt32ToDouble_Delegate d = DefaultConversionsToDouble.ConvertUInt32ToDouble_Default;
            table.SetConverter(d, IntegralType.UInt32, IntegralType.Double);
        }
        {
            ConvertInt32ToDouble_Delegate d = DefaultConversionsToDouble.ConvertInt32ToDouble_Default;
            table.SetConverter(d, IntegralType.Int32, IntegralType.Double);
        }
        {
            ConvertUInt64ToDouble_Delegate d = DefaultConversionsToDouble.ConvertUInt64ToDouble_Default;
            table.SetConverter(d, IntegralType.UInt64, IntegralType.Double);
        }
        {
            ConvertInt64ToDouble_Delegate d = DefaultConversionsToDouble.ConvertInt64ToDouble_Default;
            table.SetConverter(d, IntegralType.Int64, IntegralType.Double);
        }
        {
            ConvertFloatToDouble_Delegate d = DefaultConversionsToDouble.ConvertFloatToDouble_Default;
            table.SetConverter(d, IntegralType.Float, IntegralType.Double);
        }
        {
            ConvertDoubleToDouble_Delegate d = DefaultConversionsToDouble.ConvertDoubleToDouble_Default;
            table.SetConverter(d, IntegralType.Double, IntegralType.Double);
        }

    }
}
