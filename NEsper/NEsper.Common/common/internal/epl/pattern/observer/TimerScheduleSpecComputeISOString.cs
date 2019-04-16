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
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using static com.espertech.esper.common.@internal.epl.pattern.observer.TimerScheduleObserverForge;

namespace com.espertech.esper.common.@internal.epl.pattern.observer
{
    public class TimerScheduleSpecComputeISOString : TimerScheduleSpecCompute
    {
        private readonly ExprEvaluator parameter;

        public TimerScheduleSpecComputeISOString(ExprEvaluator parameter)
        {
            this.parameter = parameter;
        }

        public TimerScheduleSpec Compute(
            MatchedEventConvertor optionalConvertor,
            MatchedEventMap beginState,
            ExprEvaluatorContext exprEvaluatorContext,
            TimeZoneInfo timeZone,
            TimeAbacus timeAbacus)
        {
            EventBean[] events = optionalConvertor == null ? null : optionalConvertor.Convert(beginState);
            return Compute(parameter, events, exprEvaluatorContext);
        }

        protected internal static TimerScheduleSpec Compute(
            ExprEvaluator parameter,
            EventBean[] events,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            object param = PatternExpressionUtil.EvaluateChecked(NAME_OBSERVER, parameter, events, exprEvaluatorContext);
            string iso = (string) param;
            if (iso == null) {
                throw new ScheduleParameterException("Received null parameter value");
            }

            return TimerScheduleISO8601Parser.Parse(iso);
        }
    }
} // end of namespace