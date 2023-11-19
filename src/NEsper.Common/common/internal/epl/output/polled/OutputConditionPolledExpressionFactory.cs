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
        private EventType _builtinPropertiesEventType;

        public OutputConditionPolled MakeFromState(
            ExprEvaluatorContext exprEvaluatorContext,
            OutputConditionPolledState state)
        {
            ObjectArrayEventBean builtinProperties = null;
            if (IsUsingBuiltinProperties) {
                InitType(exprEvaluatorContext);
                builtinProperties = new ObjectArrayEventBean(
                    OutputConditionExpressionTypeUtil.OAPrototype,
                    _builtinPropertiesEventType);
            }

            var expressionState = (OutputConditionPolledExpressionState)state;
            return new OutputConditionPolledExpression(this, expressionState, exprEvaluatorContext, builtinProperties);
        }

        public OutputConditionPolled MakeNew(ExprEvaluatorContext exprEvaluatorContext)
        {
            ObjectArrayEventBean builtinProperties = null;
            long? lastOutputTimestamp = null;
            if (IsUsingBuiltinProperties) {
                InitType(exprEvaluatorContext);
                builtinProperties = new ObjectArrayEventBean(
                    OutputConditionExpressionTypeUtil.OAPrototype,
                    _builtinPropertiesEventType);
                lastOutputTimestamp = exprEvaluatorContext.TimeProvider.Time;
            }

            var state = new OutputConditionPolledExpressionState(0, 0, 0, 0, lastOutputTimestamp);
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

        public ExprEvaluator WhenExpression { get; set; }

        public VariableReadWritePackage VariableReadWritePackage { get; set; }

        public bool IsUsingBuiltinProperties { get; set; }
    }
} // end of namespace