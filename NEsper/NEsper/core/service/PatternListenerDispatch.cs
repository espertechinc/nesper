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
using com.espertech.esper.compat.logging;
using com.espertech.esper.dispatch;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Dispatchable for dispatching events to Update listeners.
    /// </summary>

    public class PatternListenerDispatch : Dispatchable
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EPStatement _statement;
        private readonly EPServiceProvider _serviceProvider;
        private readonly ICollection<UpdateEventHandler> _eventHandlers;
        private EventBean _singleEvent;
        private List<EventBean> _eventList;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="statement">The statement.</param>
        /// <param name="eventHandlers">The event handlers.</param>

        public PatternListenerDispatch(EPServiceProvider serviceProvider,
                                       EPStatement statement,
                                       ICollection<UpdateEventHandler> eventHandlers)
        {
            _serviceProvider = serviceProvider;
            _statement = statement;
            _eventHandlers = eventHandlers;
        }

        /// <summary>
        /// Add an event to be dispatched.
        /// </summary>
        /// <param name="theEvent">event to add</param>

        public virtual void Add(EventBean theEvent)
        {
            if (_singleEvent == null)
            {
                _singleEvent = theEvent;
            }
            else
            {
                if (_eventList == null)
                {
                    _eventList = new List<EventBean>();
                    _eventList.Add(_singleEvent);
                }

                _eventList.Add(theEvent);
            }
        }

        /// <summary>
        /// Fires the Update event.
        /// </summary>
        /// <param name="newEvents">The new events.</param>
        /// <param name="oldEvents">The old events.</param>
        protected void FireUpdateEvent(EventBean[] newEvents, EventBean[] oldEvents)
        {
            if ((_eventHandlers != null) && (_eventHandlers.Count != 0))
            {
                var e = new UpdateEventArgs(_serviceProvider, _statement, newEvents, oldEvents);
                foreach (var eventHandler in _eventHandlers)
                {
                    try
                    {
                        eventHandler(this, e);
                    }
                    catch (Exception ex)
                    {
                        String message = "Unexpected exception invoking listener Update method on listener class '" +
                                         eventHandler.GetType().Name + "' : " + ex.GetType().Name + " : " + ex.Message;
                        Log.Error(message, ex);
                    }
                }
            }
        }

        /// <summary>
        /// Execute any listeners.
        /// </summary>
        public virtual void Execute()
        {
            EventBean[] eventArray;

            if (_eventList != null)
            {
                eventArray = _eventList.ToArray();
                _eventList = null;
                _singleEvent = null;
            }
            else
            {
                eventArray = new[] { _singleEvent };
                _singleEvent = null;
            }

            FireUpdateEvent(eventArray, null);
        }

        /// <summary> Returns true if at least one event has been added.</summary>
        /// <returns> true if it has data, false if not
        /// </returns>

        public virtual bool HasData
        {
            get { return _singleEvent != null; }
        }
    }
}
