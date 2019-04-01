///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.datetime.calop;
using com.espertech.esper.common.@internal.epl.datetime.interval;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public abstract class DTLocalEvaluatorCalOpsIntervalBase : DTLocalEvaluator,
        DTLocalEvaluatorIntervalComp
    {
        internal readonly IList<CalendarOp> calendarOps;
        internal readonly IntervalOp intervalOp;

        internal DTLocalEvaluatorCalOpsIntervalBase(IList<CalendarOp> calendarOps, IntervalOp intervalOp)
        {
            this.calendarOps = calendarOps;
            this.intervalOp = intervalOp;
        }

        public abstract object Evaluate(
            object target, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext);

        public abstract object Evaluate(
            object startTimestamp, object endTimestamp, EventBean[] eventsPerStream, bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext);
    }
} // end of namespace