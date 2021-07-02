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
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.exttimedwin
{
    /// <summary>
    /// View for a moving window extending the specified amount of time into the past, driven entirely by external timing
    /// supplied within long-type timestamp values in a field of the event beans that the view receives.
    /// <para />The view is completely driven by timestamp values that are supplied by the events it receives,
    /// and does not use the schedule service time.
    /// It requires a field name as parameter for a field that returns ascending long-type timestamp values.
    /// It also requires a long-type parameter setting the time length in milliseconds of the time window.
    /// Events are expected to provide long-type timestamp values in natural order. The view does
    /// itself not use the current system time for keeping track of the time window, but just the
    /// timestamp values supplied by the events sent in.
    /// <para />The arrival of new events with a newer timestamp then past events causes the window to be re-evaluated and the oldest
    /// events pushed out of the window. Ie. Assume event X1 with timestamp T1 is in the window.
    /// When event Xn with timestamp Tn arrives, and the window time length in milliseconds is t, then if
    /// ((Tn - T1) &gt; t == true) then event X1 is pushed as oldData out of the window. It is assumed that
    /// events are sent in in their natural order and the timestamp values are ascending.
    /// </summary>
    public class ExternallyTimedWindowView : ViewSupport,
        DataWindowView
    {
        private readonly ExternallyTimedWindowViewFactory factory;

        private readonly EventBean[] eventsPerStream = new EventBean[1];
        internal readonly TimeWindow timeWindow;
        private readonly ViewUpdatedCollection viewUpdatedCollection;
        protected AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext;
        private readonly TimePeriodProvide timePeriodProvide;

        public ExternallyTimedWindowView(
            ExternallyTimedWindowViewFactory factory,
            ViewUpdatedCollection viewUpdatedCollection,
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext,
            TimePeriodProvide timePeriodProvide)
        {
            this.factory = factory;
            this.viewUpdatedCollection = viewUpdatedCollection;
            timeWindow = new TimeWindow(agentInstanceViewFactoryContext.IsRemoveStream);
            this.agentInstanceViewFactoryContext = agentInstanceViewFactoryContext;
            this.timePeriodProvide = timePeriodProvide;
        }

        public override EventType EventType {
            get {
                // The schema is the parent view's schema
                return Parent.EventType;
            }
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            AgentInstanceContext agentInstanceContext = agentInstanceViewFactoryContext.AgentInstanceContext;
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, factory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(factory, newData, oldData);
            long timestamp = -1;

            // add data points to the window
            // we don't care about removed data from a prior view
            if (newData != null) {
                for (int i = 0; i < newData.Length; i++) {
                    timestamp = GetLongValue(newData[i]);
                    timeWindow.Add(timestamp, newData[i]);
                }
            }

            // Remove from the window any events that have an older timestamp then the last event's timestamp
            ArrayDeque<EventBean> expired = null;
            if (timestamp != -1) {
                expired = timeWindow.ExpireEvents(
                    timestamp -
                    timePeriodProvide.DeltaSubtract(timestamp, null, true, agentInstanceViewFactoryContext) +
                    1);
            }

            EventBean[] oldDataUpdate = null;
            if ((expired != null) && (!expired.IsEmpty())) {
                oldDataUpdate = expired.ToArray();
            }

            if ((oldData != null) && (agentInstanceViewFactoryContext.IsRemoveStream)) {
                foreach (EventBean anOldData in oldData) {
                    timeWindow.Remove(anOldData);
                }

                if (oldDataUpdate == null) {
                    oldDataUpdate = oldData;
                }
                else {
                    oldDataUpdate = CollectionUtil.AddArrayWithSetSemantics(oldData, oldDataUpdate);
                }
            }

            viewUpdatedCollection?.Update(newData, oldDataUpdate);

            // If there are child views, fireStatementStopped update method
            if (Child != null) {
                agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, newData, oldDataUpdate);
                Child.Update(newData, oldDataUpdate);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();
            }

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return timeWindow.GetEnumerator();
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            timeWindow.VisitView(viewDataVisitor, factory);
        }

        private long GetLongValue(EventBean obj)
        {
            eventsPerStream[0] = obj;
            var num = factory.timestampEval.Evaluate(eventsPerStream, true, agentInstanceViewFactoryContext);
            return num.AsInt64();
        }

        /// <summary>
        /// Returns true to indicate the window is empty, or false if the view is not empty.
        /// </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty {
            get => timeWindow.IsEmpty();
        }

        public ViewUpdatedCollection ViewUpdatedCollection {
            get => viewUpdatedCollection;
        }

        public ViewFactory ViewFactory {
            get => factory;
        }
    }
} // end of namespace