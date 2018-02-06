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
using com.espertech.esper.epl.expression.time;

namespace com.espertech.esper.epl.datetime.eval.reformat
{
    internal class DTLocalEvaluatorLongOpsReformat : DTLocalEvaluatorCalopReformatBase
    {
        private readonly TimeZoneInfo _timeZone;
        private readonly TimeAbacus _timeAbacus;

        internal DTLocalEvaluatorLongOpsReformat(
            IList<CalendarOp> calendarOps,
            ReformatOp reformatOp,
            TimeZoneInfo timeZone,
            TimeAbacus timeAbacus)
            : base(calendarOps, reformatOp)
        {
            _timeZone = timeZone;
            _timeAbacus = timeAbacus;
        }

        public override object Evaluate(object target, EvaluateParams evaluateParams)
        {
            var dtx = DateTimeEx.GetInstance(_timeZone);
            _timeAbacus.CalendarSet((long)target, dtx);
            DTLocalEvaluatorBase.EvaluateDtxOps(CalendarOps, dtx, evaluateParams);
            return ReformatOp.Evaluate(dtx, evaluateParams.EventsPerStream, evaluateParams.IsNewData, evaluateParams.ExprEvaluatorContext);
        }
    }
}
