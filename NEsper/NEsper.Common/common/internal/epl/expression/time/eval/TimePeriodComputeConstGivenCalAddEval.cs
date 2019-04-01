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
	public class TimePeriodComputeConstGivenCalAddEval : TimePeriodCompute, TimePeriodProvide {
	    private TimePeriodAdder[] adders;
	    private int[] added;
	    private TimeAbacus timeAbacus;
	    private int indexMicroseconds;
	    private TimeZone timeZone;

	    public TimePeriodComputeConstGivenCalAddEval() {
	    }

	    public TimePeriodComputeConstGivenCalAddEval(TimePeriodAdder[] adders, int[] added, TimeAbacus timeAbacus, int indexMicroseconds, TimeZone timeZone) {
	        this.adders = adders;
	        this.added = added;
	        this.timeAbacus = timeAbacus;
	        this.indexMicroseconds = indexMicroseconds;
	        this.timeZone = timeZone;
	    }

	    public void SetAdders(TimePeriodAdder[] adders) {
	        this.adders = adders;
	    }

	    public void SetAdded(int[] added) {
	        this.added = added;
	    }

	    public void SetTimeAbacus(TimeAbacus timeAbacus) {
	        this.timeAbacus = timeAbacus;
	    }

	    public void SetIndexMicroseconds(int indexMicroseconds) {
	        this.indexMicroseconds = indexMicroseconds;
	    }

	    public void SetTimeZone(TimeZone timeZone) {
	        this.timeZone = timeZone;
	    }

	    public long DeltaAdd(long fromTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        long target = AddSubtract(fromTime, 1);
	        return target - fromTime;
	    }

	    public long DeltaSubtract(long fromTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        long target = AddSubtract(fromTime, -1);
	        return fromTime - target;
	    }

	    public long DeltaUseRuntimeTime(EventBean[] eventsPerStream, ExprEvaluatorContext context, TimeProvider timeProvider) {
	        return DeltaAdd(timeProvider.Time, eventsPerStream, true, context);
	    }

	    public TimePeriodDeltaResult DeltaAddWReference(long fromTime, long reference, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        // find the next-nearest reference higher then the current time, compute delta, return reference one lower
	        if (reference > fromTime) {
	            while (reference > fromTime) {
	                reference = reference - DeltaSubtract(reference, eventsPerStream, isNewData, context);
	            }
	        }

	        long next = reference;
	        long last;
	        do {
	            last = next;
	            next = next + DeltaAdd(last, eventsPerStream, isNewData, context);
	        }
	        while (next <= fromTime);
	        return new TimePeriodDeltaResult(next - fromTime, last);
	    }

	    public TimePeriodProvide GetNonVariableProvide(ExprEvaluatorContext context) {
	        return this;
	    }

	    private long AddSubtract(long fromTime, int factor) {
	        Calendar cal = Calendar.GetInstance(timeZone);
	        long remainder = timeAbacus.DateTimeSet(fromTime, cal);
	        for (int i = 0; i < adders.Length; i++) {
	            adders[i].Add(cal, factor * added[i]);
	        }
	        long result = timeAbacus.DateTimeGet(cal, remainder);
	        if (indexMicroseconds != -1) {
	            result += factor * added[indexMicroseconds];
	        }
	        return result;
	    }
	}
} // end of namespace