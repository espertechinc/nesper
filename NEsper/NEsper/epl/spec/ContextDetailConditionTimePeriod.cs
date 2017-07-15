///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.filter;

namespace com.espertech.esper.epl.spec
{
    [Serializable]
    public class ContextDetailConditionTimePeriod : ContextDetailCondition
    {
        public ContextDetailConditionTimePeriod(ExprTimePeriod timePeriod, bool immediate)
        {
            ScheduleCallbackId = -1;
            TimePeriod = timePeriod;
            IsImmediate = immediate;
        }

        public ExprTimePeriod TimePeriod { get; set; }

        public IList<FilterSpecCompiled> FilterSpecIfAny
        {
            get { return null; }
        }

        public bool IsImmediate { get; private set; }

        public int ScheduleCallbackId { get; set; }

        public long GetExpectedEndTime(AgentInstanceContext agentInstanceContext)
        {
            var current = agentInstanceContext.StatementContext.TimeProvider.Time;
            var msec = TimePeriod.NonconstEvaluator().DeltaAdd(current, null, true, agentInstanceContext);
            return current + msec;
        }
    }
}
