///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.firstunique
{
    /// <summary>
    ///     This view retains the first event for each multi-key of distinct property values.
    ///     <para />
    ///     The view does not post a remove stream unless explicitly deleted from.
    ///     <para />
    ///     The view swallows any insert stream events that provide no new distinct set of property values.
    /// </summary>
    public class FirstUniqueByPropertyView : ViewSupport,
        DataWindowView
    {
        private readonly AgentInstanceContext agentInstanceContext;
        private readonly IDictionary<object, EventBean> firstEvents = new Dictionary<object, EventBean>();
        private readonly FirstUniqueByPropertyViewFactory viewFactory;
        private readonly EventBean[] eventsPerStream = new EventBean[1];

        public FirstUniqueByPropertyView(
            FirstUniqueByPropertyViewFactory viewFactory,
            AgentInstanceViewFactoryChainContext agentInstanceContext)
        {
            this.viewFactory = viewFactory;
            this.agentInstanceContext = agentInstanceContext.AgentInstanceContext;
        }

        /// <summary>
        ///     Returns true if empty.
        /// </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty => firstEvents.IsEmpty();

        public ViewFactory ViewFactory => viewFactory;

        public override EventType EventType => parent.EventType;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, viewFactory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(viewFactory, newData, oldData);

            EventBean[] newDataToPost = null;
            EventBean[] oldDataToPost = null;

            if (oldData != null) {
                foreach (var oldEvent in oldData) {
                    // Obtain unique value
                    var key = GetUniqueKey(oldEvent);

                    // If the old event is the current unique event, remove and post as old data
                    var lastValue = firstEvents.Get(key);

                    if (lastValue != oldEvent) {
                        continue;
                    }

                    if (oldDataToPost == null) {
                        oldDataToPost = new[] { oldEvent };
                    }
                    else {
                        oldDataToPost = EventBeanUtility.AddToArray(oldDataToPost, oldEvent);
                    }

                    firstEvents.Remove(key);
                }
            }

            if (newData != null) {
                foreach (var newEvent in newData) {
                    // Obtain unique value
                    var key = GetUniqueKey(newEvent);

                    // already-seen key
                    if (firstEvents.ContainsKey(key)) {
                        continue;
                    }

                    // store
                    firstEvents.Put(key, newEvent);

                    // Post the new value
                    if (newDataToPost == null) {
                        newDataToPost = new[] { newEvent };
                    }
                    else {
                        newDataToPost = EventBeanUtility.AddToArray(newDataToPost, newEvent);
                    }
                }
            }

            if (child != null && (newDataToPost != null || oldDataToPost != null)) {
                agentInstanceContext.InstrumentationProvider.QViewIndicate(viewFactory, newDataToPost, oldDataToPost);
                child.Update(newDataToPost, oldDataToPost);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();
            }

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return firstEvents.Values.GetEnumerator();
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(firstEvents, true, viewFactory.ViewName, firstEvents.Count, firstEvents.Count);
        }

        public override string ToString()
        {
            return GetType().Name;
        }

        protected object GetUniqueKey(EventBean theEvent)
        {
            eventsPerStream[0] = theEvent;
            return viewFactory.CriteriaEval.Evaluate(eventsPerStream, true, agentInstanceContext);
        }
    }
} // end of namespace