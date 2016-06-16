///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.util;

namespace com.espertech.esper.view.std
{
    public class GroupByViewReclaimAged
        : ViewSupport
        , CloneableView
        , GroupByView
    {
        private readonly ExprNode[] _criteriaExpressions;
        private readonly ExprEvaluator[] _criteriaEvaluators;
        private readonly AgentInstanceViewFactoryChainContext _agentInstanceContext;
        private readonly long _reclaimMaxAge;
        private readonly long _reclaimFrequency;
    
        private readonly EventBean[] _eventsPerStream = new EventBean[1];
        private readonly String[] _propertyNames;

        private readonly IDictionary<Object, GroupByViewAgedEntry> _subViewsPerKey = 
            new Dictionary<Object, GroupByViewAgedEntry>();
        private readonly Dictionary<GroupByViewAgedEntry, Pair<Object, Object>> _groupedEvents =
            new Dictionary<GroupByViewAgedEntry, Pair<Object, Object>>();
        private long? _nextSweepTime = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="agentInstanceContext">contains required view services</param>
        /// <param name="criteriaExpressions">is the fields from which to pull the values to group by</param>
        /// <param name="criteriaEvaluators">The criteria evaluators.</param>
        /// <param name="reclaimMaxAge">age after which to reclaim group</param>
        /// <param name="reclaimFrequency">frequency in which to check for groups to reclaim</param>
        public GroupByViewReclaimAged(AgentInstanceViewFactoryChainContext agentInstanceContext,
                                      ExprNode[] criteriaExpressions,
                                      ExprEvaluator[] criteriaEvaluators,
                                      double reclaimMaxAge, double reclaimFrequency)
        {
            _agentInstanceContext = agentInstanceContext;
            _criteriaExpressions = criteriaExpressions;
            _criteriaEvaluators = criteriaEvaluators;
            _reclaimMaxAge = (long) (reclaimMaxAge * 1000d);
            _reclaimFrequency = (long) (reclaimFrequency * 1000d);
    
            _propertyNames = new String[criteriaExpressions.Length];
            for (var i = 0; i < criteriaExpressions.Length; i++)
            {
                _propertyNames[i] = ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(criteriaExpressions[i]);
            }
        }
    
        public View CloneView()
        {
            return new GroupByViewReclaimAged(_agentInstanceContext, _criteriaExpressions, _criteriaEvaluators, _reclaimMaxAge, _reclaimFrequency);
        }

        /// <summary>Returns the field name that provides the key valie by which to group by. </summary>
        /// <value>field name providing group-by key.</value>
        public ExprNode[] CriteriaExpressions
        {
            get { return _criteriaExpressions; }
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
            var currentTime = _agentInstanceContext.TimeProvider.Time;
            if ((_nextSweepTime == null) || (_nextSweepTime <= currentTime))
            {
                if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
                {
                    Log.Debug("Reclaiming groups older then " + _reclaimMaxAge + " msec and every " + _reclaimFrequency + "msec in frequency");
                }
                _nextSweepTime = currentTime + _reclaimFrequency;
                Sweep(currentTime);
            }
    
            // Algorithm for single new event
            if ((newData != null) && (oldData == null) && (newData.Length == 1))
            {
                var theEvent = newData[0];
                var newDataToPost = new EventBean[] {theEvent};
    
                var groupByValuesKey = GetGroupKey(theEvent);
    
                // Get child views that belong to this group-by value combination
                var subViews = _subViewsPerKey.Get(groupByValuesKey);
    
                // If this is a new group-by value, the list of subviews is null and we need to make clone sub-views
                if (subViews == null)
                {
                    var subviewsList = GroupByViewImpl.MakeSubViews(this, _propertyNames, groupByValuesKey, _agentInstanceContext);
                    subViews = new GroupByViewAgedEntry(subviewsList, currentTime);
                    _subViewsPerKey.Put(groupByValuesKey, subViews);
                }
                else {
                    subViews.LastUpdateTime = currentTime;
                }
    
                GroupByViewImpl.UpdateChildViews(subViews.SubviewHolder, newDataToPost, null);
            }
            else
            {
    
                // Algorithm for dispatching multiple events
                if (newData != null)
                {
                    foreach (var newValue in newData)
                    {
                        HandleEvent(newValue, true);
                    }
                }
    
                if (oldData != null)
                {
                    foreach (var oldValue in oldData)
                    {
                        HandleEvent(oldValue, false);
                    }
                }
    
                // Update child views
                foreach (var entry in _groupedEvents)
                {
                    var newEvents = GroupByViewImpl.ConvertToArray(entry.Value.First);
                    var oldEvents = GroupByViewImpl.ConvertToArray(entry.Value.Second);
                    GroupByViewImpl.UpdateChildViews(entry.Key, newEvents, oldEvents);
                }
    
                _groupedEvents.Clear();
            }
        }
    
        private void HandleEvent(EventBean theEvent, bool isNew)
        {
            var groupByValuesKey = GetGroupKey(theEvent);
    
            // Get child views that belong to this group-by value combination
            var subViews = _subViewsPerKey.Get(groupByValuesKey);
    
            // If this is a new group-by value, the list of subviews is null and we need to make clone sub-views
            if (subViews == null)
            {
                var subviewsList = GroupByViewImpl.MakeSubViews(this, _propertyNames, groupByValuesKey, _agentInstanceContext);
                var currentTime = _agentInstanceContext.StatementContext.TimeProvider.Time;
                subViews = new GroupByViewAgedEntry(subviewsList, currentTime);
                _subViewsPerKey.Put(groupByValuesKey, subViews);
            }
            else {
                subViews.LastUpdateTime = _agentInstanceContext.StatementContext.TimeProvider.Time;
            }
    
            // Construct a pair of lists to hold the events for the grouped value if not already there
            var pair = _groupedEvents.Get(subViews);
            if (pair == null) {
                pair = new Pair<Object, Object>(null, null);
                _groupedEvents.Put(subViews, pair);
            }
    
            // Add event to a child view event list for later child Update that includes new and old events
            if (isNew) {
                pair.First = GroupByViewImpl.AddUpgradeToDequeIfPopulated(pair.First, theEvent);
            }
            else {
                pair.Second = GroupByViewImpl.AddUpgradeToDequeIfPopulated(pair.Second, theEvent);
            }
        }
    
        public override IEnumerator<EventBean> GetEnumerator()
        {
            throw new UnsupportedOperationException("Cannot iterate over group view, this operation is not supported");
        }
    
        public override String ToString()
        {
            return GetType().FullName + " groupFieldNames=" + _criteriaExpressions.Render();
        }
    
        public void VisitViewContainer(ViewDataVisitorContained viewDataVisitor) {
            viewDataVisitor.VisitPrimary(GroupByViewImpl.VIEWNAME, _subViewsPerKey.Count);
            foreach (var entry in _subViewsPerKey) {
                GroupByViewImpl.VisitView(viewDataVisitor, entry.Key, entry.Value.SubviewHolder);
            }
        }
    
        private void Sweep(long currentTime)
        {
            var removed = new ArrayDeque<Object>();
            foreach (var entry in _subViewsPerKey)
            {
                var age = currentTime - entry.Value.LastUpdateTime;
                if (age > _reclaimMaxAge)
                {
                    removed.Add(entry.Key);
                }
            }
    
            foreach (var key in removed)
            {
                var entry = _subViewsPerKey.Pluck(key);
                var subviewHolder = entry.SubviewHolder;
                if (subviewHolder is IList<View>)
                {
                    var subviews = (IList<View>) subviewHolder;
                    foreach (var view in subviews) {
                        RemoveSubview(view);
                    }
                }
                else if (subviewHolder is View) {
                    RemoveSubview((View) subviewHolder);
                }
            }
        }

        public override bool RemoveView(View view)
        {
            if (!(view is GroupableView))
            {
                base.RemoveView(view);
            }
            var removed = base.RemoveView(view);
            if (!removed)
            {
                return false;
            }
            if (!HasViews)
            {
                _subViewsPerKey.Clear();
                return true;
            }
            var removedView = (GroupableView) view;
            Deque<Object> removedKeys = null;
            foreach (var entry in _subViewsPerKey)
            {
                var value = entry.Value.SubviewHolder;
                if (value is View)
                {
                    var subview = (GroupableView) value;
                    if (CompareViews(subview, removedView))
                    {
                        if (removedKeys == null)
                        {
                            removedKeys = new ArrayDeque<Object>();
                        }
                        removedKeys.Add(entry.Key);
                    }
                }
                else if (value is IList<View>)
                {
                    var subviews = (IList<View>) value;
                    for (var i = 0; i < subviews.Count; i++)
                    {
                        var subview = (GroupableView) subviews[i];
                        if (CompareViews(subview, removedView))
                        {
                            subviews.RemoveAt(i);
                            if (subviews.IsEmpty())
                            {
                                if (removedKeys == null)
                                {
                                    removedKeys = new ArrayDeque<Object>();
                                }
                                removedKeys.Add(entry.Key);
                            }
                            break;
                        }
                    }
                }
            }
            if (removedKeys != null)
            {
                foreach (var key in removedKeys)
                {
                    _subViewsPerKey.Remove(key);
                }
            }
            return true;
        }

        private bool CompareViews(GroupableView subview, GroupableView removed)
        {
            return subview.ViewFactory == removed.ViewFactory;
        }
    
        private void RemoveSubview(View view)
        {
            view.Parent = null;
            RecursiveMergeViewRemove(view);
            var stoppableView = view as StoppableView;
            if (stoppableView != null) {
                stoppableView.Stop();
            }
        }
    
        private void RecursiveMergeViewRemove(View view)
        {
            foreach (var child in view.Views)
            {
                var mergeView = child as MergeView;
                if (mergeView != null)
                {
                    mergeView.RemoveParentView(view);
                }
                else
                {
                    var stoppableView = child as StoppableView;
                    if (stoppableView != null)
                    {
                        stoppableView.Stop();
                    }
                    if (child.Views.Length > 0)
                    {
                        RecursiveMergeViewRemove(child);
                    }
                }
            }
        }
    
        private Object GetGroupKey(EventBean theEvent)
        {
            var evaluateParams = new EvaluateParams(_eventsPerStream, true, _agentInstanceContext);

            _eventsPerStream[0] = theEvent;
            if (_criteriaEvaluators.Length == 1)
            {
                return _criteriaEvaluators[0].Evaluate(evaluateParams);
            }

            var values = new Object[_criteriaEvaluators.Length];
            for (var i = 0; i < _criteriaEvaluators.Length; i++)
            {
                values[i] = _criteriaEvaluators[i].Evaluate(evaluateParams);
            }
            return new MultiKeyUntyped(values);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
