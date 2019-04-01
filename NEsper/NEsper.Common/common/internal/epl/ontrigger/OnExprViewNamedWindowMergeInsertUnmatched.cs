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
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    /// <summary>
    ///     View for the on-delete statement that handles removing events from a named window.
    /// </summary>
    public class OnExprViewNamedWindowMergeInsertUnmatched : ViewSupport
    {
        /// <summary>
        ///     The event type of the events hosted in the named window.
        /// </summary>
        private readonly AgentInstanceContext agentInstanceContext;

        private readonly InfraOnMergeViewFactory factory;

        /// <summary>
        ///     The root view accepting removals (old data).
        /// </summary>
        internal readonly NamedWindowRootViewInstance rootView;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="factory">merge view factory</param>
        /// <param name="rootView">to indicate which events to delete</param>
        /// <param name="agentInstanceContext">context for expression evalauation</param>
        public OnExprViewNamedWindowMergeInsertUnmatched(
            NamedWindowRootViewInstance rootView,
            AgentInstanceContext agentInstanceContext,
            InfraOnMergeViewFactory factory)
        {
            this.rootView = rootView;
            this.agentInstanceContext = agentInstanceContext;
            this.factory = factory;
        }

        /// <summary>
        ///     returns expr context.
        /// </summary>
        /// <returns>context</returns>
        public ExprEvaluatorContext ExprEvaluatorContext => agentInstanceContext;

        public override EventType EventType => rootView.EventType;

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            agentInstanceContext.InstrumentationProvider.QInfraOnAction(
                OnTriggerType.ON_MERGE, newData, CollectionUtil.EVENTBEANARRAY_EMPTY);

            if (newData == null) {
                agentInstanceContext.InstrumentationProvider.AInfraOnAction();
                return;
            }

            var newColl = new OneEventCollection();
            var eventsPerStream =
                new EventBean[3]; // first:named window, second: trigger, third:before-update (optional)

            foreach (var trigger in newData) {
                eventsPerStream[1] = trigger;
                factory.OnMergeHelper.InsertUnmatched.Apply(null, eventsPerStream, newColl, null, agentInstanceContext);
                OnExprViewNamedWindowMerge.ApplyDelta(newColl, null, factory, rootView, agentInstanceContext, this);
                newColl.Clear();
            }

            agentInstanceContext.InstrumentationProvider.AInfraOnAction();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return EnumerationHelper.Empty<EventBean>();
        }
    }
} // end of namespace