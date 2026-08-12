using System.Diagnostics;
using DotBase.Integral.Conversion.Numeric;

namespace DotBase.Integral.Conversion.Numeric.Defaults;


/// <summary>
/// Process-wide lazy cache of built-in scalar numeric converters.
/// </summary>
internal sealed class DefaultNumericValueDelegateTable
{
    internal static DefaultNumericValueDelegateTable Instance { get; } = new();

    private readonly Delegate?[] _converters =
        new Delegate?[NumericValueConverters.TableSize];

    private readonly object _resolveLock = new();

    private DefaultNumericValueDelegateTable()
    {
    }

    internal Delegate GetConverter(
        int index,
        IntegralType inputType,
        IntegralType outputType)
    {
        Debug.Assert(index >= 0 && index < NumericValueConverters.TableSize);

        Delegate? converter = Volatile.Read(ref _converters[index]);
        if (converter is not null)
        {
            return converter;
        }

        lock (_resolveLock)
        {
            converter = _converters[index];
            if (converter is null)
            {
                converter = Resolve(inputType, outputType);
                Volatile.Write(ref _converters[index], converter);
            }

            return converter;
        }
    }

    private static Delegate Resolve(
        IntegralType inputType,
        IntegralType outputType)
    {
        return outputType switch
        {
            IntegralType.UInt8 => ResolveToUInt8(inputType),
            IntegralType.Int8 => ResolveToInt8(inputType),
            IntegralType.UInt16 => ResolveToUInt16(inputType),
            IntegralType.Int16 => ResolveToInt16(inputType),
            IntegralType.UInt32 => ResolveToUInt32(inputType),
            IntegralType.Int32 => ResolveToInt32(inputType),
            IntegralType.UInt64 => ResolveToUInt64(inputType),
            IntegralType.Int64 => ResolveToInt64(inputType),
            IntegralType.Float => ResolveToFloat(inputType),
            IntegralType.Double => ResolveToDouble(inputType),
            _ => throw new ArgumentOutOfRangeException(nameof(outputType)),
        };
    }

    private static Delegate ResolveToUInt8(IntegralType inputType)
    {
        return inputType switch
        {
            IntegralType.UInt8 => (ConvertUInt8ToUInt8_Delegate)DefaultConversionsToUInt8.ConvertUInt8ToUInt8_Default,
            IntegralType.Int8 => (ConvertInt8ToUInt8_Delegate)DefaultConversionsToUInt8.ConvertInt8ToUInt8_Default,
            IntegralType.UInt16 => (ConvertUInt16ToUInt8_Delegate)DefaultConversionsToUInt8.ConvertUInt16ToUInt8_Default,
            IntegralType.Int16 => (ConvertInt16ToUInt8_Delegate)DefaultConversionsToUInt8.ConvertInt16ToUInt8_Default,
            IntegralType.UInt32 => (ConvertUInt32ToUInt8_Delegate)DefaultConversionsToUInt8.ConvertUInt32ToUInt8_Default,
            IntegralType.Int32 => (ConvertInt32ToUInt8_Delegate)DefaultConversionsToUInt8.ConvertInt32ToUInt8_Default,
            IntegralType.UInt64 => (ConvertUInt64ToUInt8_Delegate)DefaultConversionsToUInt8.ConvertUInt64ToUInt8_Default,
            IntegralType.Int64 => (ConvertInt64ToUInt8_Delegate)DefaultConversionsToUInt8.ConvertInt64ToUInt8_Default,
            IntegralType.Float => (ConvertFloatToUInt8_Delegate)DefaultConversionsToUInt8.ConvertFloatToUInt8_Default,
            IntegralType.Double => (ConvertDoubleToUInt8_Delegate)DefaultConversionsToUInt8.ConvertDoubleToUInt8_Default,
            _ => throw new ArgumentOutOfRangeException(nameof(inputType)),
        };
    }

