using DotBase.Log;
using DotBase.Core;
using System.Collections;

namespace DotBase.Event;


public class GenericEventCollection
    : DisposableBase
    , IEnumerable<IEventContainerInstance>
{
    protected readonly object _lock = new();

    protected readonly ILiteLog? _log;

    // For 10-15 items list is just fine.
    private readonly List<IEventContainerInstance> _collection = [];


    public GenericEventCollection(ILiteLog? log = null)
    {
        _log = log;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Clear();
        }
        base.Dispose(disposing);
    }

    public virtual void Clear(bool disposeItems = true)
    {
        lock (_lock)
        {
            if (disposeItems)
            {
                for (int i = 0; i < _collection.Count; i++)
                {
                    try
                    {
                        _collection[i]?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _log?.ExceptionOccured($"Exception during Dispose() of an item (index:{i}) in {nameof(GenericEventCollection)}.", ex);
                    }
                }
            }
            _collection.Clear();
        }
    }

    public void AddEvent<TMessage>(IEventContainer<TMessage> handler)
    {
        if (EventExists<TMessage>())
        {
            throw new ArgumentException($"Attempted to add a duplicated event type in {nameof(GenericEventCollection)}.");
        }

        lock (_lock)
        {
            if (!IsDisposed)
            {
                _collection.Add(handler);
            }
        }
    }

    public TClass? Find<TClass>() where TClass : class
    {
        lock (_lock)
        {
            if (IsDisposed)
            {
                return null;
            }

            for (int index = 0; index < _collection.Count; index++)
            {
                if (_collection[index] is TClass tobject)
                {
                    return tobject;
                }
            }

            return null;
        }
    }

    public bool Exists<TClass>() where TClass : class
        => ( null != Find<TClass>() );

    public bool EventExists<TMessage>()
        => Exists<EventContainer<TMessage>>();

    public EventContainer<TMessage>? FindEvent<TMessage>()
        => Find<EventContainer<TMessage>>();

    public IEventContainerInstance? FindEventCore<TMessage>()
        => Find<IEventContainer<TMessage>>();

    IEnumerator<IEventContainerInstance> IEnumerable<IEventContainerInstance>.GetEnumerator()
        => _collection.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => _collection.GetEnumerator();
}
