using DotBase.Buffers;

namespace DotBase.Integral.Conversion;

internal interface IConversionDelegateTable
{
    void SetCustomFunc(IntegralSpanConversionFunc func, ByteOrder inputByteOrder, IntegralType inputType, ByteOrder outputByteOrder, IntegralType outputType);

    void SetDefaultFunc(IntegralSpanConversionFunc func, ByteOrder inputByteOrder, IntegralType inputType, ByteOrder outputByteOrder, IntegralType outputType);
}
