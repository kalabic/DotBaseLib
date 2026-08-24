using DotBase.Buffers;
using DotBase.Buffers.Integral;
using DotBase.Integral;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DotBaseLib.Tests;


/// <summary>
/// Hard edge cases across unlocked / locked / waitable integral rings.
/// Focus: wrap seams, IntegralSpan endian paths, partial/atomic semantics, locked path.
/// </summary>
public class IntegralRingBufferEdgeTests
{
    public enum RingKind
    {
        Unlocked,
        Locked,
        Waitable,
    }

    private static ByteOrder Foreign =>
        BitConverter.IsLittleEndian
            ? ByteOrder.BigEndian
            : ByteOrder.LittleEndian;

    private static ByteOrder Native =>
        BitConverter.IsLittleEndian
            ? ByteOrder.LittleEndian
            : ByteOrder.BigEndian;

    private static IIntegralRingBuffer Create(RingKind kind, int capacityBytes, ByteOrder order)
    {
        return kind switch
        {
            RingKind.Unlocked => IntegralRingBuffer.CreateUnlocked(capacityBytes, order),
            RingKind.Locked => IntegralRingBuffer.CreateLocked(capacityBytes, order),
            RingKind.Waitable => IntegralRingBuffer.CreateWaitable(capacityBytes, order),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }

    /// <summary>
    /// Limits the request to the number of complete values currently stored.
    /// </summary>
    private static int ReadAvailable<T>(IIntegralRingBuffer ring, Span<T> destination)
        where T : unmanaged
    {
        int take = Math.Min(destination.Length, ring.StoredCount<T>());
        if (take == 0)
        {
            return 0;
        }

        return ring.Read(destination[..take]);
    }

    public static TheoryData<RingKind, ByteOrder> AllKindsBothEndians()
    {
        TheoryData<RingKind, ByteOrder> data = [];
        foreach (RingKind kind in Enum.GetValues<RingKind>())
        {
            data.Add(kind, ByteOrder.LittleEndian);
            data.Add(kind, ByteOrder.BigEndian);
        }

        return data;
    }

    public static TheoryData<RingKind> AllKinds()
    {
        TheoryData<RingKind> data = [];
        foreach (RingKind kind in Enum.GetValues<RingKind>())
        {
            data.Add(kind);
        }

        return data;
    }

    // -------------------------------------------------------------------------
    // IntegralSpan round-trip: host-native span vs foreign ring (must swap)
    // -------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(AllKindsBothEndians))]
    public unsafe void CheckedSpan_CapacityIsOperationalNotStructural(
        RingKind kind,
        ByteOrder ringOrder)
    {
        int[] source = [11, 22, 33];
        int[] destination = [-1, -1, -1];

        fixed (int* sourcePtr = source)
        fixed (int* destinationPtr = destination)
        {
            IntegralSpan sourceSpan = IntegralTestData.CreateSpan(
                (byte*)sourcePtr,
                source.Length,
                IntegralType.Int32,
                ByteOrder.Native);
            IntegralSpan destinationSpan = IntegralTestData.CreateSpan(
                (byte*)destinationPtr,
                destination.Length,
                IntegralType.Int32,
                ByteOrder.Native);

            using (IIntegralRingBuffer ring = Create(
                kind,
                2 * sizeof(int),
                ringOrder))
            {
                Assert.Equal(2, ring.WriteChecked(sourceSpan));
                Assert.Equal(2, ring.ReadChecked(destinationSpan));
                Assert.Equal([11, 22, -1], destination);
            }

            using (IIntegralRingBuffer ring = Create(
                kind,
                2 * sizeof(int),
                ringOrder))
            {
                Assert.False(ring.TryWriteChecked(sourceSpan));
                Assert.Equal(0, ring.StoredBytes);

                Assert.Equal(2, ring.Write((ReadOnlySpan<int>)source.AsSpan(0, 2)));
                Assert.False(ring.TryReadChecked(destinationSpan));
                Assert.Equal(2 * sizeof(int), ring.StoredBytes);
            }
        }
    }

    [Theory]
    [MemberData(nameof(AllKindsBothEndians))]
    public unsafe void SpanRoundTrip_NativeSpan_MatchesTypedPath(RingKind kind, ByteOrder ringOrder)
    {
        const int n = 7;
        int[] values = [1, -2, 3, int.MinValue, int.MaxValue, 0x01020304, unchecked((int)0xAABBCCDD)];
        int[] viaSpan = new int[n];
        int[] viaTyped = new int[n];

        using (IIntegralRingBuffer ring = Create(kind, n * sizeof(int), ringOrder))
        {
            fixed (int* p = values)
            {
                IntegralSpan src = IntegralTestData.CreateSpan(
                    (byte*)p,
                    n,
                    IntegralType.Int32,
                    ByteOrder.Native);
                Assert.Equal(n, ring.Write(src));
            }

            fixed (int* p = viaSpan)
            {
                IntegralSpan dst = IntegralTestData.CreateSpan(
                    (byte*)p,
                    n,
                    IntegralType.Int32,
                    ByteOrder.Native);
                Assert.Equal(n, ring.Read(dst));
            }
        }

        using (IIntegralRingBuffer ring = Create(kind, n * sizeof(int), ringOrder))
        {
            Assert.Equal(n, ring.Write((ReadOnlySpan<int>)values.AsSpan(0, n)));
            Assert.Equal(n, ring.Read(viaTyped.AsSpan()));
        }

        Assert.Equal(viaTyped, viaSpan);
        Assert.Equal(values.AsSpan(0, n).ToArray(), viaSpan);
    }

    [Theory]
    [MemberData(nameof(AllKindsBothEndians))]
    public unsafe void SpanRoundTrip_WireMatchingRing_DoesNotDoubleSwap(RingKind kind, ByteOrder ringOrder)
    {
        // Span declared in the same wire order as the ring: raw lane layout, no reverse.
        const int n = 5;
        int[] hostValues = [0x11223344, 0x55667788, -1, 0, 0x0F0E0D0C];
        int[] readBack = new int[n];

        using IIntegralRingBuffer ring = Create(kind, n * sizeof(int), ringOrder);

        // Materialize wire bytes as the ring expects (host → ring wire).
        byte* wire = IntegralTestData.AlignedAlloc(n * sizeof(int));
        byte* hostScratch = IntegralTestData.AlignedAlloc(n * sizeof(int));
        try
        {
            for (int i = 0; i < n; ++i)
            {
                Unsafe.WriteUnaligned(hostScratch + i * 4, hostValues[i]);
            }

            IntegralSpan hostView = IntegralTestData.CreateSpan(
                hostScratch,
                n,
                IntegralType.Int32,
                ByteOrder.Native);
            IntegralSpan wireView = IntegralTestData.CreateSpan(
                wire,
                n,
                IntegralType.Int32,
                ringOrder);

            if (hostView.IsEqual(ringOrder))
            {
                IntegralMemory.Copy(hostView, wireView, n);
            }
            else
            {
                IntegralMemory.ReverseCopy(hostView, wireView, n);
            }

            Assert.Equal(n, ring.Write(wireView));

            fixed (int* p = readBack)
            {
                IntegralSpan dst = IntegralTestData.CreateSpan(
                    (byte*)p,
                    n,
                    IntegralType.Int32,
                    ByteOrder.Native);
                Assert.Equal(n, ring.Read(dst));
            }

            Assert.Equal(hostValues, readBack);
        }
        finally
        {
            IntegralTestData.AlignedFree(wire);
            IntegralTestData.AlignedFree(hostScratch);
        }
    }

    // -------------------------------------------------------------------------
    // Wrap: two physical segments, FIFO order must be preserved
    // -------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(AllKindsBothEndians))]
    public unsafe void WrapAround_SpanReadPreservesFifoOrder(RingKind kind, ByteOrder ringOrder)
    {
        // capacity 5 ints; write 5, read 3 → free at head; write 3 more → wrap; read 5.
        const int capacity = 5;
        int[] firstWrite = [10, 20, 30, 40, 50];
        int[] secondWrite = [60, 70, 80];
        int[] expectedTail = [40, 50, 60, 70, 80];

        using IIntegralRingBuffer ring = Create(
            kind,
            capacity * sizeof(int),
            ringOrder);

        Assert.Equal(5, ring.Write((ReadOnlySpan<int>)firstWrite));
        int[] discard = new int[3];
        Assert.Equal(3, ring.Read(discard.AsSpan()));
        Assert.Equal(new[] { 10, 20, 30 }, discard);

        Assert.Equal(3, ring.Write((ReadOnlySpan<int>)secondWrite));
        Assert.Equal(5 * sizeof(int), ring.StoredBytes);

        int[] got = new int[5];
        fixed (int* p = got)
        {
            IntegralSpan dst = IntegralTestData.CreateSpan(
                (byte*)p,
                5,
                IntegralType.Int32,
                ByteOrder.Native);
            Assert.Equal(5, ring.Read(dst));
        }

        Assert.Equal(expectedTail, got);
        Assert.Equal(0, ring.StoredBytes);
    }

    [Theory]
    [MemberData(nameof(AllKindsBothEndians))]
    public void WrapAround_MultiByteValueStraddlesSeam_StillRoundTrips(RingKind kind, ByteOrder ringOrder)
    {
        // Capacity 6 bytes. Write 1 byte then a 4-byte int (straddles if write head near end).
        // Force seam: fill 5 bytes, read 4 → 1 stored at end-ish... simpler:
        // capacity 8, write 6 bytes of payload as uint16 pairs so heads land mid-buffer,
        // then write int that starts 2 bytes before wrap.
        using IIntegralRingBuffer ring = Create(kind, 8, ringOrder);

        // Fill 6 bytes
        byte[] six = [1, 2, 3, 4, 5, 6];
        Assert.Equal(6, ring.Write(six, 0, 6));
        // Drain 4 → 2 stored, write head at 6, free = 6 (2 at end + 4 at start)
        byte[] drain = new byte[4];
        Assert.Equal(4, ring.Read(drain, 0, 4));
        Assert.Equal(2, ring.StoredBytes);
        Assert.Equal(6, ring.FreeBytes);

        // Write one int (4 bytes) starting at offset 6 → 2 bytes at end + 2 at start (tear).
        int magic = unchecked((int)0xA1B2C3D4);
        Assert.True(ring.TryWrite(magic));
        Assert.Equal(6, ring.StoredBytes);

        // Drain leftover 2 bytes from the original fill
        byte[] leftover = new byte[2];
        Assert.Equal(2, ring.Read(leftover, 0, 2));
        Assert.Equal(new byte[] { 5, 6 }, leftover);

        Assert.True(ring.TryRead(out int got));
        Assert.Equal(magic, got);
        Assert.Equal(0, ring.StoredBytes);
    }

    // -------------------------------------------------------------------------
    // Partial / atomic
    // -------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(AllKinds))]
    public unsafe void PartialWrite_ThenPartialRead_SpanAndTypedAgree(RingKind kind)
    {
        using IIntegralRingBuffer ring = Create(kind, 3 * sizeof(int), Foreign);

        int[] src = [11, 22, 33, 44];
        // Only 3 fit
        Assert.Equal(3, ring.Write((ReadOnlySpan<int>)src.AsSpan(0, 4)));
        Assert.False(ring.TryWrite((ReadOnlySpan<int>)src.AsSpan(0, 1)));

        int[] a = new int[4];
        Assert.Equal(3, ReadAvailable(ring, a.AsSpan()));
        Assert.Equal(new[] { 11, 22, 33, 0 }, a);

        Assert.Equal(2, ring.Write((ReadOnlySpan<int>)src.AsSpan(1, 2))); // 22, 33
        fixed (int* p = src)
        {
            IntegralSpan one = IntegralTestData.CreateSpan(
                (byte*)(p + 3),
                1,
                IntegralType.Int32,
                ByteOrder.Native);
            Assert.True(ring.TryWrite(one));
        }

        int[] b = new int[3];
        fixed (int* p = b)
        {
            IntegralSpan dst = IntegralTestData.CreateSpan(
                (byte*)p,
                3,
                IntegralType.Int32,
                ByteOrder.Native);
            Assert.Equal(3, ring.Read(dst));
        }

        Assert.Equal(new[] { 22, 33, 44 }, b);
    }

    [Theory]
    [MemberData(nameof(AllKindsBothEndians))]
    public unsafe void TryReadSpan_InsufficientData_IsAtomic(RingKind kind, ByteOrder ringOrder)
    {
        using IIntegralRingBuffer ring = Create(kind, 4 * sizeof(int), ringOrder);
        Assert.True(ring.TryWrite(0x11111111));
        Assert.True(ring.TryWrite(0x22222222));

        int[] poison = [9, 9, 9, 9];
        fixed (int* p = poison)
        {
            IntegralSpan dst = IntegralTestData.CreateSpan(
                (byte*)p,
                4,
                IntegralType.Int32,
                ByteOrder.Native);
            Assert.False(ring.TryReadChecked(dst));
        }

        Assert.Equal(new[] { 9, 9, 9, 9 }, poison);
        Assert.Equal(2 * sizeof(int), ring.StoredBytes);

        Assert.True(ring.TryRead(out int a));
        Assert.True(ring.TryRead(out int b));
        Assert.Equal(0x11111111, a);
        Assert.Equal(0x22222222, b);
    }

    [Theory]
    [MemberData(nameof(AllKindsBothEndians))]
    public unsafe void TryWriteSpan_InsufficientSpace_IsAtomic(RingKind kind, ByteOrder ringOrder)
    {
        // Span longer than *capacity* is rejected on Checked (throws). Atomic false is for
        // span ≤ capacity that does not fit in free space.
        using IIntegralRingBuffer ring = Create(kind, 4 * sizeof(int), ringOrder);
        Assert.True(ring.TryWrite(1));
        Assert.True(ring.TryWrite(2));
        Assert.True(ring.TryWrite(3));
        // free = 1 int; request 2
        int[] two = [9, 9];
        fixed (int* p = two)
        {
            IntegralSpan src = IntegralTestData.CreateSpan(
                (byte*)p,
                2,
                IntegralType.Int32,
                ByteOrder.Native);
            Assert.False(ring.TryWriteChecked(src));
        }

        Assert.Equal(3 * sizeof(int), ring.StoredBytes);
        Assert.True(ring.TryRead(out int a));
        Assert.Equal(1, a);
    }

    // -------------------------------------------------------------------------
    // Odd sizes, close, empty, clear
    // -------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(AllKinds))]
    public void IncompleteTrailingBytes_BlockTypedReadButNotByteRead(RingKind kind)
    {
        using IIntegralRingBuffer ring = Create(kind, 16, Foreign);
        byte[] three = [0xAA, 0xBB, 0xCC];
        Assert.Equal(3, ring.Write(three, 0, 3));

        int[] one = new int[1];
        Assert.Equal(0, ReadAvailable(ring, one.AsSpan()));
        Assert.Equal(3, ring.StoredBytes);

        byte[] back = new byte[3];
        Assert.Equal(3, ring.Read(back, 0, 3));
        Assert.Equal(three, back);
    }

    [Theory]
    [MemberData(nameof(AllKinds))]
    public void Close_MakesFurtherTryWriteFail_AndDrainsStop(RingKind kind)
    {
        using IIntegralRingBuffer ring = Create(kind, 32, Native);
        Assert.True(ring.TryWrite(1));
        ring.Close();
        Assert.False(ring.IsOpen);
        Assert.False(ring.TryWrite(2));
        // Implementation may clear on close — accept empty or residual but no new writes.
        Assert.False(ring.TryWrite(3));
    }

    [Theory]
    [MemberData(nameof(AllKinds))]
    public void ClearBuffer_DropsStoredWithoutClosing(RingKind kind)
    {
        using IIntegralRingBuffer ring = Create(kind, 32, Foreign);
        Assert.True(ring.TryWrite(7));
        Assert.True(ring.TryWrite(8));
        ring.ClearBuffer();
        Assert.True(ring.IsOpen);
        Assert.Equal(0, ring.StoredBytes);
        Assert.Equal(ring.ByteCapacity, ring.FreeBytes);
        Assert.True(ring.TryWrite(9));
        Assert.True(ring.TryRead(out int v));
        Assert.Equal(9, v);
    }

    [Theory]
    [MemberData(nameof(AllKindsBothEndians))]
    public void ZeroCapacity_IsClosedEmpty(RingKind kind, ByteOrder ringOrder)
    {
        using IIntegralRingBuffer ring = Create(kind, 0, ringOrder);
        Assert.Equal(0, ring.ByteCapacity);
        Assert.False(ring.IsOpen);
        Assert.False(ring.TryWrite(1));
        Assert.False(ring.TryRead(out int _));
    }

    // -------------------------------------------------------------------------
    // Mixed APIs + advance
    // -------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(AllKindsBothEndians))]
    public unsafe void AdvanceBy_SkipsValues_ThenSpanReadContinues(RingKind kind, ByteOrder ringOrder)
    {
        using IIntegralRingBuffer ring = Create(kind, 6 * sizeof(long), ringOrder);
        long[] vals = [1, 2, 3, 4, 5, 6];
        Assert.Equal(6, ring.Write((ReadOnlySpan<long>)vals));

        ring.AdvanceBy<long>(2);
        Assert.Equal(4 * sizeof(long), ring.StoredBytes);

        long[] got = new long[4];
        fixed (long* p = got)
        {
            IntegralSpan dst = IntegralTestData.CreateSpan(
                (byte*)p,
                4,
                IntegralType.Int64,
                ByteOrder.Native);
            Assert.Equal(4, ring.Read(dst));
        }

        Assert.Equal(new long[] { 3, 4, 5, 6 }, got);
    }

    [Theory]
    [MemberData(nameof(AllKindsBothEndians))]
    public unsafe void InterleavedScalarBulkAndSpan_StayConsistent(RingKind kind, ByteOrder ringOrder)
    {
        using IIntegralRingBuffer ring = Create(kind, 16 * sizeof(int), ringOrder);

        Assert.True(ring.Write(100));
        Assert.Equal(2, ring.Write((ReadOnlySpan<int>)new[] { 200, 300 }));

        int[] chunk = [400, 500, 600];
        fixed (int* p = chunk)
        {
            Assert.Equal(
                3,
                ring.Write(IntegralTestData.CreateSpan(
                    (byte*)p,
                    3,
                    IntegralType.Int32,
                    ByteOrder.Native)));
        }

        Assert.True(ring.TryRead(out int a));
        Assert.Equal(100, a);

        int[] mid = new int[2];
        Assert.Equal(2, ring.Read(mid.AsSpan()));
        Assert.Equal(new[] { 200, 300 }, mid);

        int[] rest = new int[3];
        fixed (int* p = rest)
        {
            Assert.Equal(
                3,
                ring.Read(IntegralTestData.CreateSpan(
                    (byte*)p,
                    3,
                    IntegralType.Int32,
                    ByteOrder.Native)));
        }

        Assert.Equal(new[] { 400, 500, 600 }, rest);
        Assert.Equal(0, ring.StoredBytes);
    }

    // -------------------------------------------------------------------------
    // Float bit patterns via span on all kinds
    // -------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(AllKindsBothEndians))]
    public unsafe void Span_FloatPayloads_PreserveBits(RingKind kind, ByteOrder ringOrder)
    {
        float[] values =
        [
            BitConverter.Int32BitsToSingle(unchecked((int)0x80000000)),
            float.PositiveInfinity,
            float.NaN,
            BitConverter.Int32BitsToSingle(unchecked((int)0x7F800001)),
            -0.0f,
        ];

        using IIntegralRingBuffer ring = Create(
            kind,
            values.Length * sizeof(float),
            ringOrder);

        fixed (float* p = values)
        {
            Assert.Equal(
                values.Length,
                ring.Write(IntegralTestData.CreateSpan(
                    (byte*)p,
                    values.Length,
                    IntegralType.Float,
                    ByteOrder.Native)));
        }

        float[] got = new float[values.Length];
        fixed (float* p = got)
        {
            Assert.Equal(
                values.Length,
                ring.Read(IntegralTestData.CreateSpan(
                    (byte*)p,
                    values.Length,
                    IntegralType.Float,
                    ByteOrder.Native)));
        }

        for (int i = 0; i < values.Length; ++i)
        {
            Assert.Equal(
                BitConverter.SingleToInt32Bits(values[i]),
                BitConverter.SingleToInt32Bits(got[i]));
        }
    }

    // -------------------------------------------------------------------------
    // Waitable-specific Exact operations
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Waitable_ReadExact_BlocksUntilFullRequestAvailable()
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            4 * sizeof(int),
            Foreign);

