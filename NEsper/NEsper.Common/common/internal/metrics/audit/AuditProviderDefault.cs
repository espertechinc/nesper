///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.metrics.audit
{
    public class AuditProviderDefault : AuditProvider
    {
        public static readonly AuditProviderDefault INSTANCE = new AuditProviderDefault();

        private AuditProviderDefault()
        {
        }

        public bool Activated()
        {
            return false;
        }

        public void View(
            EventBean[] newData, EventBean[] oldData, AgentInstanceContext agentInstanceContext,
            ViewFactory viewFactory)
        {
        }

        public void Stream(EventBean @event, ExprEvaluatorContext context, string filterSpecText)
        {
        }

        public void Stream(
            EventBean[] newData, EventBean[] oldData, ExprEvaluatorContext context, string filterSpecText)
        {
        }

        public void ScheduleAdd(
            long nextScheduledTime, AgentInstanceContext agentInstanceContext, ScheduleHandle scheduleHandle,
            ScheduleObjectType type, string name)
        {
        }

        public void ScheduleRemove(
            AgentInstanceContext agentInstanceContext, ScheduleHandle scheduleHandle, ScheduleObjectType type,
            string name)
        {
        }

        public void ScheduleFire(AgentInstanceContext agentInstanceContext, ScheduleObjectType type, string name)
        {
        }

        public void Property(string name, object value, ExprEvaluatorContext exprEvaluatorContext)
        {
        }

        public void PatternInstance(
            bool increase, EvalFactoryNode factoryNode, AgentInstanceContext agentInstanceContext)
        {
        }

        public void DataflowTransition(
            string dataflowName, string dataFlowInstanceId, EPDataFlowState state, EPDataFlowState newState,
            AgentInstanceContext agentInstanceContext)
        {
        }

        public void DataflowSource(
            string dataFlowName, string dataFlowInstanceId, string operatorName, int operatorNumber,
            AgentInstanceContext agentInstanceContext)
        {
        }

        public void DataflowOp(
            string dataFlowName, string instanceId, string operatorName, int operatorNumber, object[] parameters,
            AgentInstanceContext agentInstanceContext)
        {
        }

        public void ContextPartition(bool allocate, AgentInstanceContext agentInstanceContext)
        {
        }

        public void Insert(EventBean @event, ExprEvaluatorContext exprEvaluatorContext)
        {
        }

        public void Expression(string text, object value, ExprEvaluatorContext exprEvaluatorContext)
        {
        }

        public void PatternTrue(
            EvalFactoryNode factoryNode, object from, MatchedEventMapMinimal matchEvent, bool isQuitted,
            AgentInstanceContext agentInstanceContext)
        {
        }

        public void PatternFalse(EvalFactoryNode factoryNode, object from, AgentInstanceContext agentInstanceContext)
        {
        }

        public void Exprdef(string name, object value, ExprEvaluatorContext exprEvaluatorContext)
        {
        }
    }
} // end of namespace