# `DotBase.Buffers.Integral`

Fixed-endian scalar and bulk integral operations over byte-addressed ring buffers.

Use `IntegralRingBuffer.CreateUnlocked` / `CreateLocked` / `CreateWaitable`.

## `IntegralSpan` read/write

Span transfers are **block-complete**: only whole blocks
(`BlockCapacity` values) move. Trailing values on the span are never read or written.

| Method | Policy |
|--------|--------|
| `Read` / `Write` (all buffer kinds) | **Partial, immediate** — as many complete blocks as currently fit (stored / free). |
| `ReadExact` on a waitable buffer | **Atomic and waiting when feasible** — wait until stored bytes cover all requested complete blocks; return `0` if the request cannot fit or the ring terminates first. |
| `WriteExact` on a waitable buffer | **Atomic and waiting when feasible** — wait until free space covers all requested complete blocks; return `0` if the request cannot fit or the ring terminates first. |
| `TryRead` / `TryWrite` | **Atomic, immediate** — all complete blocks of the span, or fail with no mutation. |
| `*Checked` | Structurally validate format, geometry, alignment, and pointer metadata, then use the matching trusted method. Ring capacity is an operational condition, not descriptor validation. |

Trusted methods (`Read` / `Write` / `Try*`) do **not** validate the span descriptor;
use `*Checked` when the span may be malformed or hostile.

`int` results from `Read` / `Write` are **scalar value counts** (a multiple of
`BlockCapacity`), not block counts or byte counts.

Generic scalar/bulk APIs (`T`, arrays, `Span<T>`) remain value-granular and are
separate from this block-complete span contract.

`IWaitableRingBuffer` follows the same three policies for byte, scalar, generic,
and `IntegralSpan` APIs: ordinary `Read` / `Write` methods are partial and immediate;
`Try*` methods are atomic and immediate; `*Exact` methods wait for the complete
request only when it can physically fit. A larger Exact request returns `0` or
`false` immediately. Termination before fulfillment has the same status result and
does not cause a partial Exact transfer. Exceptions are reserved for malformed
arguments and invalid checked descriptors.

## Waitable producer-consumer lifecycle

`CompleteWriting()` publishes producer EOF without closing native storage. Buffered
data remains readable through partial operations. Generic reads move only complete
values, and `IntegralSpan` reads move only complete blocks. After the buffer drains,
reads return `0` or `false`. Pending Exact reads fail if completion makes their full
request impossible; `TryRead` remains immediate and atomic.

`CompleteReading()` stops both sides and discards buffered data. `Abort(error)` does
the same while retaining the first abort error in `AbortError`. These commands are
idempotent and wake blocked readers and writers. They do not release native storage;
only `Close()` or disposal does that.

`IsWritingCompleted` and `IsReadingCompleted` report normal endpoint completion,
`IsAborted` reports abort, and `IsDrained` means writing completed and no bytes remain.
Writes are rejected after either endpoint completes or the ring aborts. Reads remain
permitted after writing completes so that buffered data can drain, but are rejected
after reading completes or the ring aborts. Operational termination uses the existing
`0` / `false` results; inspect `AbortError` for abort diagnostics.

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
