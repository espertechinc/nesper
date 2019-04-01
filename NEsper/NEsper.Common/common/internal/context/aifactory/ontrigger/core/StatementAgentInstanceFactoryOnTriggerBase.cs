///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.ontrigger;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.core
{
    public abstract class StatementAgentInstanceFactoryOnTriggerBase : StatementAgentInstanceFactory
    {
        private ViewableActivator activator;
        private IDictionary<int, SubSelectFactory> subselects;
        private IDictionary<int, ExprTableEvalStrategyFactory> tableAccesses;

        public ViewableActivator Activator {
            set => activator = value;
        }

        public EventType ResultEventType {
            set => StatementEventType = value;
        }

        public IDictionary<int, SubSelectFactory> Subselects {
            set => subselects = value;
        }

        public IDictionary<int, ExprTableEvalStrategyFactory> TableAccesses {
            set => tableAccesses = value;
        }

        public EventType StatementEventType { get; private set; }

        public void StatementCreate(StatementContext statementContext)
        {
        }

        public void StatementDestroy(StatementContext statementContext)
        {
        }

        public void StatementDestroyPreconditions(StatementContext statementContext)
        {
        }

        public abstract StatementAgentInstanceLock ObtainAgentInstanceLock(
            StatementContext statementContext, 
            int agentInstanceId);

        public abstract InfraOnExprBaseViewResult DetermineOnExprView(
            AgentInstanceContext agentInstanceContext,
            IList<AgentInstanceStopCallback> stopCallbacks,
            bool isRecoveringReslient);

        public abstract View DetermineFinalOutputView(AgentInstanceContext agentInstanceContext, View onExprView);

        // public StatementAgentInstanceFactoryOnTriggerResult NewContext()
        public StatementAgentInstanceFactoryResult NewContext(
            AgentInstanceContext agentInstanceContext, bool isRecoveringResilient)
        {
            IList<AgentInstanceStopCallback> stopCallbacks = new List<AgentInstanceStopCallback>();

            View view;
            IDictionary<int, SubSelectFactoryResult> subselectActivations;
            AggregationService aggregationService;
            EvalRootState optPatternRoot;
            IDictionary<int, ExprTableEvalStrategy> tableAccessEvals;
            ViewableActivationResult activationResult;

            try {
                var onExprViewResult = DetermineOnExprView(agentInstanceContext, stopCallbacks, isRecoveringResilient);
                view = onExprViewResult.View;
                aggregationService = onExprViewResult.OptionalAggregationService;

                // attach stream to view
                activationResult = activator.Activate(agentInstanceContext, false, isRecoveringResilient);
                activationResult.Viewable.Child = view;
                stopCallbacks.Add(activationResult.StopCallback);
                optPatternRoot = activationResult.OptionalPatternRoot;

                // determine final output view
                view = DetermineFinalOutputView(agentInstanceContext, view);

                // start subselects
                subselectActivations = SubSelectHelperStart.StartSubselects(
                    subselects, agentInstanceContext, stopCallbacks, isRecoveringResilient);

                // start table-access
                tableAccessEvals = ExprTableEvalHelperStart.StartTableAccess(tableAccesses, agentInstanceContext);
            }
            catch (EPException) {
                throw;
            }
            catch (Exception ex) {
                var stopCallbackX = AgentInstanceUtil.FinalizeSafeStopCallbacks(stopCallbacks);
                AgentInstanceUtil.StopSafe(stopCallbackX, agentInstanceContext);
                throw new EPException(ex.Message, ex);
            }

            var stopCallback = AgentInstanceUtil.FinalizeSafeStopCallbacks(stopCallbacks);
            var onTriggerResult = new StatementAgentInstanceFactoryOnTriggerResult(
                view, stopCallback, agentInstanceContext, aggregationService,
                subselectActivations, null, null, null, tableAccessEvals, null, null, activationResult);
            return onTriggerResult;
        }

        public AIRegistryRequirements RegistryRequirements {
            get {
                var subqueries = AIRegistryRequirements.GetSubqueryRequirements(subselects);
                return new AIRegistryRequirements(
                    null, null, subqueries, tableAccesses == null ? 0 : tableAccesses.Count, false);
            }
        }
    }
} // end of namespace