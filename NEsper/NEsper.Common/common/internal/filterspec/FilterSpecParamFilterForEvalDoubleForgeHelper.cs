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
using com.espertech.esper.common.@internal.epl.expression.codegen;

namespace com.espertech.esper.common.@internal.filterspec
{
    public static class FilterSpecParamFilterForEvalDoubleForgeHelper
    {
        public static CodegenExpression MakeAnonymous(
            FilterSpecParamFilterForEvalDoubleForge eval,
            Type originator,
            CodegenClassScope classScope,
            CodegenMethod method)
        {
            var anonymousClass =
                CodegenExpressionBuilder.NewAnonymousClass(method.Block, typeof(FilterSpecParamFilterForEvalDouble));

            var getFilterValueDouble = CodegenMethod
                .MakeParentNode(typeof(double), originator, classScope)
                .AddParam(FilterSpecParam.GET_FILTER_VALUE_FP);
            anonymousClass.AddMethod("GetFilterValueDouble", getFilterValueDouble);
            getFilterValueDouble.Block.MethodReturn(
                CodegenExpressionBuilder.Cast(
                    typeof(double), eval.MakeCodegen(
                        classScope, getFilterValueDouble)));

            var getFilterValue = CodegenMethod
                .MakeParentNode(typeof(object), originator, classScope)
                .AddParam(FilterSpecParam.GET_FILTER_VALUE_FP);
            anonymousClass.AddMethod("GetFilterValue", getFilterValue);
            getFilterValue.Block.MethodReturn(
                CodegenExpressionBuilder.ExprDotMethod(
                    CodegenExpressionBuilder.Ref("this"), "GetFilterValueDouble", 
                    FilterSpecParam.REF_MATCHEDEVENTMAP,
                    ExprForgeCodegenNames.REF_EXPREVALCONTEXT,
                    FilterSpecParam.REF_STMTCTXFILTEREVALENV));

            return anonymousClass;
        }
    }
}