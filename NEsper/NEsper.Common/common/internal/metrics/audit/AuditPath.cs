///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.metrics.audit
{
    /// <summary>
    ///     Global boolean for enabling and disable audit path reporting.
    /// </summary>
    public class AuditPath
    {
        /// <summary>
        ///     Logger destination for the query plan logging.
        /// </summary>
        public const string QUERYPLAN_LOG = "com.espertech.esper.queryplan";

        /// <summary>
        ///     Logger destination for the ADO logging.
        /// </summary>
        public const string ADO_LOG = "com.espertech.esper.ado";

        /// <summary>
        ///     Logger destination for the audit logging.
        /// </summary>
        public const string AUDIT_LOG = "com.espertech.esper.audit";

        private static readonly ILog AUDIT_LOG_DESTINATION = LogManager.GetLogger(AUDIT_LOG);

        private static LRUCache<AuditPatternInstanceKey, int> patternInstanceCounts;

        private static volatile AuditCallback auditCallback;

        /// <summary>
        ///     Public access.
        /// </summary>
        public static bool isAuditEnabled = false;

        private static readonly ILockable _lock = new MonitorLock(60000);

        public static bool IsInfoEnabled => AUDIT_LOG_DESTINATION.IsInfoEnabled || auditCallback != null;

        public static AuditCallback AuditCallback {
            get => auditCallback;
            set => auditCallback = value;
        }

        public static string AuditPattern { get; set; }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="theEvent">event</param>
        /// <param name="exprEvaluatorContext">ctx</param>
        public static void AuditInsert(
            EventBean theEvent,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            AuditLog(exprEvaluatorContext, AuditEnum.INSERT, EventBeanSummarizer.Summarize(theEvent));
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="newData">new events</param>
        /// <param name="oldData">old events</param>
        /// <param name="context">context</param>
        /// <param name="viewFactory">view factory</param>
        public static void AuditView(
            EventBean[] newData,
            EventBean[] oldData,
            ExprEvaluatorContext context,
            ViewFactory viewFactory)
        {
            if (IsInfoEnabled) {
                AuditLog(
                    context,
                    AuditEnum.VIEW,
                    viewFactory.ViewName +
                    " insert {" +
                    EventBeanSummarizer.Summarize(newData) +
                    "} remove {" +
                    EventBeanSummarizer.Summarize(oldData) +
                    "}");
            }
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="event">event</param>
        /// <param name="context">context</param>
        /// <param name="filterText">text for filter</param>
        public static void AuditStream(
            EventBean @event,
            ExprEvaluatorContext context,
            string filterText)
        {
            if (IsInfoEnabled) {
                var eventText = EventBeanSummarizer.Summarize(@event);
                AuditLog(context, AuditEnum.STREAM, filterText + " inserted " + eventText);
            }
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="newData">new data</param>
        /// <param name="oldData">old data</param>
        /// <param name="context">context</param>
        /// <param name="filterText">text for filter</param>
        public static void AuditStream(
            EventBean[] newData,
            EventBean[] oldData,
            ExprEvaluatorContext context,
            string filterText)
        {
            if (IsInfoEnabled) {
                var inserted = EventBeanSummarizer.Summarize(newData);
                var removed = EventBeanSummarizer.Summarize(oldData);
                AuditLog(context, AuditEnum.STREAM, filterText + " insert {" + inserted + "} remove {" + removed + "}");
            }
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="nextScheduledTime">time</param>
        /// <param name="agentInstanceContext">ctx</param>
        /// <param name="scheduleHandle">handle</param>
        /// <param name="name">name</param>
        /// <param name="objectType">object type</param>
        public static void AuditScheduleAdd(
            long nextScheduledTime,
            AgentInstanceContext agentInstanceContext,
            ScheduleHandle scheduleHandle,
            ScheduleObjectType objectType,
            string name)
        {
            if (IsInfoEnabled) {
                var message = new StringWriter();
                message.Write("add after ");
                message.Write(Convert.ToString(nextScheduledTime));
                PrintScheduleObjectType(message, objectType, name, scheduleHandle);
                AuditLog(agentInstanceContext, AuditEnum.SCHEDULE, message.ToString());
            }
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="agentInstanceContext">ctx</param>
        /// <param name="scheduleHandle">handle</param>
        /// <param name="name">name</param>
        /// <param name="objectType">object type</param>
        public static void AuditScheduleRemove(
            AgentInstanceContext agentInstanceContext,
            ScheduleHandle scheduleHandle,
            ScheduleObjectType objectType,
            string name)
        {
            if (IsInfoEnabled) {
                var message = new StringWriter();
                message.Write("remove");
                PrintScheduleObjectType(message, objectType, name, scheduleHandle);
                AuditLog(agentInstanceContext, AuditEnum.SCHEDULE, message.ToString());
            }
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="agentInstanceContext">ctx</param>
        /// <param name="objectType">object type</param>
        /// <param name="name">name</param>
        public static void AuditScheduleFire(
            AgentInstanceContext agentInstanceContext,
            ScheduleObjectType objectType,
            string name)
        {
            if (IsInfoEnabled) {
                var message = new StringWriter();
                message.Write("fire");
                PrintScheduleObjectType(message, objectType, name);
                AuditLog(agentInstanceContext, AuditEnum.SCHEDULE, message.ToString());
            }
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="value">value</param>
        /// <param name="exprEvaluatorContext">ctx</param>
        public static void AuditProperty(
            string name,
            object value,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (IsInfoEnabled) {
                var message = new StringWriter();
                message.Write(name);
                message.Write(" value ");
                RenderNonParameterValue(message, value);
                AuditLog(exprEvaluatorContext, AuditEnum.PROPERTY, message.ToString());
            }
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="text">name</param>
        /// <param name="value">value</param>
        /// <param name="exprEvaluatorContext">ctx</param>
        public static void AuditExpression(
            string text,
            object value,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (IsInfoEnabled) {
                var message = new StringWriter();
                message.Write(text);
                message.Write(" value ");
                RenderNonParameterValue(message, value);
                AuditLog(exprEvaluatorContext, AuditEnum.EXPRESSION, message.ToString());
            }
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="factoryNode">factory</param>
        /// <param name="from">from</param>
        /// <param name="matchEvent">state</param>
        /// <param name="isQuitted">quitted-flag</param>
        /// <param name="agentInstanceContext">ctx</param>
        public static void AuditPatternTrue(
            EvalFactoryNode factoryNode,
            object from,
            MatchedEventMapMinimal matchEvent,
            bool isQuitted,
            AgentInstanceContext agentInstanceContext)
        {
            if (IsInfoEnabled) {
                var message = PatternToStringEvaluateTrue(factoryNode, matchEvent, from, isQuitted);
                AuditLog(agentInstanceContext, AuditEnum.PATTERN, message);
            }
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="factoryNode">factory</param>
        /// <param name="from">from</param>
        /// <param name="agentInstanceContext">ctx</param>
        public static void AuditPatternFalse(
            EvalFactoryNode factoryNode,
            object from,
            AgentInstanceContext agentInstanceContext)
        {
            if (IsInfoEnabled) {
                var message = PatternToStringEvaluateFalse(factoryNode, from);
                AuditLog(agentInstanceContext, AuditEnum.PATTERN, message);
            }
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="value">value</param>
        /// <param name="exprEvaluatorContext">ctx</param>
        public static void AuditExprDef(
            string name,
            object value,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (IsInfoEnabled) {
                var message = new StringWriter();
                message.Write(name);
                message.Write(" value ");
                RenderNonParameterValue(message, value);
                AuditLog(exprEvaluatorContext, AuditEnum.EXPRDEF, message.ToString());
            }
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="increase">flag whether plus one or minus one</param>
        /// <param name="factoryNode">factory</param>
        /// <param name="agentInstanceContext">ctx</param>
        public static void AuditPatternInstance(
            bool increase,
            EvalFactoryNode factoryNode,
            AgentInstanceContext agentInstanceContext)
        {
            using (_lock.Acquire()) {
                if (IsInfoEnabled) {
                    if (patternInstanceCounts == null) {
                        patternInstanceCounts = new LRUCache<AuditPatternInstanceKey, int>(100);
                    }

                    var key = new AuditPatternInstanceKey(
                        agentInstanceContext.RuntimeURI,
                        agentInstanceContext.StatementId,
                        agentInstanceContext.AgentInstanceId,
                        factoryNode.TextForAudit);
                    int? existing = patternInstanceCounts.Get(key);
                    int count;
                    if (existing == null) {
                        count = increase ? 1 : 0;
                    }
                    else {
                        count = existing.Value + (increase ? 1 : -1);
                    }

                    var writer = new StringWriter();
                    patternInstanceCounts.Put(key, count);
                    WritePatternExpr(factoryNode, writer);

                    if (increase) {
                        writer.Write(" increased to " + count);
                    }
                    else {
                        writer.Write(" decreased to " + count);
                    }

                    AuditLog(agentInstanceContext, AuditEnum.PATTERNINSTANCES, writer.ToString());
                }
            }
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="dataflowName">name</param>
        /// <param name="dataFlowInstanceId">id</param>
        /// <param name="state">old state</param>
        /// <param name="newState">new state</param>
        /// <param name="agentInstanceContext">ctx</param>
        public static void AuditDataflowTransition(
            string dataflowName,
            string dataFlowInstanceId,
            EPDataFlowState? state,
            EPDataFlowState newState,
            AgentInstanceContext agentInstanceContext)
        {
            if (IsInfoEnabled) {
                var message = new StringWriter();
                WriteDataflow(message, dataflowName, dataFlowInstanceId);
                message.Write(" from state ");
                message.Write(state == null ? "(none)" : state.GetName());
                message.Write(" to state ");
                message.Write(newState.ToString());
                AuditLog(agentInstanceContext, AuditEnum.DATAFLOW_TRANSITION, message.ToString());
            }
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="dataflowName">name</param>
        /// <param name="dataFlowInstanceId">id</param>
        /// <param name="operatorName">name of op</param>
        /// <param name="operatorNumber">num of op</param>
        /// <param name="agentInstanceContext">ctx</param>
        public static void AuditDataflowSource(
            string dataflowName,
            string dataFlowInstanceId,
            string operatorName,
            int operatorNumber,
            AgentInstanceContext agentInstanceContext)
        {
            if (IsInfoEnabled) {
                var message = new StringWriter();
                WriteDataflow(message, dataflowName, dataFlowInstanceId);
                WriteDataflowOp(message, operatorName, operatorNumber);
                message.Write(" invoking source.next()");
                AuditLog(agentInstanceContext, AuditEnum.DATAFLOW_SOURCE, message.ToString());
            }
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="dataflowName">name</param>
        /// <param name="dataFlowInstanceId">id</param>
        /// <param name="operatorName">name of op</param>
        /// <param name="operatorNumber">num of op</param>
        /// <param name="agentInstanceContext">ctx</param>
        /// <param name="params">params</param>
        public static void AuditDataflowOp(
            string dataflowName,
            string dataFlowInstanceId,
            string operatorName,
            int operatorNumber,
            object[] @params,
            AgentInstanceContext agentInstanceContext)
        {
            if (IsInfoEnabled) {
                var message = new StringWriter();
                WriteDataflow(message, dataflowName, dataFlowInstanceId);
                WriteDataflowOp(message, operatorName, operatorNumber);
                message.Write(" parameters ");
                message.Write(@params.RenderAny());
                AuditLog(agentInstanceContext, AuditEnum.DATAFLOW_OP, message.ToString());
            }
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="allocate">allocate</param>
        /// <param name="agentInstanceContext">ctx</param>
        public static void AuditContextPartition(
            bool allocate,
            AgentInstanceContext agentInstanceContext)
        {
            if (IsInfoEnabled) {
                var writer = new StringWriter();
                writer.Write(allocate ? "Allocate" : "Destroy");
                writer.Write(" cpid ");
                writer.Write(Convert.ToString(agentInstanceContext.AgentInstanceId));
                AuditLog(agentInstanceContext, AuditEnum.CONTEXTPARTITION, writer.ToString());
            }
        }

        private static void AuditLog(
            ExprEvaluatorContext ctx,
            AuditEnum category,
            string message)
        {
            if (AuditPattern == null) {
                var text = AuditContext.DefaultFormat(ctx.StatementName, ctx.AgentInstanceId, category, message);
                AUDIT_LOG_DESTINATION.Info(text);
            }
            else {
                var result = AuditPattern
                    .Replace("%s", ctx.StatementName)
                    .Replace("%d", ctx.DeploymentId)
                    .Replace("%u", ctx.RuntimeURI)
                    .Replace("%i", Convert.ToString(ctx.AgentInstanceId))
                    .Replace("%c", category.Value)
                    .Replace("%m", message);
                AUDIT_LOG_DESTINATION.Info(result);
            }

            if (auditCallback != null) {
                auditCallback.Invoke(
                    new AuditContext(
                        ctx.RuntimeURI,
                        ctx.DeploymentId,
                        ctx.StatementName,
                        ctx.AgentInstanceId,
                        category,
                        message));
            }
        }

        private static void PrintScheduleObjectType(
            TextWriter message,
            ScheduleObjectType objectType,
            string name)
        {
            message.Write(" ");
            message.Write(EnumHelper.GetName(objectType));
            message.Write(" '");
            message.Write(name);
            message.Write("'");
        }

        private static void PrintScheduleObjectType(
            TextWriter message,
            ScheduleObjectType objectType,
            string name,
            ScheduleHandle scheduleHandle)
        {
            PrintScheduleObjectType(message, objectType, name);
            message.Write(" handle '");
            PrintHandle(message, scheduleHandle);
            message.Write("'");
        }

        private static void PrintHandle(
            TextWriter message,
            ScheduleHandle handle)
        {
            if (handle is EPStatementHandleCallbackSchedule) {
                var callback = (EPStatementHandleCallbackSchedule) handle;
                TypeHelper.WriteInstance(message, callback.ScheduleCallback, false);
            }
            else {
                TypeHelper.WriteInstance(message, handle, false);
            }
        }

        private static string PatternToStringEvaluateTrue(
            EvalFactoryNode factoryNode,
            MatchedEventMapMinimal matchEvent,
            object fromNode,
            bool isQuitted)
        {
            var writer = new StringWriter();

            WritePatternExpr(factoryNode, writer);
            writer.Write(" evaluate-true {");

            writer.Write(" from: ");
            TypeHelper.WriteInstance(writer, fromNode, false);

            writer.Write(" map: {");
            var delimiter = "";
            var data = matchEvent.MatchingEvents;
            for (var i = 0; i < data.Length; i++) {
                var name = matchEvent.Meta.TagsPerIndex[i];
                var value = data[i];
                writer.Write(delimiter);
                writer.Write(name);
                writer.Write("=");
                if (value is EventBean) {
                    writer.Write(((EventBean) value).Underlying.ToString());
                }
                else if (value is EventBean[]) {
                    writer.Write(EventBeanSummarizer.Summarize((EventBean[]) value));
                }

                delimiter = ", ";
            }

            writer.Write("} quitted: ");
            writer.Write(isQuitted);

            writer.Write("}");
            return writer.ToString();
        }

        private static string PatternToStringEvaluateFalse(
            EvalFactoryNode factoryNode,
            object fromNode)
        {
            var writer = new StringWriter();
            WritePatternExpr(factoryNode, writer);
            writer.Write(" evaluate-false {");

            writer.Write(" from ");
            TypeHelper.WriteInstance(writer, fromNode, false);

            writer.Write("}");
            return writer.ToString();
        }

        private static void RenderNonParameterValue(
            TextWriter message,
            object value)
        {
            CompatExtensions.RenderAny(value, message);
        }

        private static void WriteDataflow(
            TextWriter message,
            string dataflowName,
            string dataFlowInstanceId)
        {
            message.Write("dataflow ");
            message.Write(dataflowName);
            message.Write(" instance ");
            message.Write(dataFlowInstanceId == null ? "(unnamed)" : dataFlowInstanceId);
        }

        private static void WriteDataflowOp(
            TextWriter message,
            string operatorName,
            int operatorNumber)
        {
            message.Write(" operator ");
            message.Write(operatorName);
            message.Write("(");
            message.Write(Convert.ToString(operatorNumber));
            message.Write(")");
        }

        private static void WritePatternExpr(
            EvalFactoryNode factoryNode,
            TextWriter writer)
        {
            if (factoryNode.TextForAudit != null) {
                writer.Write('(');
                writer.Write(factoryNode.TextForAudit);
                writer.Write(')');
            }
            else {
                TypeHelper.WriteInstance(writer, "subexr", factoryNode);
            }
        }
    }
} // end of namespace