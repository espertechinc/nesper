///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.runtime.client;

namespace com.espertech.esperio
{
    /// <summary>
    /// Sender that abstracts the send process in terms of threading or further pre-processing.
    /// </summary>
    public abstract class AbstractSender
    {
        /// <summary>Set the engine runtime to use. </summary>
        public EPEventService Runtime { get; set; }

        /// <summary>
        /// Send an event
        /// </summary>
        /// <param name="theEvent">The event.</param>
        /// <param name="beanToSend">event object</param>
        /// <param name="eventTypeName"></param>
        public abstract void SendEvent(
            AbstractSendableEvent theEvent,
            object beanToSend,
            string eventTypeName);

        /// <summary>
        /// Send an event.
        /// </summary>
        /// <param name="theEvent">The event.</param>
        /// <param name="mapToSend">event object</param>
        /// <param name="eventTypeName">name of event</param>
        public abstract void SendEvent(
            AbstractSendableEvent theEvent, 
            IDictionary<string, object> mapToSend,
            string eventTypeName);

        /// <summary>Indicate that sender should stop. </summary>
        public abstract void OnFinish();
    }
}