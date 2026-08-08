using DotBase.Buffers;
using System.Diagnostics;

namespace DotBase.Integral.Internal;


/// <summary>
/// Internal value-format fields for <see cref="IntegralFormat"/>: byte order,
/// integral type, byte size per value, and optional extended payload.
/// <see cref="ByteOrder"/> and <see cref="IntegralType"/> share a packed
/// <see langword="ushort"/>; size and extended are separate fields.
/// Empty / size-0 is allowed only with <see cref="IntegralType.None"/>.
/// </summary>
internal readonly struct IntegralValueFormat
{
    public ByteOrder ByteOrder
    {
        get { return (ByteOrder)(_packType & BYTEORDER_MASK); }
    }

    public IntegralType ValueType
    {
        get { return (IntegralType)((_packType & VALUETYPE_MASK) >> 8); }
    }

    public int ValueSize
    {
        get { return _valueSize; }
    }

    public ushort Extended
    {
        get { return _extended; }
    }

    private const uint BYTEORDER_MASK = 0x0003;

    private const uint VALUETYPE_MASK = 0x3F00;

    private readonly int _valueSize;

    private readonly ushort _packType;

    private readonly ushort _extended;

    public IntegralValueFormat()
    {
        _valueSize = 0;
        _packType = 0;
        _extended = 0;
    }

    public IntegralValueFormat(ByteOrder byteOrder, IntegralType valueType, int valueSize)
    {
        // ByteOrder.Undefined (and other invalid orders) may be packed here;
        // IntegralFormat.Validate rejects them at use sites.
        Debug.Assert(valueType.IsValid());
        Debug.Assert(valueSize >= 0);

        // Empty sentinel: size 0 with NONE.
        Debug.Assert(valueSize > 0 || valueType == IntegralType.None);

        if (valueType.IsValid() && valueSize >= 0)
        {
            _valueSize = valueSize;
            uint packedByteOrder = ((uint)byteOrder.TrimUndefined()) & BYTEORDER_MASK;
            uint packedValueType = ((uint)valueType.TrimUndefined() << 8) & VALUETYPE_MASK;
            _packType = (ushort)(packedByteOrder | packedValueType);
            _extended = 0;
        }
        else
        {
            _valueSize = 0;
            _packType = 0;
            _extended = 0;
        }
    }

    public IntegralValueFormat(ByteOrder byteOrder, ushort extended, int valueSize)
    {
        Debug.Assert(valueSize > 0);

        if (valueSize > 0)
        {
            _valueSize = valueSize;
            uint packedByteOrder = ((uint)byteOrder.TrimUndefined()) & BYTEORDER_MASK;
            uint packedValueType = ((uint)IntegralType.None << 8) & VALUETYPE_MASK;
            _packType = (ushort)(packedByteOrder | packedValueType);
            _extended = extended;
        }
        else
        {
            _valueSize = 0;
            _packType = 0;
            _extended = 0;
        }
    }

    public bool IsEmpty()
    {
        return _valueSize == 0;
    }

    public bool IsCompatible<T>()
        where T : unmanaged
    {
        return ValueType.IsCompatible<T>();
    }
}
