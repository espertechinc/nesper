///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events.arr;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    /// Output condition for output rate limiting that handles when-then expressions for controlling output.
    /// </summary>
    public class OutputConditionExpression
        : OutputConditionBase
        , OutputCondition
    {
        private static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly OutputConditionExpressionFactory _parent;

        private readonly ScheduleSlot _scheduleSlot;
        private bool _isCallbackScheduled;
        private bool _ignoreVariableCallbacks;
        private readonly ObjectArrayEventBean _builtinProperties;
        private readonly EventBean[] _eventsPerStream;

        // ongoing builtin properties
        private int _totalNewEventsCount;
        private int _totalOldEventsCount;
        private int _totalNewEventsSum;
        private int _totalOldEventsSum;
        private long? _lastOutputTimestamp;
        private EPStatementHandleCallback _scheduleHandle;

        public OutputConditionExpression(
            OutputCallback outputCallback,
            AgentInstanceContext agentInstanceContext,
            OutputConditionExpressionFactory parent,
            bool isStartConditionOnCreation)
            : base(outputCallback)
        {
            _agentInstanceContext = agentInstanceContext;
            _parent = parent;

            _scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();
            _eventsPerStream = new EventBean[1];

            if (parent.BuiltinPropertiesEventType != null)
            {
                _builtinProperties = new ObjectArrayEventBean(
                    OutputConditionExpressionTypeUtil.OAPrototype, parent.BuiltinPropertiesEventType);
                _lastOutputTimestamp = agentInstanceContext.StatementContext.SchedulingService.Time;
            }

            if (parent.VariableNames != null)
            {
                // if using variables, register a callback on the change of the variable
                foreach (String variableName in parent.VariableNames)
                {
                    var theVariableName = variableName;
                    agentInstanceContext.StatementContext.VariableService.RegisterCallback(
                        variableName, agentInstanceContext.AgentInstanceId, Update);
                    agentInstanceContext.AddTerminationCallback(
                        new ProxyStopCallback(
                            () => _agentInstanceContext.StatementContext.VariableService.UnregisterCallback(
                                theVariableName, agentInstanceContext.AgentInstanceId, Update)));
                }
            }

            if (isStartConditionOnCreation)
            {
                Update(0, 0);
            }
        }

        public override void UpdateOutputCondition(int newEventsCount, int oldEventsCount)
        {
            _totalNewEventsCount += newEventsCount;
            _totalOldEventsCount += oldEventsCount;
            _totalNewEventsSum += newEventsCount;
            _totalOldEventsSum += oldEventsCount;

            bool isOutput = Evaluate(_parent.WhenExpressionNodeEval);
            if (isOutput)
            {
                ExecuteThenAssignments();
                OutputCallback.Invoke(true, true);
                ResetBuiltinProperties();
            }
        }

        public void Update(Object newValue, Object oldValue)
        {
            if (_ignoreVariableCallbacks)
            {
                Log.Debug(".Update Ignoring variable callback");
                return;
            }

            _agentInstanceContext.StatementContext.VariableService.SetLocalVersion();
            bool isOutput = Evaluate(_parent.WhenExpressionNodeEval);
            if ((isOutput) && (!_isCallbackScheduled))
            {
                ScheduleCallback();
            }
        }

        public override void Stop()
        {
            if (_scheduleHandle != null)
            {
                _agentInstanceContext.StatementContext.SchedulingService.Remove(_scheduleHandle, _scheduleSlot);
                _scheduleHandle = null;
            }
        }

        public override void Terminated()
        {
            bool output = true;
            if (_parent.AndWhenTerminatedExpressionNodeEval != null)
            {
                output = Evaluate(_parent.AndWhenTerminatedExpressionNodeEval);
            }
            if (_parent.VariableReadWritePackageAfterTerminated != null)
            {
                if (_builtinProperties != null)
                {
                    PopulateBuiltinProps();
                    _eventsPerStream[0] = _builtinProperties;
                }

                _ignoreVariableCallbacks = true;
                try
                {
                    _parent.VariableReadWritePackageAfterTerminated.WriteVariables(
                        _agentInstanceContext.StatementContext.VariableService, _eventsPerStream, null,
                        _agentInstanceContext);
                }
                finally
                {
                    _ignoreVariableCallbacks = false;
                }
            }
            if (output)
            {
                base.Terminated();
            }
        }

        private bool Evaluate(ExprEvaluator evaluator)
        {
            if (_builtinProperties != null)
            {
                PopulateBuiltinProps();
                _eventsPerStream[0] = _builtinProperties;
            }

            var result = false;
            var output = (bool?) evaluator.Evaluate(new EvaluateParams(_eventsPerStream, true, _agentInstanceContext));
            if ((output != null) && (output.Value))
            {
                result = true;
            }

            return result;
        }

        private void ScheduleCallback()
        {
            _isCallbackScheduled = true;
            long current = _agentInstanceContext.StatementContext.SchedulingService.Time;

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(
                    ".scheduleCallback Scheduled new callback for " +
                    " afterMsec=" + 0 +
                    " now=" + current);
            }

            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback
            {
                ProcScheduledTrigger = extensionServicesContext => Instrument.With(
                    i => i.QOutputRateConditionScheduledEval(),
                    i => i.AOutputRateConditionScheduledEval(),
                    () =>
                    {
                        _isCallbackScheduled = false;
                        OutputCallback.Invoke(true, true);
                        ResetBuiltinProperties();
                    })
            };
            _scheduleHandle = new EPStatementHandleCallback(
                _agentInstanceContext.EpStatementAgentInstanceHandle, callback);
            _agentInstanceContext.StatementContext.SchedulingService.Add(0, _scheduleHandle, _scheduleSlot);
            _agentInstanceContext.AddTerminationCallback(new ProxyStopCallback(Stop));

            // execute assignments
            ExecuteThenAssignments();
        }

        private void ExecuteThenAssignments()
        {
            if (_parent.VariableReadWritePackage != null)
            {
                if (_builtinProperties != null)
                {
                    PopulateBuiltinProps();
                    _eventsPerStream[0] = _builtinProperties;
                }

                _ignoreVariableCallbacks = true;
                try
                {
                    _parent.VariableReadWritePackage.WriteVariables(
                        _agentInstanceContext.StatementContext.VariableService, _eventsPerStream, null,
                        _agentInstanceContext);
                }
                finally
                {
                    _ignoreVariableCallbacks = false;
                }
            }
        }

        private void ResetBuiltinProperties()
        {
            if (_builtinProperties != null)
            {
                _totalNewEventsCount = 0;
                _totalOldEventsCount = 0;
                _lastOutputTimestamp = _agentInstanceContext.StatementContext.SchedulingService.Time;
            }
        }

        private void PopulateBuiltinProps()
        {
            OutputConditionExpressionTypeUtil.Populate(
                _builtinProperties.Properties, 
                _totalNewEventsCount, 
                _totalOldEventsCount, 
                _totalNewEventsSum, 
                _totalOldEventsSum, 
                _lastOutputTimestamp);
        }
    }
}
