///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.activator;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.deploy;
using com.espertech.esper.core.service.multimatch;
using com.espertech.esper.core.start;
using com.espertech.esper.core.thread;
using com.espertech.esper.dataflow.core;
using com.espertech.esper.epl.agg.factory;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.db;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.metric;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events.avro;
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
using com.espertech.esper.view;
using com.espertech.esper.view.stream;
using Castle.MicroKernel.Registration;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Factory for services context.
    /// </summary>
    public class EPServicesContextFactoryDefault : EPServicesContextFactory
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static ExceptionHandlingService InitExceptionHandling(
            string engineURI,
            ConfigurationEngineDefaults.ExceptionHandlingConfig exceptionHandling,
            ConfigurationEngineDefaults.ConditionHandlingConfig conditionHandling,
            EngineImportService engineImportService)
        {
            IList<ExceptionHandler> exceptionHandlers;
            if (exceptionHandling.HandlerFactories == null || exceptionHandling.HandlerFactories.IsEmpty())
            {
                exceptionHandlers = Collections.GetEmptyList<ExceptionHandler>();
            }
            else
            {
                exceptionHandlers = new List<ExceptionHandler>();
                var context = new ExceptionHandlerFactoryContext(engineURI);
                foreach (var className in exceptionHandling.HandlerFactories)
                {
                    try
                    {
                        var factory = TypeHelper.Instantiate<ExceptionHandlerFactory>(
                            className, engineImportService.GetClassForNameProvider());
                        var handler = factory.GetHandler(context);
                        if (handler == null)
                        {
                            Log.Warn(
                                "Exception handler factory '" + className +
                                "' returned a null handler, skipping factory");
                            continue;
                        }
                        exceptionHandlers.Add(handler);
                    }
                    catch (Exception ex)
                    {
                        throw new ConfigurationException(
                            "Exception initializing exception handler from exception handler factory '" + className +
                            "': " + ex.Message, ex);
                    }
                }
            }

            IList<ConditionHandler> conditionHandlers;
            if (conditionHandling.HandlerFactories == null || conditionHandling.HandlerFactories.IsEmpty())
            {
                conditionHandlers = Collections.GetEmptyList<ConditionHandler>();
            }
            else
            {
                conditionHandlers = new List<ConditionHandler>();
                var context = new ConditionHandlerFactoryContext(engineURI);
                foreach (var className in conditionHandling.HandlerFactories)
                {
                    try
                    {
                        var factory = TypeHelper.Instantiate<ConditionHandlerFactory>(
                            className, engineImportService.GetClassForNameProvider());
                        var handler = factory.GetHandler(context);
                        if (handler == null)
                        {
                            Log.Warn(
                                "Condition handler factory '" + className +
                                "' returned a null handler, skipping factory");
                            continue;
                        }
                        conditionHandlers.Add(handler);
                    }
                    catch (Exception ex)
                    {
                        throw new ConfigurationException(
                            "Exception initializing exception handler from exception handler factory '" + className +
                            "': " + ex.Message, ex);
                    }
                }
            }

            return new ExceptionHandlingService(
                engineURI,
                exceptionHandlers,
                conditionHandlers);
        }

        /// <summary>
        /// Makes the time source provider.
        /// </summary>
        /// <param name="configSnapshot">the configuration</param>
        /// <returns>time source provider</returns>
        internal static TimeSourceService MakeTimeSource(ConfigurationInformation configSnapshot)
        {
            if (configSnapshot.EngineDefaults.TimeSource.TimeSourceType ==
                ConfigurationEngineDefaults.TimeSourceType.NANO)
            {
                // this is a static variable to keep overhead down for getting a current time
                TimeSourceServiceImpl.IsSystemCurrentTime = false;
            }
            return new TimeSourceServiceImpl();
        }

        /// <summary>
        /// Adds configured variables to the variable service.
        /// </summary>
        /// <param name="variableService">service to add to</param>
        /// <param name="variables">configured variables</param>
        /// <param name="engineImportService">engine imports</param>
        internal static void InitVariables(
            VariableService variableService,
            IDictionary<string, ConfigurationVariable> variables,
            EngineImportService engineImportService)
        {
            foreach (var entry in variables)
            {
                try
                {
                    var arrayType = TypeHelper.IsGetArrayType(entry.Value.VariableType);
                    variableService.CreateNewVariable(
                        null, entry.Key, arrayType.First, entry.Value.IsConstant, arrayType.Second, false,
                        entry.Value.InitializationValue, engineImportService);
                    variableService.AllocateVariableState(
                        entry.Key, EPStatementStartMethodConst.DEFAULT_AGENT_INSTANCE_ID, null, false);
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

        /// <summary>
        /// Initialize event adapter service for config snapshot.
        /// </summary>
        /// <param name="eventAdapterService">is events adapter</param>
        /// <param name="configSnapshot">is the config snapshot</param>
        /// <param name="engineImportService">engine import service</param>
        /// <param name="resourceManager">The resource manager.</param>
        /// <exception cref="ConfigurationException">
        /// Error configuring engine: " + ex.Message
        /// or
        /// Error configuring engine: " + ex.Message
        /// or
        /// or
        /// Error configuring engine: " + ex.Message
        /// or
        /// Error configuring engine, dependency graph between map type names is circular: " + e.Message
        /// or
        /// Error configuring engine: " + ex.Message
        /// or
        /// Error configuring engine, dependency graph between object array type names is circular: " +
        ///                     e.Message
        /// or
        /// Error configuring engine: " + ex.Message
        /// or
        /// Failed to load plug-in event representation class '" + className + "'
        /// or
        /// Failed to instantiate plug-in event representation class '" + className +
        ///                         "' via default constructor
        /// or
        /// Failed to instantiate plug-in event representation class '" + className +
        ///                         "' via default constructor
        /// or
        /// Illegal access to instantiate plug-in event representation class '" + className +
        ///                         "' via default constructor
        /// or
        /// Illegal access to instantiate plug-in event representation class '" + className +
        ///                         "' via default constructor
        /// or
        /// Plug-in event representation class '" + className +
        ///                         "' does not implement the required interface " + typeof (PlugInEventRepresentation).Name
        /// or
        /// Plug-in event representation class '" + className + "' and URI '" + eventRepURI +
        ///                         "' did not initialize correctly : " + e.Message
        /// </exception>
        internal static void Init(
            EventAdapterService eventAdapterService,
            ConfigurationInformation configSnapshot,
            EngineImportService engineImportService,
            IResourceManager resourceManager)
        {
            // Extract legacy event type definitions for each event type name, if supplied.
            //
            // We supply this information as setup information to the event adapter service
            // to allow discovery of superclasses and interfaces during event type construction for bean events,
            // such that superclasses and interfaces can use the legacy type definitions.
            var classLegacyInfo = new Dictionary<string, ConfigurationEventTypeLegacy>();
            foreach (var entry in configSnapshot.EventTypeNames)
            {
                var typeName = entry.Key;
                var className = entry.Value;
                var legacyDef = configSnapshot.EventTypesLegacy.Get(typeName);
                if (legacyDef != null)
                {
                    classLegacyInfo.Put(className, legacyDef);
                }
            }
            eventAdapterService.TypeLegacyConfigs = classLegacyInfo;
            eventAdapterService.DefaultPropertyResolutionStyle =
                configSnapshot.EngineDefaults.EventMeta.ClassPropertyResolutionStyle;
            eventAdapterService.DefaultAccessorStyle = configSnapshot.EngineDefaults.EventMeta.DefaultAccessorStyle;

            foreach (var typeNamespace in configSnapshot.EventTypeAutoNamePackages)
            {
                eventAdapterService.AddAutoNamePackage(typeNamespace);
            }

            // Add from the configuration the event class names
            var typeNames = configSnapshot.EventTypeNames;
            foreach (var entry in typeNames)
            {
                // Add class
                try
                {
                    var typeName = entry.Key;
                    eventAdapterService.AddBeanType(typeName, entry.Value, false, true, true, true);
                }
                catch (EventAdapterException ex)
                {
                    throw new ConfigurationException("Error configuring engine: " + ex.Message, ex);
                }
            }

            // Add from the configuration the Java event class names
            var avroNames = configSnapshot.EventTypesAvro;
            foreach (var entry in avroNames)
            {
                try
                {
                    eventAdapterService.AddAvroType(entry.Key, entry.Value, true, true, true, false, false);
                }
                catch (EventAdapterException ex)
                {
                    throw new ConfigurationException("Error configuring engine: " + ex.Message, ex);
                }
            }

            // Add from the configuration the XML DOM names and type def
            var xmlDOMNames = configSnapshot.EventTypesXMLDOM;
            foreach (var entry in xmlDOMNames)
            {
                SchemaModel schemaModel = null;
                if ((entry.Value.SchemaResource != null) || (entry.Value.SchemaText != null))
                {
                    try
                    {
                        schemaModel = XSDSchemaMapper.LoadAndMap(
                            entry.Value.SchemaResource, 
                            entry.Value.SchemaText, 
                            engineImportService,
                            resourceManager);
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
            ICollection<string> dependentMapOrder;
            try
            {
                var typesReferences = ToTypesReferences(configSnapshot.MapTypeConfigurations);
                dependentMapOrder = GraphUtil.GetTopDownOrder(typesReferences);
            }
            catch (GraphCircularDependencyException e)
            {
                throw new ConfigurationException(
                    "Error configuring engine, dependency graph between map type names is circular: " + e.Message, e);
            }

            var mapNames = configSnapshot.EventTypesMapEvents;
            var nestableMapNames = configSnapshot.EventTypesNestableMapEvents;
            dependentMapOrder.AddAll(mapNames.Keys);
            dependentMapOrder.AddAll(nestableMapNames.Keys);
            try
            {
                foreach (var mapName in dependentMapOrder)
                {
                    var mapConfig = configSnapshot.MapTypeConfigurations.Get(mapName);
                    var propertiesUnnested = mapNames.Get(mapName);
                    if (propertiesUnnested != null)
                    {
                        var propertyTypes = CreatePropertyTypes(propertiesUnnested, engineImportService);
                        var propertyTypesCompiled = EventTypeUtility.CompileMapTypeProperties(
                            propertyTypes, eventAdapterService);
                        eventAdapterService.AddNestableMapType(
                            mapName, propertyTypesCompiled, mapConfig, true, true, true, false, false);
                    }

                    var propertiesNestable = nestableMapNames.Get(mapName);
                    if (propertiesNestable != null)
                    {
                        var propertiesNestableCompiled = EventTypeUtility.CompileMapTypeProperties(
                            propertiesNestable, eventAdapterService);
                        eventAdapterService.AddNestableMapType(
                            mapName, propertiesNestableCompiled, mapConfig, true, true, true, false, false);
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
                    "Error configuring engine, dependency graph between object array type names is circular: " +
                    e.Message, e);
            }
            var nestableObjectArrayNames = configSnapshot.EventTypesNestableObjectArrayEvents;
            dependentObjectArrayOrder.AddAll(nestableObjectArrayNames.Keys);
            try
            {
                foreach (var objectArrayName in dependentObjectArrayOrder)
                {
                    var objectArrayConfig = configSnapshot.ObjectArrayTypeConfigurations.Get(objectArrayName);
                    var propertyTypes = nestableObjectArrayNames.Get(objectArrayName);
                    propertyTypes = ResolveClassesForStringPropertyTypes(propertyTypes, engineImportService);
                    var propertyTypesCompiled = EventTypeUtility.CompileMapTypeProperties(
                        propertyTypes, eventAdapterService);
                    eventAdapterService.AddNestableObjectArrayType(
                        objectArrayName, propertyTypesCompiled, objectArrayConfig, true, true, true, false, false, false,
                        null);
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
                    throw new ConfigurationException(
                        "Failed to load plug-in event representation class '" + className + "'", ex);
                }

                Object pluginEventRepObj;
                try
                {
                    pluginEventRepObj = Activator.CreateInstance(eventRepClass);
                }
                catch (TypeInstantiationException ex)
                {
                    throw new ConfigurationException(
                        "Failed to instantiate plug-in event representation class '" + className +
                        "' via default constructor", ex);
                }
                catch (TargetInvocationException ex)
                {
                    throw new ConfigurationException(
                        "Failed to instantiate plug-in event representation class '" + className +
                        "' via default constructor", ex);
                }
                catch (MethodAccessException ex)
                {
                    throw new ConfigurationException(
                        "Illegal access to instantiate plug-in event representation class '" + className +
                        "' via default constructor", ex);
                }
                catch (MemberAccessException ex)
                {
                    throw new ConfigurationException(
                        "Illegal access to instantiate plug-in event representation class '" + className +
                        "' via default constructor", ex);
                }

                if (!(pluginEventRepObj is PlugInEventRepresentation))
                {
                    throw new ConfigurationException(
                        "Plug-in event representation class '" + className +
                        "' does not implement the required interface " + typeof (PlugInEventRepresentation).Name);
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
                    throw new ConfigurationException(
                        "Plug-in event representation class '" + className + "' and URI '" + eventRepURI +
                        "' did not initialize correctly : " + e.Message, e);
                }
            }

            // Add plug-in event type names
            var plugInNames = configSnapshot.PlugInEventTypes;
            foreach (var entry in plugInNames)
            {
                var name = entry.Key;
                var config = entry.Value;
                eventAdapterService.AddPlugInEventType(
                    name, config.EventRepresentationResolutionURIs, config.Initializer);
            }
        }

        private static IDictionary<string, ICollection<string>> ToTypesReferences<T>(
            IDictionary<string, T> mapTypeConfigurations)
            where T : ConfigurationEventTypeWithSupertype
        {
            var result = new Dictionary<string, ICollection<string>>();
            foreach (var entry in mapTypeConfigurations)
            {
                result[entry.Key] = entry.Value.SuperTypes;
            }
            return result;
        }

        /// <summary>
        /// Constructs the auto import service.
        /// </summary>
        /// <param name="classLoaderProvider">The class loader provider.</param>
        /// <param name="configSnapshot">config INFO</param>
        /// <param name="aggregationFactoryFactory">factory of aggregation service provider</param>
        /// <param name="engineURI">The engine URI.</param>
        /// <returns>
        /// service
        /// </returns>
        /// <exception cref="ConfigurationException">Invalid time-source time unit of " + timeUnit + ", expected millis or micros
        /// or
        /// Error configuring engine: " + ex.Message</exception>
        internal static EngineImportService MakeEngineImportService(
            ClassLoaderProvider classLoaderProvider,
            ConfigurationInformation configSnapshot,
            AggregationFactoryFactory aggregationFactoryFactory,
            String engineURI)
        {
            TimeUnit timeUnit = configSnapshot.EngineDefaults.TimeSource.TimeUnit;
            TimeAbacus timeAbacus;
            if (timeUnit == TimeUnit.MILLISECONDS)
            {
                timeAbacus = TimeAbacusMilliseconds.INSTANCE;
            }
            else if (timeUnit == TimeUnit.MICROSECONDS)
            {
                timeAbacus = TimeAbacusMicroseconds.INSTANCE;
            }
            else
            {
                throw new ConfigurationException(
                    "Invalid time-source time unit of " + timeUnit + ", expected millis or micros");
            }

            var codegenGetters = configSnapshot.EngineDefaults.Execution.CodeGeneration.IsEnablePropertyGetter;
            if (codegenGetters)
            {
                if (CheckPackageName("uri_" + engineURI) == PackageName.INVALID)
                {
                    throw new ConfigurationException("Invalid engine URI '" + engineURI + "', code generation requires an engine URI that is a valid Java-language identifier and may not contain Java language keywords");
                }
            }

            var expression = configSnapshot.EngineDefaults.Expression;
            var engineImportService = new EngineImportServiceImpl(
                expression.IsExtendedAggregation,
                expression.IsUdfCache, expression.IsDuckTyping,
                configSnapshot.EngineDefaults.Language.IsSortUsingCollator,
                configSnapshot.EngineDefaults.Expression.MathContext,
                configSnapshot.EngineDefaults.Expression.TimeZone, timeAbacus,
                configSnapshot.EngineDefaults.Execution.ThreadingProfile,
                configSnapshot.TransientConfiguration,
                aggregationFactoryFactory,
                codegenGetters,
                engineURI,
                null,
                classLoaderProvider);

            engineImportService.AddMethodRefs(configSnapshot.MethodInvocationReferences);

            // Add auto-imports
            try
            {
                foreach (var importName in configSnapshot.Imports)
                {
                    engineImportService.AddImport(importName);
                }

                foreach (var importName in configSnapshot.AnnotationImports)
                {
                    engineImportService.AddAnnotationImport(importName);
                }

                foreach (var config in configSnapshot.PlugInAggregationFunctions)
                {
                    engineImportService.AddAggregation(config.Name, config);
                }

                foreach (var config in configSnapshot.PlugInAggregationMultiFunctions)
                {
                    engineImportService.AddAggregationMultiFunction(config);
                }

                foreach (var config in configSnapshot.PlugInSingleRowFunctions)
                {
                    engineImportService.AddSingleRow(
                        config.Name, config.FunctionClassName, config.FunctionMethodName, config.ValueCache,
                        config.FilterOptimizable, config.IsRethrowExceptions, config.EventTypeName);
                }
            }
            catch (EngineImportException ex)
            {
                throw new ConfigurationException("Error configuring engine: " + ex.Message, ex);
            }

            return engineImportService;
        }

        /// <summary>
        /// Creates the database config service.
        /// </summary>
        /// <param name="configSnapshot">is the config snapshot</param>
        /// <param name="schedulingService">is the timer stuff</param>
        /// <param name="schedulingMgmtService">for statement schedule management</param>
        /// <param name="engineImportService">engine import service</param>
        /// <returns>database config svc</returns>
        internal static DatabaseConfigService MakeDatabaseRefService(
            ConfigurationInformation configSnapshot,
            SchedulingService schedulingService,
            SchedulingMgmtService schedulingMgmtService,
            EngineImportService engineImportService)
        {
            DatabaseConfigService databaseConfigService;

            // Add auto-imports
            try
            {
                var allStatementsBucket = schedulingMgmtService.AllocateBucket();
                databaseConfigService = new DatabaseConfigServiceImpl(
                    configSnapshot.DatabaseReferences, schedulingService, allStatementsBucket, engineImportService);
            }
            catch (ArgumentException ex)
            {
                throw new ConfigurationException("Error configuring engine: " + ex.Message, ex);
            }

            return databaseConfigService;
        }

        private static IDictionary<string, Object> CreatePropertyTypes(
            Properties properties,
            EngineImportService engineImportService)
        {
            var propertyTypes = new Dictionary<string, Object>();
            foreach (var entry in properties)
            {
                var property = entry.Key;
                var className = entry.Value;
                var clazz = ResolveClassForTypeName(className, engineImportService);
                if (clazz != null)
                {
                    propertyTypes.Put(property, clazz);
                }
            }
            return propertyTypes;
        }

        private static IDictionary<string, Object> ResolveClassesForStringPropertyTypes(
            IDictionary<string, Object> properties,
            EngineImportService engineImportService)
        {
            var propertyTypes = new Dictionary<string, Object>();
            foreach (var entry in properties)
            {
                var property = entry.Key;
                propertyTypes.Put(property, entry.Value);
                if (!(entry.Value is string))
                {
                    continue;
                }
                var className = (string) entry.Value;
                var clazz = ResolveClassForTypeName(className, engineImportService);
                if (clazz != null)
                {
                    propertyTypes.Put(property, clazz);
                }
            }
            return propertyTypes;
        }

        private static Type ResolveClassForTypeName(string type, EngineImportService engineImportService)
        {
            var isArray = false;
            if (type != null && EventTypeUtility.IsPropertyArray(type))
            {
                isArray = true;
                type = EventTypeUtility.GetPropertyRemoveArray(type);
            }

            if (type == null)
            {
                throw new ConfigurationException("A null value has been provided for the type");
            }
            Type clazz = TypeHelper.GetTypeForSimpleName(type);
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

        public EPServicesContext CreateServicesContext(
            IContainer container,
            EPServiceProvider epServiceProvider,
            ConfigurationInformation configSnapshot)
        {
            var lockManager = container.LockManager();
            var rwLockManager = container.RWLockManager();
            var threadLocalManager = container.ThreadLocalManager();
            var resourceManager = container.ResourceManager();

            // Directory for binding resources
            var resourceDirectory = container.Resolve<Directory>();
            
            // Engine import service
            var engineImportService = MakeEngineImportService(
                container.Resolve<ClassLoaderProvider>(),
                configSnapshot,
                AggregationFactoryFactoryDefault.INSTANCE,
                epServiceProvider.URI);

            // Event Type Id Generation
            EventTypeIdGenerator eventTypeIdGenerator;
            if (configSnapshot.EngineDefaults.AlternativeContext == null ||
                configSnapshot.EngineDefaults.AlternativeContext.EventTypeIdGeneratorFactory == null)
            {
                eventTypeIdGenerator = new EventTypeIdGeneratorImpl();
            }
            else
            {
                var eventTypeIdGeneratorFactory = TypeHelper.Instantiate<EventTypeIdGeneratorFactory>(
                    configSnapshot.EngineDefaults.AlternativeContext.EventTypeIdGeneratorFactory,
                    engineImportService.GetClassForNameProvider());
                eventTypeIdGenerator = eventTypeIdGeneratorFactory.Create(
                    new EventTypeIdGeneratorContext(epServiceProvider.URI));
            }

            // Make services that depend on snapshot config entries
            EventAdapterAvroHandler avroHandler = EventAdapterAvroHandlerUnsupported.INSTANCE;
            if (configSnapshot.EngineDefaults.EventMeta.AvroSettings.IsEnableAvro)
            {
                try
                {
                    avroHandler = TypeHelper.Instantiate<EventAdapterAvroHandler>(
                        EventAdapterAvroHandlerConstants.HANDLER_IMPL, engineImportService.GetClassForNameProvider());
                }
                catch (Exception e)
                {
                    Log.Debug(
                        "Avro provider {0} not instantiated, not enabling Avro support: {1}",
                        EventAdapterAvroHandlerConstants.HANDLER_IMPL, e.Message);
                }
                try
                {
                    avroHandler.Init(configSnapshot.EngineDefaults.EventMeta.AvroSettings, engineImportService);
                }
                catch (Exception e)
                {
                    throw new ConfigurationException("Failed to initialize Esper-Avro: " + e.Message, e);
                }
            }

            var eventAdapterService = new EventAdapterServiceImpl(
                container,
                eventTypeIdGenerator,
                configSnapshot.EngineDefaults.EventMeta.AnonymousCacheSize,
                avroHandler,
                engineImportService);
            Init(eventAdapterService, configSnapshot, engineImportService, resourceManager);

            // New read-write lock for concurrent event processing
            var eventProcessingRwLock = rwLockManager.CreateLock(GetType());

            var timeSourceService = MakeTimeSource(configSnapshot);
            var schedulingService = SchedulingServiceProvider.NewService(timeSourceService, lockManager);
            var schedulingMgmtService = new SchedulingMgmtServiceImpl();
            var engineSettingsService = new EngineSettingsService(
                configSnapshot.EngineDefaults, configSnapshot.PlugInEventTypeResolutionURIs);
            var databaseConfigService = MakeDatabaseRefService(
                configSnapshot, schedulingService, schedulingMgmtService, engineImportService);

            var plugInViews = new PluggableObjectCollection();
            plugInViews.AddViews(
                configSnapshot.PlugInViews, configSnapshot.PlugInVirtualDataWindows, engineImportService);
            var plugInPatternObj = new PluggableObjectCollection();
            plugInPatternObj.AddPatternObjects(configSnapshot.PlugInPatternObjects, engineImportService);

            // exception handling
            var exceptionHandlingService = InitExceptionHandling(
                epServiceProvider.URI, configSnapshot.EngineDefaults.ExceptionHandling,
                configSnapshot.EngineDefaults.ConditionHandling, engineImportService);

            // Statement context factory
            Type systemVirtualDWViewFactory = null;
            if (configSnapshot.EngineDefaults.AlternativeContext.VirtualDataWindowViewFactory != null)
            {
                try
                {
                    systemVirtualDWViewFactory =
                        engineImportService.GetClassForNameProvider()
                            .ClassForName(configSnapshot.EngineDefaults.AlternativeContext.VirtualDataWindowViewFactory);
                    if (!TypeHelper.IsImplementsInterface(systemVirtualDWViewFactory, typeof (VirtualDataWindowFactory)))
                    {
                        throw new ConfigurationException(
                            "Type " + systemVirtualDWViewFactory.Name + " does not implement the interface " +
                            typeof (VirtualDataWindowFactory).Name);
                    }
                }
                catch (TypeLoadException)
                {
                    throw new ConfigurationException("Failed to look up class " + systemVirtualDWViewFactory);
                }
            }
            var statementContextFactory = new StatementContextFactoryDefault(
                plugInViews, plugInPatternObj, systemVirtualDWViewFactory);

            var msecTimerResolution = configSnapshot.EngineDefaults.Threading.InternalTimerMsecResolution;
            if (msecTimerResolution <= 0)
            {
                throw new ConfigurationException(
                    "Timer resolution configuration not set to a valid value, expecting a non-zero value");
            }
            var timerService = new TimerServiceImpl(epServiceProvider.URI, msecTimerResolution);

            var variableService = new VariableServiceImpl(
                container,
                configSnapshot.EngineDefaults.Variables.MsecVersionRelease,
                schedulingService,
                eventAdapterService,
                null);

            InitVariables(variableService, configSnapshot.Variables, engineImportService);

            var tableService = new TableServiceImpl(container);

            var statementLockFactory = new StatementLockFactoryImpl(
                rwLockManager,
                configSnapshot.EngineDefaults.Execution.IsFairlock,
                configSnapshot.EngineDefaults.Execution.IsDisableLocking
            );

            var streamFactoryService = StreamFactoryServiceProvider.NewService(
                epServiceProvider.URI, configSnapshot.EngineDefaults.ViewResources.IsShareViews);
            var filterService = FilterServiceProvider.NewService(
                lockManager,
                rwLockManager,
                configSnapshot.EngineDefaults.Execution.FilterServiceProfile,
                configSnapshot.EngineDefaults.Execution.IsAllowIsolatedService);
            var metricsReporting = new MetricReportingServiceImpl(
                configSnapshot.EngineDefaults.MetricsReporting,
                epServiceProvider.URI,
                rwLockManager);
            var namedWindowMgmtService = new NamedWindowMgmtServiceImpl(
                configSnapshot.EngineDefaults.Logging.IsEnableQueryPlan,
                metricsReporting);
            var namedWindowDispatchService = new NamedWindowDispatchServiceImpl(
                schedulingService, variableService, tableService,
                engineSettingsService.EngineSettings.Execution.IsPrioritized, eventProcessingRwLock,
                exceptionHandlingService, metricsReporting, threadLocalManager);

            var valueAddEventService = new ValueAddEventServiceImpl(lockManager);
            valueAddEventService.Init(
                configSnapshot.RevisionEventTypes, configSnapshot.VariantStreams, eventAdapterService,
                eventTypeIdGenerator);

            var statementEventTypeRef = new StatementEventTypeRefImpl(rwLockManager);
            var statementVariableRef = new StatementVariableRefImpl(
                variableService, tableService, namedWindowMgmtService, rwLockManager);

            var threadingService = new ThreadingServiceImpl(configSnapshot.EngineDefaults.Threading);

            var internalEventRouterImpl = new InternalEventRouterImpl(epServiceProvider.URI);

            var statementIsolationService = new StatementIsolationServiceImpl();

            var deploymentStateService = new DeploymentStateServiceImpl(lockManager);

            StatementMetadataFactory stmtMetadataFactory;
            if (configSnapshot.EngineDefaults.AlternativeContext.StatementMetadataFactory == null)
            {
                stmtMetadataFactory = new StatementMetadataFactoryDefault();
            }
            else
            {
                stmtMetadataFactory = TypeHelper.Instantiate<StatementMetadataFactory>(
                    configSnapshot.EngineDefaults.AlternativeContext.StatementMetadataFactory,
                    engineImportService.GetClassForNameProvider());
            }

            var contextManagementService = new ContextManagementServiceImpl();

            PatternSubexpressionPoolEngineSvc patternSubexpressionPoolSvc = null;
            if (configSnapshot.EngineDefaults.Patterns.MaxSubexpressions != null)
            {
                patternSubexpressionPoolSvc =
                    new PatternSubexpressionPoolEngineSvc(
                        configSnapshot.EngineDefaults.Patterns.MaxSubexpressions.Value,
                        configSnapshot.EngineDefaults.Patterns.IsMaxSubexpressionPreventStart);
            }

            MatchRecognizeStatePoolEngineSvc matchRecognizeStatePoolEngineSvc = null;
            if (configSnapshot.EngineDefaults.MatchRecognize.MaxStates != null)
            {
                matchRecognizeStatePoolEngineSvc =
                    new MatchRecognizeStatePoolEngineSvc(
                        configSnapshot.EngineDefaults.MatchRecognize.MaxStates.Value,
                        configSnapshot.EngineDefaults.MatchRecognize.IsMaxStatesPreventStart);
            }

            var scriptingService = new ScriptingServiceImpl();
            scriptingService.DiscoverEngines();

            // New services context

            var services = new EPServicesContext(
                container, epServiceProvider.URI,
                schedulingService,
                eventAdapterService,
                engineImportService,
                engineSettingsService,
                databaseConfigService, 
                plugInViews,
                statementLockFactory,
                eventProcessingRwLock, 
                null,
                resourceDirectory,
                statementContextFactory,
                plugInPatternObj,
                timerService,
                filterService,
                streamFactoryService,
                namedWindowMgmtService,
                namedWindowDispatchService,
                variableService,
                tableService,
                timeSourceService,
                valueAddEventService,
                metricsReporting,
                statementEventTypeRef,
                statementVariableRef, 
                configSnapshot, 
                threadingService, 
                internalEventRouterImpl,
                statementIsolationService, 
                schedulingMgmtService,
                deploymentStateService,
                exceptionHandlingService,
                new PatternNodeFactoryImpl(),
                eventTypeIdGenerator,
                stmtMetadataFactory,
                contextManagementService,
                patternSubexpressionPoolSvc,
                matchRecognizeStatePoolEngineSvc,
                new DataFlowServiceImpl(epServiceProvider, new DataFlowConfigurationStateServiceImpl(), lockManager),
                new ExprDeclaredServiceImpl(lockManager),
                new ContextControllerFactoryFactorySvcImpl(),
                new ContextManagerFactoryServiceImpl(),
                new EPStatementFactoryDefault(),
                new RegexHandlerFactoryDefault(container),
                new ViewableActivatorFactoryDefault(),
                new FilterNonPropertyRegisteryServiceImpl(),
                new ResultSetProcessorHelperFactoryImpl(),
                new ViewServicePreviousFactoryImpl(),
                new EventTableIndexServiceImpl(),
                new EPRuntimeIsolatedFactoryImpl(),
                new FilterBooleanExpressionFactoryImpl(),
                new DataCacheFactory(),
                new MultiMatchHandlerFactoryImpl(),
                NamedWindowConsumerMgmtServiceImpl.INSTANCE,
                AggregationFactoryFactoryDefault.INSTANCE,
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
            statementIsolationService.SetEpServicesContext(services);

            return services;
        }

        private static PackageName CheckPackageName(String name)
        {
            var ret = PackageName.INVALID;
            int index = 0, dotex = -1;

            if (String.IsNullOrWhiteSpace(name))
                return ret;
            if (name[0] == '.')
                return ret;

            var codePoint = name.ToCharArray();
            while (index <= codePoint.Length)
            {
                if (index == codePoint.Length)
                {
                    if (codePoint[index - 1] == '.')
                    {
                        return PackageName.INVALID;
                    }

                    int start = dotex + 1;
                    int end = index;
                    var test = name.Between(start, end);
                    if (!(Array.BinarySearch(RESERVED, test) < 0))
                    {
                        return PackageName.INVALID;
                    }

                    if (!(ret == PackageName.QUALIFIED))
                        ret = PackageName.SIMPLE;

                    return ret;
                }
                if (codePoint[index] == '.')
                {
                    if (codePoint[index - 1] == '.')
                    {
                        return PackageName.INVALID;
                    }
                    else
                    {
                        int start = dotex + 1;
                        int end = index;
                        var test = name.Between(start, end);
                        if (!(Array.BinarySearch(RESERVED, test) < 0))
                            return ret;
                        dotex = index;
                        ret = PackageName.QUALIFIED;
                    }
                }
                /*
                else if (Character.isJavaIdentifierStart(codePoint[index]))
                {
                    if (needStart) needStart = false;
                }
                else if (!Character.isJavaIdentifierPart(codePoint[index]))
                {
                    ret = Namespace.INVALID;
                    break escape;
                }
                */
                index++;
            }

            return ret;
        }

        private static string[] RESERVED = new String[]
        {
            "abstract", "assert", "boolean", "break", "byte",
            "case", "catch", "char", "class", "const", "continue", "default", "do",
            "double", "else", "enum", "extends", "false", "final", "finally",
            "float", "for", "if", "goto", "implements", "import", "instanceof",
            "int", "interface", "long", "native", "new", "null", "package",
            "private", "protected", "public", "return", "short", "static",
            "strictfp", "super", "switch", "synchronized", "this", "throw",
            "throws", "transient", "true", "try", "void", "volatile", "while"
        };

        public enum PackageName
        {
            SIMPLE,
            QUALIFIED,
            INVALID
        }
    }
} // end of namespace