    private static Delegate ResolveToInt8(IntegralType inputType)
    {
        return inputType switch
        {
            IntegralType.UInt8 => (ConvertUInt8ToInt8_Delegate)DefaultConversionsToInt8.ConvertUInt8ToInt8_Default,
            IntegralType.Int8 => (ConvertInt8ToInt8_Delegate)DefaultConversionsToInt8.ConvertInt8ToInt8_Default,
            IntegralType.UInt16 => (ConvertUInt16ToInt8_Delegate)DefaultConversionsToInt8.ConvertUInt16ToInt8_Default,
            IntegralType.Int16 => (ConvertInt16ToInt8_Delegate)DefaultConversionsToInt8.ConvertInt16ToInt8_Default,
            IntegralType.UInt32 => (ConvertUInt32ToInt8_Delegate)DefaultConversionsToInt8.ConvertUInt32ToInt8_Default,
            IntegralType.Int32 => (ConvertInt32ToInt8_Delegate)DefaultConversionsToInt8.ConvertInt32ToInt8_Default,
            IntegralType.UInt64 => (ConvertUInt64ToInt8_Delegate)DefaultConversionsToInt8.ConvertUInt64ToInt8_Default,
            IntegralType.Int64 => (ConvertInt64ToInt8_Delegate)DefaultConversionsToInt8.ConvertInt64ToInt8_Default,
            IntegralType.Float => (ConvertFloatToInt8_Delegate)DefaultConversionsToInt8.ConvertFloatToInt8_Default,
            IntegralType.Double => (ConvertDoubleToInt8_Delegate)DefaultConversionsToInt8.ConvertDoubleToInt8_Default,
            _ => throw new ArgumentOutOfRangeException(nameof(inputType)),
        };
    }

    private static Delegate ResolveToUInt16(IntegralType inputType)
    {
        return inputType switch
        {
            IntegralType.UInt8 => (ConvertUInt8ToUInt16_Delegate)DefaultConversionsToUInt16.ConvertUInt8ToUInt16_Default,
            IntegralType.Int8 => (ConvertInt8ToUInt16_Delegate)DefaultConversionsToUInt16.ConvertInt8ToUInt16_Default,
            IntegralType.UInt16 => (ConvertUInt16ToUInt16_Delegate)DefaultConversionsToUInt16.ConvertUInt16ToUInt16_Default,
            IntegralType.Int16 => (ConvertInt16ToUInt16_Delegate)DefaultConversionsToUInt16.ConvertInt16ToUInt16_Default,
            IntegralType.UInt32 => (ConvertUInt32ToUInt16_Delegate)DefaultConversionsToUInt16.ConvertUInt32ToUInt16_Default,
            IntegralType.Int32 => (ConvertInt32ToUInt16_Delegate)DefaultConversionsToUInt16.ConvertInt32ToUInt16_Default,
            IntegralType.UInt64 => (ConvertUInt64ToUInt16_Delegate)DefaultConversionsToUInt16.ConvertUInt64ToUInt16_Default,
            IntegralType.Int64 => (ConvertInt64ToUInt16_Delegate)DefaultConversionsToUInt16.ConvertInt64ToUInt16_Default,
            IntegralType.Float => (ConvertFloatToUInt16_Delegate)DefaultConversionsToUInt16.ConvertFloatToUInt16_Default,
            IntegralType.Double => (ConvertDoubleToUInt16_Delegate)DefaultConversionsToUInt16.ConvertDoubleToUInt16_Default,
            _ => throw new ArgumentOutOfRangeException(nameof(inputType)),
        };
    }

