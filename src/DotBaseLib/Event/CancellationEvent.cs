namespace DotBase.Event;


public class CancellationEvent { }


public class CancellationEventConsumer
    : EventConsumer<CancellationEvent>
{ }


public class CancellationEventProducer
    : EventProducer<CancellationEvent>
{ }
