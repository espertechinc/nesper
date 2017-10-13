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
        long DeltaAdd(long currentTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);

        long DeltaSubtract(long currentTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);

        long DeltaUseEngineTime(EventBean[] eventsPerStream, AgentInstanceContext agentInstanceContext);

        ExprTimePeriodEvalDeltaResult DeltaAddWReference(
            long current,
            long reference,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);
    }
} // end of namespace
