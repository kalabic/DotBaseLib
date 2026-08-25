using System.Diagnostics;
using System.Runtime.CompilerServices;
using DotBase.Integral;
using DotBase.Integral.Internal;

namespace DotBase.Buffers.Integral.Internal.Endian;


internal sealed class WaitableRingBufferLE
    : WaitableRingBuffer
{
    private const int ScratchByteCount = 512;

    internal WaitableRingBufferLE(int capacity)
        : base(capacity, IntegralFormat.LittleEndianStream)
    {
    }

    internal WaitableRingBufferLE(int capacity, IntegralFormat format)
        : base(capacity, format)
    {
    }

    public override ByteOrder ByteOrder => ByteOrder.LittleEndian;

    protected override int ReadIntegralSpan(in IntegralSpan destination)
    {
        return IntegralRingOperationsLE.Read(ref _storage, destination);
    }

    protected override bool TryReadIntegralSpan(in IntegralSpan destination)
    {
        return IntegralRingOperationsLE.TryRead(ref _storage, destination);
    }

    protected override bool TryReadIntegralSpanChecked(in IntegralSpan destination)
    {
        return IntegralRingOperationsLE.TryReadChecked(ref _storage, destination);
    }

    protected override int WriteIntegralSpan(in IntegralSpan source)
    {
        return IntegralRingOperationsLE.Write(ref _storage, source);
    }

    protected override bool TryWriteIntegralSpan(in IntegralSpan source)
    {
        return IntegralRingOperationsLE.TryWrite(ref _storage, source);
    }

    protected override bool TryWriteIntegralSpanChecked(in IntegralSpan source)
    {
        return IntegralRingOperationsLE.TryWriteChecked(ref _storage, source);
    }

    protected override unsafe bool TryReadScalar<T>(out T value)
    {
        int n = Unsafe.SizeOf<T>();
        if (!_storage.IsOpen || _storage.StoredBytes < n)
        {
            value = default;
            return false;
        }

        T tmp = default;
        byte* p = (byte*)&tmp;
        if (BitConverter.IsLittleEndian)
        {
            switch (n)
            {
                case 1:
                    _storage.Read(p, 1);
                    break;
                case 2:
                    _storage.ReadBE2(p);
                    break;
                case 4:
                    _storage.ReadBE4(p);
                    break;
                case 8:
                    _storage.ReadBE8(p);
                    break;
                default:
                    _storage.Read(p, n);
                    break;
            }
        }
        else
        {
            switch (n)
            {
                case 1:
                    _storage.Read(p, 1);
                    break;
                case 2:
                    _storage.ReadLE2(p);
                    break;
                case 4:
                    _storage.ReadLE4(p);
                    break;
                case 8:
                    _storage.ReadLE8(p);
                    break;
                default:
                    _storage.Read(p, n);
                    tmp = IntegralEndianness.ReverseValue(tmp);
                    break;
            }
        }

        value = tmp;
        return true;
    }

    protected override unsafe bool TryWriteScalar<T>(T value)
    {
        int n = Unsafe.SizeOf<T>();
        if (!_storage.IsOpen || _storage.FreeBytes < n)
        {
            return false;
        }

        if (BitConverter.IsLittleEndian)
        {
            switch (n)
            {
                case 1:
                    _storage.Write((byte*)&value, 1);
                    break;
                case 2:
                    _storage.WriteBE2((byte*)&value);
                    break;
                case 4:
                    _storage.WriteBE4((byte*)&value);
                    break;
                case 8:
                    _storage.WriteBE8((byte*)&value);
                    break;
                default:
                    _storage.Write((byte*)&value, n);
                    break;
            }
        }
        else
        {
            switch (n)
            {
                case 1:
                    _storage.Write((byte*)&value, 1);
                    break;
                case 2:
                    _storage.WriteLE2((byte*)&value);
                    break;
                case 4:
                    _storage.WriteLE4((byte*)&value);
                    break;
                case 8:
                    _storage.WriteLE8((byte*)&value);
                    break;
                default:
                    value = IntegralEndianness.ReverseValue(value);
                    _storage.Write((byte*)&value, n);
                    break;
            }
        }

        return true;
    }

    protected override unsafe int ReadCore<T>(T* destination, int count)
    {
        if (count <= 0)
        {
            return 0;
        }

        int n = Unsafe.SizeOf<T>();
        int elementCount = Math.Min(count, _storage.StoredBytes / n);
        if (elementCount == 0)
        {
            return 0;
        }

        int bytes = _storage.Read(
            (byte*)destination,
            checked((int)((long)elementCount * n)));
        Debug.Assert(bytes == elementCount * n);

        if (!BitConverter.IsLittleEndian && n > 1)
        {
            IntegralPrimitives.ReverseLanesInPlace((byte*)destination, elementCount, n);
        }

        return elementCount;
    }

    protected override unsafe bool TryReadCore<T>(T* destination, int count)
    {
        long requiredBytes = (long)count * Unsafe.SizeOf<T>();
        if (!_storage.IsOpen || _storage.StoredBytes < requiredBytes)
        {
            return false;
        }

        int elementCount = ReadCore(destination, count);
        Debug.Assert(elementCount == count);
        return true;
    }

    protected override unsafe int WriteCore<T>(T* source, int count)
    {
        if (count <= 0)
        {
            return 0;
        }

        int n = Unsafe.SizeOf<T>();
        int elementCount = Math.Min(count, _storage.FreeBytes / n);
        if (elementCount == 0)
        {
            return 0;
        }

        if (BitConverter.IsLittleEndian || n <= 1)
        {
            int bytes = _storage.Write(
                (byte*)source,
                checked((int)((long)elementCount * n)));
            Debug.Assert(bytes == elementCount * n);
            return elementCount;
        }

        WriteReversed(source, elementCount);
        return elementCount;
    }

    protected override unsafe bool TryWriteCore<T>(T* source, int count)
    {
        long requiredBytes = (long)count * Unsafe.SizeOf<T>();
        if (!_storage.IsOpen || _storage.FreeBytes < requiredBytes)
        {
            return false;
        }

        int elementCount = WriteCore(source, count);
        Debug.Assert(elementCount == count);
        return true;
    }

    private unsafe void WriteReversed<T>(T* source, int count)
        where T : unmanaged
    {
        int n = Unsafe.SizeOf<T>();
        int scratchCount = Math.Max(1, ScratchByteCount / n);
        byte* scratch = stackalloc byte[scratchCount * n];

        int position = 0;
        while (position < count)
        {
            int chunk = Math.Min(scratchCount, count - position);
            IntegralPrimitives.ReverseCopyLanes(
                (byte*)(source + position),
                scratch,
                chunk,
                n);

            int bytes = _storage.Write(
                scratch,
                checked((int)((long)chunk * n)));
            Debug.Assert(bytes == chunk * n);
            position += chunk;
        }
    }
}
