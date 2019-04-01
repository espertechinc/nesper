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
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.expression.time.adder;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.time.eval
{
	public class TimePeriodComputeConstGivenCalAddForge : TimePeriodComputeForge {
	    private readonly TimePeriodAdder[] adders;
	    private readonly int[] added;
	    private readonly TimeAbacus timeAbacus;
	    private readonly int indexMicroseconds;

	    public TimePeriodComputeConstGivenCalAddForge(TimePeriodAdder[] adders, int[] added, TimeAbacus timeAbacus) {
	        this.adders = adders;
	        this.added = added;
	        this.timeAbacus = timeAbacus;
	        this.indexMicroseconds = ExprTimePeriodUtil.FindIndexMicroseconds(adders);
	    }

	    public TimePeriodCompute Evaluator
	    {
	        get => new TimePeriodComputeConstGivenCalAddEval(adders, added, timeAbacus, indexMicroseconds, TimeZone.Default);
	    }

	    public CodegenExpression MakeEvaluator(CodegenMethodScope parent, CodegenClassScope classScope) {
	        CodegenMethod method = parent.MakeChild(typeof(TimePeriodComputeConstGivenCalAddEval), this.GetType(), classScope);
	        method.Block
	                .DeclareVar(typeof(TimePeriodComputeConstGivenCalAddEval), "eval", NewInstance(typeof(TimePeriodComputeConstGivenCalAddEval)))
	                .ExprDotMethod(@Ref("eval"), "setAdders", TimePeriodAdderUtil.MakeArray(adders, parent, classScope))
	                .ExprDotMethod(@Ref("eval"), "setAdded", Constant(added))
	                .ExprDotMethod(@Ref("eval"), "setTimeAbacus", classScope.AddOrGetFieldSharable(TimeAbacusField.INSTANCE))
	                .ExprDotMethod(@Ref("eval"), "setIndexMicroseconds", Constant(indexMicroseconds))
	                .ExprDotMethod(@Ref("eval"), "setTimeZone", classScope.AddOrGetFieldSharable(RuntimeSettingsTimeZoneField.INSTANCE))
	                .MethodReturn(@Ref("eval"));
	        return LocalMethod(method);
	    }
	}
} // end of namespace