using System;
using System.Linq;
using System.Messaging;
using System.Threading;
using System.Xml.Linq;

using com.espertech.esper.client;
using com.espertech.esper.events.xml;

namespace NEsper.Examples.MSMQ
{
    class MsmqConsumer
    {
        /// <summary>
        /// Message queue
        /// </summary>
        private MessageQueue _messageQueue;

        /// <summary>
        /// Event handler for this consumer
        /// </summary>
        private EventHandler<UpdateEventArgs> _eventHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsmqConsumer"/> class.
        /// </summary>
        /// <param name="mqPath">The mq path.</param>
        /// <param name="eventHandler"></param>
        public MsmqConsumer(string mqPath, EventHandler<UpdateEventArgs> eventHandler)
        {
            if (!MessageQueue.Exists(mqPath)) {
                throw new ArgumentException("invalid msmq path", "mqPath");
            }

            _eventHandler = eventHandler;
            _messageQueue = new MessageQueue(mqPath);
            _messageQueue.Formatter = new BinaryMessageFormatter();
            _messageQueue.ReceiveCompleted += FinishReceive;
            _messageQueue.BeginReceive();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _eventHandler = null;

            var messageQueue = Interlocked.Exchange(ref _messageQueue, null);
            if (messageQueue != null) {
                messageQueue.Close();
                messageQueue.Dispose();
            }
        }

        /// <summary>
        /// Finishes the asynchronous receive.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Messaging.ReceiveCompletedEventArgs"/> instance containing the event data.</param>
        private void FinishReceive(object sender, ReceiveCompletedEventArgs e)
        {
            if (_messageQueue == null) {
                return;
            }

            try
            {
                _messageQueue.EndReceive(e.AsyncResult);
                // receive the current message
                var message = e.Message;
                // process the message
                var eventElement = XElement.Parse((string) message.Body);
                // find the xelements that represent the old and new event beans
                // and convert them back into an eventBean form
                var oldEvents = eventElement
                    .Element(XName.Get("old"))
                    .Elements()
                    .Select(element => new XEventBean(element, null))
                    .ToArray();
                var newEvents = eventElement
                    .Element(XName.Get("new"))
                    .Elements()
                    .Select(element => new XEventBean(element, null))
                    .ToArray();

                // construct event args
                var updateEventArgs = new UpdateEventArgs(
                    null,
                    null,
                    newEvents,
                    oldEvents);

                // send the event(s) along
                _eventHandler.Invoke(
                    null, updateEventArgs);
            } catch(MessageQueueException) {
            } catch(NullReferenceException) {
            } finally {
                // begin receiving the next message
                _messageQueue.BeginReceive();
            }
        }
    }
}
