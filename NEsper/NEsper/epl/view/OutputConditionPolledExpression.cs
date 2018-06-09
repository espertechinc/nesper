///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    /// Output condition for output rate limiting that handles when-then expressions for controlling output.
    /// </summary>
    public class OutputConditionPolledExpression : OutputConditionPolled
    {
        private readonly OutputConditionPolledExpressionFactory _factory;
        private readonly OutputConditionPolledExpressionState _state;
        private readonly AgentInstanceContext _agentInstanceContext;

        private readonly ObjectArrayEventBean _builtinProperties;
        private readonly EventBean[] _eventsPerStream = new EventBean[1];

        public OutputConditionPolledExpression(
            OutputConditionPolledExpressionFactory factory,
            OutputConditionPolledExpressionState state,
            AgentInstanceContext agentInstanceContext,
            ObjectArrayEventBean builtinProperties)
        {
            _factory = factory;
            _state = state;
            _builtinProperties = builtinProperties;
            _agentInstanceContext = agentInstanceContext;
        }

        public OutputConditionPolledState State => _state;

        public virtual bool UpdateOutputCondition(int newEventsCount, int oldEventsCount)
        {
            _state.TotalNewEventsCount = _state.TotalNewEventsCount + newEventsCount;
            _state.TotalOldEventsCount = _state.TotalOldEventsCount + oldEventsCount;
            _state.TotalNewEventsSum = _state.TotalNewEventsSum + newEventsCount;
            _state.TotalOldEventsSum = _state.TotalOldEventsCount + oldEventsCount;

            bool isOutput = Evaluate();
            if (isOutput)
            {
                ResetBuiltinProperties();

                // execute assignments
                if (_factory.VariableReadWritePackage != null)
                {
                    if (_builtinProperties != null)
                    {
                        PopulateBuiltinProperties();
                        _eventsPerStream[0] = _builtinProperties;
                    }

                    _factory.VariableReadWritePackage.WriteVariables(_agentInstanceContext.StatementContext.VariableService, _eventsPerStream, null, _agentInstanceContext);
                }
            }
            return isOutput;
        }

        private void PopulateBuiltinProperties()
        {
            OutputConditionExpressionTypeUtil.Populate(_builtinProperties.Properties, _state.TotalNewEventsCount,
                    _state.TotalOldEventsCount, _state.TotalNewEventsSum,
                    _state.TotalOldEventsSum, _state.LastOutputTimestamp);
        }

        private bool Evaluate()
        {
            if (_builtinProperties != null)
            {
                PopulateBuiltinProperties();
                _eventsPerStream[0] = _builtinProperties;
            }

            var result = false;
            var output = _factory.WhenExpressionNode.Evaluate(new EvaluateParams(_eventsPerStream, true, _agentInstanceContext));
            //if ((output != null) && (true.Equals(output))) {
            if (true.Equals(output))
            {
                result = true;
            }

            return result;
        }

        private void ResetBuiltinProperties()
        {
            if (_builtinProperties != null)
            {
                _state.TotalNewEventsCount = 0;
                _state.TotalOldEventsCount = 0;
                _state.LastOutputTimestamp = _agentInstanceContext.StatementContext.SchedulingService.Time;
            }
        }
    }
} // end of namespace
