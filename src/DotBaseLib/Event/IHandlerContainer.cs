namespace DotBase.Event;


/// <summary>
/// 
/// This exists so that instances of explicit instantiations (i.e. constructed types)
/// of <see cref="HandlerContainer{TMessage}"/> can be placed inside the same container
/// class, like for example a List.
/// 
/// </summary>
public interface IHandlerContainer
{
    void ConnectTo(IEventConsumerCollection collection);

    void DisconnectFrom(IEventConsumerCollection collection);
}
