///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.rank
{
    /// <summary>
    ///     Window sorting by values in the specified field extending a specified number of elements
    ///     from the lowest value up or the highest value down and retaining only the last unique value per key.
    ///     <para>
    ///         The type of the field to be sorted in the event must implement the Comparable interface.
    ///     </para>
    ///     <para>
    ///         The natural order in which events arrived is used as the second sorting criteria. Thus should events arrive
    ///         with equal sort values the oldest event leaves the sort window first.
    ///     </para>
    ///     <para>
    ///         Old values removed from a another view are removed from the sort view.
    ///     </para>
    /// </summary>
    public class RankWindowView : ViewSupport,
        DataWindowView
    {
        private readonly AgentInstanceContext agentInstanceContext;
        private readonly EventBean[] eventsPerStream = new EventBean[1];
        private readonly IStreamSortRankRandomAccess optionalRankedRandomAccess;
        private readonly RankWindowViewFactory rankWindowViewFactory;

        private readonly OrderedDictionary<object, object> sortedEvents; // key is computed sort-key, value is either List<EventBean> or EventBean

        private readonly int sortWindowSize;

        private readonly IDictionary<object, object> uniqueKeySortKeys; // key is computed unique-key, value is computed sort-key

        private int numberOfEvents;

        public RankWindowView(
            RankWindowViewFactory rankWindowViewFactory,
            int sortWindowSize,
            IStreamSortRankRandomAccess optionalRankedRandomAccess,
            AgentInstanceViewFactoryChainContext agentInstanceContext)
        {
            this.rankWindowViewFactory = rankWindowViewFactory;
            this.sortWindowSize = sortWindowSize;
            this.optionalRankedRandomAccess = optionalRankedRandomAccess;
            this.agentInstanceContext = agentInstanceContext.AgentInstanceContext;

            sortedEvents = new OrderedDictionary<object, object>(rankWindowViewFactory.Comparer);
            uniqueKeySortKeys = new Dictionary<object, object>();
        }

        public ViewFactory ViewFactory => rankWindowViewFactory;

        public override EventType EventType => Parent.EventType;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, rankWindowViewFactory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(rankWindowViewFactory, newData, oldData);

            var removedEvents = new OneEventCollection();

            // Remove old data
            if (oldData != null) {
                for (var i = 0; i < oldData.Length; i++) {
                    var uniqueKey = GetUniqueKey(oldData[i]);
                    var existingSortKey = uniqueKeySortKeys.Get(uniqueKey);

                    if (existingSortKey == null) {
                        continue;
                    }

                    var @event = RemoveFromSortedEvents(existingSortKey, uniqueKey);
                    if (@event != null) {
                        numberOfEvents--;
                        uniqueKeySortKeys.Remove(uniqueKey);
                        removedEvents.Add(@event);
                        InternalHandleRemovedKey(existingSortKey, oldData[i]);
                    }
                }
            }

            // Add new data
            if (newData != null) {
                for (var i = 0; i < newData.Length; i++) {
                    var uniqueKey = GetUniqueKey(newData[i]);
                    var newSortKey = GetSortValues(newData[i]);
                    var existingSortKey = uniqueKeySortKeys.Get(uniqueKey);

                    // not currently found: its a new entry
                    if (existingSortKey == null) {
                        CompareAndAddOrPassthru(newData[i], uniqueKey, newSortKey, removedEvents);
                    }
                    else {
                        // same unique-key event found already, remove and add again
                        // key did not change, perform in-place substitute of event
                        if (existingSortKey.Equals(newSortKey)) {
                            var replaced = InplaceReplaceSortedEvents(existingSortKey, uniqueKey, newData[i]);
                            if (replaced != null) {
                                removedEvents.Add(replaced);
                            }

                            InternalHandleReplacedKey(newSortKey, newData[i], replaced);
                        }
                        else {
                            var removed = RemoveFromSortedEvents(existingSortKey, uniqueKey);
                            if (removed != null) {
                                numberOfEvents--;
                                removedEvents.Add(removed);
                                InternalHandleRemovedKey(existingSortKey, removed);
                            }

                            CompareAndAddOrPassthru(newData[i], uniqueKey, newSortKey, removedEvents);
                        }
                    }
                }
            }

            // Remove data that sorts to the bottom of the window
            if (numberOfEvents > sortWindowSize) {
                while (numberOfEvents > sortWindowSize) {
                    var lastKey = sortedEvents.Keys.Last();
                    var existing = sortedEvents.Get(lastKey);
                    if (existing is IList<EventBean> existingList) {
                        while (numberOfEvents > sortWindowSize && !existingList.IsEmpty()) {
                            var newestEvent = existingList.DeleteAt(0);
                            var uniqueKey = GetUniqueKey(newestEvent);
                            uniqueKeySortKeys.Remove(uniqueKey);
                            numberOfEvents--;
                            removedEvents.Add(newestEvent);
                            InternalHandleRemovedKey(existing, newestEvent);
                        }

                        if (existingList.IsEmpty()) {
                            sortedEvents.Remove(lastKey);
                        }
                    }
                    else {
                        var lastSortedEvent = (EventBean) existing;
                        var uniqueKey = GetUniqueKey(lastSortedEvent);
                        uniqueKeySortKeys.Remove(uniqueKey);
                        numberOfEvents--;
                        removedEvents.Add(lastSortedEvent);
                        sortedEvents.Remove(lastKey);
                        InternalHandleRemovedKey(lastKey, lastSortedEvent);
                    }
                }
            }

            // If there are child views, fireStatementStopped update method
            if (optionalRankedRandomAccess != null) {
                optionalRankedRandomAccess.Refresh(sortedEvents, numberOfEvents, sortWindowSize);
            }

            if (Child != null) {
                EventBean[] expiredArr = null;
                if (!removedEvents.IsEmpty()) {
                    expiredArr = removedEvents.ToArray();
                }

                agentInstanceContext.InstrumentationProvider.QViewIndicate(rankWindowViewFactory, newData, expiredArr);
                Child.Update(newData, expiredArr);
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
            viewDataVisitor.VisitPrimary(
                sortedEvents, false, rankWindowViewFactory.ViewName, numberOfEvents, sortedEvents.Count);
        }

        public void InternalHandleReplacedKey(
            object newSortKey,
            EventBean newEvent,
            EventBean oldEvent)
        {
            // no action
        }

        public void InternalHandleRemovedKey(
            object sortKey,
            EventBean eventBean)
        {
            // no action
        }

        public void InternalHandleAddedKey(
            object sortKey,
            EventBean eventBean)
        {
            // no action
        }

        private void CompareAndAddOrPassthru(
            EventBean eventBean,
            object uniqueKey,
            object newSortKey,
            OneEventCollection removedEvents)
        {
            // determine full or not
            if (numberOfEvents >= sortWindowSize) {
                int compared = rankWindowViewFactory.Comparer.Compare(sortedEvents.Keys.Last(), newSortKey);

                // this new event will fall outside of the ranks or coincides with the last entry, so its an old event already
                if (compared < 0) {
                    removedEvents.Add(eventBean);
                }
                else {
                    // this new event is higher in sort key then the last entry so we are interested
                    uniqueKeySortKeys.Put(uniqueKey, newSortKey);
                    numberOfEvents++;
                    CollectionUtil.AddEventByKeyLazyListMapBack(newSortKey, eventBean, sortedEvents);
                    InternalHandleAddedKey(newSortKey, eventBean);
                }
            }
            else {
                // not yet filled, need to add
                uniqueKeySortKeys.Put(uniqueKey, newSortKey);
                numberOfEvents++;
                CollectionUtil.AddEventByKeyLazyListMapBack(newSortKey, eventBean, sortedEvents);
                InternalHandleAddedKey(newSortKey, eventBean);
            }
        }

        private EventBean RemoveFromSortedEvents(
            object sortKey,
            object uniqueKeyToRemove)
        {
            var existing = sortedEvents.Get(sortKey);

            EventBean removedOldEvent = null;
            if (existing != null) {
                if (existing is IList<EventBean> existingList) {
                    var it = existingList.GetEnumerator();
                    for (; it.MoveNext();) {
                        var eventForRank = it.Current;
                        if (GetUniqueKey(eventForRank).Equals(uniqueKeyToRemove)) {
                            it.Remove();
                            removedOldEvent = eventForRank;
                            break;
                        }
                    }

                    if (existingList.IsEmpty()) {
                        sortedEvents.Remove(sortKey);
                    }
                }
                else {
                    removedOldEvent = (EventBean) existing;
                    sortedEvents.Remove(sortKey);
                }
            }

            return removedOldEvent;
        }

        private EventBean InplaceReplaceSortedEvents(
            object sortKey,
            object uniqueKeyToReplace,
            EventBean newData)
        {
            var existing = sortedEvents.Get(sortKey);

            EventBean replaced = null;
            if (existing != null) {
                if (existing is IList<EventBean> existingList) {
                    var it = existingList.GetEnumerator();
                    for (; it.MoveNext();) {
                        var eventForRank = it.Current;
                        if (GetUniqueKey(eventForRank).Equals(uniqueKeyToReplace)) {
                            it.Remove();
                            replaced = eventForRank;
                            break;
                        }
                    }

                    existingList.Add(newData); // add to back as this is now the newest event
                }
                else {
                    replaced = (EventBean) existing;
                    sortedEvents.Put(sortKey, newData);
                }
            }

            return replaced;
        }

        public override string ToString()
        {
            return GetType().Name +
                   " isDescending=" + rankWindowViewFactory.IsDescendingValues.RenderAny() +
                   " sortWindowSize=" + sortWindowSize;
        }

        public object GetUniqueKey(EventBean theEvent)
        {
            return GetUniqueKey(
                eventsPerStream, rankWindowViewFactory.UniqueEvaluators, theEvent, agentInstanceContext);
        }

        public object GetSortValues(EventBean theEvent)
        {
            return GetSortKey(
                eventsPerStream, rankWindowViewFactory.SortCriteriaEvaluators, theEvent, agentInstanceContext);
        }

        public static object GetUniqueKey(
            EventBean[] eventsPerStream,
            ExprEvaluator[] evaluators,
            EventBean theEvent,
            ExprEvaluatorContext evalContext)
        {
            eventsPerStream[0] = theEvent;
            if (evaluators.Length > 1) {
                return GetCriteriaMultiKey(eventsPerStream, evaluators, evalContext);
            }

            return evaluators[0].Evaluate(eventsPerStream, true, evalContext);
        }

        public static object GetSortKey(
            EventBean[] eventsPerStream,
            ExprEvaluator[] evaluators,
            EventBean theEvent,
            ExprEvaluatorContext evalContext)
        {
            eventsPerStream[0] = theEvent;
            if (evaluators.Length > 1) {
                return GetSortMultiKey(eventsPerStream, evaluators, evalContext);
            }

            return evaluators[0].Evaluate(eventsPerStream, true, evalContext);
        }

        public static HashableMultiKey GetCriteriaMultiKey(
            EventBean[] eventsPerStream,
            ExprEvaluator[] evaluators,
            ExprEvaluatorContext evalContext)
        {
            var result = new object[evaluators.Length];
            var count = 0;
            foreach (var expr in evaluators) {
                result[count++] = expr.Evaluate(eventsPerStream, true, evalContext);
            }

            return new HashableMultiKey(result);
        }

        public static HashableMultiKey GetSortMultiKey(
            EventBean[] eventsPerStream,
            ExprEvaluator[] evaluators,
            ExprEvaluatorContext evalContext)
        {
            var result = new object[evaluators.Length];
            var count = 0;
            foreach (var expr in evaluators) {
                result[count++] = expr.Evaluate(eventsPerStream, true, evalContext);
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