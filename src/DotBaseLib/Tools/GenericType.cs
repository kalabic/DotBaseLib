namespace DotBase.Tools;


/// <summary>
/// 
/// Enable the JIT to see GenericType<byte>.IsXYZ is always true, and it will
/// (hopefully) remove the else branch entirely at JIT time.
/// 
/// </summary>
public static class GenericType<T>
{
    public static readonly bool IsByte = typeof(T) == typeof(byte);

    public static readonly bool IsSByte = typeof(T) == typeof(sbyte);

    public static readonly bool IsBool = typeof(T) == typeof(bool);

    public static readonly bool IsChar = typeof(T) == typeof(char);

    public static readonly bool IsDecimal = typeof(T) == typeof(decimal);

    public static readonly bool IsDouble = typeof(T) == typeof(double);

    public static readonly bool IsFloat = typeof(T) == typeof(float);

    public static readonly bool IsInt = typeof(T) == typeof(int);

    public static readonly bool IsUInt = typeof(T) == typeof(uint);

    public static readonly bool IsNInt = typeof(T) == typeof(nint);

    public static readonly bool IsNUInt = typeof(T) == typeof(nuint);

    public static readonly bool IsLong = typeof(T) == typeof(long);

    public static readonly bool IsULong = typeof(T) == typeof(ulong);

    public static readonly bool IsShort = typeof(T) == typeof(short);

    public static readonly bool IsUShort = typeof(T) == typeof(ushort);

    public static readonly bool IsUnmanaged 
        = IsBool || IsByte || IsSByte || IsChar || IsDecimal || IsDouble || IsFloat ||
          IsInt || IsUInt || IsNInt || IsNUInt || IsLong || IsULong || IsShort || IsUShort;
}
