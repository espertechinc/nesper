///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.unique
{
    /// <summary>
    ///     This view includes only the most recent among events having the same value for the specified field or fields.
    ///     The view accepts the field name as parameter from which the unique values are obtained.
    ///     For example, a trade's symbol could be used as a unique value.
    ///     In this example, the first trade for symbol IBM would be posted as new data to child views.
    ///     When the second trade for symbol IBM arrives the second trade is posted as new data to child views,
    ///     and the first trade is posted as old data.
    ///     Should more than one trades for symbol IBM arrive at the same time (like when batched)
    ///     then the child view will get all new events in newData and all new events in oldData minus the most recent event.
    ///     When the current new event arrives as old data, the the current unique event gets thrown away and
    ///     posted as old data to child views.
    ///     Iteration through the views data shows only the most recent events received for the unique value in the order
    ///     that events arrived in.
    ///     The type of the field returning the unique value can be any type but should override equals and hashCode()
    ///     as the type plays the role of a key in a map storing unique values.
    /// </summary>
    public class UniqueByPropertyView : ViewSupport,
        DataWindowView
    {
        private readonly AgentInstanceContext agentInstanceContext;
        private readonly EventBean[] eventsPerStream = new EventBean[1];
        private readonly IDictionary<object, EventBean> mostRecentEvents = new Dictionary<object, EventBean>();
        private readonly UniqueByPropertyViewFactory viewFactory;

        public UniqueByPropertyView(
            UniqueByPropertyViewFactory viewFactory,
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            this.viewFactory = viewFactory;
            agentInstanceContext = agentInstanceViewFactoryContext.AgentInstanceContext;
        }

        /// <summary>
        ///     Returns true if the view is empty.
        /// </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty => mostRecentEvents.IsEmpty();

        public ViewFactory ViewFactory => viewFactory;

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, viewFactory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(viewFactory, newData, oldData);

            if (newData != null && newData.Length == 1 && (oldData == null || oldData.Length == 0)) {
                // Shortcut
                var key = GetUniqueKey(newData[0]);
                EventBean lastValue = mostRecentEvents.Put(key, newData[0]);
                if (child != null) {
                    var oldDataToPost = lastValue == null ? null : new[] {lastValue};
                    agentInstanceContext.InstrumentationProvider.QViewIndicate(viewFactory, newData, oldDataToPost);
                    child.Update(newData, oldDataToPost);
                    agentInstanceContext.InstrumentationProvider.AViewIndicate();
                }
            }
            else {
                OneEventCollection postOldData = null;

                if (child != null) {
                    postOldData = new OneEventCollection();
                }

                if (newData != null) {
                    for (var i = 0; i < newData.Length; i++) {
                        // Obtain unique value
                        var key = GetUniqueKey(newData[i]);

                        // If there are no child views, just update the own collection
                        if (child == null) {
                            mostRecentEvents.Put(key, newData[i]);
                            continue;
                        }

                        // Post the last value as old data
                        var lastValue = mostRecentEvents.Get(key);
                        if (lastValue != null) {
                            postOldData.Add(lastValue);
                        }

                        // Override with recent event
                        mostRecentEvents.Put(key, newData[i]);
                    }
                }

                if (oldData != null) {
                    for (var i = 0; i < oldData.Length; i++) {
                        // Obtain unique value
                        var key = GetUniqueKey(oldData[i]);

                        // If the old event is the current unique event, remove and post as old data
                        var lastValue = mostRecentEvents.Get(key);
                        if (lastValue == null || !lastValue.Equals(oldData[i])) {
                            continue;
                        }

                        postOldData.Add(lastValue);
                        mostRecentEvents.Remove(key);
                    }
                }

                // If there are child views, fireStatementStopped update method
                if (child != null) {
                    if (postOldData.IsEmpty()) {
                        agentInstanceContext.InstrumentationProvider.QViewIndicate(viewFactory, newData, null);
                        child.Update(newData, null);
                        agentInstanceContext.InstrumentationProvider.AViewIndicate();
                    }
                    else {
                        EventBean[] postOldDataArray = postOldData.ToArray();
                        agentInstanceContext.InstrumentationProvider.QViewIndicate(
                            viewFactory, newData, postOldDataArray);
                        child.Update(newData, postOldDataArray);
                        agentInstanceContext.InstrumentationProvider.AViewIndicate();
                    }
                }
            }

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(
                mostRecentEvents, true, UniqueByPropertyViewFactory.NAME, mostRecentEvents.Count,
                mostRecentEvents.Count);
        }

        public override EventType EventType {
            get {
                // The schema is the parent view's schema
                return Parent.EventType;
            }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return mostRecentEvents.Values.GetEnumerator();
        }

        public override string ToString()
        {
            return GetType().Name;
        }

        internal object GetUniqueKey(EventBean theEvent)
        {
            eventsPerStream[0] = theEvent;
            var criteriaExpressionsEvals = viewFactory.CriteriaEvals;
            if (criteriaExpressionsEvals.Length == 1) {
                return criteriaExpressionsEvals[0].Evaluate(eventsPerStream, true, agentInstanceContext);
            }

            var values = new object[criteriaExpressionsEvals.Length];
            for (var i = 0; i < criteriaExpressionsEvals.Length; i++) {
                values[i] = criteriaExpressionsEvals[i].Evaluate(eventsPerStream, true, agentInstanceContext);
            }

            return new HashableMultiKey(values);
        }
    }
} // end of namespace