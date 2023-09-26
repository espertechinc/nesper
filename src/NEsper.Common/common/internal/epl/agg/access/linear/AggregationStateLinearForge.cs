///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.agg.accessagg;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    public class AggregationStateLinearForge : AggregationStateFactoryForge
    {
        internal readonly ExprAggMultiFunctionLinearAccessNode expr;
        internal readonly ExprForge optionalFilter;
        internal readonly int streamNum;

        private AggregatorAccessLinear _aggregator;

        public AggregationStateLinearForge(
            ExprAggMultiFunctionLinearAccessNode expr,
            int streamNum,
            ExprForge optionalFilter,
            bool isJoin)
        {
            this.expr = expr;
            this.streamNum = streamNum;
            this.optionalFilter = optionalFilter;

            if (!isJoin) {
                _aggregator = new AggregatorAccessLinearNonJoin(this, expr.OptionalFilter);
            }
            else {
                _aggregator = new AggregatorAccessLinearJoin(this, expr.OptionalFilter);
            }
        }

        public AggregatorAccess Aggregator => _aggregator;

        public int StreamNum => streamNum;

        public ExprForge OptionalFilter => optionalFilter;

        public AggregatorAccessLinear AggregatorLinear => _aggregator;

        public EventType EventType => expr.StreamType;

        public Type ClassType => expr.ComponentTypeCollection;

        public ExprNode Expression => expr;

        public CodegenExpression CodegenGetAccessTableState(
            int column,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return
                ConstantNull(); // not implemented for linear state as AggregationTableAccessAggReader can simple call "GetCollectionOfEvents"
        }
    }
} // end of namespace