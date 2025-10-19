namespace DotBase.Log;


internal sealed class DebugDisposalTracker
{
	public static int UNDISPOSED_COUNT = 0;
	public static int DISPOSED_COUNT = 0;
	public static int FINALIZED_COUNT = 0;


	private readonly string _typeName;
	private readonly string _allocationStack;
	private bool _disposed;


	private DebugDisposalTracker(object owner)
	{
		Interlocked.Increment(ref UNDISPOSED_COUNT);
		var t = owner.GetType();
		_typeName = t.FullName ?? t.Name;
		_allocationStack = Environment.StackTrace;
		DisposableEventSource.Log.Created(_typeName);
	}


	public static DebugDisposalTracker Attach(object owner) => new DebugDisposalTracker(owner);


	public void MarkDisposed()
	{
		Interlocked.Increment(ref DISPOSED_COUNT);
		_disposed = true;
		DisposableEventSource.Log.Disposed(_typeName);
	}


	~DebugDisposalTracker()
	{
		if (!_disposed)
		{
			Interlocked.Increment(ref FINALIZED_COUNT);
			DisposableEventSource.Log.FinalizedWithoutDispose(_typeName, _allocationStack);
		}
	}
}
