///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

namespace com.espertech.esper.common.@internal.epl.expression.subquery
{
    public interface SubselectForgeRow
    {
        CodegenExpression EvaluateCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbol,
            CodegenClassScope classScope);

        CodegenExpression EvaluateGetCollEventsCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbol,
            CodegenClassScope classScope);

        CodegenExpression EvaluateGetCollScalarCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbol,
            CodegenClassScope classScope);

        CodegenExpression EvaluateGetBeanCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope);

        CodegenExpression EvaluateTypableSinglerowCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope);

        CodegenExpression EvaluateTypableMultirowCodegen(
            CodegenMethodScope parent,
            ExprSubselectEvalMatchSymbol symbols,
            CodegenClassScope classScope);
    }
} // end of namespace