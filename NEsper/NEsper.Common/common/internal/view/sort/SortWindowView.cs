///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.sort
{
    /// <summary>
    ///     Window sorting by values in the specified field extending a specified number of elements
    ///     from the lowest value up or the highest value down.
    ///     The view accepts 3 parameters. The first parameter is the field name to get the values to sort for,
    ///     the second parameter defines whether to sort ascending or descending, the third parameter
    ///     is the number of elements to keep in the sorted list.
    ///     <para />
    ///     The type of the field to be sorted in the event must implement the IComparable interface.
    ///     <para />
    ///     The natural order in which events arrived is used as the second sorting criteria. Thus should events arrive
    ///     with equal sort values the oldest event leaves the sort window first.
    ///     <para />
    ///     Old values removed from a prior view are removed from the sort view.
    /// </summary>
    public class SortWindowView : ViewSupport,
        DataWindowView
    {
        protected internal readonly AgentInstanceContext agentInstanceContext;
        private readonly EventBean[] eventsPerStream = new EventBean[1];
        private readonly SortWindowViewFactory factory;
        private readonly IStreamSortRankRandomAccess optionalSortedRandomAccess;
        private readonly int sortWindowSize;
        protected internal int eventCount;
        protected internal OrderedDictionary<object, object> sortedEvents;

        public SortWindowView(
            SortWindowViewFactory factory,
            int sortWindowSize,
            IStreamSortRankRandomAccess optionalSortedRandomAccess,
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            this.factory = factory;
            this.sortWindowSize = sortWindowSize;
            this.optionalSortedRandomAccess = optionalSortedRandomAccess;
            agentInstanceContext = agentInstanceViewFactoryContext.AgentInstanceContext;

            sortedEvents = new OrderedDictionary<object, object>(factory.IComparer);
        }

        public ViewFactory ViewFactory => factory;

        public override EventType EventType => parent.EventType;
        
        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, factory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(factory, newData, oldData);

            OneEventCollection removedEvents = null;

            // Remove old data
            if (oldData != null) {
                for (var i = 0; i < oldData.Length; i++) {
                    var oldDataItem = oldData[i];
                    var sortValues = GetSortValues(oldDataItem);
                    var result = CollectionUtil.RemoveEventByKeyLazyListMap(sortValues, oldDataItem, sortedEvents);
                    if (result) {
                        eventCount--;
                        if (removedEvents == null) {
                            removedEvents = new OneEventCollection();
                        }

                        removedEvents.Add(oldDataItem);
                        InternalHandleRemoved(sortValues, oldDataItem);
                    }
                }
            }

            // Add new data
            if (newData != null) {
                for (var i = 0; i < newData.Length; i++) {
                    var newDataItem = newData[i];
                    var sortValues = GetSortValues(newDataItem);
                    CollectionUtil.AddEventByKeyLazyListMapFront(sortValues, newDataItem, sortedEvents);
                    eventCount++;
                    InternalHandleAdd(sortValues, newDataItem);
                }
            }

            // Remove data that sorts to the bottom of the window
            if (eventCount > sortWindowSize) {
                var removeCount = eventCount - sortWindowSize;
                for (var i = 0; i < removeCount; i++) {
                    // Remove the last element of the last key - sort order is key and then natural order of arrival
                    var lastKey = sortedEvents.Keys.Last();
                    var lastEntry = sortedEvents.Get(lastKey);
                    if (lastEntry is IList<EventBean>) {
                        var events = (IList<EventBean>) lastEntry;
                        var theEvent =
                            events.DeleteAt(events.Count - 1); // remove oldest event, newest events are first in list
                        eventCount--;
                        if (events.IsEmpty()) {
                            sortedEvents.Remove(lastKey);
                        }

                        if (removedEvents == null) {
                            removedEvents = new OneEventCollection();
                        }

                        removedEvents.Add(theEvent);
                        InternalHandleRemoved(lastKey, theEvent);
                    }
                    else {
                        var theEvent = (EventBean) lastEntry;
                        eventCount--;
                        sortedEvents.Remove(lastKey);
                        if (removedEvents == null) {
                            removedEvents = new OneEventCollection();
                        }

                        removedEvents.Add(theEvent);
                        InternalHandleRemoved(lastKey, theEvent);
                    }
                }
            }

            // If there are child views, fireStatementStopped update method
            if (optionalSortedRandomAccess != null) {
                optionalSortedRandomAccess.Refresh(sortedEvents, eventCount, sortWindowSize);
            }

            if (child != null) {
                EventBean[] expiredArr = null;
                if (removedEvents != null) {
                    expiredArr = removedEvents.ToArray();
                }

                agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, newData, expiredArr);
                child.Update(newData, expiredArr);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();
            }

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return sortedEvents.GetMultiLevelEnumerator();
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(sortedEvents, false, factory.ViewName, eventCount, null);
        }

        public void InternalHandleAdd(
            object sortValues,
            EventBean newDataItem)
        {
            // no action required
        }

        public void InternalHandleRemoved(
            object sortValues,
            EventBean oldDataItem)
        {
            // no action required
        }

        public override string ToString()
        {
            return GetType().Name +
                   " isDescending=" +
                   factory.IsDescendingValues.RenderAny() +
                   " sortWindowSize=" +
                   sortWindowSize;
        }

        protected object GetSortValues(EventBean theEvent)
        {
            eventsPerStream[0] = theEvent;
            if (factory.SortCriteriaEvaluators.Length == 1) {
                return factory.SortCriteriaEvaluators[0].Evaluate(eventsPerStream, true, agentInstanceContext);
            }

            var result = new object[factory.SortCriteriaEvaluators.Length];
            var count = 0;
            foreach (var expr in factory.SortCriteriaEvaluators) {
                result[count++] = expr.Evaluate(eventsPerStream, true, agentInstanceContext);
            }

            return new HashableMultiKey(result);
        }

        /// <summary>
        ///     True to indicate the sort window is empty, or false if not empty.
        /// </summary>
        /// <returns>true if empty sort window</returns>
        public bool IsEmpty()
        {
            if (sortedEvents == null) {
                return true;
            }

            return sortedEvents.IsEmpty();
        }
    }
} // end of namespace