///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.activator;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.context.schedule;
using com.espertech.esper.core.deploy;
using com.espertech.esper.core.thread;
using com.espertech.esper.dataflow.core;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.db;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.metric;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.events.xml;
using com.espertech.esper.filter;
using com.espertech.esper.pattern;
using com.espertech.esper.pattern.pool;
using com.espertech.esper.plugin;
using com.espertech.esper.rowregex;
using com.espertech.esper.schedule;
using com.espertech.esper.script;
using com.espertech.esper.timer;
using com.espertech.esper.util;
using com.espertech.esper.view.stream;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Factory for services context.
    /// </summary>
    public class EPServicesContextFactoryDefault : EPServicesContextFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        public EPServicesContext CreateServicesContext(EPServiceProvider epServiceProvider, ConfigurationInformation configSnapshot)
        {
            // Directory for binding resources
            var resourceDirectory = new SimpleServiceDirectory();
    
            EventTypeIdGenerator eventTypeIdGenerator;
            if (configSnapshot.EngineDefaults.AlternativeContextConfig == null || configSnapshot.EngineDefaults.AlternativeContextConfig.EventTypeIdGeneratorFactory == null) {
                eventTypeIdGenerator = new EventTypeIdGeneratorImpl();
            }
            else {
                var eventTypeIdGeneratorFactory = TypeHelper.Instantiate<EventTypeIdGeneratorFactory>( 
                    configSnapshot.EngineDefaults.AlternativeContextConfig.EventTypeIdGeneratorFactory);
                eventTypeIdGenerator = eventTypeIdGeneratorFactory.Create(new EventTypeIdGeneratorContext(epServiceProvider.URI));
            }
    
            // Make services that depend on snapshot config entries
            var eventAdapterService = new EventAdapterServiceImpl(eventTypeIdGenerator, configSnapshot.EngineDefaults.EventMetaConfig.AnonymousCacheSize);
            Init(eventAdapterService, configSnapshot);
    
            // New read-write lock for concurrent event processing
            var eventProcessingRwLock = ReaderWriterLockManager.CreateLock(
                MethodBase.GetCurrentMethod().DeclaringType);
    
            var timeSourceService = MakeTimeSource(configSnapshot);
            var schedulingService = SchedulingServiceProvider.NewService(timeSourceService);
            var schedulingMgmtService = new SchedulingMgmtServiceImpl();
            var engineImportService = MakeEngineImportService(configSnapshot);
            var engineSettingsService = new EngineSettingsService(configSnapshot.EngineDefaults, configSnapshot.PlugInEventTypeResolutionURIs);
            var databaseConfigService = MakeDatabaseRefService(configSnapshot, schedulingService, schedulingMgmtService);
    
            var plugInViews = new PluggableObjectCollection();
            plugInViews.AddViews(configSnapshot.PlugInViews, configSnapshot.PlugInVirtualDataWindows);
            var plugInPatternObj = new PluggableObjectCollection();
            plugInPatternObj.AddPatternObjects(configSnapshot.PlugInPatternObjects);
    
            // exception handling
            ExceptionHandlingService exceptionHandlingService = InitExceptionHandling(
                epServiceProvider.URI,
                configSnapshot.EngineDefaults.ExceptionHandlingConfig, 
                configSnapshot.EngineDefaults.ConditionHandlingConfig);
    
            // Statement context factory
            Type systemVirtualDWViewFactory = null;
            if (configSnapshot.EngineDefaults.AlternativeContextConfig.VirtualDataWindowViewFactory != null) {
                try {
                    systemVirtualDWViewFactory = TypeHelper.ResolveType(configSnapshot.EngineDefaults.AlternativeContextConfig.VirtualDataWindowViewFactory);
                    if (!systemVirtualDWViewFactory.IsImplementsInterface(typeof(VirtualDataWindowFactory))) {
                        throw new ConfigurationException("Class " + systemVirtualDWViewFactory.Name + " does not implement the interface " + typeof(VirtualDataWindowFactory).FullName);
                    }
                }
                catch (TypeLoadException e) {
                    throw new ConfigurationException("Failed to look up class " + systemVirtualDWViewFactory);
                }
            }
            StatementContextFactory statementContextFactory = new StatementContextFactoryDefault(plugInViews, plugInPatternObj, systemVirtualDWViewFactory);
    
            long msecTimerResolution = configSnapshot.EngineDefaults.ThreadingConfig.InternalTimerMsecResolution;
            if (msecTimerResolution <= 0)
            {
                throw new ConfigurationException("Timer resolution configuration not set to a valid value, expecting a non-zero value");
            }
            var timerService = new TimerServiceImpl(epServiceProvider.URI, msecTimerResolution);
    
            var variableService = new VariableServiceImpl(configSnapshot.EngineDefaults.VariablesConfig.MsecVersionRelease, schedulingService, eventAdapterService, null);
            InitVariables(variableService, configSnapshot.Variables, engineImportService);

            var tableService = new TableServiceImpl();

            var statementLockFactory = new StatementLockFactoryImpl(configSnapshot.EngineDefaults.ExecutionConfig.IsFairlock, configSnapshot.EngineDefaults.ExecutionConfig.IsDisableLocking);
            var streamFactoryService = StreamFactoryServiceProvider.NewService(
                epServiceProvider.URI,
                configSnapshot.EngineDefaults.ViewResourcesConfig.IsShareViews);

            var filterService = FilterServiceProvider.NewService(
                configSnapshot.EngineDefaults.ExecutionConfig.FilterServiceProfile,
                configSnapshot.EngineDefaults.ExecutionConfig.IsAllowIsolatedService);

            var metricsReporting = new MetricReportingServiceImpl(configSnapshot.EngineDefaults.MetricsReportingConfig, epServiceProvider.URI);
            var namedWindowService = new NamedWindowServiceImpl(
                schedulingService,
                variableService,
                tableService,
                engineSettingsService.EngineSettings.ExecutionConfig.IsPrioritized, 
                eventProcessingRwLock, 
                exceptionHandlingService, 
                configSnapshot.EngineDefaults.LoggingConfig.IsEnableQueryPlan, 
                metricsReporting);
    
            var valueAddEventService = new ValueAddEventServiceImpl();
            valueAddEventService.Init(configSnapshot.RevisionEventTypes, configSnapshot.VariantStreams, eventAdapterService, eventTypeIdGenerator);
    
            var statementEventTypeRef = new StatementEventTypeRefImpl();
            var statementVariableRef = new StatementVariableRefImpl(variableService, tableService);
    
            var threadingService = new ThreadingServiceImpl(
                configSnapshot.EngineDefaults.ThreadingConfig);
    
            var internalEventRouterImpl = new InternalEventRouterImpl();
    
            var statementIsolationService = new StatementIsolationServiceImpl();
    
            DeploymentStateService deploymentStateService = new DeploymentStateServiceImpl();
    
            StatementMetadataFactory stmtMetadataFactory;
            if (configSnapshot.EngineDefaults.AlternativeContextConfig.StatementMetadataFactory == null) {
                stmtMetadataFactory = new StatementMetadataFactoryDefault();
            }
            else {
                stmtMetadataFactory = TypeHelper.Instantiate<StatementMetadataFactory>(configSnapshot.EngineDefaults.AlternativeContextConfig.StatementMetadataFactory);
            }
    
            ContextManagementService contextManagementService = new ContextManagementServiceImpl();
    
            SchedulableAgentInstanceDirectory schedulableAgentInstanceDirectory = null;     // not required for Non-HA.
    
            PatternSubexpressionPoolEngineSvc patternSubexpressionPoolSvc = null;
            if (configSnapshot.EngineDefaults.PatternsConfig.MaxSubexpressions != null) {
                patternSubexpressionPoolSvc = new PatternSubexpressionPoolEngineSvc(
                    configSnapshot.EngineDefaults.PatternsConfig.MaxSubexpressions.GetValueOrDefault(),
                    configSnapshot.EngineDefaults.PatternsConfig.IsMaxSubexpressionPreventStart);
            }

            MatchRecognizeStatePoolEngineSvc matchRecognizeStatePoolEngineSvc = null;
            if (configSnapshot.EngineDefaults.MatchRecognizeConfig.MaxStates != null)
            {
                matchRecognizeStatePoolEngineSvc = new MatchRecognizeStatePoolEngineSvc(
                    configSnapshot.EngineDefaults.MatchRecognizeConfig.MaxStates.Value,
                    configSnapshot.EngineDefaults.MatchRecognizeConfig.IsMaxStatesPreventStart);
            }

            var scriptingService = new ScriptingServiceImpl();
            scriptingService.DiscoverEngines();
    
            // New services context
            EPServicesContext services = new EPServicesContext(
                epServiceProvider.URI, schedulingService,
                eventAdapterService, engineImportService, engineSettingsService, databaseConfigService, plugInViews,
                statementLockFactory, eventProcessingRwLock, null, resourceDirectory, statementContextFactory,
                plugInPatternObj, timerService, filterService, streamFactoryService,
                namedWindowService, variableService, tableService, timeSourceService, valueAddEventService, metricsReporting, statementEventTypeRef,
                statementVariableRef, configSnapshot, threadingService, internalEventRouterImpl, statementIsolationService, schedulingMgmtService,
                deploymentStateService, exceptionHandlingService, new PatternNodeFactoryImpl(), eventTypeIdGenerator, stmtMetadataFactory,
                contextManagementService, schedulableAgentInstanceDirectory, patternSubexpressionPoolSvc, matchRecognizeStatePoolEngineSvc,
                new DataFlowServiceImpl(epServiceProvider, new DataFlowConfigurationStateServiceImpl()),
                new ExprDeclaredServiceImpl(),
                new ContextControllerFactoryFactorySvcImpl(), 
                new ContextManagerFactoryServiceImpl(),
                new EPStatementFactoryDefault(), 
                new RegexHandlerFactoryDefault(), 
                new ViewableActivatorFactoryDefault(),
                scriptingService
                );

            // Engine services subset available to statements
            statementContextFactory.StmtEngineServices = services;
    
            // Circular dependency
            var statementLifecycleSvc = new StatementLifecycleSvcImpl(epServiceProvider, services);
            services.StatementLifecycleSvc = statementLifecycleSvc;
    
            // Observers to statement events
            statementLifecycleSvc.LifecycleEvent += (s, theEvent) => metricsReporting.Observe(theEvent);
    
            // Circular dependency
            statementIsolationService.ServicesContext = services;
    
            return services;
        }

        internal static ExceptionHandlingService InitExceptionHandling(
            String engineURI,
            ConfigurationEngineDefaults.ExceptionHandling exceptionHandling,
            ConfigurationEngineDefaults.ConditionHandling conditionHandling)
        {
            List<ExceptionHandler> exceptionHandlers;
            if (exceptionHandling.HandlerFactories == null || exceptionHandling.HandlerFactories.IsEmpty())
            {
                exceptionHandlers = new List<ExceptionHandler>();
            }
            else
            {
                exceptionHandlers = new List<ExceptionHandler>();
                var context = new ExceptionHandlerFactoryContext(engineURI);
                foreach (String className in exceptionHandling.HandlerFactories) {
                    try {
                        var factory = TypeHelper.Instantiate<ExceptionHandlerFactory>(className);
                        var handler = factory.GetHandler(context);
                        if (handler == null) {
                            Log.Warn("Exception handler factory '" + className + "' returned a null handler, skipping factory");
                            continue;
                        }
                        exceptionHandlers.Add(handler);
                    }
                    catch (Exception ex) {
                        throw new ConfigurationException("Exception initializing exception handler from exception handler factory '" + className + "': " + ex.Message, ex);
                    }
                }
            }
    
            List<ConditionHandler> conditionHandlers;
            if (conditionHandling.HandlerFactories == null || conditionHandling.HandlerFactories.IsEmpty())
            {
                conditionHandlers = new List<ConditionHandler>();
            }
            else {
                conditionHandlers = new List<ConditionHandler>();
                var context = new ConditionHandlerFactoryContext(engineURI);
                foreach (String className in conditionHandling.HandlerFactories) {
                    try {
                        var factory = TypeHelper.Instantiate<ConditionHandlerFactory>(className);
                        var handler = factory.GetHandler(context);
                        if (handler == null) {
                            Log.Warn("Condition handler factory '" + className + "' returned a null handler, skipping factory");
                            continue;
                        }
                        conditionHandlers.Add(handler);
                    }
                    catch (Exception ex) {
                        throw new ConfigurationException("Exception initializing exception handler from exception handler factory '" + className + "': " + ex.Message, ex);
                    }
                }
            }

            return new ExceptionHandlingService(engineURI, exceptionHandlers, conditionHandlers);
        }
    
        /// <summary>Makes the time source provider. </summary>
        /// <param name="configSnapshot">the configuration</param>
        /// <returns>time source provider</returns>
        internal static TimeSourceService MakeTimeSource(ConfigurationInformation configSnapshot)
        {
            if (configSnapshot.EngineDefaults.TimeSourceConfig.TimeSourceType == ConfigurationEngineDefaults.TimeSourceType.NANO)
            {
                // this is a static variable to keep overhead down for getting a current time
                TimeSourceServiceImpl.IS_SYSTEM_CURRENT_TIME = false;
            }
            return new TimeSourceServiceImpl();
        }

        /// <summary>
        /// Adds configured variables to the variable service.
        /// </summary>
        /// <param name="variableService">service to add to</param>
        /// <param name="variables">configured variables</param>
        /// <param name="engineImportService">The engine import service.</param>
        internal static void InitVariables(VariableService variableService, IDictionary<String, ConfigurationVariable> variables, EngineImportService engineImportService)
        {
            foreach (KeyValuePair<String, ConfigurationVariable> entry in variables)
            {
                try
                {
                    var arrayType = TypeHelper.IsGetArrayType(entry.Value.VariableType);
                    variableService.CreateNewVariable(null, entry.Key, arrayType.First, entry.Value.IsConstant, arrayType.Second, false, entry.Value.InitializationValue, engineImportService);
                    variableService.AllocateVariableState(entry.Key, 0, null);
                }
                catch (VariableExistsException e)
                {
                    throw new ConfigurationException("Error configuring variables: " + e.Message, e);
                }
                catch (VariableTypeException e)
                {
                    throw new ConfigurationException("Error configuring variables: " + e.Message, e);
                }
            }
        }
    
        /// <summary>Initialize event adapter service for config snapshot. </summary>
        /// <param name="eventAdapterService">is events adapter</param>
        /// <param name="configSnapshot">is the config snapshot</param>
        internal static void Init(EventAdapterService eventAdapterService, ConfigurationInformation configSnapshot)
        {
            // Extract legacy event type definitions for each event type name, if supplied.
            //
            // We supply this information as setup information to the event adapter service
            // to allow discovery of superclasses and interfaces during event type construction for bean events,
            // such that superclasses and interfaces can use the legacy type definitions.
            IDictionary<String, ConfigurationEventTypeLegacy> classLegacyInfo = new Dictionary<String, ConfigurationEventTypeLegacy>();
            foreach (KeyValuePair<String, String> entry in configSnapshot.EventTypeNames)
            {
                String typeName = entry.Key;
                String className = entry.Value;
                ConfigurationEventTypeLegacy legacyDef = configSnapshot.EventTypesLegacy.Get(typeName);
                if (legacyDef != null)
                {
                    classLegacyInfo.Put(className, legacyDef);
                }
            }
            eventAdapterService.TypeLegacyConfigs = classLegacyInfo;
            eventAdapterService.DefaultPropertyResolutionStyle = configSnapshot.EngineDefaults.EventMetaConfig.ClassPropertyResolutionStyle;
            eventAdapterService.DefaultAccessorStyle = configSnapshot.EngineDefaults.EventMetaConfig.DefaultAccessorStyle;
    
            foreach (String typeNamespace in configSnapshot.EventTypeAutoNamePackages)
            {
                eventAdapterService.AddAutoNamePackage(typeNamespace);
            }
    
            // Add from the configuration the event class names
            IDictionary<String, String> typeNames = configSnapshot.EventTypeNames;
            foreach (KeyValuePair<String, String> entry in typeNames)
            {
                // Add class
                try
                {
                    String typeName = entry.Key;
                    eventAdapterService.AddBeanType(typeName, entry.Value, false, true, true, true);
                }
                catch (EventAdapterException ex)
                {
                    throw new ConfigurationException("Error configuring engine: " + ex.Message, ex);
                }
            }
    
            // Add from the configuration the XML DOM names and type def
            IDictionary<String, ConfigurationEventTypeXMLDOM> xmlDOMNames = configSnapshot.EventTypesXMLDOM;
            foreach (KeyValuePair<String, ConfigurationEventTypeXMLDOM> entry in xmlDOMNames)
            {
                SchemaModel schemaModel = null;
                if ((entry.Value.SchemaResource != null) || (entry.Value.SchemaText != null))
                {
                    try
                    {
                        schemaModel = XSDSchemaMapper.LoadAndMap(entry.Value.SchemaResource, entry.Value.SchemaText, 2);
                    }
                    catch (Exception ex)
                    {
                        throw new ConfigurationException(ex.Message, ex);
                    }
                }
    
                // Add XML DOM type
                try
                {
                    eventAdapterService.AddXMLDOMType(entry.Key, entry.Value, schemaModel, true);
                }
                catch (EventAdapterException ex)
                {
                    throw new ConfigurationException("Error configuring engine: " + ex.Message, ex);
                }
            }
    
            // Add maps in dependency order such that supertypes are added before subtypes
            ICollection<String> dependentMapOrder;
            try
            {
                var typesReferences = ToTypesReferences(configSnapshot.MapTypeConfigurations);
                dependentMapOrder = GraphUtil.GetTopDownOrder(typesReferences);
            }
            catch (GraphCircularDependencyException e)
            {
                throw new ConfigurationException("Error configuring engine, dependency graph between map type names is circular: " + e.Message, e);
            }
    
            IDictionary<String, Properties> mapNames = configSnapshot.EventTypesMapEvents;
            IDictionary<String, IDictionary<String, Object>> nestableMapNames = configSnapshot.EventTypesNestableMapEvents;
            dependentMapOrder.AddAll(mapNames.Keys);
            dependentMapOrder.AddAll(nestableMapNames.Keys);
            try
            {
                foreach (String mapName in dependentMapOrder)
                {
                    ConfigurationEventTypeMap mapConfig = configSnapshot.MapTypeConfigurations.Get(mapName);
                    Properties propertiesUnnested = mapNames.Get(mapName);
                    if (propertiesUnnested != null)
                    {
                        IDictionary<String, Object> propertyTypes = CreatePropertyTypes(propertiesUnnested);
                        IDictionary<String, Object> propertyTypesCompiled = EventTypeUtility.CompileMapTypeProperties(propertyTypes, eventAdapterService);
                        eventAdapterService.AddNestableMapType(mapName, propertyTypesCompiled, mapConfig, true, true, true, false, false);
                    }
    
                    IDictionary<String, Object> propertiesNestable = nestableMapNames.Get(mapName);
                    if (propertiesNestable != null)
                    {
                        IDictionary<String, Object> propertiesNestableCompiled = EventTypeUtility.CompileMapTypeProperties(propertiesNestable, eventAdapterService);
                        eventAdapterService.AddNestableMapType(mapName, propertiesNestableCompiled, mapConfig, true, true, true, false, false);
                    }
                }
            }
            catch (EventAdapterException ex)
            {
                throw new ConfigurationException("Error configuring engine: " + ex.Message, ex);
            }
    
            // Add object-array in dependency order such that supertypes are added before subtypes
            ICollection<string> dependentObjectArrayOrder;
            try
            {
                var typesReferences = ToTypesReferences(configSnapshot.ObjectArrayTypeConfigurations);
                dependentObjectArrayOrder = GraphUtil.GetTopDownOrder(typesReferences);
            }
            catch (GraphCircularDependencyException e)
            {
                throw new ConfigurationException(
                    "Error configuring engine, dependency graph between object array type names is circular: " + e.Message, e);
            }

            var nestableObjectArrayNames = configSnapshot.EventTypesNestableObjectArrayEvents;
            dependentObjectArrayOrder.AddAll(nestableObjectArrayNames.Keys);
            try
            {
                foreach (string objectArrayName in dependentObjectArrayOrder)
                {
                    var objectArrayConfig = configSnapshot.ObjectArrayTypeConfigurations.Get(objectArrayName);
                    var propertyTypes = nestableObjectArrayNames.Get(objectArrayName);
                    propertyTypes = ResolveClassesForStringPropertyTypes(propertyTypes);
                    var propertyTypesCompiled = EventTypeUtility.CompileMapTypeProperties(propertyTypes, eventAdapterService);
                    eventAdapterService.AddNestableObjectArrayType(objectArrayName, propertyTypesCompiled, objectArrayConfig, true, true, true, false, false, false, null);
                }
            }
            catch (EventAdapterException ex)
            {
                throw new ConfigurationException("Error configuring engine: " + ex.Message, ex);
            }

            // Add plug-in event representations
            var plugInReps = configSnapshot.PlugInEventRepresentation;
            foreach (var entry in plugInReps)
            {
                String className = entry.Value.EventRepresentationTypeName;
                Type eventRepClass;
                try
                {
                    eventRepClass = TypeHelper.ResolveType(className);
                }
                catch (TypeLoadException ex)
                {
                    throw new ConfigurationException("Failed to load plug-in event representation class '" + className + "'", ex);
                }
    
                Object pluginEventRepObj;
                try
                {
                    pluginEventRepObj = Activator.CreateInstance(eventRepClass);
                }
                catch (TypeInstantiationException ex)
                {
                    throw new ConfigurationException("Failed to instantiate plug-in event representation class '" + className + "' via default constructor", ex);
                }
                catch (TargetInvocationException ex)
                {
                    throw new ConfigurationException("Failed to instantiate plug-in event representation class '" + className + "' via default constructor", ex);
                }
                catch (MethodAccessException ex)
                {
                    throw new ConfigurationException("Illegal access to instantiate plug-in event representation class '" + className + "' via default constructor", ex);
                }
                catch (MemberAccessException ex)
                {
                    throw new ConfigurationException("Illegal access to instantiate plug-in event representation class '" + className + "' via default constructor", ex);
                }


                if (!(pluginEventRepObj is PlugInEventRepresentation))
                {
                    throw new ConfigurationException("Plug-in event representation class '" + className + "' does not implement the required interface " + typeof(PlugInEventRepresentation).FullName);
                }
    
                var eventRepURI = entry.Key;
                var pluginEventRep = (PlugInEventRepresentation) pluginEventRepObj;
                var initializer = entry.Value.Initializer;
                var context = new PlugInEventRepresentationContext(eventAdapterService, eventRepURI, initializer);
    
                try
                {
                    pluginEventRep.Init(context);
                    eventAdapterService.AddEventRepresentation(eventRepURI, pluginEventRep);
                }
                catch (Exception e)
                {
                    throw new ConfigurationException("Plug-in event representation class '" + className + "' and URI '" + eventRepURI + "' did not initialize correctly : " + e.Message, e);
                }
            }
    
            // Add plug-in event type names
            IDictionary<String, ConfigurationPlugInEventType> plugInNames = configSnapshot.PlugInEventTypes;
            foreach (KeyValuePair<String, ConfigurationPlugInEventType> entry in plugInNames)
            {
                String name = entry.Key;
                ConfigurationPlugInEventType config = entry.Value;
                eventAdapterService.AddPlugInEventType(name, config.EventRepresentationResolutionURIs, config.Initializer);
            }
        }

        private static IDictionary<string, ICollection<string>> ToTypesReferences<T>(IDictionary<string, T> mapTypeConfigurations)
            where T : ConfigurationEventTypeWithSupertype
        {
            var result = new LinkedHashMap<string, ICollection<string>>();
            foreach (var entry in mapTypeConfigurations)
            {
                result[entry.Key] = entry.Value.SuperTypes;
            }
            return result;
        }

        /// <summary>Constructs the auto import service. </summary>
        /// <param name="configSnapshot">config info</param>
        /// <returns>service</returns>
        internal static EngineImportService MakeEngineImportService(ConfigurationInformation configSnapshot)
        {
            var expression = configSnapshot.EngineDefaults.ExpressionConfig;
            var engineImportService = new EngineImportServiceImpl(
                expression.IsExtendedAggregation,
                expression.IsUdfCache,
                expression.IsDuckTyping,
                configSnapshot.EngineDefaults.LanguageConfig.IsSortUsingCollator,
                expression.MathContext,
                expression.TimeZone,
                configSnapshot.EngineDefaults.ExecutionConfig.ThreadingProfile);
            engineImportService.AddMethodRefs(configSnapshot.MethodInvocationReferences);
    
            // Add auto-imports
            try
            {
                foreach (var importName in configSnapshot.Imports)
                {
                    engineImportService.AddImport(importName);
                }
    
                foreach (ConfigurationPlugInAggregationFunction config in configSnapshot.PlugInAggregationFunctions)
                {
                    engineImportService.AddAggregation(config.Name, config);
                }
    
                
                foreach (ConfigurationPlugInAggregationMultiFunction config in configSnapshot.PlugInAggregationMultiFunctions)
                {
                    engineImportService.AddAggregationMultiFunction(config);
                }

                foreach (ConfigurationPlugInSingleRowFunction config in configSnapshot.PlugInSingleRowFunctions)
                {
                    engineImportService.AddSingleRow(config.Name, config.FunctionClassName, config.FunctionMethodName, config.ValueCache, config.FilterOptimizable, config.RethrowExceptions);
                }
            }
            catch (EngineImportException ex)
            {
                throw new ConfigurationException("Error configuring engine: " + ex.Message, ex);
            }
    
            return engineImportService;
        }
    
        /// <summary>Creates the database config service. </summary>
        /// <param name="configSnapshot">is the config snapshot</param>
        /// <param name="schedulingService">is the timer stuff</param>
        /// <param name="schedulingMgmtService">for statement schedule management</param>
        /// <returns>database config svc</returns>
        internal static DatabaseConfigService MakeDatabaseRefService(ConfigurationInformation configSnapshot,
                                                              SchedulingService schedulingService,
                                                              SchedulingMgmtService schedulingMgmtService)
        {
            DatabaseConfigService databaseConfigService;
    
            // Add auto-imports
            try
            {
                ScheduleBucket allStatementsBucket = schedulingMgmtService.AllocateBucket();
                databaseConfigService = new DatabaseConfigServiceImpl(configSnapshot.DatabaseReferences, schedulingService, allStatementsBucket);
            }
            catch (ArgumentException ex)
            {
                throw new ConfigurationException("Error configuring engine: " + ex.Message, ex);
            }
    
            return databaseConfigService;
        }
    
        private static IDictionary<String, Object> CreatePropertyTypes(Properties properties)
        {
            IDictionary<String, Object> propertyTypes = new LinkedHashMap<String, Object>();
            foreach (var entry in properties)
            {
                var property = entry.Key;
                var className = entry.Value;
                var clazz = ResolveTypeForTypeName(className);
                if (clazz != null)
                {
                    propertyTypes[property] = clazz;
                }
            }
            return propertyTypes;
        }

        private static IDictionary<String, Object> ResolveClassesForStringPropertyTypes(IDictionary<String, Object> properties)
        {
            IDictionary<String, Object> propertyTypes = new LinkedHashMap<String, Object>();
            foreach (var entry in properties)
            {
                var property = entry.Key;

                if (entry.Value is string)
                {
                    var className = (string)entry.Value;
                    var clazz = ResolveTypeForTypeName(className);
                    if (clazz != null)
                    {
                        propertyTypes[property] = clazz;
                    }
                }
                else
                {
                    propertyTypes[property] = entry.Value;
                }
            }

            return propertyTypes;
        }

        private static Type ResolveTypeForTypeName(String type)
        {
            bool isArray = false;
            if (type != null && EventTypeUtility.IsPropertyArray(type))
            {
                isArray = true;
                type = EventTypeUtility.GetPropertyRemoveArray(type);
            }

            if (type == null)
            {
                throw new ConfigurationException("A null value has been provided for the type");
            }

            var clazz = TypeHelper.GetTypeForSimpleName(type);
            if (clazz == null)
            {
                throw new ConfigurationException("The type '" + type + "' is not a recognized type");
            }

            if (isArray)
            {
                clazz = Array.CreateInstance(clazz, 0).GetType();
            }
            return clazz;
        }
    }
}
