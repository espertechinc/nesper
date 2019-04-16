///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.prior
{
    /// <summary>
    ///     Buffers view-posted insert stream (new data) and remove stream (old data) events for
    ///     use with determining prior results in these streams, for multiple different prior events.
    ///     <para>
    ///         Buffers only exactly those events in new data and old data that are being asked for via the
    ///         2 or more 'prior' functions that specify different indexes. For example "select prior(2, price), prior(1,
    ///         price)"
    ///         results in on buffer instance handling both the need to the immediatly prior (1) and the 2-events-ago
    ///         event (2).
    ///     </para>
    ///     <para>
    ///         As all views are required to post new data and post old data that removes the new data to subsequent views,
    ///         this buffer can be attached to all views and should not result in a memory leak.
    ///     </para>
    ///     <para>
    ///         When the buffer receives old data (rstream) events it removes the prior events to the rstream events
    ///         from the buffer the next time it receives a post (not immediatly) to allow queries to the buffer.
    ///     </para>
    /// </summary>
    public class PriorEventBufferMulti : ViewUpdatedCollection,
        RelativeAccessByEventNIndex
    {
        private readonly int[] priorToIndexes;
        private readonly int priorToIndexesSize;
        private EventBean[] lastOldData;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="priorToIndexSet">
        ///     holds a list of prior-event indexes.
        ///     <para />
        ///     For example, an array {0,4,6} means the current event, 4 events before the current event
        ///     and 6 events before the current event.
        /// </param>
        public PriorEventBufferMulti(int[] priorToIndexSet)
        {
            // Determine the maximum prior index to retain
            var maxPriorIndex = 0;
            foreach (var priorIndex in priorToIndexSet) {
                if (priorIndex > maxPriorIndex) {
                    maxPriorIndex = priorIndex;
                }
            }

            // Copy the set of indexes into an array, sort in ascending order
            priorToIndexesSize = priorToIndexSet.Length;
            priorToIndexes = new int[priorToIndexesSize];
            var count = 0;
            foreach (var priorIndex in priorToIndexSet) {
                priorToIndexes[count++] = priorIndex;
            }

            Array.Sort(priorToIndexSet);

            // Construct a rolling buffer of new data for holding max index + 1 (position 1 requires 2 events to keep)
            NewEvents = new RollingEventBuffer(maxPriorIndex + 1);
            PriorEventMap = new Dictionary<EventBean, EventBean[]>();
        }

        public IDictionary<EventBean, EventBean[]> PriorEventMap { get; }

        public RollingEventBuffer NewEvents { get; }

        public EventBean GetRelativeToEvent(
            EventBean theEvent,
            int priorToIndex)
        {
            if (priorToIndex >= priorToIndexesSize) {
                throw new ArgumentException(
                    "Index " + priorToIndex + " not allowed, max size is " + priorToIndexesSize);
            }

            var priorEvents = PriorEventMap.Get(theEvent);
            if (priorEvents == null) {
                throw new IllegalStateException("Event not currently in collection, event=" + theEvent);
            }

            return priorEvents[priorToIndex];
        }

        public EventBean GetRelativeToEnd(int index)
        {
            // No requirements to return events related to the end of the current buffer
            return null;
        }

        public int WindowToEventCount => 0;

        public IEnumerator<EventBean> WindowToEvent => null;

        public ICollection<EventBean> WindowToEventCollReadOnly => null;

        public void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            // Remove last old data posted in previous post
            if (lastOldData != null) {
                for (var i = 0; i < lastOldData.Length; i++) {
                    PriorEventMap.Remove(lastOldData[i]);
                }
            }

            // Post new data to rolling buffer starting with the oldest
            if (newData != null) {
                for (var i = 0; i < newData.Length; i++) {
                    var newEvent = newData[i];

                    // Add new event
                    NewEvents.Add(newEvent);

                    // Save prior index events in array
                    var priorEvents = new EventBean[priorToIndexesSize];
                    for (var j = 0; j < priorToIndexesSize; j++) {
                        var priorIndex = priorToIndexes[j];
                        priorEvents[j] = NewEvents.Get(priorIndex);
                    }

                    PriorEventMap.Put(newEvent, priorEvents);
                }
            }

            // Save old data to be removed next time we get posted results
            lastOldData = oldData;
        }

        public void Destroy()
        {
            // No action required
        }

        public int NumEventsInsertBuf => NewEvents.Count;

        public void Update(
            EventBean[] newData,
            EventBean[] oldData,
            PriorEventBufferChangeCaptureMulti capture)
        {
            // Remove last old data posted in previous post
            if (lastOldData != null) {
                for (var i = 0; i < lastOldData.Length; i++) {
                    var oldDataItem = lastOldData[i];
                    PriorEventMap.Remove(oldDataItem);
                    capture.Removed(oldDataItem);
                }
            }

            // Post new data to rolling buffer starting with the oldest
            if (newData != null) {
                for (var i = 0; i < newData.Length; i++) {
                    var newEvent = newData[i];

                    // Add new event
                    NewEvents.Add(newEvent);

                    // Save prior index events in array
                    var priorEvents = new EventBean[priorToIndexesSize];
                    for (var j = 0; j < priorToIndexesSize; j++) {
                        var priorIndex = priorToIndexes[j];
                        priorEvents[j] = NewEvents.Get(priorIndex);
                    }

                    PriorEventMap.Put(newEvent, priorEvents);
                    capture.Added(newEvent, priorEvents);
                }
            }

            // Save old data to be removed next time we get posted results
            lastOldData = oldData;
        }
    }
} // end of namespace