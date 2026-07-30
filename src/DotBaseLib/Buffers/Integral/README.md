# `DotBase.Buffers.Integral`

Fixed-endian scalar and bulk integral operations over byte-addressed ring buffers.

Use `IntegralRingBuffer` to create ordinary, locked, or waitable implementations.
Generic scalar and bulk operations support `sbyte`, `byte`, `short`, `ushort`,
`int`, `uint`, `long`, `ulong`, `nint`, `nuint`, `char`, `float`, and `double`.

`IntegralSpan` overloads transfer the ten declared `IntegralType` formats,
including `Float` and `Double`, and convert byte order while values enter or
leave the ring.

[Namespace index](../../../../README.md#namespaces)
