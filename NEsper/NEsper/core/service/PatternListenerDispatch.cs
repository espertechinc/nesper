///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.dispatch;

namespace com.espertech.esper.core.service
{
    /// <summary>Dispatchable for dispatching events to update listeners.</summary>
    public class PatternListenerDispatch : Dispatchable {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ISet<UpdateListener> listeners;
    
        private EventBean singleEvent;
        private List<EventBean> eventList;
    
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="listeners">is the listeners to dispatch to.</param>
        public PatternListenerDispatch(ISet<UpdateListener> listeners) {
            this.listeners = listeners;
        }
    
        /// <summary>
        /// Add an event to be dispatched.
        /// </summary>
        /// <param name="theEvent">to add</param>
        public void Add(EventBean theEvent) {
            if (singleEvent == null) {
                singleEvent = theEvent;
            } else {
                if (eventList == null) {
                    eventList = new List<EventBean>(5);
                    eventList.Add(singleEvent);
                }
                eventList.Add(theEvent);
            }
        }
    
        public void Execute() {
            EventBean[] eventArray;
    
            if (eventList != null) {
                eventArray = eventList.ToArray(new EventBean[eventList.Count]);
                eventList = null;
                singleEvent = null;
            } else {
                eventArray = new EventBean[]{singleEvent};
                singleEvent = null;
            }
    
            foreach (UpdateListener listener in listeners) {
                try {
                    listener.Update(eventArray, null);
                } catch (Throwable t) {
                    string message = "Unexpected exception invoking listener update method on listener class '" + listener.Class.SimpleName +
                            "' : " + t.Class.SimpleName + " : " + t.Message;
                    Log.Error(message, t);
                }
            }
        }
    
        /// <summary>
        /// Returns true if at least one event has been added.
        /// </summary>
        /// <returns>true if it has data, false if not</returns>
        public bool HasData() {
            if (singleEvent != null) {
                return true;
            }
            return false;
        }
    }
} // end of namespace
