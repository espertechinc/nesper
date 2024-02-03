///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.previous
{
    /// <summary>
    /// Provides relative access to insert stream events for certain window.
    /// </summary>
    public class IStreamRelativeAccess : RelativeAccessByEventNIndex,
        ViewUpdatedCollection
    {
        private readonly IDictionary<EventBean, int> indexPerEvent;
        private EventBean[] lastNewData;
        private readonly IStreamRelativeAccessUpdateObserver updateObserver;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="updateObserver">is invoked when updates are received</param>
        public IStreamRelativeAccess(IStreamRelativeAccessUpdateObserver updateObserver)
        {
            this.updateObserver = updateObserver;
            indexPerEvent = new Dictionary<EventBean, int>();
        }

        public void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            updateObserver.Updated(this, newData);
            indexPerEvent.Clear();
            lastNewData = newData;

            if (newData != null) {
                for (var i = 0; i < newData.Length; i++) {
                    indexPerEvent.Put(newData[i], i);
                }
            }
        }

        public EventBean GetRelativeToEvent(
            EventBean theEvent,
            int prevIndex)
        {
            if (lastNewData == null) {
                return null;
            }

            if (prevIndex == 0) {
                return theEvent;
            }

            if (!indexPerEvent.TryGetValue(theEvent, out var indexIncoming)) {
                return null;
            }

            if (prevIndex > indexIncoming) {
                return null;
            }

            var relativeIndex = indexIncoming - prevIndex;
            if (relativeIndex < lastNewData.Length && relativeIndex >= 0) {
                return lastNewData[relativeIndex];
            }

            return null;
        }

        public EventBean GetRelativeToEnd(int prevIndex)
        {
            if (lastNewData == null) {
                return null;
            }

            if (prevIndex < lastNewData.Length && prevIndex >= 0) {
                return lastNewData[prevIndex];
            }

            return null;
        }

        public IEnumerator<EventBean> WindowToEvent => Arrays.ReverseEnumerate(lastNewData).GetEnumerator();

        public ICollection<EventBean> WindowToEventCollReadOnly => Arrays.AsList(lastNewData);

        public int WindowToEventCount {
            get {
                if (lastNewData == null) {
                    return 0;
                }

                return lastNewData.Length;
            }
        }

        /// <summary>
        /// For indicating that the collection has been updated.
        /// </summary>
        public interface IStreamRelativeAccessUpdateObserver
        {
            /// <summary>
            /// Callback to indicate an update.
            /// </summary>
            /// <param name="iStreamRelativeAccess">is the collection</param>
            /// <param name="newData">is the new data available</param>
            void Updated(
                RelativeAccessByEventNIndex iStreamRelativeAccess,
                EventBean[] newData);
        }

        public void Destroy()
        {
            // No action required
        }

        public int NumEventsInsertBuf => indexPerEvent.Count;
    }
} // end of namespace