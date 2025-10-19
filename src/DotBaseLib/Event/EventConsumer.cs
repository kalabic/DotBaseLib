namespace DotBase.Event;


public class EventConsumer<TMessage> 
    : EventContainer<TMessage>
{
    // Implementation >>

    public EventConsumer() 
        : base()
    { }

    public EventConsumer(EventHandler<TMessage> eventHandler)
        : base()
    {
        AddHandler(eventHandler);
    }
}
