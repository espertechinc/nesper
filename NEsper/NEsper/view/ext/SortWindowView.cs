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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.view.ext
{
    /// <summary>
    /// Window sorting by values in the specified field extending a specified number of 
    /// elements from the lowest value up or the highest value down. The view accepts 3 
    /// parameters. The first parameter is the field name to get the values to sort for, 
    /// the second parameter defines whether to sort ascending or descending, the third 
    /// parameter is the number of elements to keep in the sorted list. The type of the 
    /// field to be sorted in the event must implement the Comparable interface. The natural 
    /// order in which events arrived is used as the second sorting criteria. Thus should 
    /// events arrive with equal sort values the oldest event leaves the sort window first. 
    /// Old values removed from a prior view are removed from the sort view.
    /// </summary>
    public class SortWindowView : ViewSupport, DataWindowView, CloneableView
    {
        private readonly SortWindowViewFactory _sortWindowViewFactory;
        private readonly ExprEvaluator[] _sortCriteriaEvaluators;
        private readonly ExprNode[] _sortCriteriaExpressions;
        private readonly EventBean[] _eventsPerStream = new EventBean[1];
        private readonly bool[] _isDescendingValues;
        private readonly int _sortWindowSize;
        private readonly IStreamSortRankRandomAccess _optionalSortedRandomAccess;
        protected readonly AgentInstanceViewFactoryChainContext AgentInstanceViewFactoryContext;

        private readonly OrderedDictionary<Object, Object> _sortedEvents;
        private int _eventCount;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="sortWindowViewFactory">for copying this view in a group-by</param>
        /// <param name="sortCriteriaExpressions">is the event property names to sort</param>
        /// <param name="sortCriteriaEvaluators">The sort criteria evaluators.</param>
        /// <param name="descendingValues">indicates whether to sort ascending or descending for each field</param>
        /// <param name="sortWindowSize">is the window size</param>
        /// <param name="optionalSortedRandomAccess">is the friend class handling the random access, if required byexpressions</param>
        /// <param name="isSortUsingCollator">for string value sorting using compare or Collator</param>
        /// <param name="agentInstanceViewFactoryContext">The agent instance view factory context.</param>
        public SortWindowView(SortWindowViewFactory sortWindowViewFactory,
                              ExprNode[] sortCriteriaExpressions,
                              ExprEvaluator[] sortCriteriaEvaluators,
                              bool[] descendingValues,
                              int sortWindowSize,
                              IStreamSortRankRandomAccess optionalSortedRandomAccess,
                              bool isSortUsingCollator,
                              AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            _sortWindowViewFactory = sortWindowViewFactory;
            _sortCriteriaExpressions = sortCriteriaExpressions;
            _sortCriteriaEvaluators = sortCriteriaEvaluators;
            _isDescendingValues = descendingValues;
            _sortWindowSize = sortWindowSize;
            _optionalSortedRandomAccess = optionalSortedRandomAccess;
            AgentInstanceViewFactoryContext = agentInstanceViewFactoryContext;
    
            var comparator = CollectionUtil.GetComparator(sortCriteriaEvaluators, isSortUsingCollator, _isDescendingValues);
            _sortedEvents = new OrderedDictionary<Object, Object>(comparator);
        }

        /// <summary>Returns the field names supplying the values to sort by. </summary>
        /// <value>field names to sort by</value>
        protected internal ExprNode[] SortCriteriaExpressions
        {
            get { return _sortCriteriaExpressions; }
        }

        /// <summary>Returns the flags indicating whether to sort in descending order on each property. </summary>
        /// <value>the isDescending value for each sort property</value>
        protected internal bool[] IsDescendingValues
        {
            get { return _isDescendingValues; }
        }

        /// <summary>Returns the number of elements kept by the sort window. </summary>
        /// <value>size of window</value>
        protected internal int SortWindowSize
        {
            get { return _sortWindowSize; }
        }

        public View CloneView()
        {
            return _sortWindowViewFactory.MakeView(AgentInstanceViewFactoryContext);
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
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewProcessIRStream(this, _sortWindowViewFactory.ViewName, newData, oldData);}
    
            OneEventCollection removedEvents = null;
    
            // Remove old data
            if (oldData != null)
            {
                for (var i = 0; i < oldData.Length; i++)
                {
                    var oldDataItem = oldData[i];
                    var sortValues = GetSortValues(oldDataItem);
                    var result = CollectionUtil.RemoveEventByKeyLazyListMap(sortValues, oldDataItem, _sortedEvents);
                    if (result)
                    {
                        _eventCount--;
                        if (removedEvents == null) {
                            removedEvents = new OneEventCollection();
                        }
                        removedEvents.Add(oldDataItem);
                        InternalHandleRemoved(sortValues, oldDataItem);
                    }
                }
            }
    
            // Add new data
            if (newData != null)
            {
                for (var i = 0; i < newData.Length; i++)
                {
                    var newDataItem = newData[i];
                    var sortValues = GetSortValues(newDataItem);
                    CollectionUtil.AddEventByKeyLazyListMapFront(sortValues, newDataItem, _sortedEvents);
                    _eventCount++;
                    InternalHandleAdd(sortValues, newDataItem);
                }
            }
    
            // Remove data that sorts to the bottom of the window
            if (_eventCount > _sortWindowSize)
            {
                var removeCount = _eventCount - _sortWindowSize;
                for (var i = 0; i < removeCount; i++)
                {
                    // Remove the last element of the last key - sort order is key and then natural order of arrival
                    var lastKey = _sortedEvents.Keys.Last();
                    var lastEntry = _sortedEvents.Get(lastKey);
                    if (lastEntry is IList<EventBean>) {
                        var events = (IList<EventBean>) lastEntry;
                        var theEvent = events.Delete(events.Count - 1);  // remove oldest event, newest events are first in list
                        _eventCount--;
                        if (events.IsEmpty()) {
                            _sortedEvents.Remove(lastKey);
                        }
                        if (removedEvents == null) {
                            removedEvents = new OneEventCollection();
                        }
                        removedEvents.Add(theEvent);
                        InternalHandleRemoved(lastKey, theEvent);
                    }
                    else {
                        var theEvent = (EventBean) lastEntry;
                        _eventCount--;
                        _sortedEvents.Remove(lastKey);
                        if (removedEvents == null) {
                            removedEvents = new OneEventCollection();
                        }
                        removedEvents.Add(theEvent);
                        InternalHandleRemoved(lastKey, theEvent);
                    }
                }
            }
    
            // If there are child views, fireStatementStopped Update method
            if (_optionalSortedRandomAccess != null)
            {
                _optionalSortedRandomAccess.Refresh(_sortedEvents, _eventCount, _sortWindowSize);
            }
    
            if (HasViews)
            {
                EventBean[] expiredArr = null;
                if (removedEvents != null)
                {
                    expiredArr = removedEvents.ToArray();
                }
    
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewIndicate(this, _sortWindowViewFactory.ViewName, newData, expiredArr);}
                UpdateChildren(newData, expiredArr);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewIndicate();}
            }
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewProcessIRStream();}
        }
    
        public void InternalHandleAdd(Object sortValues, EventBean newDataItem) {
            // no action required
        }
    
        public void InternalHandleRemoved(Object sortValues, EventBean oldDataItem) {
            // no action required
        }
    
        public override IEnumerator<EventBean> GetEnumerator()
        {
            return new SortWindowEnumerator(_sortedEvents);
        }
    
        public override String ToString()
        {
            return GetType().FullName +
                    " sortFieldName=" + CompatExtensions.Render(_sortCriteriaExpressions) +
                    " isDescending=" + CompatExtensions.Render(_isDescendingValues) +
                    " sortWindowSize=" + _sortWindowSize;
        }
    
        protected Object GetSortValues(EventBean theEvent)
        {
            var evaluateParams = new EvaluateParams(_eventsPerStream, true, AgentInstanceViewFactoryContext);

            _eventsPerStream[0] = theEvent;
            if (_sortCriteriaExpressions.Length == 1)
            {
                return _sortCriteriaEvaluators[0].Evaluate(evaluateParams);
            }

            var result = new Object[_sortCriteriaExpressions.Length];
        	var count = 0;
        	foreach (var expr in _sortCriteriaEvaluators)
        	{
        	    result[count++] = expr.Evaluate(evaluateParams);
        	}
        	return new MultiKeyUntyped(result);
        }

        /// <summary>
        /// True to indicate the sort window is empty, or false if not empty.
        /// </summary>
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
            viewDataVisitor.VisitPrimary(_sortedEvents, false, _sortWindowViewFactory.ViewName, _eventCount, null);
        }

        public ViewFactory ViewFactory
        {
            get { return _sortWindowViewFactory; }
        }
    }
}
