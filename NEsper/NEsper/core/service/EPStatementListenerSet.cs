///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Provides Update listeners for use by statement instances, and the management methods around these.
    /// <para>
    /// The collection of Update listeners is based on copy-on-write:
    /// When the engine dispatches events to a set of listeners, then while iterating through the set there
    /// may be listeners added or removed (the listener may remove itself).
    /// </para>
    /// <para>
    /// Additionally, events may be dispatched by multiple threads to the same listener.
    /// </para>
    /// </summary>
    public class EPStatementListenerSet
    {
        public CopyOnWriteArraySet<UpdateEventHandler> Events;

        /// <summary>Ctor.</summary>
        public EPStatementListenerSet()
        {
            Events = new CopyOnWriteArraySet<UpdateEventHandler>();
        }

        public bool HasEventConsumers
        {
            get
            {
                return ((Events != null) && (Events.Count != 0));
            }
        }

        /// <summary>Copy the update listener set to from another.</summary>
        /// <param name="listenerSet">a collection of Update listeners</param>
        public void Copy(EPStatementListenerSet listenerSet)
        {
            Events = listenerSet.Events;
        }

        public void RemoveAllEventHandlers()
        {
            Events.Clear();
        }

        /// <summary>
        /// Gets or sets the subscriber instance.
        /// </summary>
        public EPSubscriber Subscriber { get; set; }
    }
}
