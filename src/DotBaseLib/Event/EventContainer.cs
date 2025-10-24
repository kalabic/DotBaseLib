using DotBase.AsyncEvent;
using DotBase.Core;
using DotBase.Log;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace DotBase.Event;


public class EventContainer<TMessage> 
    : DisposableBase
    , IEventContainer<TMessage>
{
    public bool IsEmpty { get { return _event == null; } }

    private event AsyncEventHandler<TMessage>? _asyncEvent;

    private ConcurrentDictionary< EventHandler<TMessage>, AsyncEventHandler<TMessage>> _asyncHandlerMap = new();

    private event EventHandler<TMessage>? _event;

    private event EventHandler<Disposing>? _disposing;


    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _disposing?.Invoke(this, new EventContainer<TMessage>.Disposing());
            _disposing = null;

            _asyncEvent = null;
            _asyncHandlerMap.Clear();
            _event = null;
        }
        base.Dispose(disposing);
    }

    public virtual bool Invoke()
        => Invoke(null, default);

    public virtual bool Invoke(TMessage? msg)
        => Invoke(null, msg);


    /// <summary>
    /// 
    /// NOTE: This method will start a new task every time it is used following these rules:
    /// <list type="bullet">
    /// <item>If any handler was added using <see cref="AddHandlerAsync(EventHandler{TMessage})"/></item>
    /// <item>If any handler was added using <see cref="AddHandlerAsync(AsyncEventHandler{TMessage})"/></item>
    /// </list>
    /// 
    /// About unhandled exceptions inside handlers added using one of 'AddHandlerAsync' methods:
    /// <list type="bullet">
    /// <item>Unhandled exception inside <see cref="AsyncEventHandler"/> will NOT end the process.</item>
    /// <item>Unhandled exception inside synchronous (void) <see cref="EventHandler"/> will NOT end the process.</item>
    /// <item>Unhandled exception inside async (void) <see cref="EventHandler"/> WILL END the process.</item>
    /// </list>
    /// 
    /// Unhandled exceptions inside 'standard' synchronous event handlers are not handled here but are passed on to the caller.
    /// 
    /// </summary>
    public virtual bool Invoke(object? sender, TMessage? msg)
    {
        if (!IsDisposed)
        {
            var myAsyncEvent = _asyncEvent;
            var myEvent = _event;
#pragma warning disable CS8604 // Possible null reference argument.
            if (myAsyncEvent is not null)
            {
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                var task = Task.Run(() => myAsyncEvent.InvokeAsync(sender, msg));
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                task.ContinueWith(t =>
                {
                    LiteLog.Log.ExceptionOccured("Unhandled exception in async event handler.", t.Exception!);
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
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

    /// <summary>
    /// 
    /// NOTE: Unhandled exceptions inside 'async void' type of event handler will end the process.
    /// <para>
    /// For asynchronous event processing <see cref="AsyncEventHandler"/> is a better choice
    /// than async <see cref="EventHandler"/>.
    /// </para>
    ///
    /// </summary>
    public void AddHandlerAsync(EventHandler<TMessage> handler)
    {
        var asyncHandler = Extensions.Async(handler);
        if (_asyncHandlerMap.TryAdd(handler, asyncHandler))
        {
            _asyncEvent += asyncHandler;
        }
        else
        {
            Debug.Assert(false, $"Attempted to add duplicate async handler: {handler.GetType().Name}");
        }
    }

    public void AddHandlerAsync(AsyncEventHandler<TMessage> handler)
    {
        _asyncEvent += handler;
    }

    internal void AddDisposeHandler(EventHandler<Disposing> handler)
    {
        _disposing += handler;
    }

    public void RemoveHandler(EventHandler<TMessage> handler)
    {
        _event -= handler;
    }

    public void RemoveHandlerAsync(EventHandler<TMessage> handler)
    {
        if (_asyncHandlerMap.TryRemove(handler, out var asyncHandler))
        {
            _asyncEvent -= asyncHandler;
        }
    }

    public void RemoveHandlerAsync(AsyncEventHandler<TMessage> handler)
    {
        _asyncEvent -= handler;
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
