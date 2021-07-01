///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.kernel.statement
{
    using UpdateEventHandler = EventHandler<UpdateEventArgs>;

    /// <summary>
    ///     Provides update event handlers for use by statement instances, and the management methods around these.
    ///     <para>
    ///         The collection of update event handlers is based on copy-on-write:
    ///         When the runtime dispatches events to a set of event handlers, then while iterating
    ///         through the set there may be event handlers added or removed (the event handler may remove
    ///         itself).  Additionally, events may be dispatched by multiple threads to the same
    ///         event handlers.
    ///     </para>
    /// </summary>
    public class EPStatementEventHandlerSet : EPStatementHandlerBase
    {
        private static readonly UpdateEventHandler[] EMPTY_UPDATE_HANDLER_ARRAY = new UpdateEventHandler[0];

        private volatile UpdateEventHandler[] eventHandlers;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public EPStatementEventHandlerSet()
        {
            eventHandlers = EMPTY_UPDATE_HANDLER_ARRAY;
        }

        public EPStatementEventHandlerSet(UpdateEventHandler[] eventHandlers)
        {
            this.eventHandlers = eventHandlers;
        }

        /// <summary>
        ///     Returns the set of event handlers to the statement.
        /// </summary>
        /// <returns>statement event handlers</returns>
        public UpdateEventHandler[] EventHandlers {
            get => eventHandlers;
        }

        /// <summary>
        ///     Set the update event handlers set to use.
        /// </summary>
        /// <param name="eventHandlerSet">a collection of update event handlers</param>
        public void SetEventHandlers(EPStatementEventHandlerSet eventHandlerSet)
        {
            eventHandlers = eventHandlerSet.EventHandlers;
        }

        /// <summary>
        ///     Add an event handler to the statement.
        /// </summary>
        /// <param name="eventHandler">to add</param>
        public void AddEventHandler(UpdateEventHandler eventHandler)
        {
            lock (this) {
                if (eventHandler == null) {
                    throw new ArgumentException("Null event handler reference supplied");
                }

                foreach (var existing in eventHandlers) {
                    if (existing == eventHandler) {
                        return;
                    }
                }

                eventHandlers = (UpdateEventHandler[]) CollectionUtil
                    .ArrayExpandAddSingle(eventHandlers, eventHandler);
            }
        }

        /// <summary>
        ///     Remove an event handler to a statement.
        /// </summary>
        /// <param name="eventHandler">to remove</param>
        public void RemoveEventHandler(UpdateEventHandler eventHandler)
        {
            lock (this) {
                if (eventHandler == null) {
                    throw new ArgumentNullException(nameof(eventHandler), "Null event handler reference supplied");
                }

                var index = -1;
                for (var i = 0; i < eventHandlers.Length; i++) {
                    if (eventHandlers[i] == eventHandler) {
                        index = i;
                        break;
                    }
                }

                if (index == -1) {
                    return;
                }

                eventHandlers = (UpdateEventHandler[]) CollectionUtil
                    .ArrayShrinkRemoveSingle(eventHandlers, index);
            }
        }

        /// <summary>
        ///     Remove all event handlers to a statement.
        /// </summary>
        public void RemoveAllEventHandlers()
        {
            lock (this) {
                eventHandlers = EMPTY_UPDATE_HANDLER_ARRAY;
            }
        }
    }
} // end of namespace