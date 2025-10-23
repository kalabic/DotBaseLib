using DotBase.Event;

namespace DotBase.Cancellation;


public class CancellationEvent { }


public class CancellationEventConsumer
    : EventConsumer<CancellationEvent>
{ }


public class CancellationEventProducer
    : EventProducer<CancellationEvent>
{ }
