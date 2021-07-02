///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class AggregationMethodLinearFirstLastForge : AggregationMethodForge
    {
        private readonly AggregationAccessorLinearType accessType;
        private readonly ExprNode optionalEvaluator;
        private readonly Type underlyingType;

        public AggregationMethodLinearFirstLastForge(
            Type underlyingType,
            AggregationAccessorLinearType accessType,
            ExprNode optionalEvaluator)
        {
            this.underlyingType = underlyingType;
            this.accessType = accessType;
            this.optionalEvaluator = optionalEvaluator;
        }

        public CodegenExpression CodegenCreateReader(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(AggregationMethodLinearFirstLast), GetType(), classScope);
            var optionalEvaluatorExpr = optionalEvaluator == null
                ? ConstantNull()
                : ExprNodeUtilityCodegen.CodegenEvaluator(optionalEvaluator.Forge, method, GetType(), classScope);
            method.Block
                .DeclareVarNewInstance<AggregationMethodLinearFirstLast>("strat")
                .SetProperty(Ref("strat"), "AccessType", Constant(accessType))
                .SetProperty(Ref("strat"), "OptionalEvaluator", optionalEvaluatorExpr)
                .MethodReturn(Ref("strat"));
            return LocalMethod(method);
        }

        public Type ResultType => underlyingType;
    }
} // end of namespace