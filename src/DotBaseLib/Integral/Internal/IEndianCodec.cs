using DotBase.Buffers;

namespace DotBase.Integral.Internal;


internal interface IEndianCodec
{
    static abstract ByteOrder ByteOrder { get; }
}
