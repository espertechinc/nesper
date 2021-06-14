///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.rowrecog.state
{
    /// <summary>
    ///     "Prev" state for random access to event history.
    /// </summary>
    public class RowRecogStateRandomAccessImpl : RowRecogStateRandomAccess
    {
        private readonly RowRecogPreviousStrategyImpl getter;
        private readonly RollingEventBuffer newEvents;
        private readonly IDictionary<EventBean, EventBean[]> priorEventMap;
        private EventBean[] lastNew;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="getter">for access</param>
        public RowRecogStateRandomAccessImpl(RowRecogPreviousStrategyImpl getter)
        {
            this.getter = getter;

            // Construct a rolling buffer of new data for holding max index + 1 (position 1 requires 2 events to keep)
            newEvents = new RollingEventBuffer(getter.MaxPriorIndex + 1);
            if (!getter.IsUnbound()) {
                priorEventMap = new Dictionary<EventBean, EventBean[]>();
            }
            else {
                priorEventMap = null;
            }
        }

        /// <summary>
        ///     Add new event.
        /// </summary>
        /// <param name="newEvent">to add</param>
        public void NewEventPrepare(EventBean newEvent)
        {
            // Add new event
            newEvents.Add(newEvent);

            // Save prior index events in array
            var priorEvents = new EventBean[getter.IndexesRequestedLen];
            for (var j = 0; j < priorEvents.Length; j++) {
                int priorIndex = getter.IndexesRequested[j];
                priorEvents[j] = newEvents.Get(priorIndex);
            }

            priorEventMap?.Put(newEvent, priorEvents);

            lastNew = priorEvents;
            getter.RandomAccess = this;
        }

        /// <summary>
        ///     Prepare relative to existing event, for iterating.
        /// </summary>
        /// <param name="newEvent">to consider for index</param>
        public void ExistingEventPrepare(EventBean newEvent)
        {
            if (priorEventMap != null) {
                lastNew = priorEventMap.Get(newEvent);
            }

            getter.RandomAccess = this;
        }

        /// <summary>
        ///     Returns a previous event. Always immediatly preceded by #newEventPrepare.
        /// </summary>
        /// <param name="assignedRelativeIndex">index</param>
        /// <returns>event</returns>
        public EventBean GetPreviousEvent(int assignedRelativeIndex)
        {
            return lastNew?[assignedRelativeIndex];
        }

        /// <summary>
        ///     Remove events.
        /// </summary>
        /// <param name="oldEvents">to remove</param>
        public void Remove(EventBean[] oldEvents)
        {
            if (oldEvents == null) {
                return;
            }

            for (var i = 0; i < oldEvents.Length; i++) {
                Remove(oldEvents[i]);
            }
        }

        /// <summary>
        ///     Remove event.
        /// </summary>
        /// <param name="oldEvent">to remove</param>
        public void Remove(EventBean oldEvent)
        {
            priorEventMap?.Remove(oldEvent);
        }

        /// <summary>
        ///     Returns true for empty collection.
        /// </summary>
        /// <returns>indicator if empty</returns>
        public bool IsEmpty()
        {
            priorEventMap?.IsEmpty();

            return true;
        }
    }
} // end of namespace