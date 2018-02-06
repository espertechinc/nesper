///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.epl.datetime.calop;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.datetime.eval.reformat
{
    internal class DTLocalEvaluatorDtxOpsDateTimeOffset
        : DTLocalEvaluatorDtxOpsDtxBase
        , DTLocalEvaluator
    {
        private readonly TimeZoneInfo _timeZone;

        internal DTLocalEvaluatorDtxOpsDateTimeOffset(IList<CalendarOp> calendarOps, TimeZoneInfo timeZone)
            : base(calendarOps)
        {
            _timeZone = timeZone;
        }

        public object Evaluate(object target, EvaluateParams evaluateParams)
        {
            var dateValue = (DateTimeOffset) target;
            var dtx = new DateTimeEx(dateValue, _timeZone);

            DTLocalEvaluatorBase.EvaluateDtxOps(CalendarOps, dtx, evaluateParams);

            return dtx.DateTime;
        }
    }
}
