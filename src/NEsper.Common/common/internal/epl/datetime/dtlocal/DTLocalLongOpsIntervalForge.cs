///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.expression.time.abacus;

using static com.espertech.esper.common.@internal.epl.datetime.dtlocal.DTLocalUtil;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalLongOpsIntervalForge : DTLocalForgeCalOpsIntervalBase
    {
        internal readonly TimeAbacus timeAbacus;

        public DTLocalLongOpsIntervalForge(
            IList<CalendarForge> calendarForges,
            IntervalForge intervalForge,
            TimeAbacus timeAbacus)
            : base(
                calendarForges,
                intervalForge)
        {
            this.timeAbacus = timeAbacus;
        }

        public override DTLocalEvaluator DTEvaluator => new DTLocalLongOpsIntervalEval(
            GetCalendarOps(calendarForges),
            intervalForge.Op,
            TimeZoneInfo.Utc,
            timeAbacus);

        public override CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return DTLocalLongOpsIntervalEval.CodegenPointInTime(
                this,
                inner,
                innerType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public override DTLocalEvaluatorIntervalComp MakeEvaluatorComp()
        {
            return new DTLocalLongOpsIntervalEval(
                GetCalendarOps(calendarForges),
                intervalForge.Op,
                TimeZoneInfo.Utc,
                timeAbacus);
        }

        public override CodegenExpression Codegen(
            CodegenExpression start,
            CodegenExpression end,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return DTLocalLongOpsIntervalEval.CodegenStartEnd(
                this,
                start,
                end,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }
    }
} // end of namespace