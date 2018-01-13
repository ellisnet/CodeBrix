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
using Xamarin.Forms;

using CodeBrix.Services;

namespace CodeBrix.Forms.Services
{
    /// <summary>
    /// A wrapper for the Xamarin.Forms MessagingCenter - so that this implementation can be swapped out for a
    /// different implementation of IMessagingService for testing purposes.
    /// </summary>
    public class XamarinMessagingService : IMessagingService
    {
        /// <typeparam name="TSender">The type of object that sends the message.</typeparam>
        /// <typeparam name="TArgs">The type of the objects that are used as message arguments for the message.</typeparam>
        /// <param name="sender">The instance that is sending the message. Typically, this is specified with the <see langword="this" /> keyword used within the sending object.</param>
        /// <param name="message">The message that will be sent to objects that are listening for the message from instances of type <typeparamref name="TSender" />.</param>
        /// <param name="args">The arguments that will be passed to the listener's callback.</param>
        /// <summary>Sends a named message with the specified arguments.</summary>
        /// <remarks>To be added.</remarks>
        public void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class
        {
            MessagingCenter.Instance.Send<TSender, TArgs>(sender, message, args);
        }

        /// <typeparam name="TSender">The type of object that sends the message.</typeparam>
        /// <param name="sender">The instance that is sending the message. Typically, this is specified with the <see langword="this" /> keyword used within the sending object.</param>
        /// <param name="message">The message that will be sent to objects that are listening for the message from instances of type <typeparamref name="TSender" />.</param>
        /// <summary>Sends a named message that has no arguments.</summary>
        /// <remarks>To be added.</remarks>
        public void Send<TSender>(TSender sender, string message) where TSender : class
        {
            MessagingCenter.Instance.Send<TSender>(sender, message);
        }

        /// <typeparam name="TSender">The type of object that sends the message.</typeparam>
        /// <typeparam name="TArgs">The type of the objects that are used as message arguments for the message.</typeparam>
        /// <param name="subscriber">The object that is subscribing to the messages. Typically, this is specified with the <see langword="this" /> keyword used within the subscribing object.</param>
        /// <param name="message">The message that will be sent to objects that are listening for the message from instances of type <typeparamref name="TSender" />.</param>
        /// <param name="callback">A callback, which takes the sender and arguments as parameters, that is run when the message is received by the subscriber.</param>
        /// <param name="source">The object that will send the messages.</param>
        /// <summary>Run the <paramref name="callback" /> on <paramref name="subscriber" /> in response to parameterized messages that are named <paramref name="message" /> and that are created by <paramref name="source" />.</summary>
        /// <remarks>To be added.</remarks>
        public void Subscribe<TSender, TArgs>(object subscriber, string message, Action<TSender, TArgs> callback, TSender source = default(TSender)) where TSender : class
        {
            MessagingCenter.Instance.Subscribe<TSender, TArgs>(subscriber, message, callback, source);
        }

        /// <typeparam name="TSender">The type of object that sends the message.</typeparam>
        /// <param name="subscriber">The object that is subscribing to the messages. Typically, this is specified with the <see langword="this" /> keyword used within the subscribing object.</param>
        /// <param name="message">The message that will be sent to objects that are listening for the message from instances of type <typeparamref name="TSender" />.</param>
        /// <param name="callback">A callback, which takes the sender and arguments as parameters, that is run when the message is received by the subscriber.</param>
        /// <param name="source">The object that will send the messages.</param>
        /// <summary>Run the <paramref name="callback" /> on <paramref name="subscriber" /> in response to messages that are named <paramref name="message" /> and that are created by <paramref name="source" />.</summary>
        /// <remarks>To be added.</remarks>
        public void Subscribe<TSender>(object subscriber, string message, Action<TSender> callback, TSender source = default(TSender)) where TSender : class
        {
            MessagingCenter.Instance.Subscribe<TSender>(subscriber, message, callback, source);
        }

        /// <typeparam name="TSender">The type of object that sends the message.</typeparam>
        /// <typeparam name="TArgs">The type of the objects that are used as message arguments for the message.</typeparam>
        /// <param name="subscriber">The object that is subscribing to the messages. Typically, this is specified with the <see langword="this" /> keyword used within the subscribing object.</param>
        /// <param name="message">The message that will be sent to objects that are listening for the message from instances of type <typeparamref name="TSender" />.</param>
        /// <summary>Unsubscribes from the specified parameterless subscriber messages.</summary>
        /// <remarks>To be added.</remarks>
        public void Unsubscribe<TSender, TArgs>(object subscriber, string message) where TSender : class
        {
            MessagingCenter.Instance.Unsubscribe<TSender, TArgs>(subscriber, message);
        }

        /// <typeparam name="TSender">The type of object that sends the message.</typeparam>
        /// <param name="subscriber">The object that is subscribing to the messages. Typically, this is specified with the <see langword="this" /> keyword used within the subscribing object.</param>
        /// <param name="message">The message that will be sent to objects that are listening for the message from instances of type <typeparamref name="TSender" />.</param>
        /// <summary>Unsubscribes a subscriber from the specified messages that come from the specified sender.</summary>
        /// <remarks>To be added.</remarks>
        public void Unsubscribe<TSender>(object subscriber, string message) where TSender : class
        {
            MessagingCenter.Instance.Unsubscribe<TSender>(subscriber, message);
        }
    }
}
