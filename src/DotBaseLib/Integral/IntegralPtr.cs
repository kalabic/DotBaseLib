using System.Diagnostics;
using System.Runtime.InteropServices;
using DotBase.Buffers;
using DotBase.Tools;

namespace DotBase.Integral;


/// <summary>
///
/// Just a byte pointer with information about integral values in memory it is referencing.
///
/// </summary>
public unsafe readonly struct IntegralPtr
{
    public static readonly IntegralPtr Null = new IntegralPtr();

    /// <summary> The original, unadjusted base pointer. </summary>
    public readonly byte* BytePtr;

    public readonly IntegralFormat Fmt;


    public bool IsNull { get { return BytePtr == null; } }

    public IntegralPtr()
    {
        BytePtr = null;
        Fmt = IntegralFormat.Empty;
    }

    /// <summary>
    /// Does not validate <paramref name="fmt"/>; call <see cref="Validate"/> when needed.
    /// </summary>
    public IntegralPtr(byte* ptr, IntegralFormat fmt)
    {
        BytePtr = ptr;
        Fmt = fmt;
    }

    /// <summary>Whether <see cref="Fmt"/> is a valid empty or non-empty format.</summary>
    public bool IsValid() => Fmt.IsValid();

    /// <summary>Validates <see cref="Fmt"/> only.</summary>
    public void Validate() => Fmt.Validate();

    public T* GetAs<T>() where T : unmanaged
    {
        return (T*)BytePtr;
    }

    public bool IsCompatible<T>()
        where T : unmanaged
    {
        return Fmt.IsCompatible<T>();
    }

    /// <summary>Pins <paramref name="array"/> until the returned <see cref="Fixed{T}"/> is disposed.</summary>
    public static Fixed<T> Pin<T>(T[] array, IntegralType format = IntegralType.None)
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(array);
        return new Any<T>(array, format).MakeFixed();
    }

    public static implicit operator byte*(in IntegralPtr other) { return (byte*)other.BytePtr; }

    public static implicit operator short*(in IntegralPtr other) { return (short*)(byte*)other.BytePtr; }



    /// <summary>
    ///
    /// Special catch-all basket for wrapping up different integral types.
    ///
    /// </summary>
    public readonly unsafe struct Any<T>
        where T : unmanaged
    {
        public static implicit operator Any<T>(T* other) => new(other);

        public static implicit operator Any<T>(T[] other) => new(other);

        /// <summary> The GC owns Arrays. </summary>
        public readonly T[]? Arr;

        /// <summary> The caller owns pointers. </summary>
        public readonly T* Ptr;

        /// <summary> Default value determined by template parameter T. </summary>
        public readonly IntegralType Format;


        public bool IsArray { get { return Arr is not null; } }

        public Any(T* other, IntegralType format = IntegralType.None)
        {
            Arr = null;
            Ptr = other;
            Format = (!GenericType<T>.IsByte && format == IntegralType.None) ? IntegralType.None.DefaultForType<T>() : format;
            Debug.Assert(GenericType<T>.IsByte || Format.IsCompatible<T>(), $"Type {nameof(T)} cannot hold the supplied sample-value format.");
        }

        public Any(T[] other, IntegralType format = IntegralType.None)
        {
            ArgumentNullException.ThrowIfNull(other);
            Arr = other;
            Ptr = default;
            Format = (!GenericType<T>.IsByte && format == IntegralType.None) ? IntegralType.None.DefaultForType<T>() : format;
            Debug.Assert(GenericType<T>.IsByte || Format.IsCompatible<T>(), $"Type {nameof(T)} cannot hold the supplied sample-value format.");
        }

        /// <summary> Returns an object that MUST be disposed. </summary>
        public Fixed<T> MakeFixed()
        {
            return new Fixed<T>(this);
        }
    }


    /// <summary>
    ///
    /// The caller-owned disposable pointer.
    ///
    /// </summary>
    public readonly struct Fixed<T> : IDisposable
        where T : unmanaged
    {
        public static implicit operator byte*(Fixed<T> other) => (byte*)other.Ptr;

        private readonly GCHandle Handle;

        public readonly T* Ptr;

        public readonly IntegralType Format;

        /// <summary>
        /// Number of <typeparamref name="T"/> values when pinned from an array; -1 when
        /// created from a raw pointer (caller must pass count to <see cref="AsSpan(long, int, ByteOrder)"/>).
        /// </summary>
        public readonly long ValueCount;

        internal Fixed(Any<T> other)
        {
            if (other.Arr is not null)
            {
                Handle = GCHandle.Alloc(other.Arr, GCHandleType.Pinned);
                Ptr = (T*)Handle.AddrOfPinnedObject();
                Format = other.Format;
                ValueCount = other.Arr.Length;
            }
            else
            {
                Handle = default;
                Ptr = other.Ptr;
                Format = other.Format;
                ValueCount = -1;
            }
        }

        /// <summary>
        /// Full array view when this pin was created from a <typeparamref name="T"/>[].
        /// </summary>
        public IntegralSpan AsSpan(
            int blockCapacity = 1,
            ByteOrder byteOrder = ByteOrder.Native)
        {
            if (ValueCount < 0)
            {
                throw new InvalidOperationException(
                    "Value count is unknown for pointer pins. " +
                    "Call AsSpan(valueCount, blockCapacity, byteOrder) instead.");
            }

            return AsSpan(ValueCount, blockCapacity, byteOrder);
        }

        /// <summary>
        /// View over <paramref name="valueCount"/> values starting at <see cref="Ptr"/>.
        /// </summary>
        public IntegralSpan AsSpan(
            long valueCount,
            int blockCapacity = 1,
            ByteOrder byteOrder = ByteOrder.Native)
        {
            return IntegralSpan.FromValues(
                Ptr,
                valueCount,
                blockCapacity,
                byteOrder,
                Format);
        }

        public void Dispose()
        {
            if (Handle.IsAllocated)
            {
                Handle.Free();
            }
        }
    }
}
