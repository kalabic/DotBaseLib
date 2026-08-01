# `DotBase.Buffers.Integral`

Fixed-endian scalar and bulk integral operations over byte-addressed ring buffers.

Use `IntegralRingBuffer.CreateUnlocked` / `CreateLocked` / `CreateWaitable`.

## `IntegralSpan` read/write

Span transfers are **block-complete**: only whole blocks
(`BlockCapacity` values) move. Trailing values on the span are never read or written.

| Method | Policy |
|--------|--------|
| `Read` / `Write` | **Partial** — as many complete blocks as fit (stored / free). |
| `TryRead` / `TryWrite` | **Atomic** — all complete blocks of the span, or fail with no mutation. |
| `*Checked` | Validate format/geometry first, then the matching trusted method. |

Trusted methods (`Read` / `Write` / `Try*`) do **not** validate the span descriptor;
use `*Checked` when the span may be malformed or hostile.

`int` results from `Read` / `Write` are **scalar value counts** (a multiple of
`BlockCapacity`), not block counts or byte counts.

Generic scalar/bulk APIs (`T`, arrays, `Span<T>`) remain value-granular and are
separate from this block-complete span contract.

[Namespace index](../../../../README.md#namespaces)
