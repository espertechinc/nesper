///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.condition;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.compat;


namespace com.espertech.esper.common.@internal.epl.output.polled
{
    /// <summary>
    /// Output condition for output rate limiting that handles when-then expressions for controlling output.
    /// </summary>
    public class OutputConditionPolledExpression : OutputConditionPolled
    {
        private readonly OutputConditionPolledExpressionFactory factory;
        private readonly OutputConditionPolledExpressionState state;
        private readonly ExprEvaluatorContext exprEvaluatorContext;
        private ObjectArrayEventBean builtinProperties;
        private EventBean[] eventsPerStream = new EventBean[1];

        public OutputConditionPolledExpression(
            OutputConditionPolledExpressionFactory factory,
            OutputConditionPolledExpressionState state,
            ExprEvaluatorContext exprEvaluatorContext,
            ObjectArrayEventBean builtinProperties)
        {
            this.factory = factory;
            this.state = state;
            this.builtinProperties = builtinProperties;
            this.exprEvaluatorContext = exprEvaluatorContext;
        }

        public bool UpdateOutputCondition(
            int newEventsCount,
            int oldEventsCount)
        {
            state.TotalNewEventsCount = state.TotalNewEventsCount + newEventsCount;
            state.TotalOldEventsCount = state.TotalOldEventsCount + oldEventsCount;
            state.TotalNewEventsSum = state.TotalNewEventsSum + newEventsCount;
            state.TotalOldEventsSum = state.TotalOldEventsCount + oldEventsCount;
            var isOutput = Evaluate();
            if (isOutput) {
                ResetBuiltinProperties();
                // execute assignments
                if (factory.VariableReadWritePackage != null) {
                    if (builtinProperties != null) {
                        PopulateBuiltinProperties();
                        eventsPerStream[0] = builtinProperties;
                    }

                    try {
                        factory.VariableReadWritePackage.WriteVariables(eventsPerStream, null, exprEvaluatorContext);
                    }
                    finally {
                    }
                }
            }

            return isOutput;
        }

        private void PopulateBuiltinProperties()
        {
            OutputConditionExpressionTypeUtil.Populate(
                builtinProperties.Properties,
                state.TotalNewEventsCount,
                state.TotalOldEventsCount,
                state.TotalNewEventsSum,
                state.TotalOldEventsSum,
                state.LastOutputTimestamp);
        }

        private bool Evaluate()
        {
            if (builtinProperties != null) {
                PopulateBuiltinProperties();
                eventsPerStream[0] = builtinProperties;
            }

            var result = false;
            var output = factory.WhenExpression.Evaluate(eventsPerStream, true, exprEvaluatorContext).AsBoxedBoolean();
            if (output != null && true.Equals(output)) {
                result = true;
            }

            return result;
        }

        private void ResetBuiltinProperties()
        {
            if (builtinProperties != null) {
                state.TotalNewEventsCount = 0;
                state.TotalOldEventsCount = 0;
                state.LastOutputTimestamp = exprEvaluatorContext.TimeProvider.Time;
            }
        }

        public OutputConditionPolledState State => state;
    }
} // end of namespace