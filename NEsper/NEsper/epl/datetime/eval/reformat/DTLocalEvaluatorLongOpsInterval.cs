///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.datetime.calop;
using com.espertech.esper.epl.datetime.interval;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;

namespace com.espertech.esper.epl.datetime.eval.reformat
{
    internal class DTLocalEvaluatorLongOpsInterval : DTLocalEvaluatorCalOpsIntervalBase
    {
        private readonly TimeZoneInfo _timeZone;
        private readonly TimeAbacus _timeAbacus;

        internal DTLocalEvaluatorLongOpsInterval(
            IList<CalendarOp> calendarOps,
            IntervalOp intervalOp,
            TimeZoneInfo timeZone,
            TimeAbacus timeAbacus)
            : base(calendarOps, intervalOp)
        {
            _timeZone = timeZone;
            _timeAbacus = timeAbacus;
        }

        public override object Evaluate(object target, EvaluateParams evaluateParams)
        {
            var dtx = DateTimeEx.GetInstance(_timeZone);
            var startRemainder = _timeAbacus.CalendarSet((long)target, dtx);
            EvaluateDtxOps(CalendarOps, dtx, evaluateParams);
            var time = _timeAbacus.CalendarGet(dtx, startRemainder);
            return IntervalOp.Evaluate(time, time, evaluateParams);
        }

        public override object Evaluate(object startTimestamp, object endTimestamp, EvaluateParams evaluateParams)
        {
            var startLong = (long)startTimestamp;
            var endLong = (long)endTimestamp;
            var dtx = DateTimeEx.GetInstance(_timeZone);
            var startRemainder = _timeAbacus.CalendarSet(startLong, dtx);
            EvaluateDtxOps(CalendarOps, dtx, evaluateParams);
            var startTime = _timeAbacus.CalendarGet(dtx, startRemainder);
            var endTime = startTime + (endLong - startLong);
            return IntervalOp.Evaluate(startTime, endTime, evaluateParams);
        }
    }
}
