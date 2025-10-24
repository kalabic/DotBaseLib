using DotBase.AsyncEvent;

namespace DotBase.Event;


public interface IEventContainer<TMessage>
    : IEventContainerInstance
{
    void AddHandler(EventHandler<TMessage> handler);

    void AddHandlerAsync(EventHandler<TMessage> handler);

    void AddHandlerAsync(AsyncEventHandler<TMessage> handler);

    void RemoveHandler(EventHandler<TMessage> handler);

    void RemoveHandlerAsync(EventHandler<TMessage> handler);
}
