# `DotBase.Buffers.Integral`

Fixed-endian scalar and bulk integral operations over byte-addressed ring buffers.

Use `IntegralRingBuffer.CreateUnlocked` / `CreateLocked` / `CreateWaitable`.

## `IntegralSpan` read/write

Span transfers are **block-complete**: only whole blocks
(`BlockCapacity` values) move. Trailing values on the span are never read or written.

| Method | Policy |
|--------|--------|
| `Read` / `Write` on unlocked or locked buffers | **Partial** — as many complete blocks as fit (stored / free). |
| `Read` / `ReadChecked` on a waitable buffer | **Blocking when feasible** — wait until stored bytes cover all requested complete blocks; return `0` if the request cannot fit or the ring closes first. |
| `Write` / `WriteChecked` on a waitable buffer | **Blocking when feasible** — wait until free space covers all requested complete blocks; return `0` if the request cannot fit or the ring closes first. |
| `TryRead` / `TryWrite` | **Atomic** — all complete blocks of the span, or fail with no mutation. |
| `*Checked` | Structurally validate format, geometry, alignment, and pointer metadata, then use the matching trusted method. Ring capacity is an operational condition, not descriptor validation. |

Trusted methods (`Read` / `Write` / `Try*`) do **not** validate the span descriptor;
use `*Checked` when the span may be malformed or hostile.

`int` results from `Read` / `Write` are **scalar value counts** (a multiple of
`BlockCapacity`), not block counts or byte counts.

Generic scalar/bulk APIs (`T`, arrays, `Span<T>`) remain value-granular and are
separate from this block-complete span contract.

`IWaitableRingBuffer` non-`Try` reads and writes wait for the complete request
(same complete-length policy as `CircularBufferWaitable` byte I/O) only when the
request can physically fit. A larger valid request returns `0` or `false`
immediately. Closure before fulfillment has the same status result. `Try*` stay
non-blocking and atomic. Exceptions are reserved for malformed arguments and
invalid checked descriptors.

## Scalar API migration

Scalar reads and writes report operational failure explicitly:

```csharp
if (ring.Read<int>(out int value))
{
    // Use value.
}

if (!ring.Write(value))
{
    // Insufficient space, a closed ring, or a request that cannot fit.
}
```

Failed scalar reads assign `default` to the output value. The older throwing
`T Read<T>()` and `void Write<T>(T)` signatures are not retained.

[Namespace index](../../../../README.md#namespaces)
