///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames; // REF_EXPREVALCONTEXT;
using static
    com.espertech.esper.common.@internal.filterspec.FilterSpecParam; // GET_FILTER_VALUE_FP, GET_FILTER_VALUE_REFS;

namespace com.espertech.esper.common.@internal.filterspec
{
    public class FilterForEvalLimitedExprForge : FilterSpecParamInValueForge
    {
        private readonly MatchedEventConvertorForge convertor;
        private readonly Coercer numberCoercer;
        private readonly ExprNode value;

        public FilterForEvalLimitedExprForge(
            ExprNode value,
            MatchedEventConvertorForge convertor,
            Coercer numberCoercer)
        {
            this.value = value;
            this.convertor = convertor;
            this.numberCoercer = numberCoercer;
        }

        public CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent)
        {
            var method = parent.MakeChild(typeof(object), GetType(), classScope).AddParam(GET_FILTER_VALUE_FP);
            var rhsExpression = CodegenLegoMethodExpression.CodegenExpression(value.Forge, method, classScope);
            var matchEventConvertor = convertor.Make(method, classScope);

            CodegenExpression valueExpr = LocalMethod(rhsExpression, Ref("eps"), ConstantTrue(), REF_EXPREVALCONTEXT);
            if (numberCoercer != null) {
                valueExpr = numberCoercer.CoerceCodegenMayNullBoxed(
                    valueExpr,
                    value.Forge.EvaluationType,
                    method,
                    classScope);
            }

            method.Block
                .DeclareVar<EventBean[]>("eps", LocalMethod(matchEventConvertor, REF_MATCHEDEVENTMAP))
                .MethodReturn(valueExpr);

            return LocalMethod(method, GET_FILTER_VALUE_REFS);
        }

        public Type ReturnType => value.Forge.EvaluationType;

        public bool IsConstant => false;

        public object GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }

        public void ValueToString(StringBuilder @out)
        {
            @out.Append("expression '")
                .Append(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(value))
                .Append("'");
        }
    }
} // end of namespace