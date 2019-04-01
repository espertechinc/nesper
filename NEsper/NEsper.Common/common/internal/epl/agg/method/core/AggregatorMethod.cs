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
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.agg.method.core
{
    public interface AggregatorMethod
    {
        void ApplyEvalEnterCodegen(
            CodegenMethod method, ExprForgeCodegenSymbol symbols, ExprForge[] forges, CodegenClassScope classScope);

        void ApplyTableEnterCodegen(
            CodegenExpressionRef value, Type[] evaluationTypes, CodegenMethod method, CodegenClassScope classScope);

        void ApplyEvalLeaveCodegen(
            CodegenMethod method, ExprForgeCodegenSymbol symbols, ExprForge[] forges, CodegenClassScope classScope);

        void ApplyTableLeaveCodegen(
            CodegenExpressionRef value, Type[] evaluationTypes, CodegenMethod method, CodegenClassScope classScope);

        void ClearCodegen(CodegenMethod method, CodegenClassScope classScope);

        void GetValueCodegen(CodegenMethod method, CodegenClassScope classScope);

        void WriteCodegen(
            CodegenExpressionRef row, int col, CodegenExpressionRef output, CodegenExpressionRef unitKey,
            CodegenExpressionRef writer, CodegenMethod method, CodegenClassScope classScope);

        void ReadCodegen(
            CodegenExpressionRef row, int col, CodegenExpressionRef input, CodegenExpressionRef unitKey,
            CodegenMethod method, CodegenClassScope classScope);
    }
} // end of namespace