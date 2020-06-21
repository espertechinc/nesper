///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.aifactory.ontrigger.core;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.ontrigger;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.onset
{
    public class StatementAgentInstanceFactoryOnTriggerSet : StatementAgentInstanceFactoryOnTriggerBase
    {
        private ResultSetProcessorFactoryProvider _resultSetProcessorFactoryProvider;

        public VariableReadWritePackage VariableReadWrite { get; set; }

        public ResultSetProcessorFactoryProvider ResultSetProcessorFactoryProvider {
            set => _resultSetProcessorFactoryProvider = value;
        }

        public override InfraOnExprBaseViewResult DetermineOnExprView(
            AgentInstanceContext agentInstanceContext,
            IList<AgentInstanceMgmtCallback> stopCallbacks,
            bool isRecoveringReslient)
        {
            var view = new OnSetVariableView(this, agentInstanceContext);
            return new InfraOnExprBaseViewResult(view, null);
        }

        public override View DetermineFinalOutputView(
            AgentInstanceContext agentInstanceContext,
            View onExprView)
        {
            // create result-processing
            var pair =
                StatementAgentInstanceFactoryUtil.StartResultSetAndAggregation(
                    _resultSetProcessorFactoryProvider,
                    agentInstanceContext,
                    false,
                    null);
            var @out = new OutputProcessViewSimpleWProcessor(agentInstanceContext, pair.First);
            @out.Parent = onExprView;
            onExprView.Child = @out;

            return @out;
        }

        public override IReaderWriterLock ObtainAgentInstanceLock(
            StatementContext statementContext,
            int agentInstanceId)
        {
            return AgentInstanceUtil.NewLock(statementContext);
        }
    }
} // end of namespace