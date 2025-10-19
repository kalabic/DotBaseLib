using DotBase.Core;

namespace DotBase.Event;


public class EventContainer<TMessage> 
    : DisposableBase
    , IEventContainer<TMessage>
{
    public bool IsEmpty { get { return _event == null; } }

    private event EventHandler<TMessage>? _event;

    private event EventHandler<Disposing>? _disposing;


    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _disposing?.Invoke(this, new EventContainer<TMessage>.Disposing());
            _disposing = null;
            _event = null;
        }
        base.Dispose(disposing);
    }

    public virtual bool Invoke()
        => Invoke(null, default);

    public virtual bool Invoke(TMessage? msg)
        => Invoke(null, msg);

    public virtual bool Invoke(object? sender, TMessage? msg)
    {
        if (!IsDisposed)
        {
            var myEvent = _event;
#pragma warning disable CS8604 // Possible null reference argument.
            myEvent?.Invoke(sender, msg);
#pragma warning restore CS8604 // Possible null reference argument.
            return myEvent is not null;
        }
        return false;
    }

    /// <summary>
    /// 
    /// If this is base of an event consumer class (for example see <see cref="EventConsumer{TMessage}"/>), then
    /// this method is the entry point for an event invoked on event producer (see <see cref="EventProducer{TMessage}"/>).
    /// 
    /// <para>TODO: This is likely also the best place to catch unhandled exceptions.</para>
    /// </summary>
    public void HandleInvoke(object? sender, TMessage ev)
    {
        Invoke(sender, ev);
    }

    private void HandleDisposing(object? sender, EventContainer<TMessage>.Disposing ev)
    {
        if (sender is EventContainer<TMessage> other)
        {
            Disconnect(other);
        }
    }

    public void AddHandler(EventHandler<TMessage> handler)
    {
        _event += handler;
    }

    internal void AddDisposeHandler(EventHandler<Disposing> handler)
    {
        _disposing += handler;
    }

    public void RemoveHandler(EventHandler<TMessage> handler)
    {
        _event -= handler;
    }

    internal void RemoveDisposeHandler(EventHandler<Disposing> handler)
    {
        _disposing -= handler;
    }

    public void SendTo(EventContainer<TMessage> other)
    {
        _event += other.HandleInvoke;
        other.AddDisposeHandler(HandleDisposing);
    }

    public void Disconnect(EventContainer<TMessage> other)
    {
        _event -= other.HandleInvoke;
        other.RemoveDisposeHandler(HandleDisposing);
    }

    public IEventContainerInstance NewCompatibleInstance()
    {
        return new EventContainer<TMessage>();
    }


    // Private events >>

    internal class Disposing { }
}
