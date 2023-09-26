///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    public class AggregationMethodSortedKeyedForge : AggregationMethodForge
    {
        private readonly ExprNode key;
        private readonly Type underlyingClass;
        private readonly AggregationMethodSortedEnum aggMethod;
        private readonly Type resultType;

        public AggregationMethodSortedKeyedForge(
            ExprNode key,
            Type underlyingClass,
            AggregationMethodSortedEnum aggMethod,
            Type resultType)
        {
            this.key = key;
            this.underlyingClass = underlyingClass;
            this.aggMethod = aggMethod;
            this.resultType = resultType;
        }

        public CodegenExpression CodegenCreateReader(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(AggregationMultiFunctionAggregationMethod), GetType(), classScope);
            method.Block
                .DeclareVar<ExprEvaluator>(
                    "keyEval",
                    ExprNodeUtilityCodegen.CodegenEvaluator(key.Forge, method, GetType(), classScope))
                .MethodReturn(
                    StaticMethod(
                        typeof(AggregationMethodSortedKeyedFactory),
                        "MakeSortedAggregationWithKey",
                        Ref("keyEval"),
                        EnumValue(typeof(AggregationMethodSortedEnum), aggMethod.GetName()),
                        Constant(underlyingClass)));
            return LocalMethod(method);
        }

        public Type ResultType => resultType;
    }
} // end of namespace