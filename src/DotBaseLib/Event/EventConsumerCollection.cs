using DotBase.Log;

namespace DotBase.Event;


public class EventConsumerCollection
    : GenericEventCollection
    , IEventConsumerCollection
{
    public EventConsumerCollection(ILiteLog? log = null)
        : base(log)
    { }

    public void Connect<TMessage>(EventProducer<TMessage> producer)
    {
        if (IsDisposed)
        {
            return;
        }

        var consumer = Find<EventConsumer<TMessage>>();
        if (consumer is not null)
        {
            producer.SendTo(consumer);
        }
    }

    public void Disconnect<TMessage>(EventProducer<TMessage> producer)
    {
        if (IsDisposed)
        {
            return;
        }

        var consumer = Find<EventConsumer<TMessage>>();
        if (consumer is not null)
        {
            producer.Disconnect(consumer);
        }
    }

    public void AddHandler<TMessage>(EventHandler<TMessage> eventHandler)
    {
        if (IsDisposed)
        {
            return;
        }

        var existing = Find<EventConsumer<TMessage>>();
        if (existing is null)
        {
            var pevent = new EventConsumer<TMessage>(eventHandler);
            AddEvent(pevent);
        }
        else
        {
            existing.AddHandler(eventHandler);
        }
    }

    public void RemoveHandler<TMessage>(EventHandler<TMessage> eventHandler)
    {
        if (IsDisposed)
        {
            return;
        }

        var existing = Find<EventConsumer<TMessage>>();
        if (existing is not null)
        {
            existing.RemoveHandler(eventHandler);
        }
    }
}
