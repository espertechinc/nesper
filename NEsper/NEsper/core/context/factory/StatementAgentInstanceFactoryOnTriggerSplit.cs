///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.core.start;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.factory
{
    public class StatementAgentInstanceFactoryOnTriggerSplit : StatementAgentInstanceFactoryOnTriggerBase
    {
        private readonly EPStatementStartMethodOnTriggerItem[] _items;
        private readonly EventType _activatorResultEventType;

        public StatementAgentInstanceFactoryOnTriggerSplit(
            StatementContext statementContext,
            StatementSpecCompiled statementSpec,
            EPServicesContext services,
            ViewableActivator activator,
            SubSelectStrategyCollection subSelectStrategyCollection,
            EPStatementStartMethodOnTriggerItem[] items,
            EventType activatorResultEventType)
            : base(statementContext, statementSpec, services, activator, subSelectStrategyCollection)
        {
            _items = items;
            _activatorResultEventType = activatorResultEventType;
        }

        public override OnExprViewResult DetermineOnExprView(
            AgentInstanceContext agentInstanceContext,
            IList<StopCallback> stopCallbacks,
            bool isRecoveringReslient)
        {
            var processors = new ResultSetProcessor[_items.Length];
            for (int i = 0; i < processors.Length; i++)
            {
                ResultSetProcessorFactoryDesc factory = _items[i].GetFactoryDesc();
                ResultSetProcessor processor = factory.ResultSetProcessorFactory.Instantiate(
                    null, null, agentInstanceContext);
                processors[i] = processor;
            }

            var tableStateInstances = new TableStateInstance[processors.Length];
            for (int i = 0; i < _items.Length; i++)
            {
                string tableName = _items[i].GetInsertIntoTableNames();
                if (tableName != null)
                {
                    tableStateInstances[i] = agentInstanceContext.StatementContext.TableService.GetState(
                        tableName, agentInstanceContext.AgentInstanceId);
                }
            }

            var whereClauseEvals = new ExprEvaluator[_items.Length];
            for (int i = 0; i < _items.Length; i++)
            {
                whereClauseEvals[i] = _items[i].GetWhereClause() == null ? null : _items[i].GetWhereClause().ExprEvaluator;
            }

            var desc = (OnTriggerSplitStreamDesc) base.StatementSpec.OnTriggerDesc;
            var view = new RouteResultView(
                desc.IsFirst, _activatorResultEventType, base.StatementContext.EpStatementHandle, base.Services.InternalEventRouter,
                tableStateInstances, _items, processors, whereClauseEvals, agentInstanceContext);
            return new OnExprViewResult(view, null);
        }

        public override View DetermineFinalOutputView(AgentInstanceContext agentInstanceContext, View onExprView)
        {
            return onExprView;
        }
    }
} // end of namespace
