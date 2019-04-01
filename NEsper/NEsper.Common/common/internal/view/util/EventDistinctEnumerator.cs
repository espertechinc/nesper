///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.util
{
    /// <summary>
    ///     Enumerator for obtaining distinct events.
    /// </summary>
    public class EventDistinctEnumerator
    {
        public static IEnumerator<EventBean> For(
            IEnumerator<EventBean> sourceIterator,
            EventType eventType)
        {
            if (sourceIterator != null && sourceIterator.MoveNext())
            {
                // there is at least one event...
                var first = sourceIterator.Current;
                // but is there only one event?
                if (!sourceIterator.MoveNext())
                {
                    return EnumerationHelper.Singleton(first);
                }

                // build distinct set because there are multiple events
                var events = new ArrayDeque<EventBean>();
                events.Add(first);
                events.Add(sourceIterator.Current);
                while (sourceIterator.MoveNext())
                {
                    events.Add(sourceIterator.Current);
                }

                // Determine the reader that we need to use for this use case
                var eventBeanReader = GetEventBeanReader(eventType);
                var unique = EventBeanUtility.GetDistinctByProp(events, eventBeanReader);
                return unique.GetEnumerator();
            }

            return EnumerationHelper.Empty<EventBean>();
        }

        private static EventBeanReader GetEventBeanReader(EventType eventType)
        {
            EventBeanReader eventBeanReader = null;

            if (eventType is EventTypeSPI)
            {
                eventBeanReader = ((EventTypeSPI)eventType).Reader;
            }

            if (eventBeanReader == null)
            {
                eventBeanReader = new EventBeanReaderDefaultImpl(eventType);
            }

            return eventBeanReader;
        }
    }
} // end of namespace