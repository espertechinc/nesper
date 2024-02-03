///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.common.@internal.epl.expression.time.eval
{
    public class TimePeriodComputeConstGivenCalAddEval : TimePeriodCompute,
        TimePeriodProvide
    {
        public TimePeriodComputeConstGivenCalAddEval()
        {
        }

        public TimePeriodComputeConstGivenCalAddEval(
            TimePeriodAdder[] adders,
            int[] added,
            TimeAbacus timeAbacus,
            int indexMicroseconds,
            TimeZoneInfo timeZone)
        {
            Adders = adders;
            Added = added;
            TimeAbacus = timeAbacus;
            IndexMicroseconds = indexMicroseconds;
            TimeZone = timeZone;
        }

        public long DeltaAdd(
            long fromTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var target = AddSubtract(fromTime, 1);
            return target - fromTime;
        }

        public long DeltaSubtract(
            long fromTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var target = AddSubtract(fromTime, -1);
            return fromTime - target;
        }

        public long DeltaUseRuntimeTime(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext context,
            TimeProvider timeProvider)
        {
            return DeltaAdd(timeProvider.Time, eventsPerStream, true, context);
        }

        public TimePeriodDeltaResult DeltaAddWReference(
            long fromTime,
            long reference,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            // find the next-nearest reference higher then the current time, compute delta, return reference one lower
            while (reference > fromTime) {
                reference = reference - DeltaSubtract(reference, eventsPerStream, isNewData, context);
            }

            var next = reference;
            long last;
            do {
                last = next;
                next = next + DeltaAdd(last, eventsPerStream, isNewData, context);
            } while (next <= fromTime);

            return new TimePeriodDeltaResult(next - fromTime, last);
        }

        public TimePeriodProvide GetNonVariableProvide(ExprEvaluatorContext context)
        {
            return this;
        }

        public TimePeriodAdder[] Adders { get; set; }

        public int[] Added { get; set; }

        public TimeAbacus TimeAbacus { get; set; }

        public int IndexMicroseconds { get; set; }

        public TimeZoneInfo TimeZone { get; set; }

        private long AddSubtract(
            long fromTime,
            int factor)
        {
            var dateTimeEx = DateTimeEx.GetInstance(TimeZone);
            var remainder = TimeAbacus.DateTimeSet(fromTime, dateTimeEx);
            for (var i = 0; i < Adders.Length; i++) {
                Adders[i].Add(dateTimeEx, factor * Added[i]);
            }

            var result = TimeAbacus.DateTimeGet(dateTimeEx, remainder);
            if (IndexMicroseconds != -1) {
                result += factor * Added[IndexMicroseconds];
            }

            return result;
        }
    }
} // end of namespace