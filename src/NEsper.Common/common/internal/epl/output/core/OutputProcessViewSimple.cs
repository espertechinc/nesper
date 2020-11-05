///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.common.@internal.epl.output.core
{
    public class OutputProcessViewSimple : OutputProcessView
    {
        private readonly AgentInstanceContext agentInstanceContext;

        public OutputProcessViewSimple(AgentInstanceContext agentInstanceContext)
        {
            this.agentInstanceContext = agentInstanceContext;
        }

        public override int NumChangesetRows => 0;

        public override OutputCondition OptionalOutputCondition => null;

        public override EventType EventType => Parent.EventType;

        public override void Stop(AgentInstanceStopServices services)
        {
        }

        public override void Process(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            var statementResultService = agentInstanceContext.StatementResultService;
            var isGenerateSynthetic = statementResultService.IsMakeSynthetic;
            var isGenerateNatural = statementResultService.IsMakeNatural;

            if (!isGenerateSynthetic && !isGenerateNatural) {
                return;
            }

            child.NewResult(new UniformPair<EventBean[]>(newData, oldData));
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return Parent.GetEnumerator();
        }

        public override void Terminated()
        {
        }
    }
} // end of namespace