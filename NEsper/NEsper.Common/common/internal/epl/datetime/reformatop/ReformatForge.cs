///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.@join.analyze;

namespace com.espertech.esper.common.@internal.epl.datetime.reformatop
{
    public interface ReformatForge
    {
        ReformatOp Op { get; }

        Type ReturnType { get; }

        FilterExprAnalyzerAffector GetFilterDesc(
            EventType[] typesPerStream, DatetimeMethodEnum currentMethod, IList<ExprNode> currentParameters,
            ExprDotNodeFilterAnalyzerInput inputDesc);

        CodegenExpression CodegenLong(
            CodegenExpression inner, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        CodegenExpression CodegenDateTime(
            CodegenExpression inner, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        CodegenExpression CodegenDateTimeOffset(
            CodegenExpression inner, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        CodegenExpression CodegenDateTimeEx(
            CodegenExpression inner, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);
    }
} // end of namespace