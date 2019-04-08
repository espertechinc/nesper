///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Numerics;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.expression.time.adder;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.epl.expression.time.node.ExprTimePeriodForge;

namespace com.espertech.esper.common.@internal.epl.expression.time.eval
{
    public class TimePeriodComputeNCGivenTPNonCalEval : TimePeriodCompute
    {
        public TimePeriodComputeNCGivenTPNonCalEval()
        {
        }

        public TimePeriodComputeNCGivenTPNonCalEval(
            ExprEvaluator[] evaluators,
            TimePeriodAdder[] adders,
            TimeAbacus timeAbacus)
        {
            Evaluators = evaluators;
            Adders = adders;
            TimeAbacus = timeAbacus;
        }

        public ExprEvaluator[] Evaluators { get; set; }

        public TimePeriodAdder[] Adders { get; set; }

        public TimeAbacus TimeAbacus { get; set; }

        public long DeltaAdd(
            long currentTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return Evaluate(eventsPerStream, isNewData, context);
        }

        public long DeltaSubtract(
            long currentTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return Evaluate(eventsPerStream, isNewData, context);
        }

        public long DeltaUseRuntimeTime(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext context,
            TimeProvider timeProvider)
        {
            return Evaluate(eventsPerStream, true, context);
        }

        public TimePeriodDeltaResult DeltaAddWReference(
            long current,
            long reference,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var timeDelta = Evaluate(eventsPerStream, isNewData, context);
            return new TimePeriodDeltaResult(TimePeriodUtil.DeltaAddWReference(current, reference, timeDelta), reference);
        }

        public TimePeriodProvide GetNonVariableProvide(ExprEvaluatorContext context)
        {
            var delta = Evaluate(null, true, context);
            return new TimePeriodComputeConstGivenDeltaEval(delta);
        }

        private long Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            double seconds = 0;
            for (var i = 0; i < Adders.Length; i++) {
                var result = Eval(Evaluators[i], eventsPerStream, isNewData, context);
                if (result == null) {
                    throw MakeTimePeriodParamNullException("Received null value evaluating time period");
                }

                seconds += Adders[i].Compute(result.Value);
            }

            return TimeAbacus.DeltaForSecondsDouble(seconds);
        }

        private double? Eval(
            ExprEvaluator expr,
            EventBean[] events,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var value = expr.Evaluate(events, isNewData, exprEvaluatorContext);
            if (value == null) {
                return null;
            }

            if (value is decimal) {
                return value.AsDouble();
            }

            if (value is BigInteger) {
                return value.AsDouble();
            }

            return value.AsDouble();
        }
    }
} // end of namespace