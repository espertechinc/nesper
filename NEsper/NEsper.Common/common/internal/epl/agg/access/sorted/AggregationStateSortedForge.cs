///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    public class AggregationStateSortedForge : AggregationStateFactoryForge
    {
        internal readonly AggregationForgeFactoryAccessSorted factory;
        private AggregatorAccessSorted aggregatorAccess;

        public AggregationStateSortedForge(AggregationForgeFactoryAccessSorted factory)
        {
            this.factory = factory;
        }

        public SortedAggregationStateDesc Spec => factory.OptionalSortedStateDesc;

        public void InitAccessForge(
            int col, bool join, CodegenCtor ctor, CodegenMemberCol membersColumnized, CodegenClassScope classScope)
        {
            aggregatorAccess = new AggregatorAccessSortedImpl(
                join, this, col, ctor, membersColumnized, classScope, factory.Parent.OptionalFilter);
        }

        public AggregatorAccess Aggregator => aggregatorAccess;

        public CodegenExpression CodegenGetAccessTableState(
            int column, CodegenMethodScope parent, CodegenClassScope classScope)
        {
            return AggregatorAccessSortedImpl.CodegenGetAccessTableState(column, parent, classScope);
        }

        public ExprNode Expression => factory.AggregationExpression;
    }
} // end of namespace