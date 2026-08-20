# `DotBase.Buffers`

Byte-oriented ring-buffer contracts and unlocked, locked, and waitable circular buffers.

Main API: `IByteRingBuffer`, `CircularBufferUnlocked`, `CircularBufferLocked`,
`CircularBufferWaitable`, `ByteOrder`, and `ByteOrderExtensions.Resolve`.

Unlocked and locked byte buffers transfer as many bytes as immediately fit.
`CircularBufferWaitable` waits for a complete fitting request; a valid request
larger than its current capacity, or one terminated by closure, returns `0`.
Malformed array ranges and pointer arguments still throw.

[Namespace index](../../../README.md#namespaces)
