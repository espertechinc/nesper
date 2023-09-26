///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.database.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.fafquery.processor
{
    public class FireAndForgetProcessorDB : FireAndForgetProcessor
    {
        private HistoricalEventViewableDatabaseFactory factory;
        public override EventType EventTypeResultSetProcessor => factory.EventType;
        public override string ContextName => null;
        public override string ContextDeploymentId => throw new IllegalStateException("Not available");

        public override FireAndForgetInstance ProcessorInstanceNoContext =>
            throw new IllegalStateException("Not available");

        public bool IsVirtualDataWindow => false;
        public override EventType EventTypePublic => factory.EventType;
        public override StatementContext StatementContext => throw new IllegalStateException("Not available");

        public FireAndForgetProcessorDBExecUnprepared Unprepared(
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextRuntimeServices services)
        {
            var poll = factory.ActivateFireAndForget(exprEvaluatorContext, services);
            return new FireAndForgetProcessorDBExecUnprepared(poll, factory.Evaluator);
        }

        public FireAndForgetProcessorDBExecPrepared Prepared(
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextRuntimeServices services)
        {
            var poll = factory.ActivateFireAndForget(exprEvaluatorContext, services);
            return new FireAndForgetProcessorDBExecPrepared(poll, factory.Evaluator);
        }

        public FireAndForgetInstance GetProcessorInstance(AgentInstanceContext agentInstanceContext)
        {
            throw new IllegalStateException("Not available");
        }

        public override FireAndForgetInstance GetProcessorInstanceContextById(int agentInstanceId)
        {
            throw new IllegalStateException("Not available");
        }

        public HistoricalEventViewableDatabaseFactory Factory {
            get => factory;

            set => factory = value;
        }
    }
} // end of namespace