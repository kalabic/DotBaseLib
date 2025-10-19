namespace DotBase.Core;


public interface IDisposableInfo
{
    bool IsDisposed { get; }

    STATE DisposeState { get; }

    public enum STATE : int
    {
        NOT_DISPOSED,
        DISPOSED,

        /// <summary>
        /// Should appear ONLY when built with DEBUG_UNDISPOSED.
        /// <para><c>Even when enabled, if you manage to see an object with FINALIZED status something VERY wrong is happening.</c></para>
        /// </summary>
        FINALIZED
    }
}
