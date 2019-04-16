///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.bean.service;

namespace com.espertech.esper.common.@internal.epl.output.condition
{
    /// <summary>
    ///     Output condition for output rate limiting that handles when-then expressions for controlling output.
    /// </summary>
    public class OutputConditionExpressionFactory : OutputConditionFactory,
        StatementReadyCallback
    {
        private bool isUsingBuiltinProperties;
        protected int scheduleCallbackId = -1;

        public ExprEvaluator WhenExpressionNodeEval { get; set; }

        public ExprEvaluator WhenTerminatedExpressionNodeEval { get; set; }

        public bool IsStartConditionOnCreation { get; set; }

        public EventType BuiltinPropertiesEventType { get; set; }

        public VariableReadWritePackage VariableReadWritePackage { get; set; }

        public VariableReadWritePackage VariableReadWritePackageAfterTerminated { get; set; }

        public Variable[] Variables { get; set; }

        public int ScheduleCallbackId => scheduleCallbackId;

        public OutputCondition InstantiateOutputCondition(
            AgentInstanceContext agentInstanceContext,
            OutputCallback outputCallback)
        {
            return new OutputConditionExpression(outputCallback, agentInstanceContext, this);
        }

        public void Ready(
            StatementContext statementContext,
            ModuleIncidentals moduleIncidentals,
            bool recovery)
        {
            if (isUsingBuiltinProperties) {
                BuiltinPropertiesEventType = OutputConditionExpressionTypeUtil.GetBuiltInEventType(
                    statementContext.ModuleName,
                    new BeanEventTypeFactoryDisallow(statementContext.EventBeanTypedEventFactory));
            }
        }
    }
} // end of namespace