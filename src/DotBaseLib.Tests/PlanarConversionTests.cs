using DotBase.Buffers;
using DotBase.Integral;
using DotBase.Integral.Conversion;

namespace DotBaseLib.Tests;


public unsafe class PlanarConversionTests
{
    [Fact]
    public void Reader_ExtractsSelectedPlane_UInt8()
    {
        const int planeCapacity = 4;
        const int planeCount = 2;
        const int planeIndex = 1;
        const byte sentinel = 0xA5;
        byte* srcMem = IntegralTestData.AlignedAlloc(planeCount * planeCapacity);
        byte* dstMem = IntegralTestData.AlignedAlloc(planeCapacity);
        try
        {
            // Plane 0: 1,2,3,4  Plane 1: 10,20,30,40
            byte[] flat = [1, 2, 3, 4, 10, 20, 30, 40];
            for (int i = 0; i < flat.Length; i++)
            {
                srcMem[i] = flat[i];
            }

            for (int i = 0; i < planeCapacity; i++)
            {
                dstMem[i] = sentinel;
            }

            IntegralSpan src = IntegralTestData.CreateSpan(
                srcMem,
                planeCount * planeCapacity,
                IntegralType.UInt8,
                ByteOrder.LittleEndian,
                blockCapacity: 1);
            IntegralSpan dst = IntegralTestData.CreateSpan(
                dstMem,
                planeCapacity,
                IntegralType.UInt8,
                ByteOrder.LittleEndian,
                blockCapacity: 1);

            IntegralConversionHandle handle = ConversionHandles.GetPlanar(src.Format, dst.Format);
            PlanarReaderContext? ctx = ConversionHandles.GetPlanarReaderContext(
                handle,
                planeCapacity,
                blockCapacity: 1,
                planeIndex);

            Assert.False(handle.IsNull);
            Assert.NotNull(ctx);
            long n = ctx.Convert(src, dst, count: 100);
            Assert.Equal(planeCapacity, n);
            Assert.Equal(10, dstMem[0]);
            Assert.Equal(20, dstMem[1]);
            Assert.Equal(30, dstMem[2]);
            Assert.Equal(40, dstMem[3]);
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Fact]
    public void Writer_WritesSelectedPlane_PreservesOthers()
    {
        const int planeCapacity = 3;
        const int planeCount = 2;
        const int planeIndex = 0;
        const byte sentinel = 0xA5;
        byte* srcMem = IntegralTestData.AlignedAlloc(planeCapacity);
        byte* dstMem = IntegralTestData.AlignedAlloc(planeCount * planeCapacity);
        try
        {
            srcMem[0] = 11;
            srcMem[1] = 22;
            srcMem[2] = 33;

            for (int i = 0; i < planeCount * planeCapacity; i++)
            {
                dstMem[i] = sentinel;
            }

            IntegralSpan src = IntegralTestData.CreateSpan(
                srcMem, planeCapacity, IntegralType.UInt8, ByteOrder.LittleEndian, 1);
            IntegralSpan dst = IntegralTestData.CreateSpan(
                dstMem, planeCount * planeCapacity, IntegralType.UInt8, ByteOrder.LittleEndian, 1);

            IntegralConversionHandle handle = ConversionHandles.GetPlanar(src.Format, dst.Format);
            PlanarWriterContext? ctx = ConversionHandles.GetPlanarWriterContext(
                handle,
                planeCapacity,
                blockCapacity: 1,
                planeIndex);

            Assert.NotNull(ctx);
            long n = ctx.Convert(src, dst, count: 100);
            Assert.Equal(planeCapacity, n);
            Assert.Equal(11, dstMem[0]);
            Assert.Equal(22, dstMem[1]);
            Assert.Equal(33, dstMem[2]);
            Assert.Equal(sentinel, dstMem[3]);
            Assert.Equal(sentinel, dstMem[4]);
            Assert.Equal(sentinel, dstMem[5]);
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Fact]
    public void Transfer_CopiesPlaneToPlane_PreservesOtherPlanes()
    {
        const int planeCapacity = 3;
        const int planeCount = 2;
        const byte sentinel = 0xA5;
        byte* srcMem = IntegralTestData.AlignedAlloc(planeCount * planeCapacity);
        byte* dstMem = IntegralTestData.AlignedAlloc(planeCount * planeCapacity);
        try
        {
            // Source plane 0: 1,2,3  plane 1: 4,5,6 — transfer plane 1 → dest plane 0
            byte[] flat = [1, 2, 3, 4, 5, 6];
            for (int i = 0; i < flat.Length; i++)
            {
                srcMem[i] = flat[i];
            }

            for (int i = 0; i < planeCount * planeCapacity; i++)
            {
                dstMem[i] = sentinel;
            }

            IntegralSpan src = IntegralTestData.CreateSpan(
                srcMem, planeCount * planeCapacity, IntegralType.UInt8, ByteOrder.LittleEndian, 1);
            IntegralSpan dst = IntegralTestData.CreateSpan(
                dstMem, planeCount * planeCapacity, IntegralType.UInt8, ByteOrder.LittleEndian, 1);

            IntegralConversionHandle handle = ConversionHandles.GetPlanar(src.Format, dst.Format);
            PlanarTransferContext? ctx = ConversionHandles.GetPlanarTransferContext(
                handle,
                planeCapacity,
                blockCapacity: 1,
                inputPlaneIndex: 1,
                outputPlaneIndex: 0);

            Assert.NotNull(ctx);
            long n = ctx.Convert(src, dst, count: 100);
            Assert.Equal(planeCapacity, n);
            Assert.Equal(4, dstMem[0]);
            Assert.Equal(5, dstMem[1]);
            Assert.Equal(6, dstMem[2]);
            Assert.Equal(sentinel, dstMem[3]);
            Assert.Equal(sentinel, dstMem[4]);
            Assert.Equal(sentinel, dstMem[5]);
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Fact]
    public void Reader_ClampsCountToPlaneSize()
    {
        const int planeCapacity = 4;
        byte* srcMem = IntegralTestData.AlignedAlloc(planeCapacity);
        byte* dstMem = IntegralTestData.AlignedAlloc(planeCapacity);
        try
        {
            for (int i = 0; i < planeCapacity; i++)
            {
                srcMem[i] = (byte)(i + 1);
                dstMem[i] = 0xFF;
            }

            IntegralSpan src = IntegralTestData.CreateSpan(
                srcMem, planeCapacity, IntegralType.UInt8, ByteOrder.LittleEndian, 1);
            IntegralSpan dst = IntegralTestData.CreateSpan(
                dstMem, planeCapacity, IntegralType.UInt8, ByteOrder.LittleEndian, 1);

            IntegralConversionHandle handle = ConversionHandles.GetPlanar(src.Format, dst.Format);
            PlanarReaderContext? ctx = ConversionHandles.GetPlanarReaderContext(
                handle,
                planeCapacity,
                blockCapacity: 1,
                inputPlaneIndex: 0);

            Assert.NotNull(ctx);
            long n = ctx.Convert(src, dst, count: 2);
            Assert.Equal(2, n);
            Assert.Equal(1, dstMem[0]);
            Assert.Equal(2, dstMem[1]);
            Assert.Equal(0xFF, dstMem[2]);
            Assert.Equal(0xFF, dstMem[3]);
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Fact]
    public void Reader_ZeroCount_WritesNothing()
    {
        const int planeCapacity = 2;
        byte* srcMem = IntegralTestData.AlignedAlloc(planeCapacity);
        byte* dstMem = IntegralTestData.AlignedAlloc(planeCapacity);
        try
        {
            srcMem[0] = 1;
            srcMem[1] = 2;
            dstMem[0] = 0xFF;
            dstMem[1] = 0xFF;

            IntegralSpan src = IntegralTestData.CreateSpan(
                srcMem, planeCapacity, IntegralType.UInt8, ByteOrder.LittleEndian, 1);
            IntegralSpan dst = IntegralTestData.CreateSpan(
                dstMem, planeCapacity, IntegralType.UInt8, ByteOrder.LittleEndian, 1);

            IntegralConversionHandle handle = ConversionHandles.GetPlanar(src.Format, dst.Format);
            PlanarReaderContext? ctx = ConversionHandles.GetPlanarReaderContext(
                handle,
                planeCapacity,
                blockCapacity: 1,
                inputPlaneIndex: 0);

            Assert.NotNull(ctx);
            long n = ctx.Convert(src, dst, count: 0);
            Assert.Equal(0, n);
            Assert.Equal(0xFF, dstMem[0]);
            Assert.Equal(0xFF, dstMem[1]);
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Fact]
    public void Reader_ConvertsUInt8ToInt16_WithEndian()
    {
        const int planeCapacity = 2;
        const int planeCount = 2;
        const int planeIndex = 1;
        byte* srcMem = IntegralTestData.AlignedAlloc(planeCount * planeCapacity);
        byte* dstMem = IntegralTestData.AlignedAlloc(planeCapacity * sizeof(short));
        try
        {
            // Plane 0: 1,2  Plane 1: 3,4
            srcMem[0] = 1;
            srcMem[1] = 2;
            srcMem[2] = 3;
            srcMem[3] = 4;

            IntegralSpan src = IntegralTestData.CreateSpan(
                srcMem,
                planeCount * planeCapacity,
                IntegralType.UInt8,
                ByteOrder.LittleEndian,
                1);
            IntegralSpan dst = IntegralTestData.CreateSpan(
                dstMem,
                planeCapacity,
                IntegralType.Int16,
                ByteOrder.LittleEndian,
                1);

            IntegralConversionHandle handle = ConversionHandles.GetPlanar(src.Format, dst.Format);
            PlanarReaderContext? ctx = ConversionHandles.GetPlanarReaderContext(
                handle,
                planeCapacity,
                blockCapacity: 1,
                planeIndex);

            Assert.NotNull(ctx);
            long n = ctx.Convert(src, dst, count: 100);
            Assert.Equal(planeCapacity, n);

            short* dstS = (short*)dstMem;
            Assert.Equal(3, dstS[0]);
            Assert.Equal(4, dstS[1]);
        }
        finally
        {
            IntegralTestData.AlignedFree(srcMem);
            IntegralTestData.AlignedFree(dstMem);
        }
    }

    [Fact]
    public void Contexts_ExposeLayoutProperties()
    {
        IntegralFormat fmt = IntegralFormat.For<byte>();
        IntegralConversionHandle handle = ConversionHandles.GetPlanar(fmt, fmt);
        Assert.False(handle.IsNull);

        PlanarReaderContext? reader =
            ConversionHandles.GetPlanarReaderContext(handle, 8, 2, 1);
        PlanarWriterContext? writer =
            ConversionHandles.GetPlanarWriterContext(handle, 8, 2, 0);
        PlanarTransferContext? transfer =
            ConversionHandles.GetPlanarTransferContext(handle, 8, 2, 0, 1);

        Assert.NotNull(reader);
        Assert.NotNull(writer);
        Assert.NotNull(transfer);
        Assert.Equal(8L, reader.PlaneCapacity);
        Assert.Equal(2, reader.BlockCapacity);
        Assert.Equal(1, reader.PlaneIndex);
        Assert.Equal(8L, writer.PlaneCapacity);
        Assert.Equal(0, writer.PlaneIndex);
        Assert.Equal(0, transfer.InputPlaneIndex);
        Assert.Equal(1, transfer.OutputPlaneIndex);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    public void Contexts_RejectNonPositivePlaneCapacity(long planeCapacity)
    {
        IntegralFormat format = IntegralFormat.For<byte>();
        IntegralConversionHandle handle = ConversionHandles.GetPlanar(format, format);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PlanarReaderContext(handle, planeCapacity, 1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PlanarWriterContext(handle, planeCapacity, 1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PlanarTransferContext(handle, planeCapacity, 1, 0, 0));
    }

    [Fact]
    public void Contexts_AcceptAnyPositiveLongPlaneCapacity()
    {
        IntegralFormat format = IntegralFormat.For<byte>();
        IntegralConversionHandle handle = ConversionHandles.GetPlanar(format, format);
        const long largePlaneCapacity = (long)int.MaxValue + 1;

        PlanarReaderContext single = new(handle, 1, 1, 0);
        PlanarTransferContext large = new(
            handle,
            largePlaneCapacity,
            1,
            0,
            0);

        Assert.Equal(1L, single.PlaneCapacity);
        Assert.Equal(largePlaneCapacity, large.PlaneCapacity);
    }

    [Fact]
    public void Reader_RequiresBlockCompleteInputPlane()
    {
        byte* memory = stackalloc byte[16];
        IntegralSpan incompleteInput = IntegralTestData.CreateSpan(
            memory,
            8,
            IntegralType.UInt8,
            blockCapacity: 3);
        IntegralSpan completeInput = IntegralTestData.CreateSpan(
            memory,
            8,
            IntegralType.UInt8,
            blockCapacity: 2);
        IntegralSpan serialOutput = IntegralTestData.CreateSpan(
            memory + 8,
            4,
            IntegralType.UInt8,
            blockCapacity: 1);
        IntegralConversionHandle handle = ConversionHandles.GetPlanar(
            completeInput.Format,
            serialOutput.Format);
        PlanarReaderContext context = new(handle, 4, 1, 0);

        ArgumentException incomplete = Assert.Throws<ArgumentException>(() =>
            context.Convert(incompleteInput, serialOutput, 0));

        Assert.Equal("input", incomplete.ParamName);
    }

    [Fact]
    public void Writer_RequiresBlockCompleteOutputPlane()
    {
        byte* memory = stackalloc byte[16];
        IntegralSpan serialInput = IntegralTestData.CreateSpan(
            memory,
            4,
            IntegralType.UInt8,
            blockCapacity: 1);
        IntegralSpan completeOutput = IntegralTestData.CreateSpan(
            memory + 8,
            8,
            IntegralType.UInt8,
            blockCapacity: 2);
        IntegralSpan incompleteOutput = IntegralTestData.CreateSpan(
            memory + 8,
            8,
            IntegralType.UInt8,
            blockCapacity: 3);
        IntegralConversionHandle handle = ConversionHandles.GetPlanar(
            serialInput.Format,
            completeOutput.Format);
        PlanarWriterContext context = new(handle, 4, 1, 0);

        ArgumentException incomplete = Assert.Throws<ArgumentException>(() =>
            context.Convert(serialInput, incompleteOutput, 0));

        Assert.Equal("output", incomplete.ParamName);
    }

    [Fact]
    public void Transfer_RequiresBlockCompletePlanesForBothFormats()
    {
        byte* memory = stackalloc byte[16];
        IntegralSpan complete = IntegralTestData.CreateSpan(
            memory,
            8,
            IntegralType.UInt8,
            blockCapacity: 2);
        IntegralSpan incompleteInput = IntegralTestData.CreateSpan(
            memory,
            8,
            IntegralType.UInt8,
            blockCapacity: 3);
        IntegralSpan incompleteOutput = IntegralTestData.CreateSpan(
            memory + 8,
            8,
            IntegralType.UInt8,
            blockCapacity: 3);
        IntegralConversionHandle handle = ConversionHandles.GetPlanar(
            complete.Format,
            complete.Format);
        PlanarTransferContext context = new(handle, 4, 1, 0, 0);

        ArgumentException inputError = Assert.Throws<ArgumentException>(() =>
            context.Convert(incompleteInput, complete, 0));
        ArgumentException outputError = Assert.Throws<ArgumentException>(() =>
            context.Convert(complete, incompleteOutput, 0));

        Assert.Equal("input", inputError.ParamName);
        Assert.Equal("output", outputError.ParamName);
    }

    [Fact]
    public void Contexts_AcceptBlockedUnslicedSidesAndBlockCompletePlanes()
    {
        byte* memory = stackalloc byte[20];
        IntegralSpan blockInput = IntegralTestData.CreateSpan(
            memory,
            8,
            IntegralType.UInt8,
            blockCapacity: 2);
        IntegralSpan blockWriterInput = IntegralTestData.CreateSpan(
            memory,
            4,
            IntegralType.UInt8,
            blockCapacity: 2);
        IntegralSpan blockReaderOutput = IntegralTestData.CreateSpan(
            memory + 8,
            4,
            IntegralType.UInt8,
            blockCapacity: 2);
        IntegralSpan blockOutput = IntegralTestData.CreateSpan(
            memory + 12,
            8,
            IntegralType.UInt8,
            blockCapacity: 4);
        IntegralConversionHandle handle = ConversionHandles.GetPlanar(
            blockWriterInput.Format,
            blockReaderOutput.Format);

        PlanarReaderContext reader = new(handle, 4, 2, 0);
        PlanarWriterContext writer = new(handle, 4, 2, 0);
        PlanarTransferContext transfer = new(handle, 4, 2, 0, 0);

        Assert.Equal(0, reader.Convert(blockInput, blockReaderOutput, 0));
        Assert.Equal(0, writer.Convert(blockWriterInput, blockOutput, 0));
        Assert.Equal(0, transfer.Convert(blockInput, blockOutput, 0));
    }

    [Fact]
    public void Reader_ValidatesPlanarGeometryAndDelegatesSliceRangeValidation()
    {
        byte* memory = stackalloc byte[8];
        IntegralSpan emptyInput = IntegralTestData.CreateSpan(
            memory,
            0,
            IntegralType.UInt8,
            blockCapacity: 1);
        IntegralSpan indivisibleInput = IntegralTestData.CreateSpan(
            memory,
            5,
            IntegralType.UInt8,
            blockCapacity: 1);
        IntegralSpan divisibleInput = IntegralTestData.CreateSpan(
            memory,
            4,
            IntegralType.UInt8,
            blockCapacity: 1);
        IntegralSpan output = IntegralTestData.CreateSpan(
            memory + 6,
            2,
            IntegralType.UInt8,
            blockCapacity: 1);
        IntegralConversionHandle handle = ConversionHandles.GetPlanar(
            divisibleInput.Format,
            output.Format);
        PlanarReaderContext firstPlane = new(handle, 2, 1, 0);
        PlanarReaderContext missingPlane = new(handle, 2, 1, 2);

        ArgumentOutOfRangeException empty =
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                firstPlane.Convert(emptyInput, output, 0));
        ArgumentException indivisible = Assert.Throws<ArgumentException>(() =>
            firstPlane.Convert(indivisibleInput, output, 0));
        ArgumentOutOfRangeException index =
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                missingPlane.Convert(divisibleInput, output, 0));

        Assert.Equal("valueCount", empty.ParamName);
        Assert.Equal("input", indivisible.ParamName);
        Assert.Equal("valueCount", index.ParamName);
    }
}
