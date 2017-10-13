///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.start;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.variable;
using com.espertech.esper.epl.view;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.factory
{
    public class StatementAgentInstanceFactoryCreateVariable : StatementAgentInstanceFactoryBase
    {
        private readonly CreateVariableDesc _createDesc;
        private readonly StatementSpecCompiled _statementSpec;
        private readonly StatementContext _statementContext;
        private readonly EPServicesContext _services;
        private readonly VariableMetaData _variableMetaData;
        private readonly EventType _eventType;
    
       public StatementAgentInstanceFactoryCreateVariable(CreateVariableDesc createDesc, StatementSpecCompiled statementSpec, StatementContext statementContext, EPServicesContext services, VariableMetaData variableMetaData, EventType eventType)
            : base(statementContext.Annotations)
        {
            _createDesc = createDesc;
            _statementSpec = statementSpec;
            _statementContext = statementContext;
            _services = services;
            _variableMetaData = variableMetaData;
            _eventType = eventType;
        }

        protected override StatementAgentInstanceFactoryResult NewContextInternal(AgentInstanceContext agentInstanceContext, bool isRecoveringResilient)
        {
            StopCallback stopCallback = new ProxyStopCallback(() => _services.VariableService.DeallocateVariableState(_variableMetaData.VariableName, agentInstanceContext.AgentInstanceId));
            _services.VariableService.AllocateVariableState(
                _variableMetaData.VariableName, agentInstanceContext.AgentInstanceId, _statementContext.StatementExtensionServicesContext, isRecoveringResilient);

            CreateVariableView createView = new CreateVariableView(
                _statementContext.StatementId, 
                _services.EventAdapterService, 
                _services.VariableService,
                _createDesc.VariableName,
                _statementContext.StatementResultService,
                agentInstanceContext.AgentInstanceId);

            _services.VariableService.RegisterCallback(_createDesc.VariableName, agentInstanceContext.AgentInstanceId, createView.Update);
            _statementContext.StatementStopService.StatementStopped += () => _services.VariableService.UnregisterCallback(_createDesc.VariableName, 0, createView.Update);

            // Create result set processor, use wildcard selection
            _statementSpec.SelectClauseSpec.SetSelectExprList(new SelectClauseElementWildcard());
            _statementSpec.SelectStreamDirEnum = SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH;
            var typeService = new StreamTypeServiceImpl(new EventType[] {createView.EventType}, new string[] {"create_variable"}, new bool[] {true}, _services.EngineURI, false);
            OutputProcessViewBase outputViewBase;
            try {
                ResultSetProcessorFactoryDesc resultSetProcessorPrototype = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                        _statementSpec, _statementContext, typeService, null, new bool[0], true, ContextPropertyRegistryImpl.EMPTY_REGISTRY, null, _services.ConfigSnapshot, _services.ResultSetProcessorHelperFactory, false, false);
                ResultSetProcessor resultSetProcessor = EPStatementStartMethodHelperAssignExpr.GetAssignResultSetProcessor(agentInstanceContext, resultSetProcessorPrototype, false, null, false);

                // Attach output view
                OutputProcessViewFactory outputViewFactory = OutputProcessViewFactoryFactory.Make(
                    _statementSpec, 
                    _services.InternalEventRouter, 
                    agentInstanceContext.StatementContext,
                    resultSetProcessor.ResultEventType, null,
                    _services.TableService,
                    resultSetProcessorPrototype.ResultSetProcessorFactory.ResultSetProcessorType,
                    _services.ResultSetProcessorHelperFactory,
                    _services.StatementVariableRefService);
                outputViewBase = outputViewFactory.MakeView(resultSetProcessor, agentInstanceContext);
                createView.AddView(outputViewBase);
            }
            catch (ExprValidationException ex)
            {
                throw new EPException("Unexpected exception in create-variable context allocation: " + ex.Message, ex);
            }

            return new StatementAgentInstanceFactoryCreateVariableResult(outputViewBase, stopCallback, agentInstanceContext);
        }

        public override void AssignExpressions(StatementAgentInstanceFactoryResult result)
        {
        }

        public override void UnassignExpressions()
        {
        }
    }
}
