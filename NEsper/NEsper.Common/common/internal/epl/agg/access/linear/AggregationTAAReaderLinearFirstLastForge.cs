///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    public class AggregationTAAReaderLinearFirstLastForge : AggregationTableAccessAggReaderForge
    {
        private readonly AggregationAccessorLinearType accessType;
        private readonly ExprNode optionalEvaluator;

        public AggregationTAAReaderLinearFirstLastForge(
            Type underlyingType,
            AggregationAccessorLinearType accessType,
            ExprNode optionalEvaluator)
        {
            ResultType = underlyingType;
            this.accessType = accessType;
            this.optionalEvaluator = optionalEvaluator;
        }

        public Type ResultType { get; }

        public CodegenExpression CodegenCreateReader(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(AggregationTAAReaderLinearFirstLast), GetType(), classScope);
            method.Block
                .DeclareVar(
                    typeof(AggregationTAAReaderLinearFirstLast), "strat",
                    NewInstance(typeof(AggregationTAAReaderLinearFirstLast)))
                .ExprDotMethod(Ref("strat"), "setAccessType", Constant(accessType))
                .ExprDotMethod(
                    Ref("strat"), "setOptionalEvaluator",
                    optionalEvaluator == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(
                            optionalEvaluator.Forge, method, GetType(), classScope))
                .MethodReturn(Ref("strat"));
            return LocalMethod(method);
        }
    }
} // end of namespace