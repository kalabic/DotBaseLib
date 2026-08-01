# `DotBase.Integral`

Unsafe scalar memory descriptors and endian-aware memory operations.

`IntegralSpan` describes scalar values stored as signed or unsigned integers,
`float`, or `double`, with an explicit byte order and optional block layout.
`ChangeFormat` re-labels the same memory region with a different value type and
block capacity (no data conversion). Byte order and byte-rate stay from the
original span. `IntegralRange` is a half-open interval in a **parent span's
blocks only** (`BlockOffset` / `BlockCount`) — not bytes and not scalar value
counts (except when the parent is UInt8 with block capacity 1).
`BlockByteSize` freezes the parent's block size in bytes so
`ByteOffset` / `ByteLength` support raw `IntegralMemory.Copy` (memcpy) without
building a subspan. Retyping after `GetBlockSpan` does not rescale the range.
`IntegralMemory` copies, moves, clears, converts, and performs strided
transfers between compatible views.

Short names (`Copy`, `ReverseCopy`, `Convert`, `Move`, …) are **trusted**
(no span validation) and **block-complete** by default: they transfer
`min(src,dst)` complete blocks only and leave trailing partial-block values
alone. Use `*Checked` variants to validate descriptors and layout/endian
contracts (same pattern as ring span I/O). Strided APIs remain value-granular;
block framing is the caller's offset/stride.

The declared `IntegralType` formats are `UInt8`, `Int8`, `UInt16`, `Int16`,
`UInt32`, `Int32`, `UInt64`, `Int64`, `Float`, and `Double`. Typed APIs check
compatibility against the span or format via `IsCompatible<T>`.

## Value alignment

Scalar addresses in an `IntegralSpan` are **natural-aligned to the value size**
when both of the following hold:

1. The base `BytePtr` is aligned to `ValueByteCount` (1, 2, 4, or 8).
2. `Offset` is a multiple of `ValueByteCount` (enforced by the public ctor).

Under that contract, compatible host/wire endian may use a single aligned
load/store (`*(T*)`), and opposite endian uses word `Copy`/`Swap` helpers.
Unaligned bases are unsupported for those scalar wire paths; allocate aligned
storage (for example `NativeMemory.AlignedAlloc`) when constructing spans over
raw buffers.

[Namespace index](../../../README.md#namespaces)
