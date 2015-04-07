///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Returns a facility to process event objects that are of a known type.
    /// <para />
    /// Obtained via the method <seealso cref="EPRuntime.GetEventSender(string)" /> the sender is 
    /// specific to a given event type and may not process event objects of any other event 
    /// type; See the method documentatiom for more details.
    /// <para/> 
    /// Obtained via the method <seealso cref="EPRuntime.GetEventSender(Uri[])" /> the sender cooperates 
    /// with plug-in event representations to reflect upon the event object to determine an 
    /// appropriate event type to process the event.
    /// </summary>
    public interface EventSender
    {
        /// <summary>
        /// Processes the event object.
        /// <para />
        /// Use the route method for sending events into the runtime from within event code. 
        /// to avoid the possibility of a stack overflow due to nested calls to SendEvent.
        /// </summary>
        /// <param name="theEvent">to process</param>
        /// <throws>EPException if a runtime error occured.</throws>
        void SendEvent(Object theEvent);
    
        /// <summary>
        /// Route the event object back to the event stream processing runtime for internal 
        /// dispatching, to avoid the possibility of a stack overflow due to nested calls to 
        /// SendEvent. The route event is processed just like it was sent to the runtime,
        /// that is any active expressions seeking that event receive it. The routed event 
        /// has priority over other events sent to the runtime. In a single-threaded application 
        /// the routed event is processed before the next event is sent to the runtime through 
        /// the EPRuntime.SendEvent method.
        /// </summary>
        /// <param name="theEvent">to process</param>
        /// <throws>EPException is thrown when the processing of the event lead to an error</throws>
        void Route(Object theEvent);
    }

    /// <summary>
    /// Returns a facility to process event objects that are of a known type.
    /// Obtained via the method <see cref="EPRuntime.GetEventSender(String)"/> the sender 
    /// is specific to a given event type and may not process event objects of any
    /// other event type; See the method documentatiom for more details.
    /// <para/>
    /// Obtained via the method <see cref="EPRuntime.GetEventSender(Uri[])"/> the
    /// sender cooperates with plug-in event representations to reflect upon the event
    /// object to determine an appropriate event type to process the event. 
    /// </summary>
    public delegate void EventSenderDelegate(Object theEvent);
}
