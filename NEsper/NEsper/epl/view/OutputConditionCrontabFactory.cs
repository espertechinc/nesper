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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.schedule;

namespace com.espertech.esper.epl.view
{
    /// <summary>Output condition handling crontab-at schedule output.</summary>
    public class OutputConditionCrontabFactory : OutputConditionFactory {
        protected readonly ExprEvaluator[] scheduleSpecEvaluators;
        protected readonly bool isStartConditionOnCreation;
    
        public OutputConditionCrontabFactory(IList<ExprNode> scheduleSpecExpressionList,
                                             StatementContext statementContext,
                                             bool isStartConditionOnCreation)
                {
            this.scheduleSpecEvaluators = ExprNodeUtility.CrontabScheduleValidate(ExprNodeOrigin.OUTPUTLIMIT, scheduleSpecExpressionList, statementContext, false);
            this.isStartConditionOnCreation = isStartConditionOnCreation;
        }
    
        public OutputCondition Make(AgentInstanceContext agentInstanceContext, OutputCallback outputCallback) {
            ScheduleSpec scheduleSpec = ExprNodeUtility.CrontabScheduleBuild(scheduleSpecEvaluators, agentInstanceContext);
            return new OutputConditionCrontab(outputCallback, agentInstanceContext, isStartConditionOnCreation, scheduleSpec);
        }
    }
} // end of namespace
