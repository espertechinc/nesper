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
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.epl.resultset.core;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    public abstract class OutputProcessViewBaseWAfter : OutputProcessView
    {
        internal readonly AgentInstanceContext agentInstanceContext;
        internal readonly ResultSetProcessor resultSetProcessor;

        protected OutputProcessViewBaseWAfter(
            AgentInstanceContext agentInstanceContext,
            ResultSetProcessor resultSetProcessor,
            long? afterConditionTime,
            int? afterConditionNumberOfEvents,
            bool afterConditionSatisfied)
        {
            this.resultSetProcessor = resultSetProcessor;
            this.agentInstanceContext = agentInstanceContext;
            OptionalAfterConditionState = agentInstanceContext.ResultSetProcessorHelperFactory.MakeOutputConditionAfter(
                afterConditionTime,
                afterConditionNumberOfEvents,
                afterConditionSatisfied,
                agentInstanceContext);
        }

        public virtual OutputProcessViewAfterState OptionalAfterConditionState { get; }

        public override EventType EventType => resultSetProcessor.ResultEventType;

        /// <summary>
        ///     Returns true if the after-condition is satisfied.
        /// </summary>
        /// <param name="newEvents">is the view new events</param>
        /// <param name="statementContext">context</param>
        /// <returns>indicator for output condition</returns>
        public bool CheckAfterCondition(
            EventBean[] newEvents,
            StatementContext statementContext)
        {
            return OptionalAfterConditionState.CheckUpdateAfterCondition(newEvents, statementContext);
        }

        /// <summary>
        ///     Returns true if the after-condition is satisfied.
        /// </summary>
        /// <param name="newEvents">is the join new events</param>
        /// <param name="statementContext">context</param>
        /// <returns>indicator for output condition</returns>
        public bool CheckAfterCondition(
            ISet<MultiKey<EventBean>> newEvents,
            StatementContext statementContext)
        {
            return OptionalAfterConditionState.CheckUpdateAfterCondition(newEvents, statementContext);
        }

        /// <summary>
        ///     Returns true if the after-condition is satisfied.
        /// </summary>
        /// <param name="newOldEvents">is the new and old events pair</param>
        /// <param name="statementContext">context</param>
        /// <returns>indicator for output condition</returns>
        public bool CheckAfterCondition(
            UniformPair<EventBean[]> newOldEvents,
            StatementContext statementContext)
        {
            return OptionalAfterConditionState.CheckUpdateAfterCondition(newOldEvents, statementContext);
        }

        public override void Stop(AgentInstanceStopServices services)
        {
            OptionalAfterConditionState?.Destroy();
        }
    }
} // end of namespace