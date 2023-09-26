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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    public class InfraOnMergeMatch
    {
        private ExprEvaluator optionalCond;
        private IList<InfraOnMergeAction> actions;

        public InfraOnMergeMatch(
            ExprEvaluator optionalCond,
            IList<InfraOnMergeAction> actions)
        {
            this.optionalCond = optionalCond;
            this.actions = actions;
        }

        public bool IsApplies(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext context)
        {
            if (optionalCond == null) {
                return true;
            }

            var result = optionalCond.Evaluate(eventsPerStream, true, context);
            return result != null && (bool)result;
        }

        public void ApplyNamedWindow(
            EventBean matchingEvent,
            EventBean[] eventsPerStream,
            OneEventCollection newData,
            OneEventCollection oldData,
            AgentInstanceContext agentInstanceContext)
        {
            var instrumentationCommon = agentInstanceContext.InstrumentationProvider;
            instrumentationCommon.QInfraMergeWhenThenActions(actions.Count);

            var count = -1;
            foreach (var action in actions) {
                count++;
                instrumentationCommon.QInfraMergeWhenThenActionItem(count, action.Name);

                var applies = action.IsApplies(eventsPerStream, agentInstanceContext);
                if (applies) {
                    action.Apply(matchingEvent, eventsPerStream, newData, oldData, agentInstanceContext);
                }

                instrumentationCommon.AInfraMergeWhenThenActionItem(applies);
            }

            instrumentationCommon.AInfraMergeWhenThenActions();
        }

        public void ApplyTable(
            EventBean matchingEvent,
            EventBean[] eventsPerStream,
            TableInstance stateInstance,
            OnExprViewTableChangeHandler changeHandlerAdded,
            OnExprViewTableChangeHandler changeHandlerRemoved,
            AgentInstanceContext agentInstanceContext)
        {
            var instrumentationCommon = agentInstanceContext.InstrumentationProvider;
            instrumentationCommon.QInfraMergeWhenThenActions(actions.Count);

            var count = -1;
            foreach (var action in actions) {
                count++;
                instrumentationCommon.QInfraMergeWhenThenActionItem(count, action.Name);

                var applies = action.IsApplies(eventsPerStream, agentInstanceContext);
                if (applies) {
                    action.Apply(
                        matchingEvent,
                        eventsPerStream,
                        stateInstance,
                        changeHandlerAdded,
                        changeHandlerRemoved,
                        agentInstanceContext);
                }

                instrumentationCommon.AInfraMergeWhenThenActionItem(applies);
            }

            instrumentationCommon.AInfraMergeWhenThenActions();
        }
    }
} // end of namespace