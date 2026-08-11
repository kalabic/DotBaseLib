namespace DotBase.Integral.Conversion;


/// <summary>Creates a contiguous (span) conversion handle for a type pair.</summary>
public delegate IntegralConversionHandle IntegralSpanConversionHandleFunc(
    in IntegralFormat input,
    in IntegralFormat output);

/// <summary>Creates an interleaved-reader conversion handle for a type pair.</summary>
public delegate IntegralConversionHandle InterleavedReaderConversionHandleFunc(
    in IntegralFormat input,
    in IntegralFormat output);

/// <summary>Creates an interleaved-writer conversion handle for a type pair.</summary>
public delegate IntegralConversionHandle InterleavedWriterConversionHandleFunc(
    in IntegralFormat input,
    in IntegralFormat output);

/// <summary>Creates a contiguous conversion context (null = unsupported).</summary>
public delegate ConversionContext? IntegralSpanConversionContextFunc(
    in IntegralFormat input,
    in IntegralFormat output);

/// <summary>Creates an interleaved-reader context (null = unsupported).</summary>
public delegate ConversionContext? InterleavedReaderConversionContextFunc(
    in IntegralFormat input,
    in IntegralFormat output,
    int inputBlockCapacity,
    int index);

/// <summary>Creates an interleaved-writer context (null = unsupported).</summary>
public delegate ConversionContext? InterleavedWriterConversionContextFunc(
    in IntegralFormat input,
    in IntegralFormat output,
    int outputBlockCapacity,
    int index);
