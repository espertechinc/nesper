///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.agg.core;

namespace com.espertech.esper.common.client.hook.aggmultifunc
{
    /// <summary>
    ///     Aggregation multi-function return-type descriptor
    /// </summary>
    public class AggregationMultiFunctionMethodDesc
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="forge">the forge of the aggregation value reader</param>
        /// <param name="eventTypeCollection">when returning a collection of events, the event type or null if not returning a collection of events</param>
        /// <param name="componentTypeCollection">when returning a collection of object values, the type of the values or null if not returning a collection of values</param>
        /// <param name="eventTypeSingle">when returning a single event, the event type or null if not returning a single event</param>
        public AggregationMultiFunctionMethodDesc(
            AggregationMethodForge forge,
            EventType eventTypeCollection,
            Type componentTypeCollection,
            EventType eventTypeSingle)
        {
            Reader = forge;
            EventTypeCollection = eventTypeCollection;
            ComponentTypeCollection = componentTypeCollection;
            EventTypeSingle = eventTypeSingle;
        }

        /// <summary>
        ///     Returns the forge of the aggregation value reader
        /// </summary>
        /// <value>forge</value>
        public AggregationMethodForge Reader { get; }

        /// <summary>
        ///     Returns, when returning a collection of events, the event type or null if not returning a collection of events
        /// </summary>
        /// <value>event type</value>
        public EventType EventTypeCollection { get; }

        /// <summary>
        ///     Returns, when returning a collection of object values, the type of the values or null if not returning a collection of values
        /// </summary>
        /// <value>type</value>
        public Type ComponentTypeCollection { get; }

        /// <summary>
        ///     Returns, when returning a single event, the event type or null if not returning a single event
        /// </summary>
        /// <value>event type</value>
        public EventType EventTypeSingle { get; }
    }
} // end of namespace