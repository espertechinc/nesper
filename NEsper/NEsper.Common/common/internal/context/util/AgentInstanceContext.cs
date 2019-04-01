///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.client.render;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.dataflow.filtersvcadapter;
using com.espertech.esper.common.@internal.epl.enummethod.cache;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.database.connection;
using com.espertech.esper.common.@internal.epl.historical.datacache;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.rowrecog.state;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.metrics.stmtmetrics;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.statement.resource;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.util
{
    public class AgentInstanceContext : ExprEvaluatorContext
    {
        private readonly MappedEventBean contextProperties;
        private AgentInstanceScriptContext agentInstanceScriptContext;
        private StatementContextCPPair statementContextCPPair;
        private object terminationCallbacks;

        public AgentInstanceContext(
            StatementContext statementContext, int agentInstanceId,
            EPStatementAgentInstanceHandle epStatementAgentInstanceHandle,
            AgentInstanceFilterProxy agentInstanceFilterProxy, MappedEventBean contextProperties,
            AuditProvider auditProvider, InstrumentationCommon instrumentationProvider)
        {
            StatementContext = statementContext;
            FilterVersionAfterAllocation = statementContext.FilterService.FiltersVersion;
            AgentInstanceId = agentInstanceId;
            EpStatementAgentInstanceHandle = epStatementAgentInstanceHandle;
            AgentInstanceFilterProxy = agentInstanceFilterProxy;
            this.contextProperties = contextProperties;
            AuditProvider = auditProvider;
            InstrumentationProvider = instrumentationProvider;
        }

        public AgentInstanceFilterProxy AgentInstanceFilterProxy { get; }

        public Attribute[] Annotations => StatementContext.Annotations;

        public ContextManagementService ContextManagementService => StatementContext.ContextManagementService;

        public ContextServiceFactory ContextServiceFactory => StatementContext.ContextServiceFactory;

        public EPStatementAgentInstanceHandle EpStatementAgentInstanceHandle { get; }

        public RuntimeExtensionServices RuntimeExtensionServicesContext => StatementContext.RuntimeExtensionServices;

        public EventBeanTypedEventFactory EventBeanTypedEventFactory => StatementContext.EventBeanTypedEventFactory;

        public RuntimeSettingsService RuntimeSettingsService => StatementContext.RuntimeSettingsService;

        public ImportServiceRuntime ImportServiceRuntime =>
            StatementContext.ImportServiceRuntime;

        public FilterService FilterService => StatementContext.FilterService;

        public InternalEventRouter InternalEventRouter => StatementContext.InternalEventRouter;

        public InternalEventRouteDest InternalEventRouteDest => StatementContext.InternalEventRouteDest;

        public SchedulingService SchedulingService => StatementContext.SchedulingService;

        public ScheduleBucket ScheduleBucket => StatementContext.ScheduleBucket;

        public StatementContext StatementContext { get; }

        public StatementResultService StatementResultService => StatementContext.StatementResultService;

        public ViewFactoryService ViewFactoryService => StatementContext.ViewFactoryService;

        public ResultSetProcessorHelperFactory ResultSetProcessorHelperFactory =>
            StatementContext.ResultSetProcessorHelperFactory;

        public NamedWindowManagementService NamedWindowManagementService =>
            StatementContext.NamedWindowManagementService;

        public StatementResourceService StatementResourceService => StatementContext.StatementResourceService;

        public ExceptionHandlingService ExceptionHandlingService => StatementContext.ExceptionHandlingService;

        public VariableManagementService VariableManagementService => StatementContext.VariableManagementService;

        public StatementContextFilterEvalEnv StatementContextFilterEvalEnv =>
            StatementContext.StatementContextFilterEvalEnv;

        public TableManagementService TableManagementService => StatementContext.TableManagementService;

        public EventTypeResolvingBeanFactory EventTypeResolvingBeanFactory =>
            StatementContext.EventTypeResolvingBeanFactory;

        public EventTypeAvroHandler EventTypeAvroHandler => StatementContext.EventTypeAvroHandler;

        public RowRecogStateRepoFactory RowRecogStateRepoFactory => StatementContext.RowRecogStateRepoFactory;

        public EventTableIndexService EventTableIndexService => StatementContext.EventTableIndexService;

        public HistoricalDataCacheFactory HistoricalDataCacheFactory => StatementContext.HistoricalDataCacheFactory;

        public DatabaseConfigServiceRuntime DatabaseConfigService => StatementContext.DatabaseConfigService;

        public EPRuntimeEventProcessWrapped EPRuntimeEventProcessWrapped =>
            StatementContext.EPRuntimeEventProcessWrapped;

        public EventServiceSendEventCommon EPRuntimeSendEvent => StatementContext.EPRuntimeSendEvent;

        public EPRenderEventService EPRuntimeRenderEvent => StatementContext.EPRuntimeRenderEvent;

        public DataFlowFilterServiceAdapter DataFlowFilterServiceAdapter =>
            StatementContext.DataFlowFilterServiceAdapter;

        public object Runtime => StatementContext.Runtime;

        public MetricReportingService MetricReportingService => StatementContext.MetricReportingService;

        //public Context RuntimeEnvContext => StatementContext.RuntimeEnvContext;

        public long FilterVersionAfterAllocation { get; }

        public string ModuleName => StatementContext.ModuleName;

        public StatementContextCPPair StatementContextCPPair {
            get {
                if (statementContextCPPair == null) {
                    statementContextCPPair = new StatementContextCPPair(
                        StatementContext.StatementId, AgentInstanceId, StatementContext);
                }

                return statementContextCPPair;
            }
        }

        public ICollection<AgentInstanceStopCallback> TerminationCallbackRO {
            get {
                if (terminationCallbacks == null) {
                    return Collections.GetEmptyList<AgentInstanceStopCallback>();
                }

                if (terminationCallbacks is ICollection<AgentInstanceStopCallback>) {
                    return (ICollection<AgentInstanceStopCallback>) terminationCallbacks;
                }

                return Collections.SingletonList((AgentInstanceStopCallback) terminationCallbacks);
            }
        }

        public EventBean ContextProperties => contextProperties;

        public string RuntimeURI => StatementContext.RuntimeURI;

        public int AgentInstanceId { get; }

        public StatementAgentInstanceLock AgentInstanceLock =>
            EpStatementAgentInstanceHandle.StatementAgentInstanceLock;

        public EventBeanService EventBeanService => StatementContext.EventBeanService;

        public string StatementName => StatementContext.StatementName;

        public object UserObjectCompileTime => StatementContext.UserObjectCompileTime;

        public int StatementId => StatementContext.StatementId;

        public TimeProvider TimeProvider => StatementContext.TimeProvider;

        public string DeploymentId => StatementContext.DeploymentId;

        public ExpressionResultCacheService ExpressionResultCacheService =>
            StatementContext.ExpressionResultCacheServiceSharable;

        public TableExprEvaluatorContext TableExprEvaluatorContext => StatementContext.TableExprEvaluatorContext;

        public AuditProvider AuditProvider { get; }

        public InstrumentationCommon InstrumentationProvider { get; }

        public AgentInstanceScriptContext AllocateAgentInstanceScriptContext {
            get {
                if (agentInstanceScriptContext == null) {
                    agentInstanceScriptContext = AgentInstanceScriptContext.From(StatementContext);
                }

                return agentInstanceScriptContext;
            }
        }

        /// <summary>
        ///     Add a stop-callback.
        ///     Use to add a stop-callback other than already registered.
        ///     This is generally not required by views that implement AgentInstanceStopCallback as
        ///     they gets stopped as part of normal processing.
        /// </summary>
        /// <param name="callback">to add</param>
        public void AddTerminationCallback(AgentInstanceStopCallback callback)
        {
            if (terminationCallbacks == null) {
                terminationCallbacks = callback;
            }
            else if (terminationCallbacks is ICollection<AgentInstanceStopCallback>) {
                ((ICollection<AgentInstanceStopCallback>) terminationCallbacks).Add(callback);
            }
            else {
                var cb = (AgentInstanceStopCallback) terminationCallbacks;
                var q = new HashSet<AgentInstanceStopCallback>();
                q.Add(cb);
                q.Add(callback);
                terminationCallbacks = q;
            }
        }

        public void RemoveTerminationCallback(AgentInstanceStopCallback callback)
        {
            if (terminationCallbacks == null) {
            }
            else if (terminationCallbacks is ICollection<AgentInstanceStopCallback>) {
                ((ICollection<AgentInstanceStopCallback>) terminationCallbacks).Remove(callback);
            }
            else if (terminationCallbacks == callback) {
                terminationCallbacks = null;
            }
        }
    }
} // end of namespace