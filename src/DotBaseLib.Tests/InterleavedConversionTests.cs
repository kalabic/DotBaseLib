using DotBase.Buffers;
using DotBase.Integral;
using DotBase.Integral.Conversion;
using DotBase.Integral.Conversion.Internal.Interleaved;

namespace DotBaseLib.Tests;


public unsafe class InterleavedConversionTests
{
    [Theory]
    [InlineData(IntegralType.Int16, ByteOrder.LittleEndian, ByteOrder.BigEndian)]
    [InlineData(IntegralType.UInt16, ByteOrder.LittleEndian, ByteOrder.BigEndian)]
    [InlineData(IntegralType.Int16, ByteOrder.BigEndian, ByteOrder.LittleEndian)]
    [InlineData(IntegralType.UInt16, ByteOrder.BigEndian, ByteOrder.LittleEndian)]
    public void OppositeEndian16BitReader_GathersSelectedLane(
        IntegralType valueType,
        ByteOrder sourceOrder,
        ByteOrder destinationOrder)
    {
        const int blockCapacity = 3;
        const int blocks = 3;
        const int lane = 1;
        const byte sentinel = 0xA5;
        byte* srcMem = IntegralTestData.AlignedAlloc(blocks * blockCapacity * sizeof(ushort));
        byte* dstMem = IntegralTestData.AlignedAlloc(blocks * sizeof(ushort));
        try
        {
            for (int i = 0; i < blocks * blockCapacity * sizeof(ushort); ++i)
            {
                srcMem[i] = sentinel;
            }

            byte[] firstBytes = [0x12, 0x56, 0x9A];
            byte[] secondBytes = [0x34, 0x78, 0xBC];
            for (int i = 0; i < blocks; ++i)
            {
                SetRaw16(srcMem, i * blockCapacity + lane, firstBytes[i], secondBytes[i]);
                SetRaw16(dstMem, i, sentinel, sentinel);
            }

            IntegralSpan src = IntegralTestData.CreateSpan(
                srcMem, blocks * blockCapacity, valueType, sourceOrder, blockCapacity);
            IntegralSpan dst = IntegralTestData.CreateSpan(
                dstMem, blocks, valueType, destinationOrder, blockCapacity: 1);

            IntegralConversionHandle handle = ConversionHandles.GetInterleavedReader(src.Format, dst.Format);
            InterleavedReaderContext ctx = ConversionHandles.GetInterleavedReaderContext(
                handle,
                blockCapacity,
                lane);

            Assert.False(handle.IsNull);
            long n = handle.Convert(src, dst, count: 100, ctx);

            Assert.Equal(blocks, n);
            for (int i = 0; i < blocks; ++i)
            {
                AssertRaw16(dstMem, i, secondBytes[i], firstBytes[i]);

                for (int sourceLane = 0; sourceLane < blockCapacity; ++sourceLane)
                {
                    if (sourceLane == lane)
                    {
                        AssertRaw16(
                            srcMem,
                            i * blockCapacity + sourceLane,
                            firstBytes[i],
                            secondBytes[i]);
                    }
                    else
                    {
                        AssertRaw16(srcMem, i * blockCapacity + sourceLane, sentinel, sentinel);
                    }
                }
            }
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Theory]
    [InlineData(IntegralType.Int16, ByteOrder.LittleEndian, ByteOrder.BigEndian)]
    [InlineData(IntegralType.UInt16, ByteOrder.LittleEndian, ByteOrder.BigEndian)]
    [InlineData(IntegralType.Int16, ByteOrder.BigEndian, ByteOrder.LittleEndian)]
    [InlineData(IntegralType.UInt16, ByteOrder.BigEndian, ByteOrder.LittleEndian)]
    public void OppositeEndian16BitWriter_ScattersSelectedLane_AndPreservesSentinels(
        IntegralType valueType,
        ByteOrder sourceOrder,
        ByteOrder destinationOrder)
    {
        const int blockCapacity = 3;
        const int blocks = 3;
        const int lane = 1;
        const byte sentinel = 0xA5;
        byte* srcMem = IntegralTestData.AlignedAlloc(blocks * sizeof(ushort));
        byte* dstMem = IntegralTestData.AlignedAlloc(blocks * blockCapacity * sizeof(ushort));
        try
        {
            byte[] firstBytes = [0x12, 0x56, 0x9A];
            byte[] secondBytes = [0x34, 0x78, 0xBC];
            for (int i = 0; i < blocks; ++i)
            {
                SetRaw16(srcMem, i, firstBytes[i], secondBytes[i]);
            }

            for (int i = 0; i < blocks * blockCapacity * sizeof(ushort); ++i)
            {
                dstMem[i] = sentinel;
            }

            IntegralSpan src = IntegralTestData.CreateSpan(
                srcMem, blocks, valueType, sourceOrder, blockCapacity: 1);
            IntegralSpan dst = IntegralTestData.CreateSpan(
                dstMem, blocks * blockCapacity, valueType, destinationOrder, blockCapacity);

            IntegralConversionHandle handle = ConversionHandles.GetInterleavedWriter(src.Format, dst.Format);
            InterleavedWriterContext ctx = ConversionHandles.GetInterleavedWriterContext(
                handle,
                blockCapacity,
                lane);

            Assert.False(handle.IsNull);
            long n = handle.Convert(src, dst, count: 100, ctx);

            Assert.Equal(blocks, n);
            for (int i = 0; i < blocks; ++i)
            {
                for (int destinationLane = 0; destinationLane < blockCapacity; ++destinationLane)
                {
                    if (destinationLane == lane)
                    {
                        AssertRaw16(
                            dstMem,
                            i * blockCapacity + destinationLane,
                            secondBytes[i],
                            firstBytes[i]);
                    }
                    else
                    {
                        AssertRaw16(dstMem, i * blockCapacity + destinationLane, sentinel, sentinel);
                    }
                }
            }
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Fact]
    public void Reader_GathersLaneFromEachInputBlock_UInt8()
    {
        const int blockCapacity = 2;
        const int blocks = 3;
        const int index = 1;
        byte* srcMem = IntegralTestData.AlignedAlloc(blocks * blockCapacity);
        byte* dstMem = IntegralTestData.AlignedAlloc(blocks);
        try
        {
            byte[] flat = [10, 20, 30, 40, 50, 60];
            for (int i = 0; i < flat.Length; i++)
            {
                srcMem[i] = flat[i];
            }

            for (int i = 0; i < blocks; i++)
            {
                dstMem[i] = 0xFF;
            }

            IntegralSpan src = IntegralTestData.CreateSpan(
                srcMem, blocks * blockCapacity, IntegralType.UInt8, ByteOrder.LittleEndian, blockCapacity);
            IntegralSpan dst = IntegralTestData.CreateSpan(
                dstMem, blocks, IntegralType.UInt8, ByteOrder.LittleEndian, blockCapacity: 1);

            IntegralConversionHandle handle = ConversionHandles.GetInterleavedReader(
                src.Format,
                dst.Format);
            InterleavedReaderContext ctx = ConversionHandles.GetInterleavedReaderContext(
                handle,
                blockCapacity,
                index);

            Assert.False(handle.IsNull);
            long n = handle.Convert(src, dst, count: 100, ctx);
            Assert.Equal(blocks, n);
            Assert.Equal(20, dstMem[0]);
            Assert.Equal(40, dstMem[1]);
            Assert.Equal(60, dstMem[2]);
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Fact]
    public void Reader_IgnoresTrailingPartialBlock()
    {
        const int blockCapacity = 3;
        byte* srcMem = IntegralTestData.AlignedAlloc(5);
        byte* dstMem = IntegralTestData.AlignedAlloc(4);
        try
        {
            for (int i = 0; i < 5; i++)
            {
                srcMem[i] = (byte)(i + 1);
            }

            for (int i = 0; i < 4; i++)
            {
                dstMem[i] = 0xFF;
            }

            IntegralSpan src = IntegralTestData.CreateSpan(
                srcMem, 5, IntegralType.UInt8, ByteOrder.LittleEndian, blockCapacity);
            IntegralSpan dst = IntegralTestData.CreateSpan(
                dstMem, 4, IntegralType.UInt8, ByteOrder.LittleEndian, 1);

            IntegralConversionHandle handle = ConversionHandles.GetInterleavedReader(
                src.Format,
                dst.Format);
            InterleavedReaderContext ctx = ConversionHandles.GetInterleavedReaderContext(
                handle,
                blockCapacity,
                index: 0);

            long n = handle.Convert(src, dst, count: 10, ctx);
            Assert.Equal(1, n);
            Assert.Equal(1, dstMem[0]);
            Assert.Equal(0xFF, dstMem[1]);
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Fact]
    public void Writer_ScattersDenseInputIntoOutputLanes_UInt8()
    {
        const int blockCapacity = 2;
        const int blocks = 3;
        const int index = 0;
        byte* srcMem = IntegralTestData.AlignedAlloc(blocks);
        byte* dstMem = IntegralTestData.AlignedAlloc(blocks * blockCapacity);
        try
        {
            srcMem[0] = 1;
            srcMem[1] = 2;
            srcMem[2] = 3;
            for (int i = 0; i < blocks * blockCapacity; i++)
            {
                dstMem[i] = 0;
            }

            IntegralSpan src = IntegralTestData.CreateSpan(
                srcMem, blocks, IntegralType.UInt8, ByteOrder.LittleEndian, 1);
            IntegralSpan dst = IntegralTestData.CreateSpan(
                dstMem, blocks * blockCapacity, IntegralType.UInt8, ByteOrder.LittleEndian, blockCapacity);

            IntegralConversionHandle handle = ConversionHandles.GetInterleavedWriter(
                src.Format,
                dst.Format);
            InterleavedWriterContext ctx = ConversionHandles.GetInterleavedWriterContext(
                handle,
                blockCapacity,
                index);

            Assert.False(handle.IsNull);
            long n = handle.Convert(src, dst, count: 100, ctx);
            Assert.Equal(blocks, n);
            Assert.Equal(1, dstMem[0]);
            Assert.Equal(0, dstMem[1]);
            Assert.Equal(2, dstMem[2]);
            Assert.Equal(0, dstMem[3]);
            Assert.Equal(3, dstMem[4]);
            Assert.Equal(0, dstMem[5]);
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Fact]
    public void Reader_ConvertsType_UInt8ToInt32()
    {
        const int blockCapacity = 2;
        const int blocks = 2;
        byte* srcMem = IntegralTestData.AlignedAlloc(blocks * blockCapacity);
        byte* dstMem = IntegralTestData.AlignedAlloc(blocks * sizeof(int));
        try
        {
            srcMem[0] = 1;
            srcMem[1] = 2;
            srcMem[2] = 3;
            srcMem[3] = 4;

            IntegralSpan src = IntegralTestData.CreateSpan(
                srcMem, blocks * blockCapacity, IntegralType.UInt8, ByteOrder.LittleEndian, blockCapacity);
            IntegralSpan dst = IntegralTestData.CreateSpan(
                dstMem, blocks, IntegralType.Int32, ByteOrder.LittleEndian, 1);

            IntegralConversionHandle handle = ConversionHandles.GetInterleavedReader(
                src.Format,
                dst.Format);
            InterleavedReaderContext ctx = ConversionHandles.GetInterleavedReaderContext(
                handle,
                blockCapacity,
                index: 0);

            long n = handle.Convert(src, dst, count: 2, ctx);
            Assert.Equal(2, n);

            int* dstI = (int*)dstMem;
            Assert.Equal(1, dstI[0]);
            Assert.Equal(3, dstI[1]);
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Fact]
    public void Writer_ConvertsType_Int32ToUInt8()
    {
        const int blockCapacity = 2;
        const int blocks = 2;
        int* srcMem = (int*)IntegralTestData.AlignedAlloc(blocks * sizeof(int));
        byte* dstMem = IntegralTestData.AlignedAlloc(blocks * blockCapacity);
        try
        {
            srcMem[0] = 7;
            srcMem[1] = 9;
            for (int i = 0; i < blocks * blockCapacity; i++)
            {
                dstMem[i] = 0;
            }

            IntegralSpan src = IntegralTestData.CreateSpan(
                (byte*)srcMem, blocks, IntegralType.Int32, ByteOrder.LittleEndian, 1);
            IntegralSpan dst = IntegralTestData.CreateSpan(
                dstMem, blocks * blockCapacity, IntegralType.UInt8, ByteOrder.LittleEndian, blockCapacity);

            IntegralConversionHandle handle = ConversionHandles.GetInterleavedWriter(
                src.Format,
                dst.Format);
            InterleavedWriterContext ctx = ConversionHandles.GetInterleavedWriterContext(
                handle,
                blockCapacity,
                index: 1);

            long n = handle.Convert(src, dst, count: 2, ctx);
            Assert.Equal(2, n);
            Assert.Equal(0, dstMem[0]);
            Assert.Equal(7, dstMem[1]);
            Assert.Equal(0, dstMem[2]);
            Assert.Equal(9, dstMem[3]);
        }
        finally
        {
            IntegralTestData.AlignedFree((byte*)srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Fact]
    public void Table_BuildsAndHandlesResolve()
    {
        var table = InterleavedDelegateTable.Instance;
        Assert.NotNull(table);

        IntegralFormat fmt = IntegralFormat.For<byte>();
        IntegralConversionHandle handle = table.GetDefaultHandle(fmt, fmt);
        Assert.False(handle.IsNull);

        InterleavedReaderContext readerCtx =
            ConversionHandles.GetInterleavedReaderContext(handle, 2, 0);
        InterleavedWriterContext writerCtx =
            ConversionHandles.GetInterleavedWriterContext(handle, 2, 1);
        Assert.Equal(2, readerCtx.InputBlockCapacity);
        Assert.Equal(0, readerCtx.ValueIndex);
        Assert.Equal(2, writerCtx.OutputBlockCapacity);
        Assert.Equal(1, writerCtx.ValueIndex);
    }

    private static void SetRaw16(byte* memory, int index, byte first, byte second)
    {
        memory[index * sizeof(ushort)] = first;
        memory[index * sizeof(ushort) + 1] = second;
    }

    private static void AssertRaw16(byte* memory, int index, byte expectedFirst, byte expectedSecond)
    {
        Assert.Equal(expectedFirst, memory[index * sizeof(ushort)]);
        Assert.Equal(expectedSecond, memory[index * sizeof(ushort) + 1]);
    }
}
