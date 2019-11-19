///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.prior;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.view.previous;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.module
{
    public class StatementAIFactoryAssignmentsImpl : StatementAIFactoryAssignments
    {
        private readonly IDictionary<int, ExprTableEvalStrategy> tableAccesses;

        public StatementAIFactoryAssignmentsImpl(
            AggregationService aggregationResultFuture,
            PriorEvalStrategy[] priorStrategies,
            PreviousGetterStrategy[] previousStrategies,
            IDictionary<int, SubSelectFactoryResult> subselects,
            IDictionary<int, ExprTableEvalStrategy> tableAccesses,
            RowRecogPreviousStrategy rowRecogPreviousStrategy)
        {
            AggregationResultFuture = aggregationResultFuture;
            PriorStrategies = priorStrategies;
            PreviousStrategies = previousStrategies;
            Subselects = subselects;
            this.tableAccesses = tableAccesses;
            RowRecogPreviousStrategy = rowRecogPreviousStrategy;
        }

        public IDictionary<int, SubSelectFactoryResult> Subselects { get; }

        public AggregationService AggregationResultFuture { get; }

        public PriorEvalStrategy[] PriorStrategies { get; }

        public PreviousGetterStrategy[] PreviousStrategies { get; }

        public SubordTableLookupStrategy GetSubqueryLookup(int subqueryNumber)
        {
            return Subselects.Get(subqueryNumber).LookupStrategy;
        }

        public PriorEvalStrategy GetSubqueryPrior(int subqueryNumber)
        {
            return Subselects.Get(subqueryNumber).PriorStrategy;
        }

        public PreviousGetterStrategy GetSubqueryPrevious(int subqueryNumber)
        {
            return Subselects.Get(subqueryNumber).PreviousStrategy;
        }

        public AggregationService GetSubqueryAggregation(int subqueryNumber)
        {
            return Subselects.Get(subqueryNumber).AggregationService;
        }

        public ExprTableEvalStrategy GetTableAccess(int tableAccessNumber)
        {
            return tableAccesses.Get(tableAccessNumber);
        }

        public RowRecogPreviousStrategy RowRecogPreviousStrategy { get; }
    }
} // end of namespace