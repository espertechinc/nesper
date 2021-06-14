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
using com.espertech.esper.common.@internal.epl.datetime.reformatop;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public abstract class DTLocalEvaluatorCalopReformatBase : DTLocalEvaluator
    {
        internal readonly IList<CalendarOp> calendarOps;
        internal readonly ReformatOp reformatOp;

        protected DTLocalEvaluatorCalopReformatBase(
            IList<CalendarOp> calendarOps,
            ReformatOp reformatOp)
        {
            this.calendarOps = calendarOps;
            this.reformatOp = reformatOp;
        }

        public abstract object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext);
    }
} // end of namespace