///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;


namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    public class AggregationStateSortedForge : AggregationStateFactoryForge
    {
        protected readonly AggregationForgeFactoryAccessSorted factory;
        private readonly AggregatorAccessSorted aggregatorAccess;

        public AggregationStateSortedForge(
            AggregationForgeFactoryAccessSorted factory,
            bool join)
        {
            this.factory = factory;
            aggregatorAccess = new AggregatorAccessSortedImpl(join, this, factory.Parent.OptionalFilter);
        }

        public CodegenExpression CodegenGetAccessTableState(
            int column,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return AggregatorAccessSortedImpl.CodegenGetAccessTableState(column, parent, classScope);
        }

        public AggregatorAccess Aggregator => aggregatorAccess;

        public SortedAggregationStateDesc Spec => factory.OptionalSortedStateDesc;

        public ExprNode Expression => factory.AggregationExpression;
    }
} // end of namespace