///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    public class AggregationStateMinMaxByEverForge : AggregationStateFactoryForge
    {
        internal readonly AggregationForgeFactoryAccessSorted factory;
        private AggregatorAccessSortedMinMaxByEver aggregator;

        public AggregationStateMinMaxByEverForge(AggregationForgeFactoryAccessSorted factory)
        {
            this.factory = factory;
            aggregator = new AggregatorAccessSortedMinMaxByEver(this, factory.Parent.OptionalFilter);
        }

        public AggregatorAccess Aggregator => aggregator;

        public SortedAggregationStateDesc Spec => factory.OptionalSortedStateDesc;

        public ExprNode Expression => factory.AggregationExpression;

        public CodegenExpression CodegenGetAccessTableState(
            int column,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return AggregatorAccessSortedMinMaxByEver.CodegenGetAccessTableState(column, parent, classScope);
        }
    }
} // end of namespace