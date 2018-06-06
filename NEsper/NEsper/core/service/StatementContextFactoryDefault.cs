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
using com.espertech.esper.client.annotation;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.context.stmt;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service.resource;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.metric;
using com.espertech.esper.epl.script;
using com.espertech.esper.epl.spec;
using com.espertech.esper.pattern;
using com.espertech.esper.pattern.pool;
using com.espertech.esper.rowregex;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Default implementation for making a statement-specific context class.
    /// </summary>
    public class StatementContextFactoryDefault : StatementContextFactory
    {
        private readonly PluggableObjectRegistryImpl _viewRegistry;
        private readonly PluggableObjectCollection _patternObjectClasses;
        private readonly Type _systemVirtualDwViewFactory;
    
        private StatementContextEngineServices _stmtEngineServices;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="viewPlugIns">is the view plug-in object descriptions</param>
        /// <param name="plugInPatternObj">is the pattern plug-in object descriptions</param>
        /// <param name="systemVirtualDWViewFactory">virtual DW factory</param>
        public StatementContextFactoryDefault(
            PluggableObjectCollection viewPlugIns,
            PluggableObjectCollection plugInPatternObj,
            Type systemVirtualDWViewFactory)
        {
            _viewRegistry = new PluggableObjectRegistryImpl(new PluggableObjectCollection[]{ViewEnumHelper.BuiltinViews, viewPlugIns});
    
            _systemVirtualDwViewFactory = systemVirtualDWViewFactory;
    
            _patternObjectClasses = new PluggableObjectCollection();
            _patternObjectClasses.AddObjects(plugInPatternObj);
            _patternObjectClasses.AddObjects(PatternObjectHelper.BuiltinPatternObjects);
        }
    
        public static StatementContextEngineServices GetStmtCtxEngineServices(EPServicesContext services)
        {
            return new StatementContextEngineServices(
                services.Container,
                services.EngineURI,
                services.EventAdapterService,
                services.NamedWindowMgmtService,
                services.VariableService,
                services.TableService,
                services.EngineSettingsService,
                services.ValueAddEventService,
                services.ConfigSnapshot,
                services.MetricsReportingService,
                services.ViewService,
                services.ExceptionHandlingService,
                services.ExpressionResultCacheSharable,
                services.StatementEventTypeRefService,
                services.TableService.TableExprEvaluatorContext,
                services.EngineLevelExtensionServicesContext,
                services.RegexHandlerFactory,
                services.StatementLockFactory,
                services.ContextManagementService,
                services.ViewServicePreviousFactory,
                services.EventTableIndexService,
                services.PatternNodeFactory,
                services.FilterBooleanExpressionFactory,
                services.TimeSource,
                services.EngineImportService,
                services.AggregationFactoryFactory,
                services.SchedulingService,
                services.ExprDeclaredService
            );
        }

        public EPServicesContext StmtEngineServices
        {
            set { _stmtEngineServices = GetStmtCtxEngineServices(value); }
        }

        public StatementContext MakeContext(
            int statementId,
            string statementName,
            string expression,
            StatementType statementType,
            EPServicesContext engineServices,
            IDictionary<string, Object> optAdditionalContext,
            bool isFireAndForget,
            Attribute[] annotations,
            EPIsolationUnitServices isolationUnitServices,
            bool stateless,
            StatementSpecRaw statementSpecRaw,
            IList<ExprSubselectNode> subselectNodes,
            bool writesToTables,
            Object statementUserObject)
        {
            // Allocate the statement's schedule bucket which stays constant over it's lifetime.
            // The bucket allows callbacks for the same time to be ordered (within and across statements) and thus deterministic.
            var scheduleBucket = engineServices.SchedulingMgmtService.AllocateBucket();
    
            // Create a lock for the statement
            IReaderWriterLock defaultStatementAgentInstanceLock;
    
            // For on-delete statements, use the create-named-window statement lock
            var optCreateWindowDesc = statementSpecRaw.CreateWindowDesc;
            var optOnTriggerDesc = statementSpecRaw.OnTriggerDesc;
            if ((optOnTriggerDesc != null) && (optOnTriggerDesc is OnTriggerWindowDesc))
            {
                var windowName = ((OnTriggerWindowDesc) optOnTriggerDesc).WindowName;
                if (engineServices.TableService.GetTableMetadata(windowName) == null) {
                    defaultStatementAgentInstanceLock = engineServices.NamedWindowMgmtService.GetNamedWindowLock(windowName);
                    if (defaultStatementAgentInstanceLock == null) {
                        throw new EPStatementException("Named window or table '" + windowName + "' has not been declared", expression);
                    }
                } else {
                    defaultStatementAgentInstanceLock = engineServices.StatementLockFactory.GetStatementLock(statementName, annotations, stateless);
                }
            } else if (optCreateWindowDesc != null) {
                // For creating a named window, save the lock for use with on-delete/on-merge/on-update etc. statements
                defaultStatementAgentInstanceLock = engineServices.NamedWindowMgmtService.GetNamedWindowLock(optCreateWindowDesc.WindowName);
                if (defaultStatementAgentInstanceLock == null) {
                    defaultStatementAgentInstanceLock = engineServices.StatementLockFactory.GetStatementLock(statementName, annotations, false);
                    engineServices.NamedWindowMgmtService.AddNamedWindowLock(optCreateWindowDesc.WindowName, defaultStatementAgentInstanceLock, statementName);
                }
            } else {
                defaultStatementAgentInstanceLock = engineServices.StatementLockFactory.GetStatementLock(statementName, annotations, stateless);
            }
    
            StatementMetricHandle stmtMetric = null;
            if (!isFireAndForget) {
                stmtMetric = engineServices.MetricsReportingService.GetStatementHandle(statementId, statementName);
            }
    
            var annotationData = AnnotationAnalysisResult.AnalyzeAnnotations(annotations);
            var hasVariables = statementSpecRaw.HasVariables || (statementSpecRaw.CreateContextDesc != null);
            var hasTableAccess = StatementContextFactoryUtil.DetermineHasTableAccess(subselectNodes, statementSpecRaw, engineServices);
            var epStatementHandle = new EPStatementHandle(
                statementId, statementName, expression, statementType, expression, 
                hasVariables, stmtMetric, 
                annotationData.Priority, 
                annotationData.IsPremptive, 
                hasTableAccess, 
                engineServices.MultiMatchHandlerFactory.GetDefaultHandler());
    
            var patternContextFactory = new PatternContextFactoryDefault();

            var container = engineServices.Container;
            var optionalCreateNamedWindowName = statementSpecRaw.CreateWindowDesc != null ? statementSpecRaw.CreateWindowDesc.WindowName : null;
            var viewResolutionService = new ViewResolutionServiceImpl(_viewRegistry, optionalCreateNamedWindowName, _systemVirtualDwViewFactory);
            var patternResolutionService = new PatternObjectResolutionServiceImpl(container, _patternObjectClasses);
    
            var schedulingService = engineServices.SchedulingService;
            var filterService = engineServices.FilterService;
            if (isolationUnitServices != null) {
                filterService = isolationUnitServices.FilterService;
                schedulingService = isolationUnitServices.SchedulingService;
            }
    
            var scheduleAudit = AuditEnum.SCHEDULE.GetAudit(annotations);
            if (scheduleAudit != null) {
                schedulingService = new SchedulingServiceAudit(engineServices.EngineURI, statementName, schedulingService);
            }
    
            StatementAIResourceRegistry statementAgentInstanceRegistry = null;
            ContextDescriptor contextDescriptor = null;
            var optionalContextName = statementSpecRaw.OptionalContextName;
            if (optionalContextName != null) {
                contextDescriptor = engineServices.ContextManagementService.GetContextDescriptor(optionalContextName);
    
                // allocate a per-instance registry of aggregations and prev/prior/subselect
                if (contextDescriptor != null) {
                    statementAgentInstanceRegistry = contextDescriptor.AiResourceRegistryFactory.Invoke();
                }
            }
    
            var countSubexpressions = engineServices.ConfigSnapshot.EngineDefaults.Patterns.MaxSubexpressions != null;
            PatternSubexpressionPoolStmtSvc patternSubexpressionPoolStmtSvc = null;
            if (countSubexpressions) {
                var stmtCounter = new PatternSubexpressionPoolStmtHandler();
                patternSubexpressionPoolStmtSvc = new PatternSubexpressionPoolStmtSvc(engineServices.PatternSubexpressionPoolSvc, stmtCounter);
                engineServices.PatternSubexpressionPoolSvc.AddPatternContext(statementName, stmtCounter);
            }
    
            var countMatchRecogStates = engineServices.ConfigSnapshot.EngineDefaults.MatchRecognize.MaxStates != null;
            MatchRecognizeStatePoolStmtSvc matchRecognizeStatePoolStmtSvc = null;
            if (countMatchRecogStates && statementSpecRaw.MatchRecognizeSpec != null) {
                var stmtCounter = new MatchRecognizeStatePoolStmtHandler();
                matchRecognizeStatePoolStmtSvc = new MatchRecognizeStatePoolStmtSvc(engineServices.MatchRecognizeStatePoolEngineSvc, stmtCounter);
                engineServices.MatchRecognizeStatePoolEngineSvc.AddPatternContext(statementName, stmtCounter);
            }
    
            AgentInstanceScriptContext defaultAgentInstanceScriptContext = null;
            if (statementSpecRaw.ScriptExpressions != null && !statementSpecRaw.ScriptExpressions.IsEmpty()) {
                defaultAgentInstanceScriptContext = AgentInstanceScriptContext.From(engineServices.EventAdapterService);
            }
    
            // allow a special context controller factory for testing
            var contextControllerFactoryService = GetContextControllerFactoryService(annotations, engineServices.EngineImportService);
    
            // may use resource tracking
            var statementResourceService = new StatementResourceService(optionalContextName != null);
            var extensionSvcContext = new ProxyStatementExtensionSvcContext() {
                ProcStmtResources = () => statementResourceService,
                ProcExtractStatementResourceHolder = (resultOfStart) => StatementResourceHolderUtil.PopulateHolder(resultOfStart),
                ProcPreStartWalk = (selectDesc) => {},
                ProcPostProcessStart = (resultOfStart, isRecoveringResilient) => { },
                ProcContributeStopCallback = (selectResult, stopCallbacks) => {}
            };
    
            // Create statement context
            return new StatementContext(
                engineServices.Container,
                _stmtEngineServices,
                schedulingService,
                scheduleBucket,
                epStatementHandle,
                viewResolutionService,
                patternResolutionService,
                extensionSvcContext,
                new StatementStopServiceImpl(),
                patternContextFactory,
                filterService,
                new StatementResultServiceImpl(
                    statementName, 
                    engineServices.StatementLifecycleSvc, 
                    engineServices.MetricsReportingService,
                    engineServices.ThreadingService,
                    engineServices.ThreadLocalManager),
                engineServices.InternalEventEngineRouteDest,
                annotations,
                statementAgentInstanceRegistry,
                defaultStatementAgentInstanceLock,
                contextDescriptor,
                patternSubexpressionPoolStmtSvc,
                matchRecognizeStatePoolStmtSvc,
                stateless,
                contextControllerFactoryService,
                defaultAgentInstanceScriptContext,
                AggregationServiceFactoryServiceImpl.DEFAULT_FACTORY,
                engineServices.ScriptingService,
                writesToTables,
                statementUserObject,
                StatementSemiAnonymousTypeRegistryImpl.INSTANCE,
                annotationData.Priority);
        }

        private ContextControllerFactoryService GetContextControllerFactoryService(
            Attribute[] annotations,
            EngineImportService engineImportService)
        {
            try
            {
                var replacementCache = (ContextStateCache) TypeHelper.GetAnnotationHook(
                    annotations, HookType.CONTEXT_STATE_CACHE, typeof (ContextStateCache), engineImportService);
                if (replacementCache != null)
                {
                    return new ContextControllerFactoryServiceImpl(replacementCache);
                }
            }
            catch (ExprValidationException)
            {
                throw new EPException("Failed to obtain hook for " + HookType.CONTEXT_STATE_CACHE);
            }
            return ContextControllerFactoryServiceImpl.DEFAULT_FACTORY;
        }

        /// <summary>Analysis result of analysing annotations for a statement.</summary>
        public class AnnotationAnalysisResult
        {
            /// <summary>
            /// Ctor.
            /// </summary>
            /// <param name="priority">priority</param>
            /// <param name="premptive">preemptive indicator</param>
            private AnnotationAnalysisResult(int priority, bool premptive) {
                this.Priority = priority;
                IsPremptive = premptive;
            }

            /// <summary>
            /// Analyze the annotations and return priority and drop settings.
            /// </summary>
            /// <param name="annotations">to analyze</param>
            /// <returns>analysis result</returns>
            public static AnnotationAnalysisResult AnalyzeAnnotations(Attribute[] annotations)
            {
                var preemptive = false;
                var priority = 0;
                var hasPrioritySetting = false;
                foreach (var annotation in annotations)
                {
                    if (annotation is PriorityAttribute)
                    {
                        priority = ((PriorityAttribute) annotation).Value;
                        hasPrioritySetting = true;
                    }
                    if (annotation is DropAttribute)
                    {
                        preemptive = true;
                    }
                }
                if (!hasPrioritySetting && preemptive)
                {
                    priority = 1;
                }
                return new AnnotationAnalysisResult(priority, preemptive);
            }

            /// <summary>
            /// Returns execution priority.
            /// </summary>
            /// <value>priority.</value>
            public int Priority { get; private set; }

            /// <summary>
            /// Returns preemptive indicator (drop or normal).
            /// </summary>
            /// <value>true for drop</value>
            public bool IsPremptive { get; private set; }
        }
    }
} // end of namespace