    private static Delegate ResolveToInt16(IntegralType inputType)
    {
        return inputType switch
        {
            IntegralType.UInt8 => (ConvertUInt8ToInt16_Delegate)DefaultConversionsToInt16.ConvertUInt8ToInt16_Default,
            IntegralType.Int8 => (ConvertInt8ToInt16_Delegate)DefaultConversionsToInt16.ConvertInt8ToInt16_Default,
            IntegralType.UInt16 => (ConvertUInt16ToInt16_Delegate)DefaultConversionsToInt16.ConvertUInt16ToInt16_Default,
            IntegralType.Int16 => (ConvertInt16ToInt16_Delegate)DefaultConversionsToInt16.ConvertInt16ToInt16_Default,
            IntegralType.UInt32 => (ConvertUInt32ToInt16_Delegate)DefaultConversionsToInt16.ConvertUInt32ToInt16_Default,
            IntegralType.Int32 => (ConvertInt32ToInt16_Delegate)DefaultConversionsToInt16.ConvertInt32ToInt16_Default,
            IntegralType.UInt64 => (ConvertUInt64ToInt16_Delegate)DefaultConversionsToInt16.ConvertUInt64ToInt16_Default,
            IntegralType.Int64 => (ConvertInt64ToInt16_Delegate)DefaultConversionsToInt16.ConvertInt64ToInt16_Default,
            IntegralType.Float => (ConvertFloatToInt16_Delegate)DefaultConversionsToInt16.ConvertFloatToInt16_Default,
            IntegralType.Double => (ConvertDoubleToInt16_Delegate)DefaultConversionsToInt16.ConvertDoubleToInt16_Default,
            _ => throw new ArgumentOutOfRangeException(nameof(inputType)),
        };
    }

    private static Delegate ResolveToUInt32(IntegralType inputType)
    {
        return inputType switch
        {
            IntegralType.UInt8 => (ConvertUInt8ToUInt32_Delegate)DefaultConversionsToUInt32.ConvertUInt8ToUInt32_Default,
            IntegralType.Int8 => (ConvertInt8ToUInt32_Delegate)DefaultConversionsToUInt32.ConvertInt8ToUInt32_Default,
            IntegralType.UInt16 => (ConvertUInt16ToUInt32_Delegate)DefaultConversionsToUInt32.ConvertUInt16ToUInt32_Default,
            IntegralType.Int16 => (ConvertInt16ToUInt32_Delegate)DefaultConversionsToUInt32.ConvertInt16ToUInt32_Default,
            IntegralType.UInt32 => (ConvertUInt32ToUInt32_Delegate)DefaultConversionsToUInt32.ConvertUInt32ToUInt32_Default,
            IntegralType.Int32 => (ConvertInt32ToUInt32_Delegate)DefaultConversionsToUInt32.ConvertInt32ToUInt32_Default,
            IntegralType.UInt64 => (ConvertUInt64ToUInt32_Delegate)DefaultConversionsToUInt32.ConvertUInt64ToUInt32_Default,
            IntegralType.Int64 => (ConvertInt64ToUInt32_Delegate)DefaultConversionsToUInt32.ConvertInt64ToUInt32_Default,
            IntegralType.Float => (ConvertFloatToUInt32_Delegate)DefaultConversionsToUInt32.ConvertFloatToUInt32_Default,
            IntegralType.Double => (ConvertDoubleToUInt32_Delegate)DefaultConversionsToUInt32.ConvertDoubleToUInt32_Default,
            _ => throw new ArgumentOutOfRangeException(nameof(inputType)),
        };
    }

    private static Delegate ResolveToInt32(IntegralType inputType)
    {
        return inputType switch
        {
            IntegralType.UInt8 => (ConvertUInt8ToInt32_Delegate)DefaultConversionsToInt32.ConvertUInt8ToInt32_Default,
            IntegralType.Int8 => (ConvertInt8ToInt32_Delegate)DefaultConversionsToInt32.ConvertInt8ToInt32_Default,
            IntegralType.UInt16 => (ConvertUInt16ToInt32_Delegate)DefaultConversionsToInt32.ConvertUInt16ToInt32_Default,
            IntegralType.Int16 => (ConvertInt16ToInt32_Delegate)DefaultConversionsToInt32.ConvertInt16ToInt32_Default,
            IntegralType.UInt32 => (ConvertUInt32ToInt32_Delegate)DefaultConversionsToInt32.ConvertUInt32ToInt32_Default,
            IntegralType.Int32 => (ConvertInt32ToInt32_Delegate)DefaultConversionsToInt32.ConvertInt32ToInt32_Default,
            IntegralType.UInt64 => (ConvertUInt64ToInt32_Delegate)DefaultConversionsToInt32.ConvertUInt64ToInt32_Default,
            IntegralType.Int64 => (ConvertInt64ToInt32_Delegate)DefaultConversionsToInt32.ConvertInt64ToInt32_Default,
            IntegralType.Float => (ConvertFloatToInt32_Delegate)DefaultConversionsToInt32.ConvertFloatToInt32_Default,
            IntegralType.Double => (ConvertDoubleToInt32_Delegate)DefaultConversionsToInt32.ConvertDoubleToInt32_Default,
            _ => throw new ArgumentOutOfRangeException(nameof(inputType)),
        };
    }

