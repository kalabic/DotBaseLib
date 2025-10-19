using System.Diagnostics.Tracing;

namespace DotBase.Log;


[EventSource(
    Name = "DotBase.Disposal",
    Guid = "EE36453D-21FD-4FAD-B8C0-44BDD57E0EF8"
)]
internal sealed class DisposableEventSource : EventSource
{
    public static readonly DisposableEventSource Log = new();

    private DisposableEventSource() : base() { }


    [Event(1, Level = EventLevel.Verbose, Message = "Created instance of {0}")]
    public void Created(string typeName) 
        => WriteEvent(1, typeName);


    [Event(2, Level = EventLevel.Verbose, Message = "Disposed instance of {0}")]
    public void Disposed(string typeName)
        => WriteEvent(2, typeName);


    [Event(3, Level = EventLevel.Warning, Message = "Finalized {0} without Dispose. Stack: {1}")]
    public void FinalizedWithoutDispose(string typeName, string allocationStack)
        => WriteEvent(3, typeName, allocationStack);


    [Event(4, Level = EventLevel.Warning, Message = "Finalizer of {0} threw: {1}")]
    public void FinalizerThrew(string typeName, string exceptionMessage)
        => WriteEvent(4, typeName, exceptionMessage);
}
