///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events.arr;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// This view is a moving window extending the into the past until the expression passed to it returns false.
    /// </summary>
    public abstract class ExpressionViewBase
        : ViewSupport
        , DataWindowView
        , CloneableView
        , StoppableView
        , StopCallback
    {
        protected readonly ExprEvaluator ExpiryExpression;
        protected readonly ObjectArrayEventBean BuiltinEventProps;
        protected readonly EventBean[] EventsPerStream;
        protected readonly ISet<String> VariableNames;
        protected readonly AgentInstanceViewFactoryChainContext AgentInstanceContext;
        protected readonly long ScheduleSlot;
        protected readonly EPStatementHandleCallback ScheduleHandle;
        protected readonly IList<AggregationServiceAggExpressionDesc> AggregateNodes;
    
        /// <summary>Implemented to check the expiry expression. </summary>
        public abstract void ScheduleCallback();

        public abstract View CloneView();
        public abstract void VisitView(ViewDataVisitor viewDataVisitor);
        public abstract string ViewName { get; }
        public abstract ViewFactory ViewFactory { get; }

        protected ExpressionViewBase(
            ViewUpdatedCollection viewUpdatedCollection,
            ExprEvaluator expiryExpression,
            AggregationServiceFactoryDesc aggregationServiceFactoryDesc,
            ObjectArrayEventBean builtinEventProps,
            ISet<String> variableNames,
            AgentInstanceViewFactoryChainContext agentInstanceContext)
        {
            ViewUpdatedCollection = viewUpdatedCollection;
            ExpiryExpression = expiryExpression;
            BuiltinEventProps = builtinEventProps;
            EventsPerStream = new EventBean[] {null, builtinEventProps};
            VariableNames = variableNames;
            AgentInstanceContext = agentInstanceContext;
    
            if (variableNames != null && !variableNames.IsEmpty()) {
                foreach (String variable in variableNames) {
                    var variableName = variable;
                    var agentInstanceId = agentInstanceContext.AgentInstanceId;
                    var variableService = agentInstanceContext.StatementContext.VariableService;

                    agentInstanceContext.StatementContext.VariableService.RegisterCallback(variable, agentInstanceId, Update);
                    agentInstanceContext.AddTerminationCallback(
                        new ProxyStopCallback(() => variableService.UnregisterCallback(variableName, agentInstanceId, Update)));
                }
    
                ScheduleHandleCallback callback = new ProxyScheduleHandleCallback
                {
                    ProcScheduledTrigger = extensionServicesContext => Instrument.With(
                        i => i.QViewScheduledEval(this, ViewName),
                        i => i.AViewScheduledEval(),
                        ScheduleCallback)
                };
                ScheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();
                ScheduleHandle = new EPStatementHandleCallback(agentInstanceContext.EpStatementAgentInstanceHandle, callback);
                agentInstanceContext.AddTerminationCallback(this);
            }
            else {
                ScheduleSlot = -1;
                ScheduleHandle = null;
            }
    
            if (aggregationServiceFactoryDesc != null)
            {
                AggregationService = aggregationServiceFactoryDesc.AggregationServiceFactory.MakeService(
                    agentInstanceContext.AgentInstanceContext,
                    agentInstanceContext.AgentInstanceContext.StatementContext.EngineImportService, false, null);
                AggregateNodes = aggregationServiceFactoryDesc.Expressions;
            }
            else {
                AggregationService = null;
                AggregateNodes = Collections.GetEmptyList<AggregationServiceAggExpressionDesc>();
            }
        }

        public override EventType EventType => Parent.EventType;

        public override String ToString()
        {
            return GetType().FullName;
        }
    
        public void StopView() {
            StopScheduleAndVar();
            AgentInstanceContext.RemoveTerminationCallback(this);
        }
    
        public void Stop() {
            StopScheduleAndVar();
        }
    
        public void StopScheduleAndVar() {
            if (VariableNames != null && !VariableNames.IsEmpty()) {
                foreach (String variable in VariableNames) {
                    AgentInstanceContext.StatementContext.VariableService.UnregisterCallback(
                        variable, AgentInstanceContext.AgentInstanceId, Update);
                }
    
                if (AgentInstanceContext.StatementContext.SchedulingService.IsScheduled(ScheduleHandle)) {
                    AgentInstanceContext.StatementContext.SchedulingService.Remove(ScheduleHandle, ScheduleSlot);
                }
            }
        }
    
        // Handle variable updates by scheduling a re-evaluation with timers
        public virtual void Update(Object newValue, Object oldValue) {
            if (!AgentInstanceContext.StatementContext.SchedulingService.IsScheduled(ScheduleHandle)) {
                AgentInstanceContext.StatementContext.SchedulingService.Add(0, ScheduleHandle, ScheduleSlot);
            }
        }

        public virtual ViewUpdatedCollection ViewUpdatedCollection { get; private set; }

        public virtual AggregationService AggregationService { get; private set; }
    }
}
