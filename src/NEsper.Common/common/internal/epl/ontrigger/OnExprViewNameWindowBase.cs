///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    /// <summary>
    ///     View for the on-delete statement that handles removing events from a named window.
    /// </summary>
    public abstract class OnExprViewNameWindowBase : ViewSupport
    {
        internal readonly AgentInstanceContext agentInstanceContext;

        /// <summary>
        ///     The event type of the events hosted in the named window.
        /// </summary>
        private readonly SubordWMatchExprLookupStrategy lookupStrategy;

        /// <summary>
        ///     The root view accepting removals (old data).
        /// </summary>
        internal readonly NamedWindowRootViewInstance rootView;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="lookupStrategy">for handling trigger events to determine deleted events</param>
        /// <param name="rootView">to indicate which events to delete</param>
        /// <param name="agentInstanceContext">context for expression evalauation</param>
        public OnExprViewNameWindowBase(
            SubordWMatchExprLookupStrategy lookupStrategy,
            NamedWindowRootViewInstance rootView,
            AgentInstanceContext agentInstanceContext)
        {
            this.lookupStrategy = lookupStrategy;
            this.rootView = rootView;
            this.agentInstanceContext = agentInstanceContext;
        }

        /// <summary>
        ///     returns expr context.
        /// </summary>
        /// <returns>context</returns>
        public ExprEvaluatorContext ExprEvaluatorContext => agentInstanceContext;

        /// <summary>
        ///     Implemented by on-trigger views to action on the combination of trigger and matching events in the named window.
        /// </summary>
        /// <param name="triggerEvents">is the trigger events (usually 1)</param>
        /// <param name="matchingEvents">is the matching events retrieved via lookup strategy</param>
        public abstract void HandleMatching(
            EventBean[] triggerEvents,
            EventBean[] matchingEvents);

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            if (newData == null) {
                return;
            }

            if (newData.Length == 1) {
                Process(newData);
                return;
            }

            var eventsPerStream = new EventBean[1];
            foreach (var @event in newData) {
                eventsPerStream[0] = @event;
                Process(eventsPerStream);
            }
        }

        private void Process(EventBean[] events)
        {
            var eventsFound = lookupStrategy.Lookup(events, agentInstanceContext);
            HandleMatching(events, eventsFound);
        }
    }
} // end of namespace