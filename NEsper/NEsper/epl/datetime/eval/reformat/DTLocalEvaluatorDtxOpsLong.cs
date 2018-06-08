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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;

namespace com.espertech.esper.epl.datetime.eval.reformat
{
    internal class DTLocalEvaluatorDtxOpsLong
        : DTLocalEvaluatorDtxOpsDtxBase
        , DTLocalEvaluator
    {
        private readonly TimeZoneInfo _timeZone;
        private readonly TimeAbacus _timeAbacus;

        internal DTLocalEvaluatorDtxOpsLong(IList<CalendarOp> calendarOps, TimeZoneInfo timeZone, TimeAbacus timeAbacus)
            : base(calendarOps)
        {
            _timeZone = timeZone;
            _timeAbacus = timeAbacus;
        }

        public object Evaluate(object target, EvaluateParams evaluateParams)
        {
            var longValue = (long)target;
            var dtx = DateTimeEx.GetInstance(_timeZone);
            var remainder = _timeAbacus.CalendarSet(longValue, dtx);

            DTLocalEvaluatorBase.EvaluateDtxOps(CalendarOps, dtx, evaluateParams);

            return _timeAbacus.CalendarGet(dtx, remainder);
        }
    }
}
