namespace DotBase.Tools;


public static class TaskTools
{
    /// <summary>
    /// 
    /// Microsoft built an AsyncHelper (internal) class to run Async as Sync.
    /// The source code is from this SO answer: <a href="https://stackoverflow.com/a/25097498"/>
    /// 
    /// </summary>
    private static readonly TaskFactory _taskFactory = new
      TaskFactory(CancellationToken.None,
                  TaskCreationOptions.None,
                  TaskContinuationOptions.None,
                  TaskScheduler.Default);

    public static TResult RunSync<TResult>(Func<Task<TResult>> func)
    {
        return TaskTools._taskFactory
          .StartNew<Task<TResult>>(func)
          .Unwrap<TResult>()
          .GetAwaiter()
          .GetResult();
    }

    public static void RunSync(Action func, CancellationToken cancellation)
    {
        _taskFactory
          .StartNew(func, cancellation)
          .GetAwaiter()
          .GetResult();
    }


    /// <summary>
    /// 
    /// Will NOT throw if cancelled and will NOT invoke <paramref name="func"/>.
    /// 
    /// <para>FYI: Option <see cref="TaskContinuationOptions.NotOnCanceled"/> is
    /// <c>NOT</c> valid for multi-task continuations, and that is exactly what
    /// we have here.</para>
    /// 
    /// </summary>
    public static Task RunSyncDelayed(Action func, int miliseconds, CancellationToken cancellation)
    {
        return SafeTaskDelay(miliseconds, cancellation)
            .ContinueWith((task) => 
            {
                if (task.IsCompletedSuccessfully && !cancellation.IsCancellationRequested)
                {
                    func();
                }
            }, TaskScheduler.Default);
    }

    private class RestoreOnDispose : IDisposable
    {
        private readonly ExecutionContext? _captured;

        public RestoreOnDispose(ExecutionContext? captured)
        {
            _captured = captured;
        }

        public void Dispose()
        {
            if (_captured is not null)
            {
                ExecutionContext.Restore(_captured);
            }
        }
    }

    public static IDisposable IsolatedScope()
    {
        var captured = ExecutionContext.Capture();
        ExecutionContext.SuppressFlow();
        return new RestoreOnDispose(captured);
    }

    /// <summary>
    /// 
    /// Cancellable Task.Delay with a CancellationToken but without throwing a
    /// TaskCanceledException by adding an empty continuation.
    /// 
    /// <para>FYI: Option <see cref="TaskContinuationOptions.NotOnCanceled"/> is <c>NOT</c> valid for multi-task continuations. </para>
    /// </summary>
    public static Task SafeTaskDelay(int miliseconds, CancellationToken cancellation)
    {
        try
        {
            return Task.Delay(miliseconds, cancellation).ContinueWith((_) => { });
        }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) { }

        return Task.CompletedTask;
    }

    public static Task<bool> WaitAsync(ManualResetEventSlim mes, int timeout = Timeout.Infinite)
    {
        if (mes.IsSet)
            return Task.FromResult(true);

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        // Register for internal event changes
        var registration = ThreadPool.RegisterWaitForSingleObject(
            mes.WaitHandle,
            (state, timedOut) => tcs.TrySetResult(!timedOut && mes.IsSet),
            null,
            timeout,
            true
        );

        // Cleanup when the task completes
        tcs.Task.ContinueWith(
        _ =>
        {
            registration.Unregister(null);
        }, TaskContinuationOptions.RunContinuationsAsynchronously);

        return tcs.Task;
    }

    public static Task<bool> WaitAsync(ManualResetEventSlim mes, CancellationToken cancellation)
    {
        if (mes.IsSet)
            return Task.FromResult(true);

        if (cancellation.IsCancellationRequested)
            return Task.FromResult(false);

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var registration = ThreadPool.RegisterWaitForSingleObject(
            mes.WaitHandle,
            (state, timedOut) =>
            {
                // timedOut should be false since infinite; result based on set state
                tcs.TrySetResult(mes.IsSet);
            },
            null,
            Timeout.Infinite,
            true
        );

        CancellationTokenRegistration ctr = default;
        if (cancellation.CanBeCanceled)
        {
            ctr = cancellation.Register(() =>
            {
                registration.Unregister(mes.WaitHandle); // Unregister and signal to break wait
                tcs.TrySetResult(false);
            });
        }

        // Cleanup when the task completes
        tcs.Task.ContinueWith(
            _ =>
            {
                registration.Unregister(null);
                ctr.Dispose();
            },
            TaskContinuationOptions.ExecuteSynchronously
        );

        return tcs.Task;
    }
}
