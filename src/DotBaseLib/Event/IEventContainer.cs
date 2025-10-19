namespace DotBase.Event;


public interface IEventContainer<TMessage>
    : IEventContainerInstance
{
    void AddHandler(EventHandler<TMessage> handler);

    void RemoveHandler(EventHandler<TMessage> handler);
}
