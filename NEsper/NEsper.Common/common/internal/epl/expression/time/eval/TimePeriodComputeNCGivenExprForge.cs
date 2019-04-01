///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.time.eval
{
	public class TimePeriodComputeNCGivenExprForge : TimePeriodComputeForge {
	    private readonly ExprForge secondsEvaluator;
	    private readonly TimeAbacus timeAbacus;

	    public TimePeriodComputeNCGivenExprForge(ExprForge secondsEvaluator, TimeAbacus timeAbacus) {
	        this.secondsEvaluator = secondsEvaluator;
	        this.timeAbacus = timeAbacus;
	    }

	    public TimePeriodCompute Evaluator
	    {
	        get => new TimePeriodComputeNCGivenExprEval(secondsEvaluator.ExprEvaluator, timeAbacus);
	    }

	    public CodegenExpression MakeEvaluator(CodegenMethodScope parent, CodegenClassScope classScope) {
	        CodegenMethod method = parent.MakeChild(typeof(TimePeriodComputeNCGivenExprEval), this.GetType(), classScope);
	        method.Block
	                .DeclareVar(typeof(TimePeriodComputeNCGivenExprEval), "eval", NewInstance(typeof(TimePeriodComputeNCGivenExprEval)))
	                .ExprDotMethod(@Ref("eval"), "setSecondsEvaluator", ExprNodeUtilityCodegen.CodegenEvaluator(secondsEvaluator, method, this.GetType(), classScope))
	                .ExprDotMethod(@Ref("eval"), "setTimeAbacus", classScope.AddOrGetFieldSharable(TimeAbacusField.INSTANCE))
	                .MethodReturn(@Ref("eval"));
	        return LocalMethod(method);
	    }
	}
} // end of namespace