        int[] dest = new int[3];
        using ManualResetEventSlim started = new();

        Task<int> readTask = Task.Run(() =>
        {
            started.Set();
            return ring.ReadExact(dest.AsSpan());
        });

        Assert.True(started.Wait(TimeSpan.FromSeconds(2)));
        await Task.Delay(40);
        Assert.False(readTask.IsCompleted);

        Assert.True(ring.TryWrite(1));
        Assert.True(ring.TryWrite(2));
        await Task.Delay(40);
        Assert.False(readTask.IsCompleted);

        Assert.True(ring.TryWrite(3));
        Assert.Equal(3, await readTask.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.Equal(new[] { 1, 2, 3 }, dest);
    }

    [Fact]
    public async Task Waitable_IntegralSpanReadExact_BlocksUntilFullRequestAvailable()
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            4 * sizeof(int),
            Foreign);

        // Pin destination for the whole blocked read.
        int[] dest = new int[3];
        GCHandle pin = GCHandle.Alloc(dest, GCHandleType.Pinned);
        try
        {
            IntegralSpan dst;
            unsafe
            {
                byte* p = (byte*)pin.AddrOfPinnedObject();
                dst = IntegralTestData.CreateSpan(
                    p,
                    3,
                    IntegralType.Int32,
                    ByteOrder.Native);
            }

            using ManualResetEventSlim started = new();
            Task<int> readTask = Task.Run(() =>
            {
                started.Set();
                return ring.ReadExact(dst);
            });

            Assert.True(started.Wait(TimeSpan.FromSeconds(2)));
            await Task.Delay(40);
            Assert.False(readTask.IsCompleted);

            Assert.True(ring.TryWrite(10));
            Assert.True(ring.TryWrite(20));
            await Task.Delay(40);
            Assert.False(readTask.IsCompleted);

            Assert.True(ring.TryWrite(30));
            Assert.Equal(3, await readTask.WaitAsync(TimeSpan.FromSeconds(5)));
            Assert.Equal(new[] { 10, 20, 30 }, dest);
        }
        finally
        {
            pin.Free();
        }
    }

    [Fact]
    public void Waitable_Read_AfterClose_ReturnsZero()
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            4 * sizeof(int),
            Native);
        Assert.True(ring.TryWrite(1));
        ring.Close();

        byte[] bytes = new byte[4];
        Assert.Equal(0, ring.Read(bytes, 0, bytes.Length));

        int[] dest = new int[1];
        Assert.Equal(0, ring.Read(dest.AsSpan()));
    }

    [Fact]
    public unsafe void Waitable_IntegralSpanRead_AfterClose_ReturnsZero()
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            4 * sizeof(int),
            Native);
        ring.Close();

        int[] dest = new int[1];
        fixed (int* p = dest)
        {
            IntegralSpan dst = IntegralTestData.CreateSpan(
                (byte*)p,
                1,
                IntegralType.Int32,
                ByteOrder.Native);
            Assert.Equal(0, ring.Read(dst));
        }
    }

    [Fact]
    public void Waitable_ScalarRead_AfterClose_ReturnsFalseAndDefault()
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            sizeof(int),
            Native);
        ring.Close();
        int value = 42;
        Assert.False(ring.Read(out value));
        Assert.Equal(default, value);
    }

    [Fact]
    public async Task Waitable_ReadExact_CloseUnblocksWaiter()
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            4 * sizeof(int),
            Native);
        int[] dest = new int[2];
        using ManualResetEventSlim started = new();

        Task<int> readTask = Task.Run(() =>
        {
            started.Set();
            return ring.ReadExact(dest.AsSpan());
        });

        Assert.True(started.Wait(TimeSpan.FromSeconds(2)));
        await Task.Delay(40);
        Assert.False(readTask.IsCompleted);

        ring.Close();
        Assert.Equal(0, await readTask.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task Waitable_WriteExact_WaitsUntilFreeSpace()
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            2 * sizeof(int),
            Foreign);
        Assert.True(ring.TryWrite(1));
        Assert.True(ring.TryWrite(2));

        int[] source = [3, 4];
        using ManualResetEventSlim started = new();
        Task<int> writeTask = Task.Run(() =>
        {
            started.Set();
            return ring.WriteExact((ReadOnlySpan<int>)source);
        });

        Assert.True(started.Wait(TimeSpan.FromSeconds(2)));
        await Task.Delay(40);
        Assert.False(writeTask.IsCompleted);

        int[] drained = new int[2];
        Assert.Equal(2, ring.Read(drained.AsSpan()));
        Assert.Equal(new[] { 1, 2 }, drained);

        Assert.Equal(2, await writeTask.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.Equal(2 * sizeof(int), ring.StoredBytes);
    }

    [Fact]
    public async Task Waitable_IntegralSpanWriteExact_BlocksUntilFreeSpace()
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            2 * sizeof(int),
            Foreign);
        Assert.True(ring.TryWrite(1));
        Assert.True(ring.TryWrite(2));

        int[] source = [7, 8];
        GCHandle pin = GCHandle.Alloc(source, GCHandleType.Pinned);
        try
        {
            IntegralSpan src;
            unsafe
            {
                byte* p = (byte*)pin.AddrOfPinnedObject();
                src = IntegralTestData.CreateSpan(
                    p,
                    2,
                    IntegralType.Int32,
                    ByteOrder.Native);
            }

            using ManualResetEventSlim started = new();
            Task<int> writeTask = Task.Run(() =>
            {
                started.Set();
                return ring.WriteExact(src);
            });

            Assert.True(started.Wait(TimeSpan.FromSeconds(2)));
            await Task.Delay(40);
            Assert.False(writeTask.IsCompleted);

            Assert.Equal(2, ring.Read(new int[2].AsSpan()));
            Assert.Equal(2, await writeTask.WaitAsync(TimeSpan.FromSeconds(5)));
        }
        finally
        {
            pin.Free();
        }
    }

    [Fact]
    public async Task Waitable_WriteExact_CloseUnblocksWaiter()
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            sizeof(int),
            Native);
        Assert.True(ring.TryWrite(1));

        int[] source = [2];
        using ManualResetEventSlim started = new();
        Task<int> writeTask = Task.Run(() =>
        {
            started.Set();
            return ring.WriteExact((ReadOnlySpan<int>)source);
        });

        Assert.True(started.Wait(TimeSpan.FromSeconds(2)));
        await Task.Delay(40);
        Assert.False(writeTask.IsCompleted);

        ring.Close();
        Assert.Equal(0, await writeTask.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public void Waitable_ScalarWrite_AfterClose_ReturnsFalse()
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            sizeof(int),
            Native);
        ring.Close();
        Assert.False(ring.Write(1));
    }

    [Fact]
    public void Waitable_ZeroCapacity_PartialReadWriteReturnZero()
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(0, Native);
        byte[] one = [1];
        Assert.Equal(0, ring.Read(one, 0, 1));
        Assert.Equal(0, ring.Write(one, 0, 1));
    }

    [Fact]
    public void Waitable_ExactRequestLargerThanCapacity_ReturnsZero()
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            sizeof(int),
            Native);
        int[] two = new int[2];
        Assert.Equal(0, ring.ReadExact(two.AsSpan()));
        Assert.Equal(0, ring.WriteExact((ReadOnlySpan<int>)two));
        Assert.Equal(0, ring.StoredBytes);
        Assert.Equal(sizeof(int), ring.FreeBytes);
    }

    [Fact]
    public void Waitable_PartialWriteLargerThanCapacity_WritesWhatFits()
    {
        using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
            3 * sizeof(int),
            Foreign);
        int[] src = [11, 22, 33, 44];
        Assert.Equal(3, ring.Write((ReadOnlySpan<int>)src));
        Assert.Equal(0, ring.Write((ReadOnlySpan<int>)src.AsSpan(0, 3)));
    }

    [Fact]
    public unsafe void Waitable_AllOversizedExactApiFamiliesFailWithoutMutation()
    {
        foreach (ByteOrder ringOrder in new[]
                 {
                     ByteOrder.LittleEndian,
                     ByteOrder.BigEndian,
                 })
        {
            using IWaitableRingBuffer ring = IntegralRingBuffer.CreateWaitable(
                sizeof(int),
                ringOrder);
            byte[] bytes = new byte[sizeof(int) + 1];
            int[] values = [11, 22];

            fixed (byte* bytePtr = bytes)
            fixed (int* valuePtr = values)
            {
                IntegralSpan integralSpan = IntegralTestData.CreateSpan(
                    (byte*)valuePtr,
                    values.Length,
                    IntegralType.Int32,
                    ByteOrder.Native);

                Assert.Equal(0, ring.WriteExact(bytes, 0, bytes.Length));
                Assert.Equal(0, ring.WriteExact(bytePtr, 0, bytes.Length));
                Assert.Equal(0, ring.WriteExact(values, 0, values.Length));
                Assert.Equal(0, ring.WriteExact(valuePtr, 0, values.Length));
                Assert.Equal(0, ring.WriteExact((ReadOnlySpan<int>)values));
                Assert.Equal(0, ring.WriteExact(integralSpan));
                Assert.Equal(0, ring.WriteExactChecked(integralSpan));
                Assert.False(ring.TryWrite(values, 0, values.Length));
                Assert.False(ring.TryWrite(valuePtr, 0, values.Length));
                Assert.False(ring.TryWrite((ReadOnlySpan<int>)values));
                Assert.False(ring.TryWrite(integralSpan));
                Assert.False(ring.TryWriteChecked(integralSpan));
                Assert.Equal(0, ring.StoredBytes);
                Assert.Equal(sizeof(int), ring.FreeBytes);

                Assert.True(ring.Write(123));

                Assert.Equal(0, ring.ReadExact(bytes, 0, bytes.Length));
                Assert.Equal(0, ring.ReadExact(bytePtr, 0, bytes.Length));
                Assert.Equal(0, ring.ReadExact(values, 0, values.Length));
                Assert.Equal(0, ring.ReadExact(valuePtr, 0, values.Length));
                Assert.Equal(0, ring.ReadExact(values.AsSpan()));
                Assert.Equal(0, ring.ReadExact(integralSpan));
                Assert.Equal(0, ring.ReadExactChecked(integralSpan));
                Assert.False(ring.TryRead(values, 0, values.Length));
                Assert.False(ring.TryRead(valuePtr, 0, values.Length));
                Assert.False(ring.TryRead(values.AsSpan()));
                Assert.False(ring.TryRead(integralSpan));
                Assert.False(ring.TryReadChecked(integralSpan));
                Assert.Equal(sizeof(int), ring.StoredBytes);
                Assert.Equal(0, ring.FreeBytes);
            }

            Assert.True(ring.Read(out int value));
            Assert.Equal(123, value);
        }
    }

    // -------------------------------------------------------------------------
    // Locked under concurrent sequential pressure (smoke, not a full race proof)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Locked_ParallelProducersConsumers_NoLostOrDupes()
    {
        const int producers = 4;
        const int perProducer = 250;
        const int total = producers * perProducer;

        using IIntegralRingBuffer ring = IntegralRingBuffer.CreateLocked(
            64 * sizeof(int),
            Native);

        int[] consumed = new int[total];
        int readCount = 0;

        Task[] writers = Enumerable.Range(0, producers).Select(p => Task.Run(() =>
        {
            for (int i = 0; i < perProducer; ++i)
            {
                int value = p * perProducer + i;
                while (!ring.TryWrite(value))
                {
                    Thread.SpinWait(20);
                }
            }
        })).ToArray();

        Task reader = Task.Run(() =>
        {
            while (Volatile.Read(ref readCount) < total)
            {
                if (ring.TryRead(out int v))
                {
                    int idx = Interlocked.Increment(ref readCount) - 1;
                    consumed[idx] = v;
                }
                else
                {
                    Thread.SpinWait(20);
                }
            }
        });

        await Task.WhenAll(writers.Append(reader)).WaitAsync(TimeSpan.FromSeconds(30));

        Assert.Equal(total, readCount);
        Array.Sort(consumed);
        for (int i = 0; i < total; ++i)
        {
            Assert.Equal(i, consumed[i]);
        }

        Assert.Equal(0, ring.StoredBytes);
    }

    // -------------------------------------------------------------------------
    // Stress wrap + span many cycles
    // -------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(AllKindsBothEndians))]
    public unsafe void Stress_ManyWrapCycles_SpanPath(RingKind kind, ByteOrder ringOrder)
    {
        const int capacity = 5;
        using IIntegralRingBuffer ring = Create(
            kind,
            capacity * sizeof(int),
            ringOrder);

        int nextWrite = 0;
        int nextRead = 0;
        int[] buf = new int[3];

        for (int cycle = 0; cycle < 200; ++cycle)
        {
            int toWrite = 1 + (cycle % 3);
            for (int i = 0; i < toWrite; ++i)
            {
                if (ring.FreeCount<int>() == 0)
                {
                    break;
                }

                Assert.True(ring.TryWrite(nextWrite++));
            }

            int available = ring.StoredCount<int>();
            if (available == 0)
            {
                continue;
            }

            int take = Math.Min(buf.Length, available);
            fixed (int* p = buf)
            {
                IntegralSpan dst = IntegralTestData.CreateSpan(
                    (byte*)p,
                    take,
                    IntegralType.Int32,
                    ByteOrder.Native);
                // Request exactly `take` (already ≤ stored) so waitable does not block.
                int got = ring.Read(dst);
                Assert.Equal(take, got);
                for (int i = 0; i < got; ++i)
                {
                    Assert.Equal(nextRead++, buf[i]);
                }
            }
        }

        // Drain rest
        while (ring.TryRead(out int v))
        {
            Assert.Equal(nextRead++, v);
        }

        Assert.Equal(nextWrite, nextRead);
    }
}
