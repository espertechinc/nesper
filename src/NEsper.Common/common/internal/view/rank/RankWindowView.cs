///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
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
    ///         The type of the field to be sorted in the event must implement the IComparable interface.
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
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly EventBean[] _eventsPerStream = new EventBean[1];
        private readonly IStreamSortRankRandomAccess _optionalRankedRandomAccess;
        private readonly RankWindowViewFactory _rankWindowViewFactory;

        private readonly IOrderedDictionary<object, object>
            _sortedEvents; // key is computed sort-key, value is either List<EventBean> or EventBean

        private readonly int _sortWindowSize;

        private readonly IDictionary<object, object>
            _uniqueKeySortKeys; // key is computed unique-key, value is computed sort-key

        private int _numberOfEvents;

        public RankWindowView(
            RankWindowViewFactory rankWindowViewFactory,
            int sortWindowSize,
            IStreamSortRankRandomAccess optionalRankedRandomAccess,
            AgentInstanceViewFactoryChainContext agentInstanceContext)
        {
            _rankWindowViewFactory = rankWindowViewFactory;
            _sortWindowSize = sortWindowSize;
            _optionalRankedRandomAccess = optionalRankedRandomAccess;
            _agentInstanceContext = agentInstanceContext.AgentInstanceContext;

            _sortedEvents = new OrderedListDictionary<object, object>(rankWindowViewFactory.Comparer);
            _uniqueKeySortKeys = new Dictionary<object, object>();
        }

        public ViewFactory ViewFactory => _rankWindowViewFactory;

        public override EventType EventType => Parent.EventType;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            _agentInstanceContext.AuditProvider.View(newData, oldData, _agentInstanceContext, _rankWindowViewFactory);
            _agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(
                _rankWindowViewFactory,
                newData,
                oldData);

            var removedEvents = new OneEventCollection();

            // Remove old data
            if (oldData != null) {
                for (var i = 0; i < oldData.Length; i++) {
                    var uniqueKey = GetUniqueKey(oldData[i]);
                    var existingSortKey = _uniqueKeySortKeys.Get(uniqueKey);

                    if (existingSortKey == null) {
                        continue;
                    }

                    var @event = RemoveFromSortedEvents(existingSortKey, uniqueKey);
                    if (@event != null) {
                        _numberOfEvents--;
                        _uniqueKeySortKeys.Remove(uniqueKey);
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
                    var existingSortKey = _uniqueKeySortKeys.Get(uniqueKey);

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
                                _numberOfEvents--;
                                removedEvents.Add(removed);
                                InternalHandleRemovedKey(existingSortKey, removed);
                            }

                            CompareAndAddOrPassthru(newData[i], uniqueKey, newSortKey, removedEvents);
                        }
                    }
                }
            }

            // Remove data that sorts to the bottom of the window
            while (_numberOfEvents > _sortWindowSize) {
                var lastKey = _sortedEvents.Keys.Last();
                var existing = _sortedEvents.Get(lastKey);
                if (existing is IList<EventBean> existingList) {
                    while (_numberOfEvents > _sortWindowSize && !existingList.IsEmpty()) {
                        var newestEvent = existingList.DeleteAt(0);
                        var uniqueKey = GetUniqueKey(newestEvent);
                        _uniqueKeySortKeys.Remove(uniqueKey);
                        _numberOfEvents--;
                        removedEvents.Add(newestEvent);
                        InternalHandleRemovedKey(existing, newestEvent);
                    }

                    if (existingList.IsEmpty()) {
                        _sortedEvents.Remove(lastKey);
                    }
                }
                else {
                    var lastSortedEvent = (EventBean)existing;
                    var uniqueKey = GetUniqueKey(lastSortedEvent);
                    _uniqueKeySortKeys.Remove(uniqueKey);
                    _numberOfEvents--;
                    removedEvents.Add(lastSortedEvent);
                    _sortedEvents.Remove(lastKey);
                    InternalHandleRemovedKey(lastKey, lastSortedEvent);
                }
            }

            // If there are child views, fireStatementStopped update method
            _optionalRankedRandomAccess?.Refresh(_sortedEvents, _numberOfEvents, _sortWindowSize);

            if (Child != null) {
                EventBean[] expiredArr = null;
                if (!removedEvents.IsEmpty()) {
                    expiredArr = removedEvents.ToArray();
                }

                _agentInstanceContext.InstrumentationProvider.QViewIndicate(
                    _rankWindowViewFactory,
                    newData,
                    expiredArr);
                Child.Update(newData, expiredArr);
                _agentInstanceContext.InstrumentationProvider.AViewIndicate();
            }

            _agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return new RankWindowEnumerator(_sortedEvents);
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(
                _sortedEvents,
                false,
                _rankWindowViewFactory.ViewName,
                _numberOfEvents,
                _sortedEvents.Count);
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
            if (_numberOfEvents >= _sortWindowSize) {
                var compared = _rankWindowViewFactory.Comparer.Compare(_sortedEvents.Keys.Last(), newSortKey);

                // this new event will fall outside of the ranks or coincides with the last entry, so its an old event already
                if (compared < 0) {
                    removedEvents.Add(eventBean);
                }
                else {
                    // this new event is higher in sort key then the last entry so we are interested
                    _uniqueKeySortKeys.Put(uniqueKey, newSortKey);
                    _numberOfEvents++;
                    CollectionUtil.AddEventByKeyLazyListMapBack(newSortKey, eventBean, _sortedEvents);
                    InternalHandleAddedKey(newSortKey, eventBean);
                }
            }
            else {
                // not yet filled, need to add
                _uniqueKeySortKeys.Put(uniqueKey, newSortKey);
                _numberOfEvents++;
                CollectionUtil.AddEventByKeyLazyListMapBack(newSortKey, eventBean, _sortedEvents);
                InternalHandleAddedKey(newSortKey, eventBean);
            }
        }

        private EventBean RemoveFromSortedEvents(
            object sortKey,
            object uniqueKeyToRemove)
        {
            var existing = _sortedEvents.Get(sortKey);

            EventBean removedOldEvent = null;
            if (existing != null) {
                if (existing is IList<EventBean> existingList) {
                    for (var ii = 0; ii < existingList.Count; ii++) {
                        var eventForRank = existingList[ii];
                        if (GetUniqueKey(eventForRank).Equals(uniqueKeyToRemove)) {
                            existingList.RemoveAt(ii--);
                            removedOldEvent = eventForRank;
                            break;
                        }
                    }

                    if (existingList.IsEmpty()) {
                        _sortedEvents.Remove(sortKey);
                    }
                }
                else {
                    removedOldEvent = (EventBean)existing;
                    _sortedEvents.Remove(sortKey);
                }
            }

            return removedOldEvent;
        }

        private EventBean InplaceReplaceSortedEvents(
            object sortKey,
            object uniqueKeyToReplace,
            EventBean newData)
        {
            var existing = _sortedEvents.Get(sortKey);

            EventBean replaced = null;
            if (existing != null) {
                if (existing is IList<EventBean> existingList) {
                    for (var ii = 0; ii < existingList.Count; ii++) {
                        var eventForRank = existingList[ii];
                        if (GetUniqueKey(eventForRank).Equals(uniqueKeyToReplace)) {
                            existingList.RemoveAt(ii--);
                            replaced = eventForRank;
                            break;
                        }
                    }

                    existingList.Add(newData); // add to back as this is now the newest event
                }
                else {
                    replaced = (EventBean)existing;
                    _sortedEvents.Put(sortKey, newData);
                }
            }

            return replaced;
        }

        public override string ToString()
        {
            return
                $"{GetType().Name} isDescending={_rankWindowViewFactory.IsDescendingValues.RenderAny()} sortWindowSize={_sortWindowSize}";
        }

        public object GetUniqueKey(EventBean theEvent)
        {
            return GetUniqueKey(
                _eventsPerStream,
                _rankWindowViewFactory.CriteriaEval,
                theEvent,
                _agentInstanceContext);
        }

        public object GetSortValues(EventBean theEvent)
        {
            return GetSortKey(
                _eventsPerStream,
                _rankWindowViewFactory.SortCriteriaEvaluators,
                theEvent,
                _agentInstanceContext);
        }

        public static object GetUniqueKey(
            EventBean[] eventsPerStream,
            ExprEvaluator evaluator,
            EventBean theEvent,
            ExprEvaluatorContext evalContext)
        {
            eventsPerStream[0] = theEvent;
            return evaluator.Evaluate(eventsPerStream, true, evalContext);
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
            if (_sortedEvents == null) {
                return true;
            }

            return _sortedEvents.IsEmpty();
        }
    }
} // end of namespace