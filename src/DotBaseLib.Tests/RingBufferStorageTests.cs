using DotBase.Buffers.Integral.Internal;
using System.Runtime.CompilerServices;

namespace DotBaseLib.Tests;


/// <summary>
/// Direct tests of the bare native FIFO (InternalsVisibleTo).
/// Covers contiguous word path and every wrap residue for 2/4/8 LE and BE.
/// </summary>
public unsafe class RingBufferStorageTests
{
    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    public void BeRoundTripContiguousAndAllWrapOffsets(int size)
    {
        int capacity = 32;
        byte[] pattern = new byte[size];
        for (int i = 0; i < size; ++i)
        {
            pattern[i] = unchecked((byte)(0xA0 + i));
        }

        byte* pad = stackalloc byte[capacity];
        byte* dst = stackalloc byte[8];
        Unsafe.InitBlock(pad, 0x11, (uint)capacity);

        for (int head = 0; head < capacity; ++head)
        {
            RingBufferStorage storage = new(capacity);
            try
            {
                if (head > 0)
                {
                    storage.Write(pad, head);
                    storage.Advance(head);
                }

                fixed (byte* src = pattern)
                {
                    switch (size)
                    {
                        case 2:
                            storage.WriteBE2(src);
                            break;
                        case 4:
                            storage.WriteBE4(src);
                            break;
                        case 8:
                            storage.WriteBE8(src);
                            break;
                    }
                }

                switch (size)
                {
                    case 2:
                        storage.ReadBE2(dst);
                        break;
                    case 4:
                        storage.ReadBE4(dst);
                        break;
                    case 8:
                        storage.ReadBE8(dst);
                        break;
                }

                for (int i = 0; i < size; ++i)
                {
                    Assert.Equal(pattern[i], dst[i]);
                }

                Assert.Equal(0, storage.StoredBytes);
            }
            finally
            {
                storage.Close();
            }
        }
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    public void LeRoundTripContiguousAndAllWrapOffsets(int size)
    {
        int capacity = 32;
        byte[] pattern = new byte[size];
        for (int i = 0; i < size; ++i)
        {
            pattern[i] = unchecked((byte)(0x50 + i));
        }

        byte* pad = stackalloc byte[capacity];
        byte* raw = stackalloc byte[8];
        byte* dst = stackalloc byte[8];
        Unsafe.InitBlock(pad, 0x22, (uint)capacity);

        for (int head = 0; head < capacity; ++head)
        {
            RingBufferStorage storage = new(capacity);
            try
            {
                if (head > 0)
                {
                    storage.Write(pad, head);
                    storage.Advance(head);
                }

                fixed (byte* src = pattern)
                {
                    switch (size)
                    {
                        case 2:
                            storage.WriteLE2(src);
                            break;
                        case 4:
                            storage.WriteLE4(src);
                            break;
                        case 8:
                            storage.WriteLE8(src);
                            break;
                    }
                }

                switch (size)
                {
                    case 2:
                        storage.ReadBE2(raw);
                        break;
                    case 4:
                        storage.ReadBE4(raw);
                        break;
                    case 8:
                        storage.ReadBE8(raw);
                        break;
                }

                for (int i = 0; i < size; ++i)
                {
                    Assert.Equal(pattern[size - 1 - i], raw[i]);
                }
            }
            finally
            {
                storage.Close();
            }
        }

        for (int head = 0; head < capacity; ++head)
        {
            RingBufferStorage storage = new(capacity);
            try
            {
                if (head > 0)
                {
                    storage.Write(pad, head);
                    storage.Advance(head);
                }

                fixed (byte* src = pattern)
                {
                    switch (size)
                    {
                        case 2:
                            storage.WriteLE2(src);
                            break;
                        case 4:
                            storage.WriteLE4(src);
                            break;
                        case 8:
                            storage.WriteLE8(src);
                            break;
                    }
                }

                switch (size)
                {
                    case 2:
                        storage.ReadLE2(dst);
                        break;
                    case 4:
                        storage.ReadLE4(dst);
                        break;
                    case 8:
                        storage.ReadLE8(dst);
                        break;
                }

                for (int i = 0; i < size; ++i)
                {
                    Assert.Equal(pattern[i], dst[i]);
                }
            }
            finally
            {
                storage.Close();
            }
        }
    }

    [Fact]
    public void ScalarIntThroughRingBufferMatchesHostOnNativeEndian()
    {
        using var ring = DotBase.Buffers.Integral.IntegralRingBuffer.Create(
            64,
            DotBase.Buffers.ByteOrder.Native);

        int[] values = [1, -2, 0x12345678, unchecked((int)0xDEADBEEF)];
        foreach (int v in values)
        {
            Assert.True(ring.TryWrite(v));
        }

        foreach (int expected in values)
        {
            Assert.True(ring.TryRead(out int actual));
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void ScalarIntRoundTripsOnBothExplicitEndians()
    {
        foreach (DotBase.Buffers.ByteOrder order in new[]
                 {
                     DotBase.Buffers.ByteOrder.LittleEndian,
                     DotBase.Buffers.ByteOrder.BigEndian,
                 })
        {
            using var ring = DotBase.Buffers.Integral.IntegralRingBuffer.Create(
                128,
                order);

            int[] values = [0, 1, -1, 0x01020304, unchecked((int)0xFFEEDDCC)];
            foreach (int v in values)
            {
                ring.Write(v);
            }

            foreach (int expected in values)
            {
                Assert.Equal(expected, ring.Read<int>());
            }
        }
    }

    [Fact]
    public void BulkReverseMatchesScalarWhenEndianRequiresSwap()
    {
        if (!BitConverter.IsLittleEndian)
        {
            return;
        }

        using var ring = DotBase.Buffers.Integral.IntegralRingBuffer.Create(
            256,
            DotBase.Buffers.ByteOrder.BigEndian);

        int[] written = [10, 20, 30, 40, 50];
        Assert.Equal(written.Length, ring.Write<int>(written));

        int[] read = new int[written.Length];
        Assert.Equal(written.Length, ring.Read<int>(read.AsSpan()));
        Assert.Equal(written, read);
    }

    [Fact]
    public void BulkPartialReadAndWriteHonorCapacity()
    {
        using var ring = DotBase.Buffers.Integral.IntegralRingBuffer.Create(
            12,
            DotBase.Buffers.ByteOrder.Native);

        // Capacity 12 bytes => 3 ints.
        int[] source = [1, 2, 3, 4, 5];
        int written = ring.Write<int>(source);
        Assert.Equal(3, written);
        Assert.Equal(12, ring.StoredBytes);

        int[] dest = new int[5];
        int read = ring.Read<int>(dest.AsSpan());
        Assert.Equal(3, read);
        Assert.Equal(1, dest[0]);
        Assert.Equal(2, dest[1]);
        Assert.Equal(3, dest[2]);
        Assert.Equal(0, ring.StoredBytes);
    }
}
