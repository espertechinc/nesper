///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.condition;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.bean.service;

namespace com.espertech.esper.common.@internal.epl.output.polled
{
    /// <summary>
    /// Output condition for output rate limiting that handles when-then expressions for controlling output.
    /// </summary>
    public class OutputConditionPolledExpressionFactory : OutputConditionPolledFactory
    {
        private ExprEvaluator _whenExpression;
        private VariableReadWritePackage _variableReadWritePackage;
        private bool _isUsingBuiltinProperties;
        private EventType _builtinPropertiesEventType;

        public ExprEvaluator WhenExpression {
            get => _whenExpression;
            set => _whenExpression = value;
        }

        public VariableReadWritePackage VariableReadWritePackage {
            get => _variableReadWritePackage;
            set => _variableReadWritePackage = value;
        }

        public bool IsUsingBuiltinProperties {
            get => _isUsingBuiltinProperties;
            set => _isUsingBuiltinProperties = value;
        }

        public OutputConditionPolledExpressionFactory SetWhenExpression(ExprEvaluator whenExpression)
        {
            _whenExpression = whenExpression;
            return this;
        }

        public OutputConditionPolledExpressionFactory SetVariableReadWritePackage(VariableReadWritePackage variableReadWritePackage)
        {
            _variableReadWritePackage = variableReadWritePackage;
            return this;
        }

        public OutputConditionPolledExpressionFactory SetUsingBuiltinProperties(bool usingBuiltinProperties)
        {
            _isUsingBuiltinProperties = usingBuiltinProperties;
            return this;
        }

        public OutputConditionPolled MakeFromState(
            ExprEvaluatorContext exprEvaluatorContext,
            OutputConditionPolledState state)
        {
            ObjectArrayEventBean builtinProperties = null;
            if (_isUsingBuiltinProperties) {
                InitType(exprEvaluatorContext);
                builtinProperties = new ObjectArrayEventBean(
                    OutputConditionExpressionTypeUtil.OAPrototype,
                    _builtinPropertiesEventType);
            }

            OutputConditionPolledExpressionState expressionState = (OutputConditionPolledExpressionState) state;
            return new OutputConditionPolledExpression(this, expressionState, exprEvaluatorContext, builtinProperties);
        }

        public OutputConditionPolled MakeNew(ExprEvaluatorContext exprEvaluatorContext)
        {
            ObjectArrayEventBean builtinProperties = null;
            long? lastOutputTimestamp = null;
            if (_isUsingBuiltinProperties) {
                InitType(exprEvaluatorContext);
                builtinProperties = new ObjectArrayEventBean(
                    OutputConditionExpressionTypeUtil.OAPrototype,
                    _builtinPropertiesEventType);
                lastOutputTimestamp = exprEvaluatorContext.TimeProvider.Time;
            }

            OutputConditionPolledExpressionState state =
                new OutputConditionPolledExpressionState(0, 0, 0, 0, lastOutputTimestamp);
            return new OutputConditionPolledExpression(this, state, exprEvaluatorContext, builtinProperties);
        }

        private void InitType(ExprEvaluatorContext exprEvaluatorContext)
        {
            if (_builtinPropertiesEventType == null) {
                _builtinPropertiesEventType = OutputConditionExpressionTypeUtil.GetBuiltInEventType(
                    exprEvaluatorContext.ModuleName,
                    new BeanEventTypeFactoryDisallow(exprEvaluatorContext.EventBeanTypedEventFactory));
            }
        }
    }
} // end of namespace