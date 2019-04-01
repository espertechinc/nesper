///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.view.core
{
    /// <summary>
    ///     Event stream implementation that does not keep any window by itself of the events coming into the stream,
    ///     without the possibility to iterate the last event.
    /// </summary>
    public class ZeroDepthStreamNoIterate : EventStream
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="eventType">type of event</param>
        public ZeroDepthStreamNoIterate(EventType eventType)
        {
            EventType = eventType;
        }

        public virtual void Insert(EventBean theEvent)
        {
            // Get a new array created rather then re-use the old one since some client listeners
            // to this view may keep reference to the new data
            EventBean[] row = {theEvent};
            Child.Update(row, null);
        }

        public virtual void Insert(EventBean[] events)
        {
            Child.Update(events, null);
        }

        public EventType EventType { get; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            return CollectionUtil.NULL_EVENT_ITERATOR;
        }

        public View Child { get; set; }
    }
} // end of namespace