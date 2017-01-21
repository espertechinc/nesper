///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.time
{
    public interface ExprTimePeriodEvalDeltaNonConst
    {
        long DeltaMillisecondsAdd(long currentTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);
        long DeltaMillisecondsSubtract(long currentTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);
        long DeltaMillisecondsUseEngineTime(EventBean[] eventsPerStream, AgentInstanceContext agentInstanceContext);
        ExprTimePeriodEvalDeltaResult DeltaMillisecondsAddWReference(long current, long reference, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);
    }
}
