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

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public abstract class DTLocalForgeCalOpsIntervalBase : DTLocalForge,
        DTLocalForgeIntervalComp
    {
        internal readonly IList<CalendarForge> calendarForges;
        internal readonly IntervalForge intervalForge;

        protected DTLocalForgeCalOpsIntervalBase(
            IList<CalendarForge> calendarForges,
            IntervalForge intervalForge)
        {
            this.calendarForges = calendarForges;
            this.intervalForge = intervalForge;
        }

        public abstract DTLocalEvaluator DTEvaluator { get; }

        public abstract CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        public abstract CodegenExpression Codegen(
            CodegenExpressionRef start,
            CodegenExpressionRef end,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        public abstract DTLocalEvaluatorIntervalComp MakeEvaluatorComp();
    }
} // end of namespace