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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.time.node
{
    public interface ExprTimePeriodEvalDeltaNonConst
    {
        long DeltaAdd(
            long currentTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        CodegenExpression DeltaAddCodegen(
            CodegenExpression reference,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        long DeltaSubtract(
            long currentTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        long DeltaUseEngineTime(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext,
            TimeProvider timeProvider);

        TimePeriodDeltaResult DeltaAddWReference(
            long current,
            long reference,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);
    }
} // end of namespace