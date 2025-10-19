namespace DotBase.Event;


public interface IEventContainerInstance 
    : IDisposable
{
    bool IsEmpty { get; }

    IEventContainerInstance NewCompatibleInstance();
}
