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
using com.espertech.esper.common.@internal.bytecodemodel.core;
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

        public AggregationStateLinearForge(
            ExprAggMultiFunctionLinearAccessNode expr,
            int streamNum,
            ExprForge optionalFilter)
        {
            this.expr = expr;
            this.streamNum = streamNum;
            this.optionalFilter = optionalFilter;
        }

        public AggregatorAccess Aggregator => AggregatorLinear;

        public int StreamNum => streamNum;

        public ExprForge OptionalFilter => optionalFilter;

        public AggregatorAccessLinear AggregatorLinear { get; private set; }

        public EventType EventType => expr.StreamType;

        public Type ClassType => expr.ComponentTypeCollection;

        public ExprNode Expression => expr;

        public void InitAccessForge(
            int col,
            bool join,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope)
        {
            if (!join) {
                AggregatorLinear = new AggregatorAccessLinearNonJoin(
                    this,
                    col,
                    rowCtor,
                    membersColumnized,
                    classScope,
                    expr.OptionalFilter);
            }
            else {
                AggregatorLinear = new AggregatorAccessLinearJoin(
                    this,
                    col,
                    rowCtor,
                    membersColumnized,
                    classScope,
                    expr.OptionalFilter);
            }
        }

        public CodegenExpression CodegenGetAccessTableState(
            int column,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return
                ConstantNull(); // not implemented for linear state as AggregationTableAccessAggReader can simple call "getCollectionOfEvents"
        }
    }
} // end of namespace