    private static Delegate ResolveToUInt64(IntegralType inputType)
    {
        return inputType switch
        {
            IntegralType.UInt8 => (ConvertUInt8ToUInt64_Delegate)DefaultConversionsToUInt64.ConvertUInt8ToUInt64_Default,
            IntegralType.Int8 => (ConvertInt8ToUInt64_Delegate)DefaultConversionsToUInt64.ConvertInt8ToUInt64_Default,
            IntegralType.UInt16 => (ConvertUInt16ToUInt64_Delegate)DefaultConversionsToUInt64.ConvertUInt16ToUInt64_Default,
            IntegralType.Int16 => (ConvertInt16ToUInt64_Delegate)DefaultConversionsToUInt64.ConvertInt16ToUInt64_Default,
            IntegralType.UInt32 => (ConvertUInt32ToUInt64_Delegate)DefaultConversionsToUInt64.ConvertUInt32ToUInt64_Default,
            IntegralType.Int32 => (ConvertInt32ToUInt64_Delegate)DefaultConversionsToUInt64.ConvertInt32ToUInt64_Default,
            IntegralType.UInt64 => (ConvertUInt64ToUInt64_Delegate)DefaultConversionsToUInt64.ConvertUInt64ToUInt64_Default,
            IntegralType.Int64 => (ConvertInt64ToUInt64_Delegate)DefaultConversionsToUInt64.ConvertInt64ToUInt64_Default,
            IntegralType.Float => (ConvertFloatToUInt64_Delegate)DefaultConversionsToUInt64.ConvertFloatToUInt64_Default,
            IntegralType.Double => (ConvertDoubleToUInt64_Delegate)DefaultConversionsToUInt64.ConvertDoubleToUInt64_Default,
            _ => throw new ArgumentOutOfRangeException(nameof(inputType)),
        };
    }

    private static Delegate ResolveToInt64(IntegralType inputType)
    {
        return inputType switch
        {
            IntegralType.UInt8 => (ConvertUInt8ToInt64_Delegate)DefaultConversionsToInt64.ConvertUInt8ToInt64_Default,
            IntegralType.Int8 => (ConvertInt8ToInt64_Delegate)DefaultConversionsToInt64.ConvertInt8ToInt64_Default,
            IntegralType.UInt16 => (ConvertUInt16ToInt64_Delegate)DefaultConversionsToInt64.ConvertUInt16ToInt64_Default,
            IntegralType.Int16 => (ConvertInt16ToInt64_Delegate)DefaultConversionsToInt64.ConvertInt16ToInt64_Default,
            IntegralType.UInt32 => (ConvertUInt32ToInt64_Delegate)DefaultConversionsToInt64.ConvertUInt32ToInt64_Default,
            IntegralType.Int32 => (ConvertInt32ToInt64_Delegate)DefaultConversionsToInt64.ConvertInt32ToInt64_Default,
            IntegralType.UInt64 => (ConvertUInt64ToInt64_Delegate)DefaultConversionsToInt64.ConvertUInt64ToInt64_Default,
            IntegralType.Int64 => (ConvertInt64ToInt64_Delegate)DefaultConversionsToInt64.ConvertInt64ToInt64_Default,
            IntegralType.Float => (ConvertFloatToInt64_Delegate)DefaultConversionsToInt64.ConvertFloatToInt64_Default,
            IntegralType.Double => (ConvertDoubleToInt64_Delegate)DefaultConversionsToInt64.ConvertDoubleToInt64_Default,
            _ => throw new ArgumentOutOfRangeException(nameof(inputType)),
        };
    }

