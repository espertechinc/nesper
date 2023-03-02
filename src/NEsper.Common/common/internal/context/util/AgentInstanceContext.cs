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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.directory;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.context.util
{
    public class AgentInstanceContext : ExprEvaluatorContext
    {
        private readonly MappedEventBean _contextProperties;
        private AgentInstanceScriptContext _agentInstanceScriptContext;
        private StatementContextCPPair _statementContextCpPair;
        private object _terminationCallbacks;

        public AgentInstanceContext(
            StatementContext statementContext,
            EPStatementAgentInstanceHandle epStatementAgentInstanceHandle,
            AgentInstanceFilterProxy agentInstanceFilterProxy,
            MappedEventBean contextProperties,
            AuditProvider auditProvider,
            InstrumentationCommon instrumentationProvider)
        {
            StatementContext = statementContext;
            FilterVersionAfterAllocation = statementContext.FilterService.FiltersVersion;
            EpStatementAgentInstanceHandle = epStatementAgentInstanceHandle;
            AgentInstanceFilterProxy = agentInstanceFilterProxy;
            _contextProperties = contextProperties;
            AuditProvider = auditProvider;
            InstrumentationProvider = instrumentationProvider;
        }

        public virtual object FilterReboolConstant {
            get => null;
            set { }
        }

        public AgentInstanceFilterProxy AgentInstanceFilterProxy { get; }

        public Attribute[] Annotations => StatementContext.Annotations;

        public ContextManagementService ContextManagementService => StatementContext.ContextManagementService;

        public ContextServiceFactory ContextServiceFactory => StatementContext.ContextServiceFactory;

        public EPStatementAgentInstanceHandle EpStatementAgentInstanceHandle { get; }

        public RuntimeExtensionServices RuntimeExtensionServicesContext => StatementContext.RuntimeExtensionServices;

        public EventBeanTypedEventFactory EventBeanTypedEventFactory => StatementContext.EventBeanTypedEventFactory;

        public RuntimeSettingsService RuntimeSettingsService => StatementContext.RuntimeSettingsService;

        public ImportServiceRuntime ImportServiceRuntime => StatementContext.ImportServiceRuntime;

        public FilterService FilterService => StatementContext.FilterService;

        public InternalEventRouter InternalEventRouter => StatementContext.InternalEventRouter;

        public InternalEventRouteDest InternalEventRouteDest => StatementContext.InternalEventRouteDest;

        public SchedulingService SchedulingService => StatementContext.SchedulingService;

        public ScheduleBucket ScheduleBucket => StatementContext.ScheduleBucket;

        public StatementContext StatementContext { get; }

        public StatementResultService StatementResultService => StatementContext.StatementResultService;

        public ViewFactoryService ViewFactoryService => StatementContext.ViewFactoryService;

        public ResultSetProcessorHelperFactory ResultSetProcessorHelperFactory => StatementContext.ResultSetProcessorHelperFactory;

        public NamedWindowManagementService NamedWindowManagementService => StatementContext.NamedWindowManagementService;

        public StatementResourceService StatementResourceService => StatementContext.StatementResourceService;

        public ExceptionHandlingService ExceptionHandlingService => StatementContext.ExceptionHandlingService;

        public VariableManagementService VariableManagementService => StatementContext.VariableManagementService;

        public StatementContextFilterEvalEnv StatementContextFilterEvalEnv => StatementContext.StatementContextFilterEvalEnv;

        public TableManagementService TableManagementService => StatementContext.TableManagementService;

        public EventTypeResolvingBeanFactory EventTypeResolvingBeanFactory => StatementContext.EventTypeResolvingBeanFactory;

        public EventTypeAvroHandler EventTypeAvroHandler => StatementContext.EventTypeAvroHandler;

        public RowRecogStateRepoFactory RowRecogStateRepoFactory => StatementContext.RowRecogStateRepoFactory;

        public EventTableIndexService EventTableIndexService => StatementContext.EventTableIndexService;

        public HistoricalDataCacheFactory HistoricalDataCacheFactory => StatementContext.HistoricalDataCacheFactory;

        public DatabaseConfigServiceRuntime DatabaseConfigService => StatementContext.DatabaseConfigService;

        public EPRuntimeEventProcessWrapped EPRuntimeEventProcessWrapped => StatementContext.EPRuntimeEventProcessWrapped;

        public EventServiceSendEventCommon EPRuntimeSendEvent => StatementContext.EPRuntimeSendEvent;

        public EPRenderEventService EPRuntimeRenderEvent => StatementContext.EPRuntimeRenderEvent;

        public DataFlowFilterServiceAdapter DataFlowFilterServiceAdapter => StatementContext.DataFlowFilterServiceAdapter;

        public object Runtime => StatementContext.Runtime;

        public MetricReportingService MetricReportingService => StatementContext.MetricReportingService;

        public INamingContext RuntimeEnvContext => StatementContext.RuntimeEnvContext;

        public long FilterVersionAfterAllocation { get; }

        public string ModuleName => StatementContext.ModuleName;

        public TypeResolver TypeResolver => ImportServiceRuntime.TypeResolver;

        public StatementContextCPPair StatementContextCPPair {
            get {
                if (_statementContextCpPair == null) {
                    _statementContextCpPair = new StatementContextCPPair(
                        StatementContext.StatementId,
                        EpStatementAgentInstanceHandle.AgentInstanceId,
                        StatementContext);
                }

                return _statementContextCpPair;
            }
        }

        public ICollection<AgentInstanceMgmtCallback> TerminationCallbackRO {
            get {
                if (_terminationCallbacks == null) {
                    return Collections.GetEmptyList<AgentInstanceMgmtCallback>();
                }

                if (_terminationCallbacks is ICollection<AgentInstanceMgmtCallback>) {
                    return (ICollection<AgentInstanceMgmtCallback>) _terminationCallbacks;
                }

                return Collections.SingletonList((AgentInstanceMgmtCallback) _terminationCallbacks);
            }
        }

        public EventBean ContextProperties => _contextProperties;

        public string RuntimeURI => StatementContext.RuntimeURI;

        public int AgentInstanceId => EpStatementAgentInstanceHandle.AgentInstanceId;

        public IReaderWriterLock AgentInstanceLock => EpStatementAgentInstanceHandle.StatementAgentInstanceLock;

        public EventBeanService EventBeanService => StatementContext.EventBeanService;

        public string StatementName => StatementContext.StatementName;

        public object UserObjectCompileTime => StatementContext.UserObjectCompileTime;

        public int StatementId => StatementContext.StatementId;

        public TimeProvider TimeProvider => StatementContext.TimeProvider;

        public string DeploymentId => StatementContext.DeploymentId;

        public ExpressionResultCacheService ExpressionResultCacheService => StatementContext.ExpressionResultCacheServiceSharable;

        public TableExprEvaluatorContext TableExprEvaluatorContext => StatementContext.TableExprEvaluatorContext;

        public AuditProvider AuditProvider { get; }

        public InstrumentationCommon InstrumentationProvider { get; }

        public AgentInstanceScriptContext AllocateAgentInstanceScriptContext {
            get {
                if (_agentInstanceScriptContext == null) {
                    _agentInstanceScriptContext = AgentInstanceScriptContext.From(StatementContext);
                }

                return _agentInstanceScriptContext;
            }
        }

        /// <summary>
        ///     Add a stop-callback.
        ///     Use to add a stop-callback other than already registered.
        ///     This is generally not required by views that implement AgentInstanceMgmtCallback as
        ///     they gets stopped as part of normal processing.
        /// </summary>
        /// <param name="callback">to add</param>
        public void AddTerminationCallback(AgentInstanceMgmtCallback callback)
        {
            if (_terminationCallbacks == null) {
                _terminationCallbacks = callback;
            }
            else if (_terminationCallbacks is ICollection<AgentInstanceMgmtCallback>) {
                ((ICollection<AgentInstanceMgmtCallback>) _terminationCallbacks).Add(callback);
            }
            else {
                var cb = (AgentInstanceMgmtCallback) _terminationCallbacks;
                var q = new HashSet<AgentInstanceMgmtCallback>();
                q.Add(cb);
                q.Add(callback);
                _terminationCallbacks = q;
            }
        }

        public void RemoveTerminationCallback(AgentInstanceMgmtCallback callback)
        {
            if (_terminationCallbacks == null) {
            }
            else if (_terminationCallbacks is ICollection<AgentInstanceMgmtCallback>) {
                ((ICollection<AgentInstanceMgmtCallback>) _terminationCallbacks).Remove(callback);
            }
            else if (_terminationCallbacks == callback) {
                _terminationCallbacks = null;
            }
        }
    }
} // end of namespace