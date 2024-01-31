///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.output.condition
{
    /// <summary>
    ///     Output condition for output rate limiting that handles when-then expressions for controlling output.
    /// </summary>
    public class OutputConditionExpression : OutputConditionBase,
        OutputCondition,
        VariableChangeCallback
    {
        public const string NAME_AUDITPROVIDER_SCHEDULE = "expression";
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly AgentInstanceContext agentInstanceContext;
        private readonly ObjectArrayEventBean builtinProperties;
        private readonly EventBean[] eventsPerStream;
        private readonly OutputConditionExpressionFactory parent;

        private readonly long scheduleSlot;
        private bool ignoreVariableCallbacks;
        private bool isCallbackScheduled;
        private long? lastOutputTimestamp;
        private EPStatementHandleCallbackSchedule scheduleHandle;

        // ongoing builtin properties
        private int totalNewEventsCount;
        private int totalNewEventsSum;
        private int totalOldEventsCount;
        private int totalOldEventsSum;

        public OutputConditionExpression(
            OutputCallback outputCallback,
            AgentInstanceContext agentInstanceContext,
            OutputConditionExpressionFactory parent)
            : base(outputCallback)
        {
            this.agentInstanceContext = agentInstanceContext;
            this.parent = parent;

            scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();
            eventsPerStream = new EventBean[1];

            if (parent.BuiltinPropertiesEventType != null) {
                builtinProperties = new ObjectArrayEventBean(
                    OutputConditionExpressionTypeUtil.OAPrototype,
                    parent.BuiltinPropertiesEventType);
                lastOutputTimestamp = agentInstanceContext.StatementContext.SchedulingService.Time;
            }

            if (parent.Variables != null && parent.Variables.Length > 0) {
                // if using variables, register a callback on the change of the variable
                foreach (var variable in parent.Variables) {
                    var theVariableDepId = variable.DeploymentId;
                    var theVariableName = variable.MetaData.VariableName;
                    agentInstanceContext.VariableManagementService.RegisterCallback(
                        theVariableDepId,
                        theVariableName,
                        agentInstanceContext.AgentInstanceId,
                        this);
                    agentInstanceContext.AddTerminationCallback(
                        new ProxyAgentInstanceMgmtCallback {
                            ProcStop = services => {
                                services.AgentInstanceContext.VariableManagementService.UnregisterCallback(
                                    theVariableDepId,
                                    theVariableName,
                                    agentInstanceContext.AgentInstanceId,
                                    this);
                            }
                        });
                }
            }

            if (parent.IsStartConditionOnCreation) {
                Update(0, 0);
            }
        }

        public override void UpdateOutputCondition(
            int newEventsCount,
            int oldEventsCount)
        {
            totalNewEventsCount += newEventsCount;
            totalOldEventsCount += oldEventsCount;
            totalNewEventsSum += newEventsCount;
            totalOldEventsSum += oldEventsCount;

            var isOutput = Evaluate(parent.WhenExpressionNodeEval);
            if (isOutput) {
                ExecuteThenAssignments();
                outputCallback.Invoke(true, true);
                ResetBuiltinProperties();
            }
        }

        public override void Terminated()
        {
            var output = true;
            if (parent.WhenTerminatedExpressionNodeEval != null) {
                output = Evaluate(parent.WhenTerminatedExpressionNodeEval);
            }

            if (parent.VariableReadWritePackageAfterTerminated != null) {
                if (builtinProperties != null) {
                    PopulateBuiltinProps();
                    eventsPerStream[0] = builtinProperties;
                }

                ignoreVariableCallbacks = true;
                try {
                    parent.VariableReadWritePackageAfterTerminated.WriteVariables(
                        eventsPerStream,
                        null,
                        agentInstanceContext);
                }
                finally {
                    ignoreVariableCallbacks = false;
                }
            }

            if (output) {
                base.Terminated();
            }
        }

        public override void StopOutputCondition()
        {
            if (scheduleHandle != null) {
                agentInstanceContext.AuditProvider.ScheduleRemove(
                    agentInstanceContext,
                    scheduleHandle,
                    ScheduleObjectType.outputratelimiting,
                    NAME_AUDITPROVIDER_SCHEDULE);
                agentInstanceContext.StatementContext.SchedulingService.Remove(scheduleHandle, scheduleSlot);
                scheduleHandle = null;
            }
        }

        public void Update(
            object newValue,
            object oldValue)
        {
            if (ignoreVariableCallbacks) {
                Log.Debug(".update Ignoring variable callback");
                return;
            }

            agentInstanceContext.VariableManagementService.SetLocalVersion();
            var isOutput = Evaluate(parent.WhenExpressionNodeEval);
            if (isOutput && !isCallbackScheduled) {
                ScheduleCallback();
            }
        }

        private bool Evaluate(ExprEvaluator evaluator)
        {
            if (builtinProperties != null) {
                PopulateBuiltinProps();
                eventsPerStream[0] = builtinProperties;
            }

            var result = false;
            var output = evaluator.Evaluate(eventsPerStream, true, agentInstanceContext);
            if (output != null && true.Equals(output)) {
                result = true;
            }

            return result;
        }

        private void ScheduleCallback()
        {
            isCallbackScheduled = true;

            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback {
                ProcScheduledTrigger = () => {
                    agentInstanceContext.InstrumentationProvider.QOutputRateConditionScheduledEval();
                    agentInstanceContext.AuditProvider.ScheduleFire(
                        agentInstanceContext,
                        ScheduleObjectType.outputratelimiting,
                        NAME_AUDITPROVIDER_SCHEDULE);
                    isCallbackScheduled = false;
                    outputCallback.Invoke(true, true);
                    ResetBuiltinProperties();
                    agentInstanceContext.InstrumentationProvider.AOutputRateConditionScheduledEval();
                }
            };
            scheduleHandle = new EPStatementHandleCallbackSchedule(
                agentInstanceContext.EpStatementAgentInstanceHandle,
                callback);
            agentInstanceContext.AuditProvider.ScheduleAdd(
                0,
                agentInstanceContext,
                scheduleHandle,
                ScheduleObjectType.outputratelimiting,
                NAME_AUDITPROVIDER_SCHEDULE);
            agentInstanceContext.StatementContext.SchedulingService.Add(0, scheduleHandle, scheduleSlot);

            // execute assignments
            ExecuteThenAssignments();
        }

        private void ExecuteThenAssignments()
        {
            if (parent.VariableReadWritePackage != null) {
                if (builtinProperties != null) {
                    PopulateBuiltinProps();
                    eventsPerStream[0] = builtinProperties;
                }

                ignoreVariableCallbacks = true;
                try {
                    parent.VariableReadWritePackage.WriteVariables(eventsPerStream, null, agentInstanceContext);
                }
                finally {
                    ignoreVariableCallbacks = false;
                }
            }
        }

        private void ResetBuiltinProperties()
        {
            if (builtinProperties != null) {
                totalNewEventsCount = 0;
                totalOldEventsCount = 0;
                lastOutputTimestamp = agentInstanceContext.StatementContext.SchedulingService.Time;
            }
        }

        private void PopulateBuiltinProps()
        {
            OutputConditionExpressionTypeUtil.Populate(
                builtinProperties.Properties,
                totalNewEventsCount,
                totalOldEventsCount,
                totalNewEventsSum,
                totalOldEventsSum,
                lastOutputTimestamp);
        }
    }
} // end of namespace