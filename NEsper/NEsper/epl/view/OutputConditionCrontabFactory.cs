///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.schedule;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    /// Output condition handling crontab-at schedule output.
    /// </summary>
    public class OutputConditionCrontabFactory : OutputConditionFactory
    {
        public OutputConditionCrontabFactory(
            IList<ExprNode> scheduleSpecExpressionList,
            StatementContext statementContext,
            bool isStartConditionOnCreation)
        {
            ScheduleSpec = ExprNodeUtility.ToCrontabSchedule(ExprNodeOrigin.OUTPUTLIMIT, scheduleSpecExpressionList, statementContext, false);
            IsStartConditionOnCreation = isStartConditionOnCreation;
        }

        public OutputCondition Make(AgentInstanceContext agentInstanceContext, OutputCallback outputCallback)
        {
            return new OutputConditionCrontab(outputCallback, agentInstanceContext, this, IsStartConditionOnCreation);
        }

        public ScheduleSpec ScheduleSpec { get; private set; }

        public bool IsStartConditionOnCreation { get; set; }
    }
}
