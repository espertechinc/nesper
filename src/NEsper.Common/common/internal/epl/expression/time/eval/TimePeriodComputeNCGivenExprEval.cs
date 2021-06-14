///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.epl.expression.time.eval
{
    public class TimePeriodComputeNCGivenExprEval : TimePeriodCompute
    {
        private ExprEvaluator secondsEvaluator;
        private TimeAbacus timeAbacus;

        public TimePeriodComputeNCGivenExprEval()
        {
        }

        public TimePeriodComputeNCGivenExprEval(
            ExprEvaluator secondsEvaluator,
            TimeAbacus timeAbacus)
        {
            this.secondsEvaluator = secondsEvaluator;
            this.timeAbacus = timeAbacus;
        }

        public ExprEvaluator SecondsEvaluator {
            get => secondsEvaluator;
            set => secondsEvaluator = value;
        }

        public TimeAbacus TimeAbacus {
            get => timeAbacus;
            set => timeAbacus = value;
        }

        public long DeltaAdd(
            long fromTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return Eval(eventsPerStream, isNewData, context);
        }

        public long DeltaSubtract(
            long fromTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return Eval(eventsPerStream, isNewData, context);
        }

        public TimePeriodDeltaResult DeltaAddWReference(
            long fromTime,
            long reference,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var delta = Eval(eventsPerStream, isNewData, context);
            return new TimePeriodDeltaResult(TimePeriodUtil.DeltaAddWReference(fromTime, reference, delta), reference);
        }

        public long DeltaUseRuntimeTime(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext context,
            TimeProvider timeProvider)
        {
            return Eval(eventsPerStream, true, context);
        }

        public TimePeriodProvide GetNonVariableProvide(ExprEvaluatorContext context)
        {
            var msec = Eval(null, true, context);
            return new TimePeriodComputeConstGivenDeltaEval(msec);
        }

        public TimePeriodComputeNCGivenExprEval SetSecondsEvaluator(ExprEvaluator secondsEvaluator)
        {
            this.secondsEvaluator = secondsEvaluator;
            return this;
        }

        public TimePeriodComputeNCGivenExprEval SetTimeAbacus(TimeAbacus timeAbacus)
        {
            this.timeAbacus = timeAbacus;
            return this;
        }

        private long Eval(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var time = secondsEvaluator.Evaluate(eventsPerStream, isNewData, context);
            if (!ExprTimePeriodUtil.ValidateTime(time, timeAbacus)) {
                throw new EPException(
                    ExprTimePeriodUtil.GetTimeInvalidMsg(
                        "Invalid time computation result",
                        time == null ? "null" : time.ToString(),
                        time));
            }

            return timeAbacus.DeltaForSecondsNumber(time);
        }
    }
} // end of namespace