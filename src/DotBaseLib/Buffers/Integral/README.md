# `DotBase.Buffers.Integral`

Fixed-endian scalar and bulk integral operations over byte-addressed ring buffers.

Use `IntegralRingBuffer.CreateUnlocked` / `CreateLocked` / `CreateWaitable`.

## `IntegralSpan` read/write

Span transfers are **block-complete**: only whole blocks
(`BlockCapacity` values) move. Trailing values on the span are never read or written.

| Method | Policy |
|--------|--------|
| `Read` / `Write` on unlocked or locked buffers | **Partial** — as many complete blocks as fit (stored / free). |
| `Read` / `ReadChecked` on a waitable buffer | **Blocking** — wait for all complete blocks requested by the destination, or return `0` if the ring closes. |
| `Write` / `WriteChecked` on a waitable buffer | **Partial** — as many complete blocks as fit in the free space. |
| `TryRead` / `TryWrite` | **Atomic** — all complete blocks of the span, or fail with no mutation. |
| `*Checked` | Validate format/geometry first, then the matching trusted method. |

Trusted methods (`Read` / `Write` / `Try*`) do **not** validate the span descriptor;
use `*Checked` when the span may be malformed or hostile.

`int` results from `Read` / `Write` are **scalar value counts** (a multiple of
`BlockCapacity`), not block counts or byte counts.

Generic scalar/bulk APIs (`T`, arrays, `Span<T>`) remain value-granular and are
separate from this block-complete span contract.

`IWaitableRingBuffer` changes non-`Try` reads only: scalar, generic bulk, byte,
and `IntegralSpan` reads wait for the complete request. Its writes retain the
partial policy above. This differs from `CircularBufferWaitable`, whose byte
reads and writes both wait for the complete requested length.

[Namespace index](../../../../README.md#namespaces)
