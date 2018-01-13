//Adapted from here: https://github.com/xamarin/Xamarin.Forms/blob/a03c8f32d20a61a1a553b7db0e2f978c44a4ca84/docs/Xamarin.Forms.Core/Xamarin.Forms/IMessagingCenter.xml

using System;

namespace CodeBrix.Services
{
    /// <summary>
    /// Interface for abstraction of the Xamarin.Forms MessagingCenter - e.g. for testing
    /// </summary>
    public interface IMessagingService
    {
        /// <typeparam name="TSender">The type of object that sends the message.</typeparam>
        /// <typeparam name="TArgs">The type of the objects that are used as message arguments for the message.</typeparam>
        /// <param name="sender">The instance that is sending the message. Typically, this is specified with the <see langword="this" /> keyword used within the sending object.</param>
        /// <param name="message">The message that will be sent to objects that are listening for the message from instances of type <typeparamref name="TSender" />.</param>
        /// <param name="args">The arguments that will be passed to the listener's callback.</param>
        /// <summary>Sends a named message with the specified arguments.</summary>
        /// <remarks>To be added.</remarks>
        void Send<TSender, TArgs>(TSender sender, string message, TArgs args) where TSender : class;
        /// <typeparam name="TSender">The type of object that sends the message.</typeparam>
        /// <param name="sender">The instance that is sending the message. Typically, this is specified with the <see langword="this" /> keyword used within the sending object.</param>
        /// <param name="message">The message that will be sent to objects that are listening for the message from instances of type <typeparamref name="TSender" />.</param>
        /// <summary>Sends a named message that has no arguments.</summary>
        /// <remarks>To be added.</remarks>
        void Send<TSender>(TSender sender, string message) where TSender : class;
        /// <typeparam name="TSender">The type of object that sends the message.</typeparam>
        /// <typeparam name="TArgs">The type of the objects that are used as message arguments for the message.</typeparam>
        /// <param name="subscriber">The object that is subscribing to the messages. Typically, this is specified with the <see langword="this" /> keyword used within the subscribing object.</param>
        /// <param name="message">The message that will be sent to objects that are listening for the message from instances of type <typeparamref name="TSender" />.</param>
        /// <param name="callback">A callback, which takes the sender and arguments as parameters, that is run when the message is received by the subscriber.</param>
        /// <param name="source">The object that will send the messages.</param>
        /// <summary>Run the <paramref name="callback" /> on <paramref name="subscriber" /> in response to parameterized messages that are named <paramref name="message" /> and that are created by <paramref name="source" />.</summary>
        /// <remarks>To be added.</remarks>
        void Subscribe<TSender, TArgs>(object subscriber, string message, Action<TSender, TArgs> callback, TSender source = null) where TSender : class;
        /// <typeparam name="TSender">The type of object that sends the message.</typeparam>
        /// <param name="subscriber">The object that is subscribing to the messages. Typically, this is specified with the <see langword="this" /> keyword used within the subscribing object.</param>
        /// <param name="message">The message that will be sent to objects that are listening for the message from instances of type <typeparamref name="TSender" />.</param>
        /// <param name="callback">A callback, which takes the sender and arguments as parameters, that is run when the message is received by the subscriber.</param>
        /// <param name="source">The object that will send the messages.</param>
        /// <summary>Run the <paramref name="callback" /> on <paramref name="subscriber" /> in response to messages that are named <paramref name="message" /> and that are created by <paramref name="source" />.</summary>
        /// <remarks>To be added.</remarks>
        void Subscribe<TSender>(object subscriber, string message, Action<TSender> callback, TSender source = null) where TSender : class;
        /// <typeparam name="TSender">The type of object that sends the message.</typeparam>
        /// <typeparam name="TArgs">The type of the objects that are used as message arguments for the message.</typeparam>
        /// <param name="subscriber">The object that is subscribing to the messages. Typically, this is specified with the <see langword="this" /> keyword used within the subscribing object.</param>
        /// <param name="message">The message that will be sent to objects that are listening for the message from instances of type <typeparamref name="TSender" />.</param>
        /// <summary>Unsubscribes from the specified parameterless subscriber messages.</summary>
        /// <remarks>To be added.</remarks>
        void Unsubscribe<TSender, TArgs>(object subscriber, string message) where TSender : class;
        /// <typeparam name="TSender">The type of object that sends the message.</typeparam>
        /// <param name="subscriber">The object that is subscribing to the messages. Typically, this is specified with the <see langword="this" /> keyword used within the subscribing object.</param>
        /// <param name="message">The message that will be sent to objects that are listening for the message from instances of type <typeparamref name="TSender" />.</param>
        /// <summary>Unsubscribes a subscriber from the specified messages that come from the specified sender.</summary>
        /// <remarks>To be added.</remarks>
        void Unsubscribe<TSender>(object subscriber, string message) where TSender : class;
    }
}
