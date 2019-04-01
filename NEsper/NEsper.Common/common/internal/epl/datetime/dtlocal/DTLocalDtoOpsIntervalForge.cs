///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.datetime.calop;
using com.espertech.esper.common.@internal.epl.datetime.interval;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using static com.espertech.esper.common.@internal.epl.datetime.dtlocal.DTLocalUtil;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalDtoOpsIntervalForge : DTLocalForgeCalOpsIntervalBase
    {
        public DTLocalDtoOpsIntervalForge(
            IList<CalendarForge> calendarForges,
            IntervalForge intervalForge)
            :
            base(calendarForges, intervalForge)
        {
        }

        public override DTLocalEvaluator DTEvaluator => new DTLocalDtoOpsIntervalEval(
            GetCalendarOps(calendarForges), intervalForge.Op);

        public override CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return DTLocalDtoOpsIntervalEval.Codegen(
                this, inner, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        public override DTLocalEvaluatorIntervalComp MakeEvaluatorComp()
        {
            return new DTLocalDtoOpsIntervalEval(
                GetCalendarOps(calendarForges), intervalForge.Op);
        }

        public override CodegenExpression Codegen(
            CodegenExpressionRef start,
            CodegenExpressionRef end,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return DTLocalDtoOpsIntervalEval.Codegen(
                this, start, end, codegenMethodScope, exprSymbol, codegenClassScope);
        }
    }
} // end of namespace