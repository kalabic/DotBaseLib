using DotBase.Core;

namespace DotBase.Cancellation;

/// <summary>
/// Owns a set of cancellation token sources, requests cancellation without running
/// callbacks on the calling thread, and defers source disposal until callbacks
/// initiated by this set have completed.
/// </summary>
public abstract class CancellationSet : DisposableBase
{
    private readonly object _lock = new();

    private readonly List<CancellationTokenSource> _sources = new();

    private readonly List<Task> _pendingCancellations = new();

    protected CancellationTokenSource RegisterCancellationSource(
        CancellationTokenSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        lock (_lock)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            _sources.Add(source);
        }

        return source;
    }

    protected void RequestCancellation(CancellationTokenSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        lock (_lock)
        {
            if (!IsDisposed)
            {
                RequestCancellationLocked(source);
            }
        }
    }

    protected void RequestCancellation(
        IEnumerable<CancellationTokenSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);

        lock (_lock)
        {
            if (IsDisposed)
            {
                return;
            }

            foreach (CancellationTokenSource source in sources)
            {
                ArgumentNullException.ThrowIfNull(source);
                RequestCancellationLocked(source);
            }
        }
    }

    /// <summary>
    /// 
    /// DisposableBase.Dispose() guarantees that the normal disposal path begins only once.
    /// 
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            base.Dispose(disposing);
            return;
        }

        CancellationTokenSource[] sources;
        Task pendingCancellation;

        lock (_lock)
        {
            sources = _sources.ToArray();
            _sources.Clear();
            pendingCancellation = Task.WhenAll(_pendingCancellations);
            _pendingCancellations.Clear();
        }

        if (pendingCancellation.IsCompleted)
        {
            DisposeSources(sources);
        }
        else
        {
            _ = pendingCancellation.ContinueWith(
                _ => DisposeSources(sources),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        base.Dispose(disposing);
    }

    private void RequestCancellationLocked(CancellationTokenSource source)
    {
        if (!source.IsCancellationRequested)
        {
            _pendingCancellations.Add(source.CancelAsync());
        }
    }

    private static void DisposeSources(IEnumerable<CancellationTokenSource> sources)
    {
        foreach (CancellationTokenSource source in sources)
        {
            source.Dispose();
        }
    }
}
