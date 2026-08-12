using DotBase.Integral.Conversion.Internal;
using DotBase.Integral.Conversion.Numeric.Defaults;
using System.Diagnostics;

namespace DotBase.Integral.Conversion;


/// <summary>
/// Immutable scalar numeric conversion delegate table containing optional custom
/// overrides. Unmodified slots share lazily resolved built-in converters. Managed
/// delegates are retained directly; when the table is attached to a policy, the
/// process-lifetime policy registry roots the table.
/// </summary>
public sealed class NumericValueConverters
    : INumericValueDelegateTable
{
    /// <summary>10 source types x 10 destination types.</summary>
    public const int TableSize = 10 * 10;

    /// <summary>Default instance backed by lazily resolved built-in converters.</summary>
    public static NumericValueConverters Default { get; } = CreateDefault();

    private readonly Delegate?[] _overrides = new Delegate?[TableSize];

    private readonly nint[] _converterHandles = new nint[TableSize];

    // Zero means no registry entry has been requested. Positive values are
    // process-lifetime policy indexes.
    private int _policyIndex;
    private bool _isFrozen;

    private NumericValueConverters()
    {
    }

    private static NumericValueConverters CreateDefault()
    {
        NumericValueConverters table = new();
        table.Freeze();
        return table;
    }

    /// <summary>
    /// Creates a table containing the configured scalar converter overrides and
    /// returns the frozen table. Unmodified slots use shared built-in converters.
    /// </summary>
    public static NumericValueConverters Create(
        Action<INumericValueDelegateTable> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        NumericValueConverters table = new();
        INumericValueDelegateTable registration = table;
        configure(registration);
        table.Freeze();
        return table;
    }

    /// <summary>
    /// Returns the scalar converter for <paramref name="inputType"/> to
    /// <paramref name="outputType"/>.
    /// </summary>
    public Delegate GetConverter(IntegralType inputType, IntegralType outputType)
    {
        int index = TableIndex(inputType, outputType);
        Delegate? converter = Volatile.Read(ref _overrides[index]);
        return converter ?? DefaultNumericValueDelegateTable.Instance.GetConverter(index, inputType, outputType);
    }

    /// <summary>
    /// NumericValueConverters owns and caches each GCHandle.<br/>
    /// This is safe only because registered converter tables are process-lifetime.
    /// </summary>
    /// <param name="inputType"></param>
    /// <param name="outputType"></param>
    /// <returns></returns>
    internal nint GetConverterHandle(IntegralType inputType, IntegralType outputType)
    {
        int index = TableIndex(inputType, outputType);

        nint existing = Volatile.Read(ref _converterHandles[index]);
        if (existing != 0)
        {
            return existing;
        }

        return DelegateHandle.GetOrAllocate(
            ref _converterHandles[index],
            GetConverter(inputType, outputType));
    }

    internal int GetOrRegisterPolicyIndex()
    {
        EnsureFrozenForPolicy();

        int existingIndex = Volatile.Read(ref _policyIndex);
        if (existingIndex != 0)
        {
            return existingIndex;
        }

        int registeredIndex =
            ConversionPolicyRegistry.RegisterValueConverters(this);
        existingIndex = Interlocked.CompareExchange(
            ref _policyIndex,
            registeredIndex,
            comparand: 0);
        return existingIndex == 0 ? registeredIndex : existingIndex;
    }

    internal void EnsureFrozenForPolicy()
    {
        if (!Volatile.Read(ref _isFrozen))
        {
            throw new InvalidOperationException(
                "Numeric converters cannot be registered before the table is frozen.");
        }
    }

    private void Freeze()
    {
        Volatile.Write(ref _isFrozen, true);
    }

    private static int TableIndex(
        IntegralType inputType,
        IntegralType outputType)
    {
        Debug.Assert(inputType != IntegralType.None);
        Debug.Assert(outputType != IntegralType.None);
        Debug.Assert((int)inputType >= 1 && (int)inputType <= 10);
        Debug.Assert((int)outputType >= 1 && (int)outputType <= 10);

        int inputIndex = (int)inputType - 1;
        int outputIndex = (int)outputType - 1;
        return inputIndex + 10 * outputIndex;
    }

    void INumericValueDelegateTable.SetConverter(
        Delegate converter,
        IntegralType inputType,
        IntegralType outputType)
    {
        if (Volatile.Read(ref _isFrozen))
        {
            throw new InvalidOperationException(
                "NumericValueConverters table is frozen and cannot be modified.");
        }

        ArgumentNullException.ThrowIfNull(converter);
        int index = TableIndex(inputType, outputType);
        Debug.Assert(index >= 0 && index < TableSize);
        Volatile.Write(ref _overrides[index], converter);
    }
}


/// <summary>
/// Registration surface for <see cref="NumericValueConverters"/> during creation.
/// </summary>
public interface INumericValueDelegateTable
{
    void SetConverter(
        Delegate converter,
        IntegralType inputType,
        IntegralType outputType);
}
