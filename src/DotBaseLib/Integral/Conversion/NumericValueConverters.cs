using System.Diagnostics;
using System.Runtime.InteropServices;
using DotBase.Integral.Conversion.Internal;
using DotBase.Integral.Conversion.Numeric.Defaults;

namespace DotBase.Integral.Conversion;


/// <summary>
/// Scalar numeric conversion delegate table (architectural twin of the structural
/// <c>StandardDelegateTable</c>): provides one concrete
/// <c>Convert{Src}To{Dst}_Delegate</c> per integral type pair.
/// <para>
/// Built at construction and then immutable from the public surface. Registration
/// is only available through the internal table interface during construction / build.
/// </para>
/// <para>
/// Each slot is exposed as a non-managed <see cref="GCHandle"/> address
/// (<see cref="nint"/>) for storage on <see cref="Integral.IntegralConversionHandle"/>.
/// The table also exposes <see cref="SelfHandle"/> so
/// <see cref="IntegralConversionPolicy"/> can reference this instance without a
/// managed field on <see cref="IntegralFormat"/>.
/// </para>
/// <para>
/// <b>Dispose:</b> do not dispose a table while any live
/// <see cref="IntegralFormat"/> / conversion handle still holds its
/// <see cref="SelfHandle"/> or slot handles.
/// </para>
/// </summary>
public sealed class NumericValueConverters
    : INumericValueDelegateTable, IDisposable
{
    /// <summary>10 source types × 10 destination types.</summary>
    public const int TableSize = 10 * 10;

    /// <summary>Default instance with default math baked in.</summary>
    public static NumericValueConverters Default { get; } = CreateDefault();

    private static NumericValueConverters CreateDefault()
    {
        var table = new NumericValueConverters();
        INumericValueDelegateTable registration = table;
        DefaultNumericValueConvertersReg.AddToTable(registration);
        table.AssertTableFullyRegistered();
        table._isFrozen = true;
        return table;
    }

    /// <summary>GCHandle.ToIntPtr per slot; zero if empty. Roots the managed converter.</summary>
    private readonly nint[] _converterHandles;

    /// <summary>GCHandle to this instance for <see cref="IntegralConversionPolicy"/>.</summary>
    private readonly nint _selfHandle;

    /// <summary>Cached handle-factory GCHandles for <see cref="IntegralConversionPolicy.FromValueConverters"/>.</summary>
    private nint _cachedSpanHandleFactory;
    private nint _cachedReaderHandleFactory;
    private nint _cachedWriterHandleFactory;

    private bool _isFrozen;
    private bool _disposed;

    private NumericValueConverters()
    {
        _converterHandles = new nint[TableSize];
        _selfHandle = GCHandle.ToIntPtr(GCHandle.Alloc(this, GCHandleType.Normal));
    }

    /// <summary>
    /// Non-managed <see cref="GCHandle"/> address that roots this table instance.
    /// Used by <see cref="IntegralConversionPolicy.FromValueConverters"/>.
    /// </summary>
    public nint SelfHandle
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _selfHandle;
        }
    }

    /// <summary>
    /// Creates a table instance filled with default scalar converters, then applies
    /// <paramref name="configure"/> overrides via the registration surface.
    /// The returned instance is frozen (immutable).
    /// </summary>
    public static NumericValueConverters Create(Action<INumericValueDelegateTable> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var table = new NumericValueConverters();
        INumericValueDelegateTable registration = table;
        DefaultNumericValueConvertersReg.AddToTable(registration);
        configure(registration);
        table.AssertTableFullyRegistered();
        table._isFrozen = true;
        return table;
    }

    /// <summary>
    /// Returns the concrete scalar converter for <paramref name="inputType"/> →
    /// <paramref name="outputType"/>. Runtime type is the matching
    /// <c>Convert{Src}To{Dst}_Delegate</c>.
    /// </summary>
    public Delegate GetConverter(IntegralType inputType, IntegralType outputType)
    {
        nint handle = GetConverterHandle(inputType, outputType);
        object? target = GCHandle.FromIntPtr(handle).Target;
        Debug.Assert(target is Delegate);
        return (Delegate)target!;
    }

    /// <summary>
    /// Non-managed <see cref="GCHandle"/> address for the scalar converter of the given
    /// type pair. Safe to store on <see cref="Integral.IntegralConversionHandle"/> as long as
    /// this table instance remains alive (it owns and roots the handle).
    /// </summary>
    public nint GetConverterFunctionPointer(IntegralType inputType, IntegralType outputType)
        => GetConverterHandle(inputType, outputType);

    /// <summary>
    /// Process-lifetime factory for contiguous handles using this table's scalar converters.
    /// Cached so repeated <see cref="IntegralConversionPolicy.FromValueConverters"/> share slots.
    /// </summary>
    internal nint GetOrCreateSpanHandleFactory()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_cachedSpanHandleFactory == 0)
        {
            IntegralSpanConversionHandleFunc factory =
                InternalConversionDelegates.MakeSpanHandle_WithConverters(this);
            _cachedSpanHandleFactory = ConversionPolicySlot.AllocFactory(factory);
        }

        return _cachedSpanHandleFactory;
    }

    internal nint GetOrCreateReaderHandleFactory()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_cachedReaderHandleFactory == 0)
        {
            InterleavedReaderConversionHandleFunc factory =
                InternalConversionDelegates.MakeReaderHandle_WithConverters(this);
            _cachedReaderHandleFactory = ConversionPolicySlot.AllocFactory(factory);
        }

        return _cachedReaderHandleFactory;
    }

    internal nint GetOrCreateWriterHandleFactory()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_cachedWriterHandleFactory == 0)
        {
            InterleavedWriterConversionHandleFunc factory =
                InternalConversionDelegates.MakeWriterHandle_WithConverters(this);
            _cachedWriterHandleFactory = ConversionPolicySlot.AllocFactory(factory);
        }

        return _cachedWriterHandleFactory;
    }

    private nint GetConverterHandle(IntegralType inputType, IntegralType outputType)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        int index = TableIndex(inputType, outputType);
        Debug.Assert(index >= 0 && index < TableSize);
        nint handle = _converterHandles[index];
        Debug.Assert(handle != 0, $"Numeric converter slot {index} is empty.");
        return handle;
    }

    private static int TableIndex(IntegralType inputType, IntegralType outputType)
    {
        Debug.Assert(inputType != IntegralType.None);
        Debug.Assert(outputType != IntegralType.None);
        Debug.Assert((int)inputType >= 1 && (int)inputType <= 10);
        Debug.Assert((int)outputType >= 1 && (int)outputType <= 10);

        int in_type = (int)inputType - 1;
        int out_type = (int)outputType - 1;
        return in_type + 10 * out_type;
    }

    [Conditional("DEBUG")]
    private void AssertTableFullyRegistered()
    {
        for (int i = 0; i < TableSize; ++i)
        {
            Debug.Assert(
                _converterHandles[i] != 0,
                $"Numeric converter table slot {i} is still empty.");
        }
    }

    void INumericValueDelegateTable.SetConverter(
        Delegate converter,
        IntegralType inputType,
        IntegralType outputType)
    {
        if (_isFrozen)
        {
            throw new InvalidOperationException(
                "NumericValueConverters table is frozen and cannot be modified.");
        }

        ArgumentNullException.ThrowIfNull(converter);
        int index = TableIndex(inputType, outputType);
        Debug.Assert(index >= 0 && index < TableSize);

        // Replace: free previous handle if this slot was already set (override path).
        nint existing = _converterHandles[index];
        if (existing != 0)
        {
            GCHandle.FromIntPtr(existing).Free();
        }

        GCHandle gch = GCHandle.Alloc(converter, GCHandleType.Normal);
        _converterHandles[index] = GCHandle.ToIntPtr(gch);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        FreeHandles();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~NumericValueConverters()
    {
        // Best-effort free if Dispose was skipped (not for Default, which lives process-wide).
        if (_disposed || ReferenceEquals(this, Default))
        {
            return;
        }

        FreeHandles();
    }

    private void FreeHandles()
    {
        for (int i = 0; i < TableSize; ++i)
        {
            nint existing = _converterHandles[i];
            if (existing != 0)
            {
                try
                {
                    GCHandle.FromIntPtr(existing).Free();
                }
                catch
                {
                    // Dispose/finalizer path: ignore.
                }

                _converterHandles[i] = 0;
            }
        }

        if (_selfHandle != 0)
        {
            try
            {
                GCHandle.FromIntPtr(_selfHandle).Free();
            }
            catch
            {
                // Dispose/finalizer path: ignore.
            }
        }
    }
}


/// <summary>
/// Registration surface for <see cref="NumericValueConverters"/> (init / build path only).
/// </summary>
public interface INumericValueDelegateTable
{
    void SetConverter(Delegate converter, IntegralType inputType, IntegralType outputType);
}
