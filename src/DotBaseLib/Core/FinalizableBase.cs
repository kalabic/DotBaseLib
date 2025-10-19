using DotBase.Log;

namespace DotBase.Core;


public abstract class FinalizableBase 
    : DisposableBase
{
    ~FinalizableBase()
    {
        if (TryBeginDispose())
        {
            // Never throw, never assert.
            try { Dispose(false); }
            catch (Exception ex)
            {
                // Swallow: Consider logging.
#pragma warning disable CS8604 // CS8604 - Possible null reference argument for parameter.
                DisposableEventSource.Log.FinalizerThrew(GetType().FullName, ex.Message);
#pragma warning restore CS8604 // CS8604 - Possible null reference argument for parameter.
            }
        }
    }
}
