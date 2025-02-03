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

using TimeProvider = com.espertech.esper.common.@internal.schedule.TimeProvider;

namespace com.espertech.esper.common.@internal.epl.expression.time.eval
{
    public class TimePeriodComputeNCGivenTPCalForgeEval : TimePeriodCompute
    {
        public TimePeriodComputeNCGivenTPCalForgeEval()
        {
        }

        public TimePeriodComputeNCGivenTPCalForgeEval(
            ExprEvaluator[] evaluators,
            TimePeriodAdder[] adders,
            TimeAbacus timeAbacus,
            TimeZoneInfo timeZone,
            int indexMicroseconds)
        {
            Evaluators = evaluators;
            Adders = adders;
            TimeAbacus = timeAbacus;
            TimeZone = timeZone;
            IndexMicroseconds = indexMicroseconds;
        }

        public ExprEvaluator[] Evaluators { get; set; }

        public TimePeriodAdder[] Adders { get; set; }

        public TimeAbacus TimeAbacus { get; set; }

        public TimeZoneInfo TimeZone { get; set; }

        public int IndexMicroseconds { get; set; }

        public long DeltaAdd(
            long currentTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return AddSubtract(currentTime, 1, eventsPerStream, isNewData, context);
        }

        public long DeltaSubtract(
            long currentTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return AddSubtract(currentTime, -1, eventsPerStream, isNewData, context);
        }

        public long DeltaUseRuntimeTime(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext,
            TimeProvider timeProvider)
        {
            var currentTime = timeProvider.Time;
            return AddSubtract(currentTime, 1, eventsPerStream, true, exprEvaluatorContext);
        }

        public TimePeriodDeltaResult DeltaAddWReference(
            long current,
            long reference,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            // find the next-nearest reference higher then the current time, compute delta, return reference one lower
            while (reference > current) {
                reference -= DeltaSubtract(reference, eventsPerStream, isNewData, context);
            }

            var next = reference;
            long last;
            do {
                last = next;
                next += DeltaAdd(last, eventsPerStream, isNewData, context);
            } while (next <= current);

            return new TimePeriodDeltaResult(next - current, last);
        }

        public TimePeriodProvide GetNonVariableProvide(ExprEvaluatorContext context)
        {
            var added = new int[Evaluators.Length];
            for (var i = 0; i < Evaluators.Length; i++) {
                added[i] = Evaluators[i].Evaluate(null, true, context).AsInt32();
            }

            return new TimePeriodComputeConstGivenCalAddEval(Adders, added, TimeAbacus, IndexMicroseconds, TimeZone);
        }

        private long AddSubtract(
            long currentTime,
            int factor,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext context)
        {
            var dtx = DateTimeEx.GetInstance(TimeZone);
            var remainder = TimeAbacus.DateTimeSet(currentTime, dtx);

            var usec = 0;
            for (var i = 0; i < Adders.Length; i++) {
                var value = Evaluators[i].Evaluate(eventsPerStream, newData, context).AsInt32();
                if (i == IndexMicroseconds) {
                    usec = value;
                }
                else {
                    Adders[i].Add(dtx, factor * value);
                }
            }

            var result = TimeAbacus.DateTimeGet(dtx, remainder);
            if (IndexMicroseconds != -1) {
                result += factor * usec;
            }

            return result - currentTime;
        }
    }
} // end of namespace