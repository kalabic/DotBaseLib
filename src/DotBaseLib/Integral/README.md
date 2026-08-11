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
  factories independently for contiguous, interleaved-reader, and
  interleaved-writer paths.
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
for contiguous spans, one for interleaved readers, and one for interleaved
writers. Each slot is either the default conversion table, an explicit refusal,
or a rooted custom handle factory. `IntegralConversionPolicy.None` uses the
default tables for all paths; `RefuseAll` disables every path;
`FromValueConverters` installs custom scalar converters on every path; and
`FromFunc` installs a structural conversion function for the contiguous path.
`Create` is the lower-level entry point for supplying individual path slots.

`ConversionHandles` is the public routing facade. `GetHandle` resolves a
contiguous `IntegralConversionHandle`; `GetInterleavedReaderHandle` and
`GetInterleavedWriterHandle` resolve the corresponding layout-aware handles.
A null handle (`IsNull`) means the requested conversion is unavailable or was
refused by policy. Handles with no custom scalar converter can be invoked
directly. Otherwise, obtain the matching context from `ConversionHandles` and
pass it to `Convert`.

`ConversionContext` carries per-call state and invokes its bound handle.
`NumericConversionContext` additionally resolves a handle's scalar converter.
`InterleavedReaderContext` and `InterleavedWriterContext` extend that numeric
context with block capacity and lane index.

## Interleaved layouts

An interleaved block contains multiple scalar lanes, such as left/right audio
samples. An interleaved reader gathers one `ValueIndex` lane from each complete
input block into dense output. Its input block capacity is greater than one and
the output block capacity is one. An interleaved writer scatters dense input
into one `ValueIndex` lane of each complete output block. Its input block
capacity is one and the output block capacity is greater than one.

The conversion count is the number of lane values, not the total number of
scalars in the interleaved storage. Only complete blocks are considered, and a
writer changes only its selected output lane; neighboring lanes remain
untouched.

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
