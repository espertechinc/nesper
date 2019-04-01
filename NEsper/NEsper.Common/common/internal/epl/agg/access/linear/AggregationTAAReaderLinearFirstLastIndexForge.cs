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
    public class AggregationTAAReaderLinearFirstLastIndexForge : AggregationTableAccessAggReaderForge
    {
        private readonly AggregationAccessorLinearType accessType;
        private readonly int? optionalConstant;
        private readonly ExprNode optionalIndexEval;

        public AggregationTAAReaderLinearFirstLastIndexForge(
            Type underlyingType, AggregationAccessorLinearType accessType, int? optionalConstant,
            ExprNode optionalIndexEval)
        {
            ResultType = underlyingType;
            this.accessType = accessType;
            this.optionalConstant = optionalConstant;
            this.optionalIndexEval = optionalIndexEval;
        }

        public Type ResultType { get; }

        public CodegenExpression CodegenCreateReader(
            CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(AggregationTAAReaderLinearFirstLastIndex), GetType(), classScope);
            method.Block
                .DeclareVar(
                    typeof(AggregationTAAReaderLinearFirstLastIndex), "strat",
                    NewInstance(typeof(AggregationTAAReaderLinearFirstLastIndex)))
                .ExprDotMethod(Ref("strat"), "setAccessType", Constant(accessType))
                .ExprDotMethod(Ref("strat"), "setOptionalConstIndex", Constant(optionalConstant))
                .ExprDotMethod(
                    Ref("strat"), "setOptionalIndexEval",
                    optionalIndexEval == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(
                            optionalIndexEval.Forge, method, GetType(), classScope))
                .MethodReturn(Ref("strat"));
            return LocalMethod(method);
        }
    }
} // end of namespace