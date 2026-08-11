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

    public sbyte ExtendedId
    {
        get { return HasExtendedId ? (sbyte)(_extended & EXTENDED_ID_MASK) : (sbyte)-1; }
    }

    public sbyte ExtendedType
    {
        get { return HasExtendedType ? (sbyte)((_extended & EXTENDED_TYPE_MASK) >> 8) : (sbyte)-1; }
    }

    public bool HasExtendedId
    {
        get { return (_extended & EXTENDED_ID_FLAG) != 0; }
    }

    public bool HasExtendedType
    {
        get { return (_extended & EXTENDED_TYPE_FLAG) != 0; }
    }

    private const uint BYTEORDER_MASK = 0x0003u;

    private const uint VALUETYPE_MASK = 0x3F00u;

    private const uint EXTENDED_TYPE_FLAG = 0x8000u;

    private const uint EXTENDED_TYPE_MASK = 0x7F00u;

    private const uint EXTENDED_ID_FLAG = 0x0080u;

    private const uint EXTENDED_ID_MASK = 0x007Fu;

    private readonly int _valueSize;

    private readonly ushort _packType;

    private readonly ushort _extended;

    public IntegralValueFormat()
    {
        _valueSize = 0;
        _packType = 0;
        _extended = 0;
    }

    public IntegralValueFormat(ByteOrder byteOrder, IntegralType valueType, int valueSize, sbyte extendedType = -1, sbyte extendedId = -1)
    {
        // ByteOrder.Undefined (and other invalid orders) may be packed here;
        // IntegralFormat.Validate rejects them at use sites.
        Debug.Assert(valueType.IsValid());
        Debug.Assert(valueSize >= 0);

        // Empty sentinel: size 0 with None.
        Debug.Assert(valueSize > 0 || valueType == IntegralType.None);

        if (valueType.IsValid() && valueSize >= 0)
        {
            _valueSize = valueSize;
            uint packedByteOrder = ((uint)byteOrder.TrimUndefined()) & BYTEORDER_MASK;
            uint packedValueType = ((uint)valueType.TrimUndefined() << 8) & VALUETYPE_MASK;
            _packType = (ushort)(packedByteOrder | packedValueType);

            uint packedExtendedType = (extendedType >= 0) ? (uint)(extendedType << 8) : 0;
            packedExtendedType = (extendedType >= 0) ? (packedExtendedType | EXTENDED_TYPE_FLAG) : 0;

            uint packedExtendedId = (extendedId >= 0) ? (uint)extendedId : 0;
            packedExtendedId = (extendedId >= 0) ? (packedExtendedId | EXTENDED_ID_FLAG) : 0;

            _extended = (ushort)(packedExtendedType | packedExtendedId);
        }
        else
        {
            _valueSize = 0;
            _packType = 0;
            _extended = 0;
        }
    }

    public IntegralValueFormat(ByteOrder byteOrder, int valueSize, sbyte extendedType = -1, sbyte extendedId = -1)
    {
        Debug.Assert(valueSize > 0);

        if (valueSize > 0)
        {
            _valueSize = valueSize;
            uint packedByteOrder = ((uint)byteOrder.TrimUndefined()) & BYTEORDER_MASK;
            uint packedValueType = ((uint)IntegralType.None << 8) & VALUETYPE_MASK;
            _packType = (ushort)(packedByteOrder | packedValueType);

            uint packedExtendedType = (extendedType >= 0) ? (uint)(extendedType << 8) : 0;
            packedExtendedType = (extendedType >= 0) ? (packedExtendedType | EXTENDED_TYPE_FLAG) : 0;

            uint packedExtendedId = (extendedId >= 0) ? (uint)extendedId : 0;
            packedExtendedId = (extendedId >= 0) ? (packedExtendedId | EXTENDED_ID_FLAG) : 0;

            _extended = (ushort)(packedExtendedType | packedExtendedId);
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
