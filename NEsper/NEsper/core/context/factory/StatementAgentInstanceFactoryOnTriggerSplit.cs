///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.core.context.activator;
using com.espertech.esper.core.context.subselect;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.util;
using com.espertech.esper.view;
using com.espertech.esper.view.internals;

namespace com.espertech.esper.core.context.factory
{
    public class StatementAgentInstanceFactoryOnTriggerSplit : StatementAgentInstanceFactoryOnTriggerBase
    {
        private readonly StatementAgentInstanceFactoryOnTriggerSplitDesc _splitDesc;
        private readonly EventType _activatorResultEventType;
        private readonly string[] _insertIntoTableNames;
    
        public StatementAgentInstanceFactoryOnTriggerSplit(StatementContext statementContext, StatementSpecCompiled statementSpec, EPServicesContext services, ViewableActivator activator, SubSelectStrategyCollection subSelectStrategyCollection, StatementAgentInstanceFactoryOnTriggerSplitDesc splitDesc, EventType activatorResultEventType, string[] insertIntoTableNames)
            : base(statementContext, statementSpec, services, activator, subSelectStrategyCollection)
        {
            _splitDesc = splitDesc;
            _activatorResultEventType = activatorResultEventType;
            _insertIntoTableNames = insertIntoTableNames;
        }
    
        public override OnExprViewResult DetermineOnExprView(AgentInstanceContext agentInstanceContext, IList<StopCallback> stopCallbacks)
        {
            var processors = new ResultSetProcessor[_splitDesc.ProcessorFactories.Length];
            for (var i = 0; i < processors.Length; i++) {
                var factory = _splitDesc.ProcessorFactories[i];
                var processor = factory.ResultSetProcessorFactory.Instantiate(null, null, agentInstanceContext);
                processors[i] = processor;
            }
    
            var tableStateInstances = new TableStateInstance[processors.Length];
            for (var i = 0; i < _insertIntoTableNames.Length; i++) {
                var tableName = _insertIntoTableNames[i];
                if (tableName != null) {
                    tableStateInstances[i] = agentInstanceContext.StatementContext.TableService.GetState(tableName, agentInstanceContext.AgentInstanceId);
                }
            }
            var desc = (OnTriggerSplitStreamDesc) StatementSpec.OnTriggerDesc;
            View view = new RouteResultView(
                desc.IsFirst, _activatorResultEventType, StatementContext.EpStatementHandle,
                Services.InternalEventRouter, tableStateInstances, _splitDesc.NamedWindowInsert, processors,
                _splitDesc.WhereClauses, agentInstanceContext);
            return new OnExprViewResult(view, null);
        }
    
        public override View DetermineFinalOutputView(AgentInstanceContext agentInstanceContext, View onExprView)
        {
            return onExprView;
        }
    }
}
