///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.context.stmt;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.metric;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.script;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.filter;
using com.espertech.esper.pattern;
using com.espertech.esper.pattern.pool;
using com.espertech.esper.rowregex;
using com.espertech.esper.schedule;
using com.espertech.esper.script;
using com.espertech.esper.timer;
using com.espertech.esper.view;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Contains handles to the implementation of the the scheduling service for use in view evaluation.
    /// </summary>
    public sealed class StatementContext
    {
        private readonly StatementResultService _statementResultService;
        private readonly StatementContextEngineServices _stmtEngineServices;

        // settable for view-sharing

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stmtEngineServices">is the engine services for the statement</param>
        /// <param name="schedulingService">implementation for schedule registration</param>
        /// <param name="scheduleBucket">is for ordering scheduled callbacks within the view statements</param>
        /// <param name="epStatementHandle">is the statements-own handle for use in registering callbacks with services</param>
        /// <param name="viewResultionService">is a service for resolving view namespace and name to a view factory</param>
        /// <param name="patternResolutionService">is the service that resolves pattern objects for the statement</param>
        /// <param name="statementExtensionSvcContext">provide extension points for custom statement resources</param>
        /// <param name="statementStopService">for registering a callback invoked when a statement is stopped</param>
        /// <param name="patternContextFactory">is the pattern-level services and context information factory</param>
        /// <param name="filterService">is the filtering service</param>
        /// <param name="statementResultService">handles awareness of listeners/subscriptions for a statement customizing output produced</param>
        /// <param name="internalEventEngineRouteDest">routing destination</param>
        /// <param name="annotations">The annotations.</param>
        /// <param name="statementAgentInstanceRegistry">The statement agent instance registry.</param>
        /// <param name="defaultAgentInstanceLock">The default agent instance lock.</param>
        /// <param name="contextDescriptor">The context descriptor.</param>
        /// <param name="patternSubexpressionPoolSvc">The pattern subexpression pool SVC.</param>
        /// <param name="matchRecognizeStatePoolStmtSvc">The match recognize state pool statement SVC.</param>
        /// <param name="statelessSelect">if set to <c>true</c> [stateless select].</param>
        /// <param name="contextControllerFactoryService">The context controller factory service.</param>
        /// <param name="defaultAgentInstanceScriptContext">The default agent instance script context.</param>
        /// <param name="aggregationServiceFactoryService">The aggregation service factory service.</param>
        /// <param name="scriptingService">The scripting service.</param>
        /// <param name="writesToTables">if set to <c>true</c> [writes to tables].</param>
        /// <param name="statementUserObject">The statement user object.</param>
        /// <param name="statementSemiAnonymousTypeRegistry">The statement semi anonymous type registry.</param>
        /// <param name="priority">The priority.</param>
        public StatementContext(
            StatementContextEngineServices stmtEngineServices,
            SchedulingService schedulingService,
            ScheduleBucket scheduleBucket,
            EPStatementHandle epStatementHandle,
            ViewResolutionService viewResultionService,
            PatternObjectResolutionService patternResolutionService,
            StatementExtensionSvcContext statementExtensionSvcContext,
            StatementStopService statementStopService,
            PatternContextFactory patternContextFactory,
            FilterService filterService,
            StatementResultService statementResultService,
            InternalEventRouteDest internalEventEngineRouteDest,
            Attribute[] annotations,
            StatementAIResourceRegistry statementAgentInstanceRegistry,
            IReaderWriterLock defaultAgentInstanceLock,
            ContextDescriptor contextDescriptor,
            PatternSubexpressionPoolStmtSvc patternSubexpressionPoolSvc,
            MatchRecognizeStatePoolStmtSvc matchRecognizeStatePoolStmtSvc,
            bool statelessSelect,
            ContextControllerFactoryService contextControllerFactoryService,
            AgentInstanceScriptContext defaultAgentInstanceScriptContext,
            AggregationServiceFactoryService aggregationServiceFactoryService,
            ScriptingService scriptingService,
            bool writesToTables,
            object statementUserObject,
            StatementSemiAnonymousTypeRegistry statementSemiAnonymousTypeRegistry,
            int priority)
        {
            _stmtEngineServices = stmtEngineServices;
            SchedulingService = schedulingService;
            ScheduleBucket = scheduleBucket;
            EpStatementHandle = epStatementHandle;
            ViewResolutionService = viewResultionService;
            PatternResolutionService = patternResolutionService;
            StatementExtensionServicesContext = statementExtensionSvcContext;
            StatementStopService = statementStopService;
            PatternContextFactory = patternContextFactory;
            FilterService = filterService;
            _statementResultService = statementResultService;
            InternalEventEngineRouteDest = internalEventEngineRouteDest;
            ScheduleAdjustmentService = stmtEngineServices.ConfigSnapshot.EngineDefaults.Execution.IsAllowIsolatedService ? new ScheduleAdjustmentService() : null;
            Annotations = annotations;
            StatementAgentInstanceRegistry = statementAgentInstanceRegistry;
            DefaultAgentInstanceLock = defaultAgentInstanceLock;
            ContextDescriptor = contextDescriptor;
            PatternSubexpressionPoolSvc = patternSubexpressionPoolSvc;
            MatchRecognizeStatePoolStmtSvc = matchRecognizeStatePoolStmtSvc;
            IsStatelessSelect = statelessSelect;
            ContextControllerFactoryService = contextControllerFactoryService;
            DefaultAgentInstanceScriptContext = defaultAgentInstanceScriptContext;
            AggregationServiceFactoryService = aggregationServiceFactoryService;
            ScriptingService = scriptingService;
            IsWritesToTables = writesToTables;
            StatementUserObject = statementUserObject;
            StatementSemiAnonymousTypeRegistry = statementSemiAnonymousTypeRegistry;
            Priority = priority;
        }

        /// <summary>Returns the statement id. </summary>
        /// <value>statement id</value>
        public int StatementId
        {
            get { return EpStatementHandle.StatementId; }
        }

        /// <summary>Returns the statement type. </summary>
        /// <value>statement type</value>
        public StatementType StatementType
        {
            get { return EpStatementHandle.StatementType; }
        }

        /// <summary>Returns the statement name </summary>
        /// <value>statement name</value>
        public string StatementName
        {
            get { return EpStatementHandle.StatementName; }
        }

        /// <summary>Returns service to use for schedule evaluation. </summary>
        /// <value>schedule evaluation service implemetation</value>
        public SchedulingService SchedulingService { get; set; }

        /// <summary>Returns service for generating events and handling event types. </summary>
        /// <value>event adapter service</value>
        public EventAdapterService EventAdapterService
        {
            get { return _stmtEngineServices.EventAdapterService; }
        }

        /// <summary>Returns the schedule bucket for ordering schedule callbacks within this pattern. </summary>
        /// <value>schedule bucket</value>
        public ScheduleBucket ScheduleBucket { get; private set; }

        /// <summary>Returns the statement's resource locks. </summary>
        /// <value>statement resource lock/handle</value>
        public EPStatementHandle EpStatementHandle { get; private set; }

        /// <summary>Returns view resolution svc. </summary>
        /// <value>view resolution</value>
        public ViewResolutionService ViewResolutionService { get; private set; }

        /// <summary>Returns extension context for statements. </summary>
        /// <value>context</value>
        public StatementExtensionSvcContext StatementExtensionServicesContext { get; private set; }

        /// <summary>Returns statement stop subscription taker. </summary>
        /// <value>stop service</value>
        public StatementStopService StatementStopService { get; private set; }

        /// <summary>Returns the pattern context factory for the statement. </summary>
        /// <value>pattern context factory</value>
        public PatternContextFactory PatternContextFactory { get; private set; }

        public MatchRecognizeStatePoolStmtSvc MatchRecognizeStatePoolStmtSvc { get; private set; }

        /// <summary>Gets or sets the compiled statement spec.</summary>
        public StatementSpecCompiled StatementSpecCompiled { get; set; }
        /// <summary>Gets or sets the statement agent instance factory.</summary>
        public StatementAgentInstanceFactory StatementAgentInstanceFactory { get; set; }
        /// <summary>Gets or sets the statement.</summary>
        public EPStatementSPI Statement { get; set; }

        public EngineLevelExtensionServicesContext EngineExtensionServicesContext
        {
            get { return _stmtEngineServices.EngineLevelExtensionServicesContext; }
        }

        public RegexHandlerFactory RegexPartitionStateRepoFactory
        {
            get { return _stmtEngineServices.RegexHandlerFactory; }
        }

        public ViewServicePreviousFactory ViewServicePreviousFactory
        {
            get { return _stmtEngineServices.ViewServicePreviousFactory; }
        }

        public PatternNodeFactory PatternNodeFactory
        {
            get { return _stmtEngineServices.PatternNodeFactory; }
        }

        public EventTableIndexService EventTableIndexService
        {
            get { return _stmtEngineServices.EventTableIndexService; }
        }

        public StatementLockFactory StatementLockFactory
        {
            get { return _stmtEngineServices.StatementLockFactory; }
        }

        /// <summary>Returns the statement expression text </summary>
        /// <value>expression text</value>
        public string Expression
        {
            get { return EpStatementHandle.EPL; }
        }

        /// <summary>Returns the engine URI. </summary>
        /// <value>engine URI</value>
        public string EngineURI
        {
            get { return _stmtEngineServices.EngineURI; }
        }

        /// <summary>Returns the statement's resolution service for pattern objects. </summary>
        /// <value>service for resolving pattern objects</value>
        public PatternObjectResolutionService PatternResolutionService { get; private set; }

        /// <summary>Returns the named window management service. </summary>
        /// <value>service for managing named windows</value>
        public NamedWindowMgmtService NamedWindowMgmtService
        {
            get { return _stmtEngineServices.NamedWindowMgmtService; }
        }

        /// <summary>Returns variable service. </summary>
        /// <value>variable service</value>
        public VariableService VariableService
        {
            get { return _stmtEngineServices.VariableService; }
        }

        /// <summary>Returns table service.</summary>
        /// <value>The table service.</value>
        public TableService TableService
        {
            get { return _stmtEngineServices.TableService; }
        }

        /// <summary>Returns the service that handles awareness of listeners/subscriptions for a statement customizing output produced </summary>
        /// <value>statement result svc</value>
        public StatementResultService StatementResultService
        {
            get { return _statementResultService; }
        }

        /// <summary>Returns the URIs for resolving the event name against plug-inn event representations, if any </summary>
        /// <value>URIs</value>
        public IList<Uri> PlugInTypeResolutionURIs
        {
            get { return _stmtEngineServices.PlugInTypeResolutionURIs; }
        }

        /// <summary>Returns the Update event service. </summary>
        /// <value>revision service</value>
        public ValueAddEventService ValueAddEventService
        {
            get { return _stmtEngineServices.ValueAddEventService; }
        }

        /// <summary>Returns the configuration. </summary>
        /// <value>configuration</value>
        public ConfigurationInformation ConfigSnapshot
        {
            get { return _stmtEngineServices.ConfigSnapshot; }
        }

        /// <summary>Sets the filter service </summary>
        /// <value>filter service</value>
        public FilterService FilterService { get; set; }

        /// <summary>Returns the internal event router. </summary>
        /// <value>router</value>
        public InternalEventRouteDest InternalEventEngineRouteDest { get; set; }

        /// <summary>Return the service for adjusting schedules or null if not applicable. </summary>
        /// <value>service for adjusting schedules</value>
        public ScheduleAdjustmentService ScheduleAdjustmentService { get; private set; }

        /// <summary>Returns metrics svc. </summary>
        /// <value>metrics</value>
        public MetricReportingServiceSPI MetricReportingService
        {
            get { return _stmtEngineServices.MetricReportingService; }
        }

        /// <summary>Returns the time provider. </summary>
        /// <value>time provider</value>
        public TimeProvider TimeProvider
        {
            get { return SchedulingService; }
        }

        /// <summary>Returns view svc. </summary>
        /// <value>svc</value>
        public ViewService ViewService
        {
            get { return _stmtEngineServices.ViewService; }
        }

        public ExceptionHandlingService ExceptionHandlingService
        {
            get { return _stmtEngineServices.ExceptionHandlingService; }
        }

        public TableExprEvaluatorContext TableExprEvaluatorContext
        {
            get { return _stmtEngineServices.TableExprEvaluatorContext; }
        }

        public ContextManagementService ContextManagementService
        {
            get { return _stmtEngineServices.ContextManagementService; }
        }

        public Attribute[] Annotations { get; private set; }

        public ExpressionResultCacheService ExpressionResultCacheServiceSharable
        {
            get { return _stmtEngineServices.ExpressionResultCacheService; }
        }

        public int AgentInstanceId
        {
            get { throw new Exception("Statement agent instance information is not available when providing a context"); }
        }

        public StatementAIResourceRegistry StatementAgentInstanceRegistry { get; private set; }

        public IReaderWriterLock DefaultAgentInstanceLock { get; set; }

        public ContextDescriptor ContextDescriptor { get; private set; }

        public PatternSubexpressionPoolStmtSvc PatternSubexpressionPoolSvc { get; private set; }

        public bool IsStatelessSelect { get; private set; }

        public ContextControllerFactoryService ContextControllerFactoryService { get; private set; }

        public AgentInstanceScriptContext DefaultAgentInstanceScriptContext { get; private set; }

        public AggregationServiceFactoryService AggregationServiceFactoryService { get; private set; }

        public StatementSemiAnonymousTypeRegistry StatementSemiAnonymousTypeRegistry { get; private set; }

        public int Priority { get; private set; }

        public FilterFaultHandlerFactory FilterFaultHandlerFactory { get; set; }

        public FilterBooleanExpressionFactory FilterBooleanExpressionFactory
        {
            get { return _stmtEngineServices.FilterBooleanExpressionFactory; }
        }

        public EngineSettingsService EngineSettingsService
        {
            get { return _stmtEngineServices.EngineSettingsService; }
        }

        public ExprDeclaredService ExprDeclaredService
        {
            get { return _stmtEngineServices.ExprDeclaredService; }
        }

        public TimeSourceService TimeSourceService
        {
            get { return _stmtEngineServices.TimeSourceService; }
        }

        public EngineImportService EngineImportService
        {
            get { return _stmtEngineServices.EngineImportService; }
        }

        public TimeAbacus TimeAbacus
        {
            get { return _stmtEngineServices.EngineImportService.TimeAbacus; }
        }

        public AgentInstanceScriptContext AllocateAgentInstanceScriptContext
        {
            get
            {
                if (DefaultAgentInstanceScriptContext == null)
                {
                    DefaultAgentInstanceScriptContext = AgentInstanceScriptContext.From(EventAdapterService);
                }
                return DefaultAgentInstanceScriptContext;
            }
        }

        public StatementEventTypeRef StatementEventTypeRef
        {
            get { return _stmtEngineServices.StatementEventTypeRef; }
        }

        public ScriptingService ScriptingService { get; private set; }

        public string ContextName
        {
            get { return ContextDescriptor == null ? null : ContextDescriptor.ContextName; }
        }

        public bool IsWritesToTables { get; private set; }

        public object StatementUserObject { get; private set; }

        public override String ToString()
        {
            return " stmtId=" + EpStatementHandle.StatementId +
                   " stmtName=" + EpStatementHandle.StatementName;
        }
    }
}