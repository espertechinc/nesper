///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.context.aifactory.createexpression
{
    public class StatementAgentInstanceFactoryCreateExpression : StatementAgentInstanceFactory
    {
        private string _expressionName;
        private Viewable _viewable;

        public string ExpressionName {
            set => _expressionName = value;
        }

        public EventType StatementEventType {
            get => _viewable.EventType;
            set => _viewable = new ViewableDefaultImpl(value);
        }

        public void StatementCreate(StatementContext value)
        {
        }

        public void StatementDestroy(StatementContext statementContext)
        {
        }

        public void StatementDestroyPreconditions(StatementContext statementContext)
        {
        }

        public StatementAgentInstanceFactoryResult NewContext(
            AgentInstanceContext agentInstanceContext,
            bool isRecoveringResilient)
        {
            return new StatementAgentInstanceFactoryCreateExpressionResult(
                _viewable,
                AgentInstanceMgmtCallbackNoAction.INSTANCE,
                agentInstanceContext);
        }

        public AIRegistryRequirements RegistryRequirements => AIRegistryRequirements.NoRequirements();

        public IReaderWriterLock ObtainAgentInstanceLock(
            StatementContext statementContext,
            int agentInstanceId)
        {
            return AgentInstanceUtil.NewLock(statementContext, agentInstanceId);
        }
    }
} // end of namespace