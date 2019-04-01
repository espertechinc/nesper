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

namespace com.espertech.esper.epl.datetime.eval.reformat
{
    internal class DTLocalEvaluatorDateTimeOpsInterval : DTLocalEvaluatorCalOpsIntervalBase
    {
        private readonly TimeZoneInfo _timeZone;

        internal DTLocalEvaluatorDateTimeOpsInterval(
            IList<CalendarOp> calendarOps,
            IntervalOp intervalOp,
            TimeZoneInfo timeZone)
            : base(calendarOps, intervalOp)
        {
            _timeZone = timeZone;
        }

        public override object Evaluate(object target, EvaluateParams evaluateParams)
        {
            var dt = (DateTime) target;
            var dtx = DateTimeEx.GetInstance(_timeZone);
            dtx.SetUtcMillis(dt.UtcMillis());
            EvaluateDtxOps(CalendarOps, dtx, evaluateParams);
            var time = dtx.TimeInMillis;
            return IntervalOp.Evaluate(time, time, evaluateParams);
        }

        public override object Evaluate(
            object startTimestamp,
            object endTimestamp,
            EvaluateParams evaluateParams)
        {
            var startLong = ((DateTime)startTimestamp).UtcMillis();
            var endLong = ((DateTime)endTimestamp).UtcMillis();
            var dtx = DateTimeEx.GetInstance(_timeZone);
            dtx.SetUtcMillis(startLong);
            EvaluateDtxOps(CalendarOps, dtx, evaluateParams);
            var startTime = dtx.TimeInMillis;
            var endTime = startTime + (endLong - startLong);
            return IntervalOp.Evaluate(startTime, endTime, evaluateParams);
        }
    }
}
