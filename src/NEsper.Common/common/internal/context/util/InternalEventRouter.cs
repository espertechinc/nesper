///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.aifactory.update;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;

namespace com.espertech.esper.common.@internal.context.util
{
    /// <summary>
    /// Interface for a service that routes events within the runtime for further processing.
    /// </summary>
    public interface InternalEventRouter
    {
        void AddPreprocessing(
            InternalEventRouterDesc internalEventRouterDesc,
            InternalRoutePreprocessView outputView,
            StatementContext statementContext,
            bool hasSubselect);

        void RemovePreprocessing(
            EventType eventType,
            InternalEventRouterDesc desc);

        /// <summary>
        /// Route the event such that the event is processed as required.
        /// </summary>
        /// <param name="theEvent">to route</param>
        /// <param name="agentInstanceContext">agentInstanceContext</param>
        /// <param name="addToFront">indicator whether to add to front queue</param>
        /// <param name="precedence">event precedence</param>
        void Route(
            EventBean theEvent,
            AgentInstanceContext agentInstanceContext,
            bool addToFront,
            int precedence);

        bool HasPreprocessing { get; }

        EventBean Preprocess(
            EventBean theEvent,
            ExprEvaluatorContext runtimeFilterAndDispatchTimeContext,
            InstrumentationCommon instrumentation);

        InsertIntoListener InsertIntoListener { set; }
    }
} // end of namespace