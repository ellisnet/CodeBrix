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
using System.Threading.Tasks;

using Xamarin.Forms;

// ReSharper disable once CheckNamespace
namespace CodeBrix.Helpers
{
    public class MainThreadInvoker : IDisposable
    {
        private Func<Task> _functionToInvoke;
        private Action _syncActionToInvoke;
        private Action _asyncActionToInvoke;

        private TaskCompletionSource<bool> _taskSource;

        public async Task Invoke(Func<Task> functionToInvoke)
        {
            _functionToInvoke = functionToInvoke ?? throw new ArgumentNullException(nameof(functionToInvoke));

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                case Device.iOS:
                //TODO: Confirm this code works on UWP
                case Device.UWP:
                    _taskSource = new TaskCompletionSource<bool>();
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await _functionToInvoke.Invoke();
                        _taskSource.SetResult(true);
                    });
                    await _taskSource.Task;
                    break;
                default:
                    await functionToInvoke.Invoke();
                    break;
            }
        }

        //TODO: If not, the following code is the correct way to invoke on the main thread on UWP:
        /*
             protected virtual Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> functionToExecute) 
             {
                var completionSource = new TaskCompletionSource<T>();

                // ReSharper disable once UnusedVariable
                var disp = Window.Current.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {
                    try {
                        T result = await functionToExecute.Invoke();
                        completionSource.SetResult(result);
                    }
                    catch (Exception ex) {
                        completionSource.SetException(ex);
                    }
                });

                return completionSource.Task;
            }
         */

        public async Task Invoke(Action actionToInvoke)
        {
            _syncActionToInvoke = actionToInvoke ?? throw new ArgumentNullException(nameof(actionToInvoke));

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                case Device.iOS:
                //TODO: Confirm this code works on UWP
                case Device.UWP:
                    _taskSource = new TaskCompletionSource<bool>();
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        _syncActionToInvoke.Invoke();
                        _taskSource.SetResult(true);
                    });
                    await _taskSource.Task;
                    break;
                default:
                    actionToInvoke.Invoke();
                    break;
            }
        }

        /// <summary>
        /// Executes the given operation on a different thread, asynchronously - fire and forget.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="actionToInvoke">The action to invoke.</param>
        public void BeginInvoke(Action actionToInvoke)
        {
            _asyncActionToInvoke = actionToInvoke ?? throw new ArgumentNullException(nameof(actionToInvoke));

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                case Device.iOS:
                //TODO: Confirm this code works on UWP
                case Device.UWP:
                    Device.BeginInvokeOnMainThread(_asyncActionToInvoke);
                    break;
                default:
                    _asyncActionToInvoke.Invoke();
                    break;
            }
        }

        public void Dispose()
        {
            _functionToInvoke = null;
            _syncActionToInvoke = null;
            _asyncActionToInvoke = null;
            _taskSource = null;
        }
    }

    public class MainThreadInvoker<TResult> : IDisposable
    {
        private Func<Task<TResult>> _functionToInvoke;
        private TaskCompletionSource<TResult> _taskSource;

        public async Task<TResult> Invoke(Func<Task<TResult>> functionToInvoke)
        {
            _functionToInvoke = functionToInvoke ?? throw new ArgumentNullException(nameof(functionToInvoke));

            TResult result;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                case Device.iOS:
                //TODO: Confirm this code works on UWP
                case Device.UWP:
                    _taskSource = new TaskCompletionSource<TResult>();
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        TResult invokedResult = await _functionToInvoke.Invoke();
                        _taskSource.SetResult(invokedResult);
                    });
                    result = await _taskSource.Task;
                    break;
                default:
                    result = await functionToInvoke.Invoke();
                    break;
            }

            return result;
        }

        public void Dispose()
        {
            _functionToInvoke = null;
            _taskSource = null;
        }
    }
}
