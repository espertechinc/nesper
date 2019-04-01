///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.codegen;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    ///     Interface for evaluating of an event tuple.
    /// </summary>
    public interface ExprEnumerationForge
    {
        Type ComponentTypeCollection { get; }

        EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo, StatementCompileTimeServices compileTimeServices);

        EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo, StatementCompileTimeServices compileTimeServices);

        CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        ExprNodeRenderable ForgeRenderable { get; }

        ExprEnumerationEval ExprEvaluatorEnumeration { get; }
    }
} // end of namespace