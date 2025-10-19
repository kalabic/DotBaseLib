using System.Diagnostics.Tracing;

namespace DotBase.Log;


public class LiteConsoleEventListener 
    : EventListener
{
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        // Only enable events from our specific event source
        if (eventSource.Name == LiteLog.NAME)
        {
            EnableEvents(eventSource, EventLevel.Verbose, EventKeywords.All);
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        if (eventData.EventSource.Name == LiteLog.NAME)
        {
            // This is where we receive the events
            Console.Write($" * {eventData.EventName} : ");
            if (eventData.Payload != null)
            {
                for (int i = 0; i < eventData.Payload.Count; i++)
                {
                    Console.WriteLine($"{eventData.Payload[i]}");
                }
            }
        }
    }
}
