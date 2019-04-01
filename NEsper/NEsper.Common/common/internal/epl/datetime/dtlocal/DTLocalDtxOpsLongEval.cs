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
using com.espertech.esper.common.@internal.epl.datetime.calop;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.datetime.dtlocal.DTLocalUtil;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
	public class DTLocalDtxOpsLongEval : DTLocalEvaluatorCalOpsCalBase , DTLocalEvaluator {

	    private readonly TimeZoneInfo timeZone;
	    private readonly TimeAbacus timeAbacus;

	    public DTLocalDtxOpsLongEval(IList<CalendarOp> calendarOps, TimeZoneInfo timeZone, TimeAbacus timeAbacus) : base(calendarOps)
	        {
	        this.timeZone = timeZone;
	        this.timeAbacus = timeAbacus;
	    }

	    public object Evaluate(object target, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
	        long? longValue = (long?) target;
	        DateTimeEx cal = DateTimeEx.GetInstance(timeZone);
	        long remainder = timeAbacus.DateTimeSet(longValue, cal);

	        DTLocalUtil.EvaluateCalOpsCalendar(calendarOps, cal, eventsPerStream, isNewData, exprEvaluatorContext);

	        return timeAbacus.DateTimeGet(cal, remainder);
	    }

	    public static CodegenExpression Codegen(DTLocalDtxOpsLongForge forge, CodegenExpression inner, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        CodegenExpression timeZoneField = codegenClassScope.AddOrGetFieldSharable(RuntimeSettingsTimeZoneField.INSTANCE);
	        CodegenMethod methodNode = codegenMethodScope.MakeChild(typeof(long), typeof(DTLocalDtxOpsLongEval), codegenClassScope).AddParam(typeof(long), "target");

	        CodegenBlock block = methodNode.Block
	                .DeclareVar(typeof(DateTimeEx), "cal", StaticMethod(typeof(DateTimeEx), "getInstance", timeZoneField))
	                .DeclareVar(typeof(long), "remainder", forge.timeAbacus.DateTimeSetCodegen(@Ref("target"), @Ref("cal"), methodNode, codegenClassScope));
	        EvaluateCalOpsCalendarCodegen(block, forge.calendarForges, @Ref("cal"), methodNode, exprSymbol, codegenClassScope);
	        block.MethodReturn(forge.timeAbacus.DateTimeGetCodegen(@Ref("cal"), @Ref("remainder"), codegenClassScope));
	        return LocalMethod(methodNode, inner);
	    }
	}
} // end of namespace