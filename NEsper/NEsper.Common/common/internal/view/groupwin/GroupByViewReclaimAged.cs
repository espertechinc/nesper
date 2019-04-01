///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;
using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.view.groupwin
{
    public class GroupByViewReclaimAged : ViewSupport,
        GroupByView,
        AgentInstanceStopCallback
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly EventBean[] eventsPerStream = new EventBean[1];

        private readonly Dictionary<GroupByViewAgedEntry, Pair<object, object>> groupedEvents =
            new Dictionary<GroupByViewAgedEntry, Pair<object, object>>();

        internal readonly IDictionary<object, GroupByViewAgedEntry> subViewPerKey =
            new Dictionary<object, GroupByViewAgedEntry>();

        private long? nextSweepTime;

        public GroupByViewReclaimAged(
            GroupByViewFactory groupByViewFactory, AgentInstanceViewFactoryChainContext agentInstanceContext)
        {
            ViewFactory = groupByViewFactory;
            AgentInstanceContext = agentInstanceContext;
            MergeView = new MergeView(this, groupByViewFactory.eventType);
        }

        public void Stop(AgentInstanceStopServices services)
        {
            foreach (var entry in subViewPerKey) {
                GroupByViewUtil.RemoveSubview(entry.Value.Subview, services);
            }
        }

        public GroupByViewFactory ViewFactory { get; }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            var aiContext = AgentInstanceContext.AgentInstanceContext;
            aiContext.AuditProvider.View(newData, oldData, aiContext, ViewFactory);
            aiContext.InstrumentationProvider.QViewProcessIRStream(ViewFactory, newData, oldData);

            var currentTime = AgentInstanceContext.TimeProvider.Time;
            if (nextSweepTime == null || nextSweepTime <= currentTime) {
                if (ExecutionPathDebugLog.IsEnabled && Log.IsDebugEnabled) {
                    Log.Debug(
                        "Reclaiming groups older then " + ViewFactory.ReclaimMaxAge + " msec and every " +
                        ViewFactory.ReclaimFrequency + "msec in frequency");
                }

                nextSweepTime = currentTime + ViewFactory.ReclaimFrequency;
                Sweep(currentTime);
            }

            // Algorithm for single new event
            if (newData != null && oldData == null && newData.Length == 1) {
                var theEvent = newData[0];
                var groupByValuesKey = GetGroupKey(theEvent);

                // Get child views that belong to this group-by value combination
                var subView = subViewPerKey.Get(groupByValuesKey);

                // If this is a new group-by value, the list of subviews is null and we need to make clone sub-views
                if (subView == null) {
                    var subview = GroupByViewUtil.MakeSubView(this, groupByValuesKey);
                    subView = new GroupByViewAgedEntry(subview, currentTime);
                    subViewPerKey.Put(groupByValuesKey, subView);
                }
                else {
                    subView.SetLastUpdateTime(currentTime);
                }

                AgentInstanceContext.InstrumentationProvider.QViewIndicate(ViewFactory, newData, null);
                subView.Subview.Update(newData, null);
                AgentInstanceContext.InstrumentationProvider.AViewIndicate();
            }
            else {
                // Algorithm for dispatching multiple events
                if (newData != null) {
                    foreach (var newValue in newData) {
                        HandleEvent(newValue, true);
                    }
                }

                if (oldData != null) {
                    foreach (var oldValue in oldData) {
                        HandleEvent(oldValue, false);
                    }
                }

                // Update child views
                foreach (var entry in groupedEvents) {
                    var newEvents = GroupByViewImpl.ConvertToArray(entry.Value.First);
                    var oldEvents = GroupByViewImpl.ConvertToArray(entry.Value.Second);
                    AgentInstanceContext.InstrumentationProvider.QViewIndicate(ViewFactory, newEvents, oldEvents);
                    entry.Key.Subview.Update(newEvents, oldEvents);
                    AgentInstanceContext.InstrumentationProvider.AViewIndicate();
                }

                groupedEvents.Clear();
            }

            AgentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public AgentInstanceViewFactoryChainContext AgentInstanceContext { get; }

        public MergeView MergeView { get; }

        public void VisitViewContainer(ViewDataVisitorContained viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(GroupByViewImpl.VIEWNAME, subViewPerKey.Count);
            foreach (var entry in subViewPerKey) {
                GroupByViewImpl.VisitView(viewDataVisitor, entry.Key, entry.Value.Subview);
            }
        }

        public override EventType EventType => parent.EventType;

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return MergeView.GetEnumerator();
        }

        private void HandleEvent(EventBean theEvent, bool isNew)
        {
            var groupByValuesKey = GetGroupKey(theEvent);

            // Get child views that belong to this group-by value combination
            var subViews = subViewPerKey.Get(groupByValuesKey);

            // If this is a new group-by value, the list of subviews is null and we need to make clone sub-views
            if (subViews == null) {
                var subview = GroupByViewUtil.MakeSubView(this, groupByValuesKey);
                var currentTime = AgentInstanceContext.StatementContext.TimeProvider.Time;
                subViews = new GroupByViewAgedEntry(subview, currentTime);
                subViewPerKey.Put(groupByValuesKey, subViews);
            }
            else {
                subViews.SetLastUpdateTime(AgentInstanceContext.StatementContext.TimeProvider.Time);
            }

            // Construct a pair of lists to hold the events for the grouped value if not already there
            var pair = groupedEvents.Get(subViews);
            if (pair == null) {
                pair = new Pair<object, object>(null, null);
                groupedEvents.Put(subViews, pair);
            }

            // Add event to a child view event list for later child update that includes new and old events
            if (isNew) {
                pair.First = GroupByViewImpl.AddUpgradeToDequeIfPopulated(pair.First, theEvent);
            }
            else {
                pair.Second = GroupByViewImpl.AddUpgradeToDequeIfPopulated(pair.Second, theEvent);
            }
        }

        public override string ToString()
        {
            return GetType().Name + " groupFieldNames=" + ViewFactory.PropertyNames.RenderAny();
        }

        private void Sweep(long currentTime)
        {
            var removed = new ArrayDeque<object>();
            foreach (var entry in subViewPerKey) {
                var age = currentTime - entry.Value.LastUpdateTime;
                if (age > ViewFactory.ReclaimMaxAge) {
                    removed.Add(entry.Key);
                }
            }

            foreach (var key in removed) {
                var entry = subViewPerKey.Delete(key);
                GroupByViewUtil.RemoveSubview(
                    entry.Subview, new AgentInstanceStopServices(AgentInstanceContext.AgentInstanceContext));
            }
        }

        private object GetGroupKey(EventBean theEvent)
        {
            eventsPerStream[0] = theEvent;
            var criteriaEvaluators = ViewFactory.CriteriaEvals;
            if (criteriaEvaluators.Length == 1) {
                return criteriaEvaluators[0].Evaluate(eventsPerStream, true, AgentInstanceContext);
            }

            var values = new object[criteriaEvaluators.Length];
            for (var i = 0; i < criteriaEvaluators.Length; i++) {
                values[i] = criteriaEvaluators[i].Evaluate(eventsPerStream, true, AgentInstanceContext);
            }

            return new HashableMultiKey(values);
        }
    }
} // end of namespace