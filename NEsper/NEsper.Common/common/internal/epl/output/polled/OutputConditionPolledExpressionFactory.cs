///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.condition;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.output.polled
{
    /// <summary>
    /// Output condition for output rate limiting that handles when-then expressions for controlling output.
    /// </summary>
    public class OutputConditionPolledExpressionFactory : OutputConditionPolledFactory
    {
        private ExprEvaluator whenExpression;
        private VariableReadWritePackage variableReadWritePackage;
        private bool isUsingBuiltinProperties;
        private EventType builtinPropertiesEventType;

        public ExprEvaluator WhenExpression {
            get => whenExpression;
            set => whenExpression = value;
        }

        public VariableReadWritePackage VariableReadWritePackage {
            get => variableReadWritePackage;
            set => variableReadWritePackage = value;
        }

        public bool IsUsingBuiltinProperties {
            get => isUsingBuiltinProperties;
            set => isUsingBuiltinProperties = value;
        }

        public OutputConditionPolledExpressionFactory SetWhenExpression(ExprEvaluator whenExpression)
        {
            this.whenExpression = whenExpression;
            return this;
        }

        public OutputConditionPolledExpressionFactory SetVariableReadWritePackage(VariableReadWritePackage variableReadWritePackage)
        {
            this.variableReadWritePackage = variableReadWritePackage;
            return this;
        }

        public OutputConditionPolledExpressionFactory SetUsingBuiltinProperties(bool usingBuiltinProperties)
        {
            isUsingBuiltinProperties = usingBuiltinProperties;
            return this;
        }

        public OutputConditionPolled MakeFromState(
            AgentInstanceContext agentInstanceContext,
            OutputConditionPolledState state)
        {
            ObjectArrayEventBean builtinProperties = null;
            if (isUsingBuiltinProperties) {
                InitType(agentInstanceContext);
                builtinProperties = new ObjectArrayEventBean(
                    OutputConditionExpressionTypeUtil.OAPrototype,
                    builtinPropertiesEventType);
            }

            OutputConditionPolledExpressionState expressionState = (OutputConditionPolledExpressionState) state;
            return new OutputConditionPolledExpression(this, expressionState, agentInstanceContext, builtinProperties);
        }

        public OutputConditionPolled MakeNew(AgentInstanceContext agentInstanceContext)
        {
            ObjectArrayEventBean builtinProperties = null;
            long? lastOutputTimestamp = null;
            if (isUsingBuiltinProperties) {
                InitType(agentInstanceContext);
                builtinProperties = new ObjectArrayEventBean(
                    OutputConditionExpressionTypeUtil.OAPrototype,
                    builtinPropertiesEventType);
                lastOutputTimestamp = agentInstanceContext.StatementContext.SchedulingService.Time;
            }

            OutputConditionPolledExpressionState state =
                new OutputConditionPolledExpressionState(0, 0, 0, 0, lastOutputTimestamp);
            return new OutputConditionPolledExpression(this, state, agentInstanceContext, builtinProperties);
        }

        private void InitType(AgentInstanceContext agentInstanceContext)
        {
            if (builtinPropertiesEventType == null) {
                builtinPropertiesEventType = OutputConditionExpressionTypeUtil.GetBuiltInEventType(
                    agentInstanceContext.ModuleName,
                    new BeanEventTypeFactoryDisallow(agentInstanceContext.EventBeanTypedEventFactory));
            }
        }
    }
} // end of namespace