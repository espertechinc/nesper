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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.expression.time.adder;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.time.eval
{
	public class TimePeriodComputeNCGivenTPCalForge : TimePeriodComputeForge {
	    private readonly ExprTimePeriodForge timePeriodForge;
	    private readonly int indexMicroseconds;

	    public TimePeriodComputeNCGivenTPCalForge(ExprTimePeriodForge timePeriodForge) {
	        this.timePeriodForge = timePeriodForge;
	        this.indexMicroseconds = ExprTimePeriodUtil.FindIndexMicroseconds(timePeriodForge.Adders);
	    }

	    public TimePeriodCompute Evaluator
	    {
	        get => new TimePeriodComputeNCGivenTPCalForgeEval(timePeriodForge.Evaluators, timePeriodForge.Adders, timePeriodForge.TimeAbacus, TimeZone.Default, indexMicroseconds);
	    }

	    public CodegenExpression MakeEvaluator(CodegenMethodScope parent, CodegenClassScope classScope) {
	        CodegenMethod method = parent.MakeChild(typeof(TimePeriodComputeNCGivenTPCalForgeEval), this.GetType(), classScope);
	        method.Block
	                .DeclareVar(typeof(TimePeriodComputeNCGivenTPCalForgeEval), "eval", NewInstance(typeof(TimePeriodComputeNCGivenTPCalForgeEval)))
	                .ExprDotMethod(@Ref("eval"), "setAdders", TimePeriodAdderUtil.MakeArray(timePeriodForge.Adders, parent, classScope))
	                .ExprDotMethod(@Ref("eval"), "setEvaluators", ExprNodeUtilityCodegen.CodegenEvaluators(timePeriodForge.Forges, method, this.GetType(), classScope))
	                .ExprDotMethod(@Ref("eval"), "setTimeAbacus", classScope.AddOrGetFieldSharable(TimeAbacusField.INSTANCE))
	                .ExprDotMethod(@Ref("eval"), "setTimeZone", classScope.AddOrGetFieldSharable(RuntimeSettingsTimeZoneField.INSTANCE))
	                .ExprDotMethod(@Ref("eval"), "setIndexMicroseconds", Constant(indexMicroseconds))
	                .MethodReturn(@Ref("eval"));
	        return LocalMethod(method);
	    }
	}
} // end of namespace