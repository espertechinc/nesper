///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.variable;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.factory
{
    public class StatementAgentInstanceFactoryCreateVariable : StatementAgentInstanceFactoryBase
    {
        private readonly StatementContext _statementContext;
        private readonly EPServicesContext _services;
        private readonly VariableMetaData _variableMetaData;
        private readonly EventType _eventType;
    
        public StatementAgentInstanceFactoryCreateVariable(StatementContext statementContext, EPServicesContext services, VariableMetaData variableMetaData, EventType eventType)
            : base(statementContext.Annotations)
        {
            _statementContext = statementContext;
            _services = services;
            _variableMetaData = variableMetaData;
            _eventType = eventType;
        }

        protected override StatementAgentInstanceFactoryResult NewContextInternal(AgentInstanceContext agentInstanceContext, bool isRecoveringResilient)
        {
            StopCallback stopCallback = () => _services.VariableService.DeallocateVariableState(_variableMetaData.VariableName, agentInstanceContext.AgentInstanceId);
            _services.VariableService.AllocateVariableState(_variableMetaData.VariableName, agentInstanceContext.AgentInstanceId, _statementContext.StatementExtensionServicesContext);
            return new StatementAgentInstanceFactoryCreateVariableResult(new ViewableDefaultImpl(_eventType), stopCallback, agentInstanceContext);
        }

        public override void AssignExpressions(StatementAgentInstanceFactoryResult result)
        {
        }

        public override void UnassignExpressions()
        {
        }
    }
}
