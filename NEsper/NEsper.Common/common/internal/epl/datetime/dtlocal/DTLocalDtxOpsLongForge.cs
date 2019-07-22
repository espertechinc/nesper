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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.datetime.dtlocal.DTLocalUtil;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalDtxOpsLongForge : DTLocalForgeCalOpsCalBase,
        DTLocalForge
    {
        internal readonly TimeAbacus timeAbacus;

        public DTLocalDtxOpsLongForge(
            IList<CalendarForge> calendarForges,
            TimeAbacus timeAbacus)
            : base(calendarForges)
        {
            this.timeAbacus = timeAbacus;
        }

        public DTLocalEvaluator DTEvaluator {
            get => new DTLocalDtxOpsLongEval(GetCalendarOps(calendarForges), TimeZoneInfo.Local, timeAbacus);
        }

        public CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return DTLocalDtxOpsLongEval.Codegen(this, inner, codegenMethodScope, exprSymbol, codegenClassScope);
        }
    }
} // end of namespace