///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.subselect;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.start;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.spec;
using com.espertech.esper.util;

namespace com.espertech.esper.core.context.factory
{
    public class StatementAgentInstanceFactoryUpdate : StatementAgentInstanceFactoryBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly StatementContext _statementContext;
        private readonly EPServicesContext _services;
        private readonly EventType _streamEventType;
        private readonly UpdateDesc _desc;
        private readonly InternalRoutePreprocessView _onExprView;
        private readonly InternalEventRouterDesc _routerDesc;
        private readonly SubSelectStrategyCollection _subSelectStrategyCollection;

        public StatementAgentInstanceFactoryUpdate(StatementContext statementContext, EPServicesContext services, EventType streamEventType, UpdateDesc desc, InternalRoutePreprocessView onExprView, InternalEventRouterDesc routerDesc, SubSelectStrategyCollection subSelectStrategyCollection)
            : base(statementContext.Annotations)
        {
            _statementContext = statementContext;
            _services = services;
            _streamEventType = streamEventType;
            _desc = desc;
            _onExprView = onExprView;
            _subSelectStrategyCollection = subSelectStrategyCollection;
            _routerDesc = routerDesc;
        }

        protected override StatementAgentInstanceFactoryResult NewContextInternal(AgentInstanceContext agentInstanceContext, bool isRecoveringResilient)
        {
            StopCallback stopCallback;
            IList<StopCallback> stopCallbacks = new List<StopCallback>();
            IDictionary<ExprSubselectNode, SubSelectStrategyHolder> subselectStrategies;

            try
            {
                stopCallbacks.Add(new ProxyStopCallback(() => _services.InternalEventRouter.RemovePreprocessing(_streamEventType, _desc)));

                _services.InternalEventRouter.AddPreprocessing(_routerDesc, _onExprView, agentInstanceContext.AgentInstanceLock, !_subSelectStrategyCollection.Subqueries.IsEmpty());

                // start subselects
                subselectStrategies = EPStatementStartMethodHelperSubselect.StartSubselects(_services, _subSelectStrategyCollection, agentInstanceContext, stopCallbacks, isRecoveringResilient);
            }
            catch (Exception)
            {
                stopCallback = StatementAgentInstanceUtil.GetStopCallback(stopCallbacks, agentInstanceContext);
                StatementAgentInstanceUtil.StopSafe(stopCallback, _statementContext);
                throw;
            }

            StatementAgentInstanceFactoryUpdateResult result = new StatementAgentInstanceFactoryUpdateResult(_onExprView, null, agentInstanceContext, subselectStrategies);
            if (_statementContext.StatementExtensionServicesContext != null)
            {
                _statementContext.StatementExtensionServicesContext.ContributeStopCallback(result, stopCallbacks);
            }

            stopCallback = StatementAgentInstanceUtil.GetStopCallback(stopCallbacks, agentInstanceContext);
            result.StopCallback = stopCallback;
            return result;
        }

        public override void AssignExpressions(StatementAgentInstanceFactoryResult result)
        {
        }

        public override void UnassignExpressions()
        {
        }
    }
}
