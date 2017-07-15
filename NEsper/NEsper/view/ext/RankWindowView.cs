///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.view.ext
{
    /// <summary>
    /// Window sorting by values in the specified field extending a specified number of elements
    /// from the lowest value up or the highest value down and retaining only the last unique
    /// value per key. The type of the field to be sorted in the event must implement the Comparable
    /// interface. The natural order in which events arrived is used as the second sorting criteria.
    /// Thus should events arrive with equal sort values the oldest event leaves the sort window first.
    /// Old values removed from a another view are removed from the sort view.
    /// </summary>
    public class RankWindowView
        : ViewSupport
        , DataWindowView
        , CloneableView
    {
        private readonly RankWindowViewFactory _rankWindowViewFactory;
        private readonly ExprEvaluator[] _sortCriteriaEvaluators;
        private readonly ExprNode[] _sortCriteriaExpressions;
        private readonly ExprEvaluator[] _uniqueCriteriaEvaluators;
        private readonly ExprNode[] _uniqueCriteriaExpressions;
        private readonly EventBean[] _eventsPerStream = new EventBean[1];
        private readonly bool[] _isDescendingValues;
        private readonly int _sortWindowSize;
        private readonly IStreamSortRankRandomAccess _optionalRankedRandomAccess;
        private readonly AgentInstanceViewFactoryChainContext _agentInstanceViewFactoryContext;

        private readonly IComparer<Object> _comparator;

        private readonly OrderedDictionary<object, object> _sortedEvents; // key is computed sort-key, value is either List<EventBean> or EventBean
        private readonly IDictionary<Object, Object> _uniqueKeySortKeys;  // key is computed unique-key, value is computed sort-key
        private int _numberOfEvents;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="rankWindowViewFactory">for copying this view in a group-by</param>
        /// <param name="uniqueCriteriaExpressions">The unique criteria expressions.</param>
        /// <param name="uniqueCriteriaEvaluators">The unique criteria evaluators.</param>
        /// <param name="sortCriteriaExpressions">is the event property names to sort</param>
        /// <param name="sortCriteriaEvaluators">The sort criteria evaluators.</param>
        /// <param name="descendingValues">indicates whether to sort ascending or descending for each field</param>
        /// <param name="sortWindowSize">is the window size</param>
        /// <param name="optionalRankedRandomAccess">is the friend class handling the random access, if required byexpressions</param>
        /// <param name="isSortUsingCollator">for string value sorting using compare or Collator</param>
        /// <param name="agentInstanceViewFactoryContext">The agent instance view factory context.</param>
        public RankWindowView(RankWindowViewFactory rankWindowViewFactory,
                              ExprNode[] uniqueCriteriaExpressions,
                              ExprEvaluator[] uniqueCriteriaEvaluators,
                              ExprNode[] sortCriteriaExpressions,
                              ExprEvaluator[] sortCriteriaEvaluators,
                              bool[] descendingValues,
                              int sortWindowSize,
                              IStreamSortRankRandomAccess optionalRankedRandomAccess,
                              bool isSortUsingCollator,
                              AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            _rankWindowViewFactory = rankWindowViewFactory;
            _uniqueCriteriaExpressions = uniqueCriteriaExpressions;
            _uniqueCriteriaEvaluators = uniqueCriteriaEvaluators;
            _sortCriteriaExpressions = sortCriteriaExpressions;
            _sortCriteriaEvaluators = sortCriteriaEvaluators;
            _isDescendingValues = descendingValues;
            _sortWindowSize = sortWindowSize;
            _optionalRankedRandomAccess = optionalRankedRandomAccess;
            _agentInstanceViewFactoryContext = agentInstanceViewFactoryContext;

            _comparator = CollectionUtil.GetComparator(sortCriteriaEvaluators, isSortUsingCollator, _isDescendingValues);
            _sortedEvents = new OrderedDictionary<object, object>(_comparator);
            _uniqueKeySortKeys = new Dictionary<Object, Object>();
        }

        public View CloneView()
        {
            return _rankWindowViewFactory.MakeView(_agentInstanceViewFactoryContext);
        }

        public override EventType EventType
        {
            get
            {
                // The schema is the parent view's schema
                return Parent.EventType;
            }
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewProcessIRStream(this, _rankWindowViewFactory.ViewName, newData, oldData); }

            var removedEvents = new OneEventCollection();

            // Remove old data
            if (oldData != null)
            {
                for (var i = 0; i < oldData.Length; i++)
                {
                    var uniqueKey = GetUniqueValues(oldData[i]);
                    var existingSortKey = _uniqueKeySortKeys.Get(uniqueKey);

                    if (existingSortKey == null)
                    {
                        continue;
                    }

                    var theEvent = RemoveFromSortedEvents(existingSortKey, uniqueKey);
                    if (theEvent != null)
                    {
                        _numberOfEvents--;
                        _uniqueKeySortKeys.Remove(uniqueKey);
                        removedEvents.Add(theEvent);
                        InternalHandleRemovedKey(existingSortKey, oldData[i]);
                    }
                }
            }

            // Add new data
            if (newData != null)
            {
                for (var i = 0; i < newData.Length; i++)
                {
                    var uniqueKey = GetUniqueValues(newData[i]);
                    var newSortKey = GetSortValues(newData[i]);
                    var existingSortKey = _uniqueKeySortKeys.Get(uniqueKey);

                    // not currently found: its a new entry
                    if (existingSortKey == null)
                    {
                        CompareAndAddOrPassthru(newData[i], uniqueKey, newSortKey, removedEvents);
                    }
                    // same unique-key event found already, remove and add again
                    else
                    {
                        // key did not change, perform in-place substitute of event
                        if (existingSortKey.Equals(newSortKey))
                        {
                            var replaced = InplaceReplaceSortedEvents(existingSortKey, uniqueKey, newData[i]);
                            if (replaced != null)
                            {
                                removedEvents.Add(replaced);
                            }
                            InternalHandleReplacedKey(newSortKey, newData[i], replaced);
                        }
                        else
                        {
                            var removed = RemoveFromSortedEvents(existingSortKey, uniqueKey);
                            if (removed != null)
                            {
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
            if (_numberOfEvents > _sortWindowSize)
            {
                while (_numberOfEvents > _sortWindowSize)
                {
                    var lastKey = _sortedEvents.Keys.Last();
                    var existing = _sortedEvents.Get(lastKey);
                    if (existing is IList<EventBean>)
                    {
                        var existingList = (IList<EventBean>)existing;
                        while (_numberOfEvents > _sortWindowSize && !existingList.IsEmpty())
                        {
                            var newestEvent = existingList.Delete(0);
                            var uniqueKey = GetUniqueValues(newestEvent);
                            _uniqueKeySortKeys.Remove(uniqueKey);
                            _numberOfEvents--;
                            removedEvents.Add(newestEvent);
                            InternalHandleRemovedKey(existing, newestEvent);
                        }
                        if (existingList.IsEmpty())
                        {
                            _sortedEvents.Remove(lastKey);
                        }
                    }
                    else
                    {
                        var lastSortedEvent = (EventBean)existing;
                        var uniqueKey = GetUniqueValues(lastSortedEvent);
                        _uniqueKeySortKeys.Remove(uniqueKey);
                        _numberOfEvents--;
                        removedEvents.Add(lastSortedEvent);
                        _sortedEvents.Remove(lastKey);
                        InternalHandleRemovedKey(lastKey, lastSortedEvent);
                    }
                }
            }

            // If there are child views, fireStatementStopped Update method
            if (_optionalRankedRandomAccess != null)
            {
                _optionalRankedRandomAccess.Refresh(_sortedEvents, _numberOfEvents, _sortWindowSize);
            }
            if (HasViews)
            {
                EventBean[] expiredArr = null;
                if (!removedEvents.IsEmpty())
                {
                    expiredArr = removedEvents.ToArray();
                }

                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewIndicate(this, _rankWindowViewFactory.ViewName, newData, expiredArr); }
                UpdateChildren(newData, expiredArr);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewIndicate(); }
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewProcessIRStream(); }
        }

        public void InternalHandleReplacedKey(Object newSortKey, EventBean newEvent, EventBean oldEvent)
        {
            // no action
        }

        public void InternalHandleRemovedKey(Object sortKey, EventBean eventBean)
        {
            // no action
        }

        public void InternalHandleAddedKey(Object sortKey, EventBean eventBean)
        {
            // no action
        }

        private void CompareAndAddOrPassthru(EventBean eventBean, Object uniqueKey, Object newSortKey, OneEventCollection removedEvents)
        {
            // determine full or not
            if (_numberOfEvents >= _sortWindowSize)
            {
                var compared = _comparator.Compare(_sortedEvents.Keys.Last(), newSortKey);

                // this new event will fall outside of the ranks or coincides with the last entry, so its an old event already
                if (compared < 0)
                {
                    removedEvents.Add(eventBean);
                }
                // this new event is higher in sort key then the last entry so we are interested
                else
                {
                    _uniqueKeySortKeys.Put(uniqueKey, newSortKey);
                    _numberOfEvents++;
                    CollectionUtil.AddEventByKeyLazyListMapBack(newSortKey, eventBean, _sortedEvents);
                    InternalHandleAddedKey(newSortKey, eventBean);
                }
            }
            // not yet filled, need to add
            else
            {
                _uniqueKeySortKeys.Put(uniqueKey, newSortKey);
                _numberOfEvents++;
                CollectionUtil.AddEventByKeyLazyListMapBack(newSortKey, eventBean, _sortedEvents);
                InternalHandleAddedKey(newSortKey, eventBean);
            }
        }

        private EventBean RemoveFromSortedEvents(Object sortKey, Object uniqueKeyToRemove)
        {
            var existing = _sortedEvents.Get(sortKey);

            EventBean removedOldEvent = null;
            if (existing != null)
            {
                if (existing is IList<EventBean>)
                {
                    var existingList = (IList<EventBean>)existing;
                    for (int ii = existingList.Count - 1; ii >= 0; ii--)
                    {
                        var eventForRank = existingList[ii];
                        if (Equals(GetUniqueValues(eventForRank), uniqueKeyToRemove))
                        {
                            existingList.RemoveAt(ii);
                            removedOldEvent = eventForRank;
                            break;
                        }
                    }

                    if (existingList.IsEmpty())
                    {
                        _sortedEvents.Remove(sortKey);
                    }
                }
                else
                {
                    removedOldEvent = (EventBean)existing;
                    _sortedEvents.Remove(sortKey);
                }
            }
            return removedOldEvent;
        }

        private EventBean InplaceReplaceSortedEvents(Object sortKey, Object uniqueKeyToReplace, EventBean newData)
        {
            var existing = _sortedEvents.Get(sortKey);

            EventBean replaced = null;
            if (existing != null)
            {
                if (existing is IList<EventBean>)
                {
                    var existingList = (IList<EventBean>)existing;
                    for (int ii = existingList.Count - 1; ii >= 0; ii--)
                    {
                        var eventForRank = existingList[ii];
                        if (Equals(GetUniqueValues(eventForRank), uniqueKeyToReplace))
                        {
                            existingList.RemoveAt(ii);
                            replaced = eventForRank;
                            break;
                        }
                    }
                    existingList.Add(newData);  // add to back as this is now the newest event
                }
                else
                {
                    replaced = (EventBean)existing;
                    _sortedEvents.Put(sortKey, newData);
                }
            }
            return replaced;
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return new RankWindowEnumerator(_sortedEvents);
        }

        public override String ToString()
        {
            return GetType().FullName +
                    " uniqueFieldName=" + _uniqueCriteriaExpressions.Render() +
                    " sortFieldName=" + _sortCriteriaExpressions.Render() +
                    " isDescending=" + _isDescendingValues.Render() +
                    " sortWindowSize=" + _sortWindowSize;
        }

        public Object GetUniqueValues(EventBean theEvent)
        {
            return GetCriteriaKey(_eventsPerStream, _uniqueCriteriaEvaluators, theEvent, _agentInstanceViewFactoryContext);
        }

        public Object GetSortValues(EventBean theEvent)
        {
            return GetCriteriaKey(_eventsPerStream, _sortCriteriaEvaluators, theEvent, _agentInstanceViewFactoryContext);
        }

        public static Object GetCriteriaKey(EventBean[] eventsPerStream, ExprEvaluator[] evaluators, EventBean theEvent, ExprEvaluatorContext evalContext)
        {
            eventsPerStream[0] = theEvent;
            if (evaluators.Length > 1)
            {
                return GetCriteriaMultiKey(eventsPerStream, evaluators, evalContext);
            }
            else
            {
                return evaluators[0].Evaluate(new EvaluateParams(eventsPerStream, true, evalContext));
            }
        }

        public static MultiKeyUntyped GetCriteriaMultiKey(EventBean[] eventsPerStream, ExprEvaluator[] evaluators, ExprEvaluatorContext evalContext)
        {
            var result = new Object[evaluators.Length];
            var count = 0;
            foreach (var expr in evaluators)
            {
                result[count++] = expr.Evaluate(new EvaluateParams(eventsPerStream, true, evalContext));
            }
            return new MultiKeyUntyped(result);
        }

        /// <summary>True to indicate the sort window is empty, or false if not empty. </summary>
        /// <returns>true if empty sort window</returns>
        public bool IsEmpty()
        {
            if (_sortedEvents == null)
            {
                return true;
            }
            return _sortedEvents.IsEmpty();
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(_sortedEvents, false, _rankWindowViewFactory.ViewName, _numberOfEvents, _sortedEvents.Count);
        }

        public ViewFactory ViewFactory
        {
            get { return _rankWindowViewFactory; }
        }
    }
}
