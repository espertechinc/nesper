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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.filterspec.FilterSpecParam;

namespace com.espertech.esper.common.@internal.filterspec
{
    public interface FilterSpecParamFilterForEvalDoubleForge : FilterSpecParamFilterForEvalForge
    {
#if MIXIN
        static CodegenExpression makeAnonymous(
            FilterSpecParamFilterForEvalDoubleForge eval, Class originator, CodegenClassScope classScope,
            CodegenMethod method)
        {
            CodegenExpressionNewAnonymousClass anonymousClass =
                newAnonymousClass(method.getBlock(), FilterSpecParamFilterForEvalDouble.class);

            CodegenMethod getFilterValueDouble = CodegenMethod.makeParentNode(typeof(double), originator, classScope)
                .addParam(GET_FILTER_VALUE_FP);
            anonymousClass.addMethod("getFilterValueDouble", getFilterValueDouble);
            getFilterValueDouble.getBlock().methodReturn(cast(Double.class, eval.makeCodegen(
                classScope, getFilterValueDouble)));

            CodegenMethod getFilterValue = CodegenMethod.makeParentNode(Object.class, originator, classScope).addParam(
                GET_FILTER_VALUE_FP);
            anonymousClass.addMethod("getFilterValue", getFilterValue);
            getFilterValue.getBlock().methodReturn(
                exprDotMethod(
                    ref ("this"), "getFilterValueDouble", REF_MATCHEDEVENTMAP, REF_EXPREVALCONTEXT,
                    REF_STMTCTXFILTEREVALENV));

            return anonymousClass;
        }
#endif
    }
}