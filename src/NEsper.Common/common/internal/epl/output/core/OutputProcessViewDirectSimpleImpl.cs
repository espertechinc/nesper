///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.condition;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;

namespace com.espertech.esper.common.@internal.epl.output.core
{
    public class OutputProcessViewDirectSimpleImpl : OutputProcessView
    {
        private readonly ResultSetProcessor resultSetProcessor;
        private readonly AgentInstanceContext agentInstanceContext;

        public OutputProcessViewDirectSimpleImpl(
            ResultSetProcessor resultSetProcessor,
            AgentInstanceContext agentInstanceContext)
        {
            this.resultSetProcessor = resultSetProcessor;
            this.agentInstanceContext = agentInstanceContext;
        }

        public override EventType EventType => resultSetProcessor.ResultEventType;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            var instrumentationCommon = agentInstanceContext.InstrumentationProvider;
            instrumentationCommon.QOutputProcessNonBuffered(newData, oldData);
            var statementResultService = agentInstanceContext.StatementResultService;
            var isGenerateSynthetic = statementResultService.IsMakeSynthetic;
            var isGenerateNatural = statementResultService.IsMakeNatural;
            var newOldEvents = resultSetProcessor.ProcessViewResult(newData, oldData, isGenerateSynthetic);
            if (!isGenerateSynthetic && !isGenerateNatural) {
                return;
            }

            if (child != null) {
                if (newOldEvents != null) {
                    if (newOldEvents.First != null || newOldEvents.Second != null) {
                        child.NewResult(newOldEvents);
                    }
                    else if (newData == null && oldData == null) {
                        child.NewResult(newOldEvents);
                    }
                }
                else {
                    if (newData == null && oldData == null) {
                        child.NewResult(newOldEvents);
                    }
                }
            }

            instrumentationCommon.AOutputProcessNonBuffered();
        }

        public override void Process(
            ISet<MultiKeyArrayOfKeys<EventBean>> newData,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var instrumentationCommon = agentInstanceContext.InstrumentationProvider;
            instrumentationCommon.QOutputProcessNonBufferedJoin(newData, oldData);
            var statementResultService = agentInstanceContext.StatementResultService;
            var isGenerateSynthetic = statementResultService.IsMakeSynthetic;
            var isGenerateNatural = statementResultService.IsMakeNatural;
            var newOldEvents = resultSetProcessor.ProcessJoinResult(newData, oldData, isGenerateSynthetic);
            if (!isGenerateSynthetic && !isGenerateNatural) {
                return;
            }

            if (newOldEvents == null) {
                return;
            }

            if (newOldEvents.First != null || newOldEvents.Second != null) {
                child.NewResult(newOldEvents);
            }
            else if (newData == null && oldData == null) {
                child.NewResult(newOldEvents);
            }

            instrumentationCommon.AOutputProcessNonBufferedJoin();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return OutputStrategyUtil.GetEnumerator(joinExecutionStrategy, resultSetProcessor, parentView, false, null);
        }

        public override void Stop(AgentInstanceStopServices svc)
        {
        }

        public override void Terminated()
        {
        }

        public override int NumChangesetRows => 0;

        public override OutputCondition OptionalOutputCondition => null;
    }
} // end of namespace