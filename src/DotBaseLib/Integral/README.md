# `DotBase.Integral`

Unsafe scalar memory descriptors and endian-aware memory operations.

`IntegralSpan` describes scalar values stored as signed or unsigned integers,
`float`, or `double`, with an explicit byte order and optional block layout.
`ChangeFormat` re-labels the same memory region with a different value type and
block capacity (no data conversion). Byte order and conversion policy stay
from the original span. `IntegralRange` is a half-open interval in a **parent
span's blocks only** (`BlockOffset` / `BlockCount`) - not bytes and not scalar
value counts (except when the parent is UInt8 with block capacity 1).
`BlockByteSize` freezes the parent's block size in bytes so
`ByteOffset` / `ByteLength` support raw `IntegralMemory.Copy` (memcpy) without
building a subspan. Retyping after `GetBlockSpan` does not rescale the range.
`IntegralMemory` copies, moves, clears, converts, and performs strided
transfers between compatible views.

Short transfer names (`Copy`, `ReverseCopy`, `Convert`, `Move`, ...) are
**trusted** (no span validation) and **block-complete** by default. For
compatible block capacities, they transfer the largest scalar prefix that is
complete in both source and destination, leaving trailing partial-block values
alone. `IntegralMemory.CountBlockCompleteValues` exposes that calculation. Use
`*Checked` variants to validate descriptors and the applicable layout/endian
contracts (same pattern as ring span I/O). `Clear` is different: it clears the
span's full byte length, including trailing partial-block values. Strided APIs
remain value-granular; block framing is the caller's offset/stride.

## Main API

- `IntegralType` and `IntegralFormat` describe scalar representation, byte
  order, and block capacity.
- `IntegralCapacity` reports byte, scalar-value, complete-block, and trailing
  value counts.
- `IntegralPtr`, `IntegralSpan`, and `IntegralRange` describe unsafe memory and
  slices without taking ownership of it. `IntegralPtr.Pin<T>` provides an
  owned pin for managed arrays.
- `NumericScaleBias` configures an optional numeric scale and bias.
- `IntegralConversionPolicy` selects default, refused, or custom conversion
  factories independently for contiguous, interleaved, and planar paths.
- `ConversionHandles` resolves conversion handles and their contexts.
- `IntegralMemory` provides trusted and checked copy, reverse, conversion,
  move, clear, and strided operations.

The declared `IntegralType` formats are `UInt8`, `Int8`, `UInt16`, `Int16`,
`UInt32`, `Int32`, `UInt64`, `Int64`, `Float`, and `Double`. Typed APIs check
compatibility against the span or format via `IsCompatible<T>`.
`IntegralFormat.Empty` is the empty/no-buffer format sentinel.

## Numeric conversion and routing

`NumericScaleBias` is passed to `IntegralMemory.Convert` when a transfer needs
the transform `value * Scale + Bias` before conversion to the destination
type. Both values must be finite. `NumericScaleBias.Identity` applies no
transform and enables same-layout copy and endian-swap fast paths where
possible.

An `IntegralConversionPolicy` is stored in an `IntegralFormat` and controls
conversion into that format. It contains three independent policy slots: one
for contiguous spans, one for interleaved layout, and one for planar layout.
Each slot is either the default conversion table, an explicit refusal, or a
rooted custom handle factory. `IntegralConversionPolicy.None` uses the default
tables for all paths; `RefuseAll` disables every path; `FromValueConverters`
installs custom scalar converters on every path; and `FromFunc` installs a
structural conversion function for the contiguous path. `Create` is the
lower-level entry point for supplying individual path slots.

`ConversionHandles` is the public routing facade:

- `GetHandle` / `GetHandle(...)` — contiguous span conversion
  (`StandardDelegateTable`)
- `GetInterleavedHandle` / `GetInterleaved` — interleaved lane conversion
  (`InterleavedDelegateTable`)
- `GetPlanarHandle` / `GetPlanar` — planar plane conversion
  (`StandardDelegateTable`; layout applied by planar contexts)

