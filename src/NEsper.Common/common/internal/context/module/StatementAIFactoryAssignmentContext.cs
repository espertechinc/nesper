///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.prior;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.view.previous;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.module
{
    public class StatementAIFactoryAssignmentContext : StatementAIFactoryAssignments
    {
        private readonly StatementAIResourceRegistry registry;

        public StatementAIFactoryAssignmentContext(StatementAIResourceRegistry registry)
        {
            this.registry = registry;
        }

        public AggregationService AggregationResultFuture => registry.AgentInstanceAggregationService;

        public PriorEvalStrategy[] PriorStrategies => registry.AgentInstancePriorEvalStrategies;

        public PreviousGetterStrategy[] PreviousStrategies => registry.AgentInstancePreviousGetterStrategies;

        public RowRecogPreviousStrategy RowRecogPreviousStrategy => registry.AgentInstanceRowRecogPreviousStrategy;

        public SubordTableLookupStrategy GetSubqueryLookup(int subqueryNumber)
        {
            return registry.AgentInstanceSubselects.Get(subqueryNumber).LookupStrategies;
        }

        public PriorEvalStrategy GetSubqueryPrior(int subqueryNumber)
        {
            return registry.AgentInstanceSubselects.Get(subqueryNumber).PriorEvalStrategies;
        }

        public PreviousGetterStrategy GetSubqueryPrevious(int subqueryNumber)
        {
            return registry.AgentInstanceSubselects.Get(subqueryNumber).PreviousGetterStrategies;
        }

        public AggregationService GetSubqueryAggregation(int subqueryNumber)
        {
            return registry.AgentInstanceSubselects.Get(subqueryNumber).AggregationServices;
        }

        public ExprTableEvalStrategy GetTableAccess(int tableAccessNumber)
        {
            return registry.AgentInstanceTableAccesses.Get(tableAccessNumber);
        }
    }
} // end of namespace