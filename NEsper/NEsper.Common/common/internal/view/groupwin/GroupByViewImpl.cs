///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.view.groupwin
{
    /// <summary>
    ///     The group view splits the data in a stream to multiple subviews, based on a key index.
    ///     The key is one or more fields in the stream. Any view that follows the GROUP view will be executed
    ///     separately on each subview, one per unique key.
    ///     <para />
    ///     The view takes a single parameter which is the field name returning the key value to group.
    ///     <para />
    ///     This view can, for example, be used to calculate the average price per symbol for a list of symbols.
    ///     <para />
    ///     The view treats its child views and their child views as prototypes. It dynamically instantiates copies
    ///     of each child view and their child views, and the child view's child views as so on. When there are
    ///     no more child views or the special merge view is encountered, it ends. The view installs a special merge
    ///     view unto each leaf child view that merges the value key that was grouped by back into the stream
    ///     using the group-by field name.
    /// </summary>
    public class GroupByViewImpl : ViewSupport,
        GroupByView,
        AgentInstanceMgmtCallback
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public const string VIEWNAME = "Group-By";

        private readonly AgentInstanceViewFactoryChainContext agentInstanceContext;

        private readonly Dictionary<View, Pair<object, object>> groupedEvents =
            new Dictionary<View, Pair<object, object>>();

        private readonly IDictionary<object, View> subViewPerKey = new Dictionary<object, View>()
            .WithNullKeySupport();

        private readonly EventBean[] eventsPerStream = new EventBean[1];
        private readonly GroupByViewFactory _groupByViewFactory;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="groupByGroupByViewFactory">view factory</param>
        /// <param name="agentInstanceContext">contains required view services</param>
        public GroupByViewImpl(
            GroupByViewFactory groupByGroupByViewFactory,
            AgentInstanceViewFactoryChainContext agentInstanceContext)
        {
            _groupByViewFactory = groupByGroupByViewFactory;
            this.agentInstanceContext = agentInstanceContext;
            MergeView = new MergeView(this, groupByGroupByViewFactory.EventType);
        }

        public void Stop(AgentInstanceStopServices services)
        {
            foreach (KeyValuePair<object, View> entry in subViewPerKey) {
                GroupByViewUtil.RemoveSubview(entry.Value, services);
            }
        }

        public GroupByViewFactory ViewFactory => _groupByViewFactory;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            AgentInstanceContext aiContext = agentInstanceContext.AgentInstanceContext;
            aiContext.AuditProvider.View(newData, oldData, aiContext, ViewFactory);
            aiContext.InstrumentationProvider.QViewProcessIRStream(ViewFactory, newData, oldData);

            // Algorithm for single new event
            if (newData != null && oldData == null && newData.Length == 1) {
                var theEvent = newData[0];
                var newDataToPost = new EventBean[] {theEvent};

                var groupByValuesKey = GetGroupKey(theEvent);

                // Get child views that belong to this group-by value combination
                var subView = subViewPerKey.Get(groupByValuesKey);

                // If this is a new group-by value, the list of subviews is null and we need to make clone sub-views
                if (subView == null) {
                    subView = GroupByViewUtil.MakeSubView(this, groupByValuesKey);
                    subViewPerKey.Put(groupByValuesKey, subView);
                }

                agentInstanceContext.InstrumentationProvider.QViewIndicate(ViewFactory, newDataToPost, null);
                subView.Update(newDataToPost, null);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();
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
                foreach (KeyValuePair<View, Pair<object, object>> entry in groupedEvents) {
                    var newEvents = ConvertToArray(entry.Value.First);
                    var oldEvents = ConvertToArray(entry.Value.Second);
                    agentInstanceContext.InstrumentationProvider.QViewIndicate(ViewFactory, newEvents, oldEvents);
                    entry.Key.Update(newEvents, oldEvents);
                    agentInstanceContext.InstrumentationProvider.AViewIndicate();
                }

                groupedEvents.Clear();
            }

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public void VisitViewContainer(ViewDataVisitorContained viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(VIEWNAME, subViewPerKey.Count);
            foreach (KeyValuePair<object, View> entry in subViewPerKey) {
                VisitView(viewDataVisitor, entry.Key, entry.Value);
            }
        }

        public MergeView MergeView { get; }

        public AgentInstanceViewFactoryChainContext AgentInstanceContext => agentInstanceContext;

        public override EventType EventType {
            get {
                // The schema is the parent view's schema
                return parent.EventType;
            }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return MergeView.GetEnumerator();
        }

        public override string ToString()
        {
            return GetType().Name + " groupFieldNames=" + ViewFactory.PropertyNames.RenderAny();
        }

        public static void VisitView(
            ViewDataVisitorContained viewDataVisitor,
            object groupkey,
            View view)
        {
            if (view == null) {
                return;
            }

            viewDataVisitor.VisitContained(groupkey, view);
        }

        private void HandleEvent(
            EventBean theEvent,
            bool isNew)
        {
            var groupByValuesKey = GetGroupKey(theEvent);

            // Get child views that belong to this group-by value combination
            var subView = subViewPerKey.Get(groupByValuesKey);

            // If this is a new group-by value, the list of subviews is null and we need to make clone sub-views
            if (subView == null) {
                subView = GroupByViewUtil.MakeSubView(this, groupByValuesKey);
                subViewPerKey.Put(groupByValuesKey, subView);
            }

            // Construct a pair of lists to hold the events for the grouped value if not already there
            Pair<object, object> pair = groupedEvents.Get(subView);
            if (pair == null) {
                pair = new Pair<object, object>(null, null);
                groupedEvents.Put(subView, pair);
            }

            // Add event to a child view event list for later child update that includes new and old events
            if (isNew) {
                pair.First = AddUpgradeToDequeIfPopulated(pair.First, theEvent);
            }
            else {
                pair.Second = AddUpgradeToDequeIfPopulated(pair.Second, theEvent);
            }
        }

        private object GetGroupKey(EventBean theEvent)
        {
            eventsPerStream[0] = theEvent;
            return _groupByViewFactory.CriteriaEval.Evaluate(eventsPerStream, true, agentInstanceContext);
        }

        protected internal static object AddUpgradeToDequeIfPopulated(
            object holder,
            EventBean theEvent)
        {
            if (holder == null) {
                return theEvent;
            }

            if (holder is Deque<EventBean>) {
                var deque = (Deque<EventBean>) holder;
                deque.Add(theEvent);
                return deque;
            }
            else {
                var deque = new ArrayDeque<EventBean>(4);
                deque.Add((EventBean) holder);
                deque.Add(theEvent);
                return deque;
            }
        }

        protected internal static EventBean[] ConvertToArray(object eventOrDeque)
        {
            if (eventOrDeque == null) {
                return null;
            }

            if (eventOrDeque is EventBean) {
                return new[] {(EventBean) eventOrDeque};
            }

            return ((ArrayDeque<EventBean>) eventOrDeque).ToArray();
        }
    }
} // end of namespace