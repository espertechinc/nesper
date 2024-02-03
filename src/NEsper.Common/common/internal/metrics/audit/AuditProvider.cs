///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    public interface AuditProvider : AuditProviderView,
        AuditProviderStream,
        AuditProviderSchedule,
        AuditProviderProperty,
        AuditProviderInsert,
        AuditProviderExpression,
        AuditProviderPattern,
        AuditProviderPatternInstances,
        AuditProviderExprDef,
        AuditProviderDataflowTransition,
        AuditProviderDataflowSource,
        AuditProviderDataflowOp,
        AuditProviderContextPartition
    {
        bool Activated();
    }

    public class ProxyAuditProvider : AuditProvider
    {
        public delegate bool ActivatedFunc();

        public delegate void ContextPartitionFunc(
            bool allocate,
            AgentInstanceContext agentInstanceContext);

        public delegate void DataflowOpFunc(
            string dataFlowName,
            string instanceId,
            string operatorName,
            int operatorNumber,
            object[] parameters,
            AgentInstanceContext agentInstanceContext);

        public delegate void DataflowSourceFunc(
            string dataFlowName,
            string dataFlowInstanceId,
            string operatorName,
            int operatorNumber,
            AgentInstanceContext agentInstanceContext);

        public delegate void DataflowTransitionFunc(
            string dataflowName,
            string dataFlowInstanceId,
            EPDataFlowState state,
            EPDataFlowState newState,
            AgentInstanceContext agentInstanceContext);

        public delegate void ExprdefFunc(
            string name,
            object value,
            ExprEvaluatorContext exprEvaluatorContext);

        public delegate void ExpressionFunc(
            string text,
            object value,
            ExprEvaluatorContext exprEvaluatorContext);

        public delegate void InsertFunc(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext);

        public delegate void PatternFalseFunc(
            EvalFactoryNode factoryNode,
            object from,
            AgentInstanceContext agentInstanceContext);

        public delegate void PatternInstanceFunc(
            bool increase,
            EvalFactoryNode factoryNode,
            AgentInstanceContext agentInstanceContext);

        public delegate void PatternTrueFunc(
            EvalFactoryNode factoryNode,
            object from,
            MatchedEventMapMinimal matchEvent,
            bool isQuitted,
            AgentInstanceContext agentInstanceContext);

        public delegate void PropertyFunc(
            string name,
            object value,
            ExprEvaluatorContext exprEvaluatorContext);

        public delegate void ScheduleAddFunc(
            long nextScheduledTime,
            AgentInstanceContext agentInstanceContext,
            ScheduleHandle scheduleHandle,
            ScheduleObjectType objectType,
            string name);

        public delegate void ScheduleFireFunc(
            AgentInstanceContext agentInstanceContext,
            ScheduleObjectType objectType,
            string name);

        public delegate void ScheduleRemoveFunc(
            AgentInstanceContext agentInstanceContext,
            ScheduleHandle scheduleHandle,
            ScheduleObjectType objectType,
            string name);

        public delegate void StreamMultiFunc(
            EventBean[] newData,
            EventBean[] oldData,
            ExprEvaluatorContext context,
            string filterSpecText);

        public delegate void StreamSingleFunc(
            EventBean @event,
            ExprEvaluatorContext context,
            string filterSpecText);

        public delegate void ViewFunc(
            EventBean[] newData,
            EventBean[] oldData,
            AgentInstanceContext agentInstanceContext,
            ViewFactory viewFactory);

        public ViewFunc ProcView { get; set; }
        public StreamSingleFunc ProcStreamSingle { get; set; }
        public StreamMultiFunc ProcStreamMulti { get; set; }
        public ScheduleAddFunc ProcScheduleAdd { get; set; }
        public ScheduleRemoveFunc ProcScheduleRemove { get; set; }
        public ScheduleFireFunc ProcScheduleFire { get; set; }
        public PropertyFunc ProcProperty { get; set; }
        public InsertFunc ProcInsert { get; set; }
        public ExpressionFunc ProcExpression { get; set; }
        public PatternTrueFunc ProcPatternTrue { get; set; }
        public PatternFalseFunc ProcPatternFalse { get; set; }
        public PatternInstanceFunc ProcPatternInstance { get; set; }
        public ExprdefFunc ProcExprdef { get; set; }
        public DataflowTransitionFunc ProcDataflowTransition { get; set; }
        public DataflowSourceFunc ProcDataflowSource { get; set; }
        public DataflowOpFunc ProcDataflowOp { get; set; }
        public ContextPartitionFunc ProcContextPartition { get; set; }
        public ActivatedFunc ProcActivated { get; set; }

        public void View(
            EventBean[] newData,
            EventBean[] oldData,
            AgentInstanceContext agentInstanceContext,
            ViewFactory viewFactory)
        {
            ProcView?.Invoke(
                newData,
                oldData,
                agentInstanceContext,
                viewFactory);
        }

        public void StreamSingle(
            EventBean @event,
            ExprEvaluatorContext context,
            string filterSpecText)
        {
            ProcStreamSingle?.Invoke(
                @event,
                context,
                filterSpecText);
        }

        public void StreamMulti(
            EventBean[] newData,
            EventBean[] oldData,
            ExprEvaluatorContext context,
            string filterSpecText)
        {
            ProcStreamMulti?.Invoke(
                newData,
                oldData,
                context,
                filterSpecText);
        }

        public void ScheduleAdd(
            long nextScheduledTime,
            AgentInstanceContext agentInstanceContext,
            ScheduleHandle scheduleHandle,
            ScheduleObjectType objectType,
            string name)
        {
            ProcScheduleAdd?.Invoke(
                nextScheduledTime,
                agentInstanceContext,
                scheduleHandle,
                objectType,
                name);
        }

        public void ScheduleRemove(
            AgentInstanceContext agentInstanceContext,
            ScheduleHandle scheduleHandle,
            ScheduleObjectType objectType,
            string name)
        {
            ProcScheduleRemove?.Invoke(
                agentInstanceContext,
                scheduleHandle,
                objectType,
                name);
        }

        public void ScheduleFire(
            AgentInstanceContext agentInstanceContext,
            ScheduleObjectType objectType,
            string name)
        {
            ProcScheduleFire?.Invoke(
                agentInstanceContext,
                objectType,
                name);
        }

        public void Property(
            string name,
            object value,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            ProcProperty?.Invoke(
                name,
                value,
                exprEvaluatorContext);
        }

        public void Insert(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            ProcInsert?.Invoke(
                @event,
                exprEvaluatorContext);
        }

        public void Expression(
            string text,
            object value,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            ProcExpression?.Invoke(
                text,
                value,
                exprEvaluatorContext);
        }

        public void PatternTrue(
            EvalFactoryNode factoryNode,
            object from,
            MatchedEventMapMinimal matchEvent,
            bool isQuitted,
            AgentInstanceContext agentInstanceContext)
        {
            ProcPatternTrue?.Invoke(
                factoryNode,
                from,
                matchEvent,
                isQuitted,
                agentInstanceContext);
        }

        public void PatternFalse(
            EvalFactoryNode factoryNode,
            object from,
            AgentInstanceContext agentInstanceContext)
        {
            ProcPatternFalse?.Invoke(
                factoryNode,
                from,
                agentInstanceContext);
        }

        public void PatternInstance(
            bool increase,
            EvalFactoryNode factoryNode,
            AgentInstanceContext agentInstanceContext)
        {
            ProcPatternInstance?.Invoke(
                increase,
                factoryNode,
                agentInstanceContext);
        }

        public void Exprdef(
            string name,
            object value,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            ProcExprdef?.Invoke(
                name,
                value,
                exprEvaluatorContext);
        }

        public void DataflowTransition(
            string dataflowName,
            string dataFlowInstanceId,
            EPDataFlowState state,
            EPDataFlowState newState,
            AgentInstanceContext agentInstanceContext)
        {
            ProcDataflowTransition?.Invoke(
                dataflowName,
                dataFlowInstanceId,
                state,
                newState,
                agentInstanceContext);
        }

        public void DataflowSource(
            string dataFlowName,
            string dataFlowInstanceId,
            string operatorName,
            int operatorNumber,
            AgentInstanceContext agentInstanceContext)
        {
            ProcDataflowSource?.Invoke(
                dataFlowName,
                dataFlowInstanceId,
                operatorName,
                operatorNumber,
                agentInstanceContext);
        }

        public void DataflowOp(
            string dataFlowName,
            string instanceId,
            string operatorName,
            int operatorNumber,
            object[] parameters,
            AgentInstanceContext agentInstanceContext)
        {
            ProcDataflowOp?.Invoke(
                dataFlowName,
                instanceId,
                operatorName,
                operatorNumber,
                parameters,
                agentInstanceContext);
        }

        public void ContextPartition(
            bool allocate,
            AgentInstanceContext agentInstanceContext)
        {
            ProcContextPartition?.Invoke(allocate, agentInstanceContext);
        }

        public bool Activated()
        {
            return ProcActivated.Invoke();
        }
    }
} // end of namespace