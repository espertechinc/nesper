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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.expression.time.adder;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.time.eval
{
	public class TimePeriodComputeNCGivenTPCalForgeEval : TimePeriodCompute {
	    private ExprEvaluator[] evaluators;
	    private TimePeriodAdder[] adders;
	    private TimeAbacus timeAbacus;
	    private TimeZone timeZone;
	    private int indexMicroseconds;

	    public TimePeriodComputeNCGivenTPCalForgeEval() {
	    }

	    public TimePeriodComputeNCGivenTPCalForgeEval(ExprEvaluator[] evaluators, TimePeriodAdder[] adders, TimeAbacus timeAbacus, TimeZone timeZone, int indexMicroseconds) {
	        this.evaluators = evaluators;
	        this.adders = adders;
	        this.timeAbacus = timeAbacus;
	        this.timeZone = timeZone;
	        this.indexMicroseconds = indexMicroseconds;
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

	    public void SetTimeZone(TimeZone timeZone) {
	        this.timeZone = timeZone;
	    }

	    public void SetIndexMicroseconds(int indexMicroseconds) {
	        this.indexMicroseconds = indexMicroseconds;
	    }

	    public long DeltaAdd(long currentTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        return AddSubtract(currentTime, 1, eventsPerStream, isNewData, context);
	    }

	    public long DeltaSubtract(long currentTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        return AddSubtract(currentTime, -1, eventsPerStream, isNewData, context);
	    }

	    public long DeltaUseRuntimeTime(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext, TimeProvider timeProvider) {
	        long currentTime = timeProvider.Time;
	        return AddSubtract(currentTime, 1, eventsPerStream, true, exprEvaluatorContext);
	    }

	    public TimePeriodDeltaResult DeltaAddWReference(long current, long reference, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        // find the next-nearest reference higher then the current time, compute delta, return reference one lower
	        if (reference > current) {
	            while (reference > current) {
	                reference = reference - DeltaSubtract(reference, eventsPerStream, isNewData, context);
	            }
	        }

	        long next = reference;
	        long last;
	        do {
	            last = next;
	            next = next + DeltaAdd(last, eventsPerStream, isNewData, context);
	        }
	        while (next <= current);
	        return new TimePeriodDeltaResult(next - current, last);
	    }

	    public TimePeriodProvide GetNonVariableProvide(ExprEvaluatorContext context) {
	        int[] added = new int[evaluators.Length];
	        for (int i = 0; i < evaluators.Length; i++) {
	            added[i] = (evaluators[i].Evaluate(null, true, context)).AsInt();
	        }
	        return new TimePeriodComputeConstGivenCalAddEval(adders, added, timeAbacus, indexMicroseconds, timeZone);
	    }

	    private long AddSubtract(long currentTime, int factor, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext context) {
	        Calendar cal = Calendar.GetInstance(timeZone);
	        long remainder = timeAbacus.DateTimeSet(currentTime, cal);

	        int usec = 0;
	        for (int i = 0; i < adders.Length; i++) {
	            int value = (evaluators[i].Evaluate(eventsPerStream, newData, context)).AsInt();
	            if (i == indexMicroseconds) {
	                usec = value;
	            } else {
	                adders[i].Add(cal, factor * value);
	            }
	        }

	        long result = timeAbacus.DateTimeGet(cal, remainder);
	        if (indexMicroseconds != -1) {
	            result += factor * usec;
	        }
	        return result - currentTime;
	    }
	}
} // end of namespace