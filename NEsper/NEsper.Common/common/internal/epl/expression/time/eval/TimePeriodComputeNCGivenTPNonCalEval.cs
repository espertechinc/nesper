///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.expression.time.adder;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.expression.time.node.ExprTimePeriodForge;

namespace com.espertech.esper.common.@internal.epl.expression.time.eval
{
	public class TimePeriodComputeNCGivenTPNonCalEval : TimePeriodCompute {

	    private ExprEvaluator[] evaluators;
	    private TimePeriodAdder[] adders;
	    private TimeAbacus timeAbacus;

	    public TimePeriodComputeNCGivenTPNonCalEval() {
	    }

	    public TimePeriodComputeNCGivenTPNonCalEval(ExprEvaluator[] evaluators, TimePeriodAdder[] adders, TimeAbacus timeAbacus) {
	        this.evaluators = evaluators;
	        this.adders = adders;
	        this.timeAbacus = timeAbacus;
	    }

	    public void SetEvaluators(ExprEvaluator[] evaluators) {
	        this.evaluators = evaluators;
	    }

	    public void SetAdders(TimePeriodAdder[] adders) {
	        this.adders = adders;
	    }

	    public void SetTimeAbacus(TimeAbacus timeAbacus) {
	        this.timeAbacus = timeAbacus;
	    }

	    public long DeltaAdd(long currentTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        return Evaluate(eventsPerStream, isNewData, context);
	    }

	    public long DeltaSubtract(long currentTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        return Evaluate(eventsPerStream, isNewData, context);
	    }

	    public long DeltaUseRuntimeTime(EventBean[] eventsPerStream, ExprEvaluatorContext context, TimeProvider timeProvider) {
	        return Evaluate(eventsPerStream, true, context);
	    }

	    public TimePeriodDeltaResult DeltaAddWReference(long current, long reference, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        long timeDelta = Evaluate(eventsPerStream, isNewData, context);
	        return new TimePeriodDeltaResult(TimePeriodUtil.DeltaAddWReference(current, reference, timeDelta), reference);
	    }

	    public TimePeriodProvide GetNonVariableProvide(ExprEvaluatorContext context) {
	        long delta = Evaluate(null, true, context);
	        return new TimePeriodComputeConstGivenDeltaEval(delta);
	    }

	    private long Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        double seconds = 0;
	        for (int i = 0; i < adders.Length; i++) {
	            Double result = Eval(evaluators[i], eventsPerStream, isNewData, context);
	            if (result == null) {
	                throw MakeTimePeriodParamNullException("Received null value evaluating time period");
	            }
	            seconds += adders[i].Compute(result);
	        }
	        return timeAbacus.DeltaForSecondsDouble(seconds);
	    }

	    private Double Eval(ExprEvaluator expr, EventBean[] events, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
	        object value = expr.Evaluate(events, isNewData, exprEvaluatorContext);
	        if (value == null) {
	            return null;
	        }
	        if (value is BigDecimal) {
	            return (value).AsDouble();
	        }
	        if (value is BigInteger) {
	            return (value).AsDouble();
	        }
	        return (value).AsDouble();
	    }
	}
} // end of namespace