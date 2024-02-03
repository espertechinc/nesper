///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    public interface ExprDotEvalRootChildInnerForge
    {
        EventType EventTypeCollection { get; }

        EventType EventTypeSingle { get; }

        Type ComponentTypeCollection { get; }

        EPChainableType TypeInfo { get; }

        ExprDotEvalRootChildInnerEval InnerEvaluator { get; }

        CodegenExpression CodegenEvaluate(
            CodegenMethod parentMethod,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethod parentMethod,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethod parentMethod,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethod parentMethod,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);
    }
} // end of namespace