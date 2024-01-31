///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.core;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    public abstract class InfraOnMergeAction
    {
        private readonly ExprEvaluator optionalFilter;

        protected InfraOnMergeAction(ExprEvaluator optionalFilter)
        {
            this.optionalFilter = optionalFilter;
        }

        public bool IsApplies(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext context)
        {
            if (optionalFilter == null) {
                return true;
            }

            var result = optionalFilter.Evaluate(eventsPerStream, true, context);
            return result != null && (bool)result;
        }

        public abstract void Apply(
            EventBean matchingEvent,
            EventBean[] eventsPerStream,
            OneEventCollection newData,
            OneEventCollection oldData,
            AgentInstanceContext agentInstanceContext);

        public abstract void Apply(
            EventBean matchingEvent,
            EventBean[] eventsPerStream,
            TableInstance tableStateInstance,
            OnExprViewTableChangeHandler changeHandlerAdded,
            OnExprViewTableChangeHandler changeHandlerRemoved,
            AgentInstanceContext agentInstanceContext);

        public abstract string Name { get; }
    }
} // end of namespace