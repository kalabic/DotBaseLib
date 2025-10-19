namespace DotBase.Event;


public interface IEventProducer<TMessage> 
    : IEventContainer<TMessage>
{
    void SendTo(EventContainer<TMessage> other);
}
