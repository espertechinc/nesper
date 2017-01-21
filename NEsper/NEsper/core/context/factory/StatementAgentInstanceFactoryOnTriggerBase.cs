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

using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.activator;
using com.espertech.esper.core.context.subselect;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.start;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.epl.spec;
using com.espertech.esper.pattern;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.factory
{
    public abstract class StatementAgentInstanceFactoryOnTriggerBase : StatementAgentInstanceFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        protected readonly StatementContext StatementContext;
        protected readonly StatementSpecCompiled StatementSpec;
        protected readonly EPServicesContext Services;
        private readonly ViewableActivator _activator;
        private readonly SubSelectStrategyCollection _subSelectStrategyCollection;

        public abstract OnExprViewResult DetermineOnExprView(AgentInstanceContext agentInstanceContext, IList<StopCallback> stopCallbacks, bool isRecoveringResilient);
        public abstract View DetermineFinalOutputView(AgentInstanceContext agentInstanceContext, View onExprView);
    
        public StatementAgentInstanceFactoryOnTriggerBase(StatementContext statementContext, StatementSpecCompiled statementSpec, EPServicesContext services, ViewableActivator activator, SubSelectStrategyCollection subSelectStrategyCollection)
        {
            this.StatementContext = statementContext;
            this.StatementSpec = statementSpec;
            this.Services = services;
            this._activator = activator;
            this._subSelectStrategyCollection = subSelectStrategyCollection;
        }
    
        public StatementAgentInstanceFactoryResult NewContext(AgentInstanceContext agentInstanceContext, bool isRecoveringResilient)
        {
            IList<StopCallback> stopCallbacks = new List<StopCallback>();
            View view;
            IDictionary<ExprSubselectNode, SubSelectStrategyHolder> subselectStrategies;
            AggregationService aggregationService;
            EvalRootState optPatternRoot;
            IDictionary<ExprTableAccessNode, ExprTableAccessEvalStrategy> tableAccessStrategies;
            ViewableActivationResult activationResult;

            StopCallback stopCallback;
            try
            {
                var onExprViewResult = DetermineOnExprView(agentInstanceContext, stopCallbacks, isRecoveringResilient);

                view = onExprViewResult.OnExprView;
                aggregationService = onExprViewResult.OptionalAggregationService;
    
                // attach stream to view
                activationResult = _activator.Activate(agentInstanceContext, false, isRecoveringResilient);
                activationResult.Viewable.AddView(view);
                stopCallbacks.Add(activationResult.StopCallback);
                optPatternRoot = activationResult.OptionalPatternRoot;
    
                // determine final output view
                view = DetermineFinalOutputView(agentInstanceContext, view);
    
                // start subselects
                subselectStrategies = EPStatementStartMethodHelperSubselect.StartSubselects(Services, _subSelectStrategyCollection, agentInstanceContext, stopCallbacks, isRecoveringResilient);
    
                // plan table access
                tableAccessStrategies = EPStatementStartMethodHelperTableAccess.AttachTableAccess(Services, agentInstanceContext, StatementSpec.TableNodes);
            }
            catch (Exception)
            {
                stopCallback = StatementAgentInstanceUtil.GetStopCallback(stopCallbacks, agentInstanceContext);
                StatementAgentInstanceUtil.StopSafe(stopCallback, StatementContext);
                throw;
            }

            StatementAgentInstanceFactoryOnTriggerResult onTriggerResult = new StatementAgentInstanceFactoryOnTriggerResult(view, null, agentInstanceContext, aggregationService, subselectStrategies, optPatternRoot, tableAccessStrategies, activationResult);
            if (StatementContext.StatementExtensionServicesContext != null)
            {
                StatementContext.StatementExtensionServicesContext.ContributeStopCallback(onTriggerResult, stopCallbacks);
            }

            Log.Debug(".start Statement start completed");
            stopCallback = StatementAgentInstanceUtil.GetStopCallback(stopCallbacks, agentInstanceContext);
            onTriggerResult.StopCallback = stopCallback;

            return onTriggerResult;
        }

        public virtual void AssignExpressions(StatementAgentInstanceFactoryResult result)
        {
        }

        public virtual void UnassignExpressions()
        {
        }
    
        public class OnExprViewResult
        {
            public OnExprViewResult(View onExprView, AggregationService optionalAggregationService)
            {
                OnExprView = onExprView;
                OptionalAggregationService = optionalAggregationService;
            }

            public View OnExprView { get; private set; }

            public AggregationService OptionalAggregationService { get; private set; }
        }
    }
}