    private static Delegate ResolveToFloat(IntegralType inputType)
    {
        return inputType switch
        {
            IntegralType.UInt8 => (ConvertUInt8ToFloat_Delegate)DefaultConversionsToFloat.ConvertUInt8ToFloat_Default,
            IntegralType.Int8 => (ConvertInt8ToFloat_Delegate)DefaultConversionsToFloat.ConvertInt8ToFloat_Default,
            IntegralType.UInt16 => (ConvertUInt16ToFloat_Delegate)DefaultConversionsToFloat.ConvertUInt16ToFloat_Default,
            IntegralType.Int16 => (ConvertInt16ToFloat_Delegate)DefaultConversionsToFloat.ConvertInt16ToFloat_Default,
            IntegralType.UInt32 => (ConvertUInt32ToFloat_Delegate)DefaultConversionsToFloat.ConvertUInt32ToFloat_Default,
            IntegralType.Int32 => (ConvertInt32ToFloat_Delegate)DefaultConversionsToFloat.ConvertInt32ToFloat_Default,
            IntegralType.UInt64 => (ConvertUInt64ToFloat_Delegate)DefaultConversionsToFloat.ConvertUInt64ToFloat_Default,
            IntegralType.Int64 => (ConvertInt64ToFloat_Delegate)DefaultConversionsToFloat.ConvertInt64ToFloat_Default,
            IntegralType.Float => (ConvertFloatToFloat_Delegate)DefaultConversionsToFloat.ConvertFloatToFloat_Default,
            IntegralType.Double => (ConvertDoubleToFloat_Delegate)DefaultConversionsToFloat.ConvertDoubleToFloat_Default,
            _ => throw new ArgumentOutOfRangeException(nameof(inputType)),
        };
    }

    private static Delegate ResolveToDouble(IntegralType inputType)
    {
        return inputType switch
        {
            IntegralType.UInt8 => (ConvertUInt8ToDouble_Delegate)DefaultConversionsToDouble.ConvertUInt8ToDouble_Default,
            IntegralType.Int8 => (ConvertInt8ToDouble_Delegate)DefaultConversionsToDouble.ConvertInt8ToDouble_Default,
            IntegralType.UInt16 => (ConvertUInt16ToDouble_Delegate)DefaultConversionsToDouble.ConvertUInt16ToDouble_Default,
            IntegralType.Int16 => (ConvertInt16ToDouble_Delegate)DefaultConversionsToDouble.ConvertInt16ToDouble_Default,
            IntegralType.UInt32 => (ConvertUInt32ToDouble_Delegate)DefaultConversionsToDouble.ConvertUInt32ToDouble_Default,
            IntegralType.Int32 => (ConvertInt32ToDouble_Delegate)DefaultConversionsToDouble.ConvertInt32ToDouble_Default,
            IntegralType.UInt64 => (ConvertUInt64ToDouble_Delegate)DefaultConversionsToDouble.ConvertUInt64ToDouble_Default,
            IntegralType.Int64 => (ConvertInt64ToDouble_Delegate)DefaultConversionsToDouble.ConvertInt64ToDouble_Default,
            IntegralType.Float => (ConvertFloatToDouble_Delegate)DefaultConversionsToDouble.ConvertFloatToDouble_Default,
            IntegralType.Double => (ConvertDoubleToDouble_Delegate)DefaultConversionsToDouble.ConvertDoubleToDouble_Default,
            _ => throw new ArgumentOutOfRangeException(nameof(inputType)),
        };
    }
}
