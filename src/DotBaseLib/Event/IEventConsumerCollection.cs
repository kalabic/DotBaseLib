namespace DotBase.Event;


public interface IEventConsumerCollection 
{
    void AddEvent<TMessage>(IEventContainer<TMessage> handler);

    void Connect<TMessage>(EventProducer<TMessage> producer);

    void Disconnect<TMessage>(EventProducer<TMessage> producer);

    void AddHandler<TMessage>(EventHandler<TMessage> eventHandler);

    void RemoveHandler<TMessage>(EventHandler<TMessage> eventHandler);

    TClass? Find<TClass>() where TClass : class;
}
