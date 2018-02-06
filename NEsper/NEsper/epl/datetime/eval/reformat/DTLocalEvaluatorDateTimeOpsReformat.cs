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
using com.espertech.esper.epl.datetime.reformatop;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.datetime.eval.reformat
{
    internal class DTLocalEvaluatorDateTimeOpsReformat : DTLocalEvaluatorCalopReformatBase
    {
        private readonly TimeZoneInfo _timeZone;

        internal DTLocalEvaluatorDateTimeOpsReformat(
            IList<CalendarOp> calendarOps,
            ReformatOp reformatOp,
            TimeZoneInfo timeZone)
            : base(calendarOps, reformatOp)
        {
            _timeZone = timeZone;
        }

        public override object Evaluate(object target, EvaluateParams evaluateParams)
        {
            var dt = (DateTime) target;
            var dtx = DateTimeEx.GetInstance(_timeZone);
            dtx.SetUtcMillis(dt.UtcMillis());
            DTLocalEvaluatorBase.EvaluateDtxOps(CalendarOps, dtx, evaluateParams);
            return ReformatOp.Evaluate(dtx, evaluateParams.EventsPerStream, evaluateParams.IsNewData, evaluateParams.ExprEvaluatorContext);
        }
    }
}
