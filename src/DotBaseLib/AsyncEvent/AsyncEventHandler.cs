/*

MIT License

Copyright (c) David Fallah <davidfallah@atp-group.com>

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


History:
[22/10/2025] Forked from: https://github.com/TAGC/AsyncEvent/commits/master/src/AsyncEvent/AsyncEventHandler.cs

*/


using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotBase.AsyncEvent
{
    /// <summary>
    /// Represents an asynchronous event handler.
    /// </summary>
    /// <param name="sender">The object firing the event.</param>
    /// <param name="eventArgs">The object containing the event data.</param>
    /// <returns>A task that completes when this handler is done handling the event.</returns>
    public delegate Task AsyncEventHandler(object sender, EventArgs eventArgs);

    /// <summary>
    /// Represents an asynchronous event handler.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <param name="sender">The object firing the event.</param>
    /// <param name="eventArgs">The object containing the event data.</param>
    /// <returns>A task that completes when this handler is done handling the event.</returns>
    public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs eventArgs);

    /// <summary>
    /// Provides extension methods for use with <see cref="AsyncEventHandler" /> and
    /// <see cref="AsyncEventHandler{TEventArgs}" />, as well as functions to convert synchronous event handlers to
    /// asynchronous event handlers.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Converts a synchronous event handler to an asynchronous event handler that performs the same actions and returns
        /// <see cref="Task.CompletedTask" />.
        /// </summary>
        /// <param name="eventHandler">The synchronous event handler.</param>
        /// <returns>An asynchronous event handler that performs the same logic and returns a completed task.</returns>
        public static AsyncEventHandler Async(EventHandler eventHandler)
        {
            return (sender, eventArgs) =>
            {
                var delegates = eventHandler.GetInvocationList().Cast<EventHandler>();
                var tasks = delegates.Select(it => Task.Run(() => it.Invoke(sender, eventArgs)));
                return Task.WhenAll(tasks);
            };
        }

        /// <summary>
        /// Converts a synchronous event handler to an asynchronous event handler that performs the same actions and returns
        /// <see cref="Task.CompletedTask" />.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
        /// <param name="eventHandler">The synchronous event handler.</param>
        /// <returns>An asynchronous event handler that performs the same logic and returns a completed task.</returns>
        public static AsyncEventHandler<TEventArgs> Async<TEventArgs>(EventHandler<TEventArgs> eventHandler)
        {
            return (sender, eventArgs) =>
            {
                var delegates = eventHandler.GetInvocationList().Cast<EventHandler<TEventArgs>>();
                var tasks = delegates.Select(it => Task.Run(() => it.Invoke(sender, eventArgs)));
                return Task.WhenAll(tasks);
            };
        }

        /// <summary>
        /// Asynchronously invokes an event, dispatching the provided event arguments to all registered handlers.
        /// </summary>
        /// <param name="eventHandler">This event handler.</param>
        /// <param name="sender">The object firing the event.</param>
        /// <param name="eventArgs">The object containing the event data.</param>
        /// <returns>
        /// A <see cref="Task"/> that completes only when all registered handlers complete. A completed task is returned
        /// if the event handler is <c>null</c>.
        /// </returns>
        public static Task InvokeAsync(this AsyncEventHandler eventHandler, object sender, EventArgs eventArgs)
        {
            if (eventHandler == null)
            {
                return Task.CompletedTask;
            }

            var delegates = eventHandler.GetInvocationList().Cast<AsyncEventHandler>();
            var tasks = delegates.Select(it => it.Invoke(sender, eventArgs));

            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Asynchronously invokes an event, dispatching the provided event arguments to all registered handlers.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
        /// <param name="eventHandler">This event handler.</param>
        /// <param name="sender">The object firing the event.</param>
        /// <param name="eventArgs">The object containing the event data.</param>
        /// <returns>
        /// A <see cref="Task"/> that completes only when all registered handlers complete. A completed task is returned
        /// if the event handler is <c>null</c>.
        /// </returns>
        public static Task InvokeAsync<TEventArgs>(
            this AsyncEventHandler<TEventArgs> eventHandler,
            object sender,
            TEventArgs eventArgs)
        {
            if (eventHandler == null)
            {
                return Task.CompletedTask;
            }

            var delegates = eventHandler.GetInvocationList().Cast<AsyncEventHandler<TEventArgs>>();
            var tasks = delegates.Select(it => it.Invoke(sender, eventArgs));

            return Task.WhenAll(tasks);
        }
    }
}
