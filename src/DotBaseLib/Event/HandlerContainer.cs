namespace DotBase.Event;


public class HandlerContainer<TMessage>
    : IHandlerContainer
{
    public readonly EventHandler<TMessage> _handler;

    public HandlerContainer(EventHandler<TMessage> eventHandler)
    {
        _handler = eventHandler;
    }

    public void ConnectTo(IEventConsumerCollection collection)
    {
        collection.AddHandler(_handler);
    }

    public void DisconnectFrom(IEventConsumerCollection collection)
    {
        collection.RemoveHandler(_handler);
    }
}