A null handle (`IsNull`) means the requested conversion is unavailable or was
refused by policy. For contiguous conversion, a handle with
`NeedsContext == false` can be invoked directly with `handle.Convert(...)`.
When `NeedsContext` is true, obtain a context from `ConversionHandles` and call
`context.Convert(...)`. Context factories return `null` for a null handle.

`IntegralConversionHandle` is an unmanaged, process-local value. It contains
borrowed tokens for delegates rooted by process-lifetime conversion tables and
policy registry entries; do not persist it or transfer it between processes.

`ConversionContext` carries per-call state and invokes its bound handle.
`NumericConversionContext` additionally resolves a handle's scalar converter.
Layout subclasses add block/plane geometry:

| Family | Contexts |
| --- | --- |
| Interleaved | `InterleavedReaderContext`, `InterleavedWriterContext`, `InterleavedTransferContext` |
| Planar | `PlanarReaderContext`, `PlanarWriterContext`, `PlanarTransferContext` |

## Interleaved layouts

An interleaved block contains multiple scalar lanes, such as left/right audio
samples. Layout is applied by the interleaved kernels from the context (not by
slicing the span):

- **Reader** gathers one `ValueIndex` lane from each complete multi-value input
  block into dense serial output (`input` block capacity &gt; 1, `output` block
  capacity 1).
- **Writer** scatters dense serial input into one `ValueIndex` lane of each
  complete multi-value output block (`input` block capacity 1, `output` block
  capacity &gt; 1).
- **Transfer** gathers one input lane from each complete multi-value input
  block and scatters it into one output lane of each complete multi-value
  output block (both block capacities &gt; 1).

Typical call:

```csharp
var ctx = ConversionHandles.GetInterleavedReaderContext(
    src.Format,
    dst.Format,
    blockCapacity,
    lane);
if (ctx is null)
{
    return 0;
}

long n = ctx.Convert(src, dst, count);
```

The conversion count is the number of lane values, not the total number of
scalars in the interleaved storage. Only complete blocks are considered, and a
writer or transfer changes only its selected output lane; neighboring lanes
remain untouched.

## Planar layouts

A planar buffer stores complete planes (channels) stacked contiguously:
`[plane0…][plane1…]…`. `PlaneCapacity` is the number of scalar values in one
plane. Span value capacity must be a multiple of `PlaneCapacity`. Only complete
planes can be selected. `BlockCapacity` on the context is informational;
conversion uses `PlaneCapacity`.

Planar conversion reuses the **standard contiguous** kernels by slicing the
selected plane(s) inside the context's `Convert` override:

- **Reader** converts one input plane into dense serial output.
- **Writer** converts dense serial input into one output plane.
- **Transfer** converts one input plane into one output plane.

**Important:** call `ctx.Convert(src, dst, count)` so plane slicing runs.
`IntegralConversionHandle.Convert` performs only context-free contiguous
conversion and cannot apply planar layout.

```csharp
var ctx = ConversionHandles.GetPlanarReaderContext(
    src.Format,
    dst.Format,
    planeCapacity,
    blockCapacity,
    planeIndex);
if (ctx is null)
{
    return 0;
}

long n = ctx.Convert(src, dst, count);
```

## Value alignment

Scalar addresses in an `IntegralSpan` are **natural-aligned to the value size**
when both of the following hold:

1. The base `BytePtr` is aligned to `ValueByteCount` (1, 2, 4, or 8).
2. `Offset` is a multiple of `ValueByteCount`.

Public constructors build descriptors without validating format, ranges, or
alignment. Call `IsValid` / `Validate`, or use a checked operation, before
using a descriptor that is not already trusted.

Under that contract, compatible host/wire endian may use a single aligned
load/store (`*(T*)`), and opposite endian uses word `Copy`/`Swap` helpers.
Unaligned bases are unsupported for those scalar wire paths; allocate aligned
storage (for example `NativeMemory.AlignedAlloc`) when constructing spans over
raw buffers.

[Namespace index](../../../README.md#namespaces)
