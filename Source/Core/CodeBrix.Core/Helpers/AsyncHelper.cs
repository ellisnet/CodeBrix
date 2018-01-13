/* Copyright 2018 Ellisnet - Jeremy Ellis (jeremy@ellisnet.com)

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CodeBrix.Helpers
{
    /// <summary>
    /// From here:  http://stackoverflow.com/questions/5095183/how-would-i-run-an-async-taskt-method-synchronously
    /// </summary>
    public static class AsyncHelper
    {
        /// <summary>
        /// Execute's an async Task&lt;T&gt; method which has a void return value synchronously.
        /// </summary>
        /// <param name="task">Task&lt;T&gt; method to execute.</param>
        public static void RunSync(Func<Task> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            synch.Post(async _ =>
            {
                try
                {
                    await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();

            SynchronizationContext.SetSynchronizationContext(oldContext);
        }

        /// <summary>
        /// Execute's an async Task&lt;T&gt; method which has a T return type synchronously.
        /// </summary>
        /// <typeparam name="T">Return Type.</typeparam>
        /// <param name="task">Task&lt;T&gt; method to execute.</param>
        /// <returns>
        /// A T.
        /// </returns>
        public static T RunSync<T>(Func<Task<T>> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            T ret = default(T);
            synch.Post(async _ =>
            {
                try
                {
                    ret = await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
            return ret;
        }

        /// <summary>
        /// An exclusive synchronization context.
        /// </summary>
        private class ExclusiveSynchronizationContext : SynchronizationContext
        {
            /// <summary>
            /// true to done.
            /// </summary>
            private bool _done;

            /// <summary>
            /// Gets or sets the inner exception.
            /// </summary>
            /// <value>
            /// The inner exception.
            /// </value>
            public Exception InnerException { private get; set; }

            /// <summary>
            /// The work items waiting.
            /// </summary>
            private readonly AutoResetEvent _workItemsWaiting = new AutoResetEvent(false);

            /// <summary>
            /// The items.
            /// </summary>
            private readonly Queue<Tuple<SendOrPostCallback, object>> _items =
                new Queue<Tuple<SendOrPostCallback, object>>();

            /// <summary>
            /// When overridden in a derived class, dispatches a synchronous message to a synchronization
            /// context.
            /// </summary>
            /// <exception cref="NotSupportedException">Thrown when the requested operation is not supported.</exception>
            /// <param name="d">    The <see cref="T:System.Threading.SendOrPostCallback" />
            ///  delegate to call.</param>
            /// <param name="state">The object passed to the delegate.</param>
            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("We cannot send to our same thread");
            }

            /// <summary>
            /// When overridden in a derived class, dispatches an asynchronous message to a synchronization
            /// context.
            /// </summary>
            /// <param name="d">    The <see cref="T:System.Threading.SendOrPostCallback" />
            ///  delegate to call.</param>
            /// <param name="state">The object passed to the delegate.</param>
            public override void Post(SendOrPostCallback d, object state)
            {
                lock (_items)
                {
                    _items.Enqueue(Tuple.Create(d, state));
                }
                _workItemsWaiting.Set();
            }

            /// <summary>
            /// Ends message loop.
            /// </summary>
            public void EndMessageLoop()
            {
                Post(_ => _done = true, null);
            }

            /// <summary>
            /// Begins message loop.
            /// </summary>
            /// <exception cref="AggregateException">Thrown when an Aggregate error condition occurs.</exception>
            public void BeginMessageLoop()
            {
                while (!_done)
                {
                    Tuple<SendOrPostCallback, object> task = null;
                    lock (_items)
                    {
                        if (_items.Count > 0)
                        {
                            task = _items.Dequeue();
                        }
                    }
                    if (task != null)
                    {
                        task.Item1(task.Item2);
                        if (InnerException != null) // the method threw an exeption
                        {
                            throw new AggregateException("AsyncHelper.RunSync method threw an exception.", InnerException);
                        }
                    }
                    else
                    {
                        _workItemsWaiting.WaitOne();
                    }
                }
            }

            /// <summary>
            /// Creates the copy.
            /// </summary>
            /// <returns>
            /// The new copy.
            /// </returns>
            public override SynchronizationContext CreateCopy()
            {
                return this;
            }
        }
    }
}
