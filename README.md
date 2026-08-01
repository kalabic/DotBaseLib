# DotBaseLib

A helper library with foundational components for .NET applications.

## Namespaces

| Namespace | Purpose |
| --- | --- |
| [`DotBase.AsyncEvent`](src/DotBaseLib/AsyncEvent/README.md) | Asynchronous event delegates and invocation helpers. |
| [`DotBase.Buffers`](src/DotBaseLib/Buffers/README.md) | Byte-oriented circular and ring buffers. |
| [`DotBase.Buffers.Integral`](src/DotBaseLib/Buffers/Integral/README.md) | Fixed-endian integral operations over byte ring buffers. |
| [`DotBase.Cancellation`](src/DotBaseLib/Cancellation/README.md) | Cancellation-aware events and waits. |
| [`DotBase.Core`](src/DotBaseLib/Core/README.md) | Disposal and finalization foundations. |
| [`DotBase.Event`](src/DotBaseLib/Event/README.md) | Typed event producers, consumers, and containers. |
| [`DotBase.Integral`](src/DotBaseLib/Integral/README.md) | Typed scalar memory views, numeric conversion, and endian-aware memory operations. |
| [`DotBase.Log`](src/DotBaseLib/Log/README.md) | Lightweight logging and console utilities. |
| [`DotBase.Tools`](src/DotBaseLib/Tools/README.md) | General-purpose type, task, and synchronization helpers. |

## DotBaseTestApp

A very simple Windows app to test `WindowsEventLogBridge`.

## DotBaseTestSetup

Updates the Windows Event Log source. Run this setup app as an administrator.

## DotBaseWinLib

Windows-specific class library. Contains the `WindowsEventLogBridge` class, which forwards .NET events to the Windows Event Log.

## Dependencies and Third Party Notices

Parts of library use code from [NAudio](https://github.com/naudio/NAudio) project by Mark Heath. Third-party notices are recorded in [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
