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
    public class AggregationMethodLinearFirstLastIndexForge : AggregationMethodForge
    {
        private readonly AggregationAccessorLinearType accessType;
        private readonly int? optionalConstant;
        private readonly ExprNode optionalIndexEval;
        private readonly Type underlyingType;

        public AggregationMethodLinearFirstLastIndexForge(
            Type underlyingType,
            AggregationAccessorLinearType accessType,
            int? optionalConstant,
            ExprNode optionalIndexEval)
        {
            this.underlyingType = underlyingType;
            this.accessType = accessType;
            this.optionalConstant = optionalConstant;
            this.optionalIndexEval = optionalIndexEval;
        }

        public CodegenExpression CodegenCreateReader(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(AggregationMethodLinearFirstLastIndex), GetType(), classScope);
            var optionalIndexExpr = optionalIndexEval == null
                ? ConstantNull()
                : ExprNodeUtilityCodegen.CodegenEvaluator(optionalIndexEval.Forge, method, GetType(), classScope);

            method.Block
                .DeclareVarNewInstance<AggregationMethodLinearFirstLastIndex>("strat")
                .SetProperty(Ref("strat"), "AccessType", Constant(accessType))
                .SetProperty(Ref("strat"), "OptionalConstIndex", Constant(optionalConstant))
                .SetProperty(Ref("strat"), "OptionalIndexEval", optionalIndexExpr)
                .MethodReturn(Ref("strat"));
            return LocalMethod(method);
        }

        public Type ResultType => underlyingType;
    }
} // end of namespace