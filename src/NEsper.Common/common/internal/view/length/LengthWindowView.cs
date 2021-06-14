///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.length
{
    /// <summary>
    ///     This view is a moving window extending the specified number of elements into the past.
    /// </summary>
    public class LengthWindowView : ViewSupport
    {
        internal readonly AgentInstanceContext agentInstanceContext;
        internal readonly ArrayDeque<EventBean> events = new ArrayDeque<EventBean>();
        private readonly LengthWindowViewFactory viewFactory;

        /// <summary>
        /// Constructor creates a moving window extending the specified number of elements into the past.
        /// </summary>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <param name="viewFactory">for copying this view in a group-by</param>
        /// <param name="size">is the specified number of elements into the past</param>
        /// <param name="viewUpdatedCollection">is a collection that the view must update when receiving events</param>
        /// <exception cref="ArgumentException">Illegal argument for size of length window</exception>
        public LengthWindowView(
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            LengthWindowViewFactory viewFactory,
            int size,
            ViewUpdatedCollection viewUpdatedCollection)
        {
            if (size < 1) {
                throw new ArgumentException("Illegal argument for size of length window");
            }

            this.agentInstanceContext = agentInstanceContext.AgentInstanceContext;
            this.viewFactory = viewFactory;
            Size = size;
            ViewUpdatedCollection = viewUpdatedCollection;
        }

        public bool IsEmpty => events.IsEmpty();

        public int Size { get; }

        public ViewUpdatedCollection ViewUpdatedCollection { get; }

        public override EventType EventType => Parent.EventType;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, viewFactory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(viewFactory, newData, oldData);

            // add data points to the window
            // we don't care about removed data from a prior view
            if (newData != null) {
                foreach (var @event in newData) {
                    events.Add(@event);
                }
            }

            // Check for any events that get pushed out of the window
            var expiredCount = events.Count - Size;
            EventBean[] expiredArr = null;
            if (expiredCount > 0) {
                expiredArr = new EventBean[expiredCount];
                for (var i = 0; i < expiredCount; i++) {
                    expiredArr[i] = events.RemoveFirst();
                }
            }

            // update event buffer for access by expressions, if any
            ViewUpdatedCollection?.Update(newData, expiredArr);

            // If there are child views, call update method
            if (Child != null) {
                agentInstanceContext.InstrumentationProvider.QViewIndicate(viewFactory, newData, expiredArr);
                Child.Update(newData, expiredArr);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();
            }

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return events.GetEnumerator();
        }

        public override string ToString()
        {
            return GetType().Name + " size=" + Size;
        }
    }
} // end of namespace