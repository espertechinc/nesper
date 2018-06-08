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
    internal class DTLocalEvaluatorDtxOpsDateTime
        : DTLocalEvaluatorDtxOpsDtxBase
        , DTLocalEvaluator
    {
        private readonly TimeZoneInfo _timeZone;

        internal DTLocalEvaluatorDtxOpsDateTime(IList<CalendarOp> calendarOps, TimeZoneInfo timeZone)
            : base(calendarOps)
        {
            _timeZone = timeZone;
        }

        public object Evaluate(object target, EvaluateParams evaluateParams)
        {
            var dateValue = (DateTime) target;
            var dtx = DateTimeEx.GetInstance(_timeZone);
            dtx.SetUtcMillis(dateValue.UtcMillis());

            DTLocalEvaluatorBase.EvaluateDtxOps(CalendarOps, dtx, evaluateParams);

            return dtx.DateTime.DateTime;
        }
    }
}
