///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.view.expression
{
    /// <summary>
    ///     This view is a moving window extending the into the past until the expression passed to it returns false.
    /// </summary>
    public abstract class ExpressionViewBase : ViewSupport,
        DataWindowView,
        AgentInstanceMgmtCallback,
        VariableChangeCallback
    {
        internal readonly AgentInstanceContext agentInstanceContext;
        internal readonly AggregationService aggregationService;
        internal readonly ObjectArrayEventBean builtinEventProps;
        internal readonly EventBean[] eventsPerStream;

        internal readonly ExpressionViewFactoryBase factory;
        internal readonly EPStatementHandleCallbackSchedule scheduleHandle;
        internal readonly long scheduleSlot;
        internal readonly ViewUpdatedCollection viewUpdatedCollection;

        public ExpressionViewBase(
            ExpressionViewFactoryBase factory,
            ViewUpdatedCollection viewUpdatedCollection,
            ObjectArrayEventBean builtinEventProps,
            AgentInstanceViewFactoryChainContext agentInstanceContext)
        {
            this.factory = factory;
            this.viewUpdatedCollection = viewUpdatedCollection;
            this.builtinEventProps = builtinEventProps;
            eventsPerStream = new EventBean[] {null, builtinEventProps};
            this.agentInstanceContext = agentInstanceContext.AgentInstanceContext;

            if (factory.Variables != null && factory.Variables.Length > 0) {
                foreach (var variable in factory.Variables) {
                    var variableDepId = variable.DeploymentId;
                    var variableName = variable.MetaData.VariableName;
                    int agentInstanceId = agentInstanceContext.AgentInstanceId;
                    agentInstanceContext.StatementContext.VariableManagementService.RegisterCallback(
                        variable.DeploymentId,
                        variableName,
                        agentInstanceId,
                        this);
                    agentInstanceContext.AgentInstanceContext.AddTerminationCallback(
                        new ProxyAgentInstanceMgmtCallback {
                            ProcStop = services => {
                                services.AgentInstanceContext.VariableManagementService
                                    .UnregisterCallback(variableDepId, variableName, agentInstanceId, this);
                            }
                        });
                }

                ScheduleHandleCallback callback = new ProxyScheduleHandleCallback {
                    ProcScheduledTrigger = () => {
                        agentInstanceContext.AuditProvider.ScheduleFire(
                            agentInstanceContext.AgentInstanceContext,
                            ScheduleObjectType.view,
                            factory.ViewName);
                        agentInstanceContext.InstrumentationProvider.QViewScheduledEval(factory);
                        ScheduleCallback();
                        agentInstanceContext.InstrumentationProvider.AViewScheduledEval();
                    }
                };
                scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();
                scheduleHandle = new EPStatementHandleCallbackSchedule(
                    agentInstanceContext.EpStatementAgentInstanceHandle,
                    callback);
            }
            else {
                scheduleSlot = -1;
                scheduleHandle = null;
            }

            if (factory.AggregationServiceFactory != null) {
                aggregationService = factory.AggregationServiceFactory.MakeService(
                    agentInstanceContext.AgentInstanceContext,
                    agentInstanceContext.ImportService,
                    false,
                    null,
                    null);
            }
            else {
                aggregationService = null;
            }
        }

        public ViewUpdatedCollection ViewUpdatedCollection => viewUpdatedCollection;

        public AggregationService AggregationService => aggregationService;

        public ViewFactory ViewFactory => factory;

        public void Stop(AgentInstanceStopServices services)
        {
            StopScheduleAndVar();
            agentInstanceContext.RemoveTerminationCallback(this);
        }

        public override EventType EventType => Parent.EventType;

        /// <summary>
        ///     Implemented to check the expiry expression.
        /// </summary>
        public abstract void ScheduleCallback();

        public override string ToString()
        {
            return GetType().Name;
        }

        private void StopScheduleAndVar()
        {
            if (factory.Variables != null && factory.Variables.Length > 0) {
                foreach (var variable in factory.Variables) {
                    agentInstanceContext.StatementContext.VariableManagementService.UnregisterCallback(
                        variable.DeploymentId,
                        variable.MetaData.VariableName,
                        agentInstanceContext.AgentInstanceId,
                        this);
                }

                if (agentInstanceContext.StatementContext.SchedulingService.IsScheduled(scheduleHandle)) {
                    agentInstanceContext.AuditProvider.ScheduleRemove(
                        agentInstanceContext,
                        scheduleHandle,
                        ScheduleObjectType.view,
                        factory.ViewName);
                    agentInstanceContext.StatementContext.SchedulingService.Remove(scheduleHandle, scheduleSlot);
                }
            }
        }

        // Handle variable updates by scheduling a re-evaluation with timers
        public virtual void Update(
            object newValue,
            object oldValue)
        {
            if (!agentInstanceContext.StatementContext.SchedulingService.IsScheduled(scheduleHandle)) {
                agentInstanceContext.AuditProvider.ScheduleAdd(
                    0,
                    agentInstanceContext,
                    scheduleHandle,
                    ScheduleObjectType.view,
                    factory.ViewName);
                agentInstanceContext.StatementContext.SchedulingService.Add(0, scheduleHandle, scheduleSlot);
            }
        }

        public virtual void Transfer(AgentInstanceTransferServices services)
        {
        }

        public abstract void VisitView(ViewDataVisitor viewDataVisitor);
    }
} // end of namespace