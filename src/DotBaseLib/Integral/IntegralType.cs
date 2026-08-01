using DotBase.Tools;

namespace DotBase.Integral;


public enum IntegralType
{
    NONE = 0,

    UInt8,
    Int8,
    UInt16,
    Int16,
    UInt32,
    Int32,
    UInt64,
    Int64,

    Float,
    Double,
}

/// <summary>Public helpers for <see cref="IntegralType"/> size, CLR mapping, and compatibility.</summary>
public static class IntegralTypeExtensions
{
    /// <summary>Byte width of one scalar of this type; 0 for <see cref="IntegralType.NONE"/>.</summary>
    public static int Size(this IntegralType type)
    {
        return type switch
        {
            IntegralType.NONE => 0,
            IntegralType.UInt8 => 1,
            IntegralType.Int8 => 1,
            IntegralType.UInt16 => 2,
            IntegralType.Int16 => 2,
            IntegralType.UInt32 => 4,
            IntegralType.Int32 => 4,
            IntegralType.UInt64 => 8,
            IntegralType.Int64 => 8,
            IntegralType.Float => 4,
            IntegralType.Double => 8,
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };
    }

    /// <summary>
    /// Default <see cref="IntegralType"/> for CLR type <typeparamref name="T"/>,
    /// or <see cref="IntegralType.NONE"/> if unsupported. The receiver is unused
    /// (call as <c>default(IntegralType).DefaultForType&lt;T&gt;()</c> or
    /// <c>IntegralType.NONE.DefaultForType&lt;T&gt;()</c>).
    /// </summary>
    public static IntegralType DefaultForType<T>(this IntegralType id)
        where T : unmanaged
    {
        _ = id;

        if (GenericType<T>.IsByte)
        {
            return IntegralType.UInt8;
        }
        if (GenericType<T>.IsSByte)
        {
            return IntegralType.Int8;
        }
        if (GenericType<T>.IsShort)
        {
            return IntegralType.Int16;
        }
        if (GenericType<T>.IsUShort)
        {
            return IntegralType.UInt16;
        }
        if (GenericType<T>.IsInt)
        {
            return IntegralType.Int32;
        }
        if (GenericType<T>.IsUInt)
        {
            return IntegralType.UInt32;
        }
        if (GenericType<T>.IsLong)
        {
            return IntegralType.Int64;
        }
        if (GenericType<T>.IsULong)
        {
            return IntegralType.UInt64;
        }
        if (GenericType<T>.IsDouble)
        {
            return IntegralType.Double;
        }
        if (GenericType<T>.IsFloat)
        {
            return IntegralType.Float;
        }

        return IntegralType.NONE;
    }

    /// <summary>
    /// Whether a value of CLR type <typeparamref name="T"/> can hold this integral type
    /// (same size and signedness family as used by span/pointer APIs).
    /// </summary>
    public static bool IsCompatible<T>(this IntegralType id)
        where T : unmanaged
    {
        switch (id)
        {
            case IntegralType.NONE:
                return false;

            case IntegralType.Int8:
                return GenericType<T>.IsInt8;

            case IntegralType.UInt8:
                return GenericType<T>.IsUInt8;

            case IntegralType.Int16:
                return GenericType<T>.IsShort;

            case IntegralType.UInt16:
                return GenericType<T>.IsUShort;

            case IntegralType.Int32:
                return GenericType<T>.IsInt;

            case IntegralType.UInt32:
                return GenericType<T>.IsUInt;

            case IntegralType.Int64:
                return GenericType<T>.IsLong;

            case IntegralType.UInt64:
                return GenericType<T>.IsULong;

            case IntegralType.Float:
                return GenericType<T>.IsFloat;

            case IntegralType.Double:
                return GenericType<T>.IsDouble;

            default:
                throw new ArgumentException($"Invalid integral type identifier: {id}");
        }
    }
}
