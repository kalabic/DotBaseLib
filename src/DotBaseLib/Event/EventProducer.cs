namespace DotBase.Event;


public class EventProducer<TMessage> 
    : EventContainer<TMessage>
    , IEventProducer<TMessage>
{
}
