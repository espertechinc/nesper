///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
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
            var getFilterValueDouble = new CodegenExpressionLambda(method.Block)
                .WithParams(FilterSpecParam.GET_FILTER_VALUE_FP)
                .WithBody(
                    block => block.BlockReturn(
                        CodegenExpressionBuilder.Cast(
                            typeof(double),
                            eval.MakeCodegen(classScope, method))));

            //anonymousClass.AddMethod("GetFilterValueDouble", getFilterValueDouble);
            //getFilterValueDouble.Block.MethodReturn(
            //    CodegenExpressionBuilder.Cast(
            //        typeof(double),
            //        eval.MakeCodegen(
            //            classScope,
            //            getFilterValueDouble)));

            //var getFilterValue = new CodegenExpressionLambda(method.Block)
            //    .WithParams(FilterSpecParam.GET_FILTER_VALUE_FP)
            //    .WithBody(
            //        block => block.BlockReturn(
            //            CodegenExpressionBuilder.ExprDotMethod(
            //                CodegenExpressionBuilder.Ref("this"),
            //                "GetFilterValueDouble",
            //                FilterSpecParam.REF_MATCHEDEVENTMAP,
            //                ExprForgeCodegenNames.REF_EXPREVALCONTEXT,
            //                FilterSpecParam.REF_STMTCTXFILTEREVALENV)));

            //anonymousClass.AddMethod("GetFilterValue", getFilterValue);
            //getFilterValue.Block.MethodReturn(
            //    CodegenExpressionBuilder.ExprDotMethod(
            //        CodegenExpressionBuilder.Ref("this"),
            //        "GetFilterValueDouble",
            //        FilterSpecParam.REF_MATCHEDEVENTMAP,
            //        ExprForgeCodegenNames.REF_EXPREVALCONTEXT,
            //        FilterSpecParam.REF_STMTCTXFILTEREVALENV));

            return CodegenExpressionBuilder.NewInstance<ProxyFilterSpecParamFilterForEvalDouble>(getFilterValueDouble);
        }
    }
}