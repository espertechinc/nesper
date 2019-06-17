///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.configuration.runtime;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.metric;
using com.espertech.esper.common.client.render;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.client.variable;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.eventtyperepo;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.directory;
using com.espertech.esper.compat.function;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.option;
using com.espertech.esper.runtime.client.plugin;
using com.espertech.esper.runtime.client.util;
using com.espertech.esper.runtime.@internal.kernel.statement;
using com.espertech.esper.runtime.@internal.kernel.thread;
using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    /// <summary>
    ///     Service provider encapsulates the runtime's services for runtime and administration interfaces.
    /// </summary>
    public class EPRuntimeImpl : EPRuntimeSPI
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private Configuration configAtInitialization;
        private Configuration configLastProvided;

        private volatile EPRuntimeEnv runtimeEnvironment;
        private readonly IDictionary<string, EPRuntimeSPI> runtimes;
        private readonly CopyOnWriteArraySet<EPRuntimeStateListener> serviceListeners;

        /// <summary>
        ///     Constructor - initializes services.
        /// </summary>
        /// <param name="configuration">is the runtimeconfiguration</param>
        /// <param name="runtimeURI">
        ///     is the runtime URI or "default" (or null which it assumes as "default") if this is the default
        ///     provider
        /// </param>
        /// <param name="runtimes">map of URI and runtime</param>
        /// <throws>ConfigurationException is thrown to indicate a configuraton error</throws>
        public EPRuntimeImpl(
            Configuration configuration,
            string runtimeURI,
            IDictionary<string, EPRuntimeSPI> runtimes)
        {
            if (configuration == null) {
                throw new ArgumentNullException(nameof(configuration), "Unexpected null value received for configuration");
            }

            this.runtimes = runtimes;
            URI = runtimeURI ?? throw new ArgumentNullException(nameof(runtimeURI), "runtime URI should not be null at this stage");

            serviceListeners = new CopyOnWriteArraySet<EPRuntimeStateListener>();

            configLastProvided = TakeSnapshot(configuration);

            DoInitialize(null);
        }

        /// <summary>
        ///     Invoked after an initialize operation.
        /// </summary>
        public void PostInitialize()
        {
            // plugin-loaders
            var pluginLoaders = runtimeEnvironment.Services.ConfigSnapshot.Runtime.PluginLoaders;
            // in the order configured
            foreach (var config in pluginLoaders) {
                try {
                    var plugin = (PluginLoader) runtimeEnvironment.Services.RuntimeEnvContext.Lookup("plugin-loader/" + config.LoaderName);
                    plugin.PostInitialize();
                }
                catch (Exception ex) {
                    var message = "Error post-initializing plugin class " + config.ClassName + ": " + ex.Message;
                    Log.Error(message, ex);
                    throw new EPException(message, ex);
                }
            }
        }

        /// <summary>
        ///     Sets runtime configuration information for use in the next initialize.
        /// </summary>
        /// <param name="configuration">is the runtimeconfigs</param>
        public void SetConfiguration(Configuration configuration)
        {
            configLastProvided = TakeSnapshot(configuration);
        }

        public string URI { get; }

        public EPEventService EventService {
            get {
                if (runtimeEnvironment == null) {
                    throw new EPRuntimeDestroyedException(URI);
                }

                return runtimeEnvironment.Runtime;
            }
        }

        public EPDeploymentService DeploymentService {
            get {
                if (runtimeEnvironment == null) {
                    throw new EPRuntimeDestroyedException(URI);
                }

                return runtimeEnvironment.DeploymentService;
            }
        }

        public EPServicesContext ServicesContext {
            get {
                if (runtimeEnvironment == null) {
                    throw new EPRuntimeDestroyedException(URI);
                }

                return runtimeEnvironment.Services;
            }
        }

        public Configuration ConfigurationDeepCopy => TakeSnapshot(configAtInitialization);

        public IDictionary<string, object> ConfigurationTransient => configLastProvided.Common.TransientConfiguration;

        public void Destroy()
        {
            lock (this) {
                if (runtimeEnvironment != null) {
                    Log.Info("Destroying runtime URI '" + URI + "'");

                    // first invoke listeners
                    foreach (var listener in serviceListeners) {
                        try {
                            listener.OnEPRuntimeDestroyRequested(this);
                        }
                        catch (Exception ex) {
                            Log.Error("Runtime exception caught during an onEPRuntimeDestroyRequested callback:" + ex.Message, ex);
                        }
                    }

                    if (configLastProvided.Runtime.MetricsReporting.IsRuntimeMetrics) {
                        DestroyEngineMetrics(runtimeEnvironment.Services.RuntimeURI);
                    }

                    // assign null value
                    var runtimeToDestroy = runtimeEnvironment;
                    runtimeToDestroy.Services.TimerService.StopInternalClock(false);

                    // plugin-loaders - destroy in opposite order
                    var pluginLoaders = runtimeToDestroy.Services.ConfigSnapshot.Runtime.PluginLoaders;
                    if (!pluginLoaders.IsEmpty()) {
                        var reversed = new List<ConfigurationRuntimePluginLoader>(pluginLoaders);
                        reversed.Reverse();
                        foreach (var config in reversed) {
                            PluginLoader plugin;
                            try {
                                plugin = (PluginLoader) runtimeToDestroy.Services.RuntimeEnvContext.Lookup("plugin-loader/" + config.LoaderName);
                                plugin.Dispose();
                            }
                            catch (Exception e) {
                                Log.Error("Error destroying plug-in loader: " + config.LoaderName, e);
                            }
                        }
                    }

                    runtimeToDestroy.Services.ThreadingService.Dispose();

                    // assign null - making EPRuntime and EPAdministrator unobtainable
                    runtimeEnvironment = null;

                    runtimeToDestroy.Runtime.Destroy();
                    runtimeToDestroy.DeploymentService.Destroy();
                    runtimeToDestroy.Services.Destroy();
                    runtimes.Remove(URI);

                    runtimeToDestroy.Services.Initialize();
                }
            }
        }

        public bool IsDestroyed => runtimeEnvironment == null;

        public void Initialize()
        {
            InitializeInternal(null);
        }

        public void Initialize(long? currentTime)
        {
            InitializeInternal(currentTime);
        }

        public INamingContext Context {
            get {
                if (runtimeEnvironment == null) {
                    throw new EPRuntimeDestroyedException(URI);
                }

                return runtimeEnvironment.Services.RuntimeEnvContext;
            }
        }

        public IReaderWriterLock RuntimeInstanceWideLock {
            get {
                if (runtimeEnvironment == null) {
                    throw new EPRuntimeDestroyedException(URI);
                }

                return runtimeEnvironment.Services.EventProcessingRWLock.Lock;
            }
        }

        public AtomicBoolean ServiceStatusProvider { get; private set; }

        public void AddRuntimeStateListener(EPRuntimeStateListener listener)
        {
            serviceListeners.Add(listener);
        }

        public bool RemoveRuntimeStateListener(EPRuntimeStateListener listener)
        {
            return serviceListeners.Remove(listener);
        }

        public EPEventServiceSPI EventServiceSPI {
            get {
                if (runtimeEnvironment == null) {
                    throw new EPRuntimeDestroyedException(URI);
                }

                return runtimeEnvironment.Runtime;
            }
        }

        public void RemoveAllRuntimeStateListeners()
        {
            serviceListeners.Clear();
        }

        public EPDataFlowService DataFlowService {
            get {
                if (runtimeEnvironment == null) {
                    throw new EPRuntimeDestroyedException(URI);
                }

                return runtimeEnvironment.Services.DataflowService;
            }
        }

        public EPContextPartitionService ContextPartitionService {
            get {
                if (runtimeEnvironment == null) {
                    throw new EPRuntimeDestroyedException(URI);
                }

                return runtimeEnvironment.ContextPartitionService;
            }
        }

        public EPVariableService VariableService {
            get {
                if (runtimeEnvironment == null) {
                    throw new EPRuntimeDestroyedException(URI);
                }

                return runtimeEnvironment.VariableService;
            }
        }

        public EPMetricsService MetricsService {
            get {
                if (runtimeEnvironment == null) {
                    throw new EPRuntimeDestroyedException(URI);
                }

                return runtimeEnvironment.MetricsService;
            }
        }

        public EPEventTypeService EventTypeService {
            get {
                if (runtimeEnvironment == null) {
                    throw new EPRuntimeDestroyedException(URI);
                }

                return runtimeEnvironment.EventTypeService;
            }
        }

        public EPRenderEventService RenderEventService {
            get {
                if (runtimeEnvironment == null) {
                    throw new EPRuntimeDestroyedException(URI);
                }

                return runtimeEnvironment.Services.EventRenderer;
            }
        }

        public EPFireAndForgetService FireAndForgetService {
            get {
                if (runtimeEnvironment == null) {
                    throw new EPRuntimeDestroyedException(URI);
                }

                return runtimeEnvironment.FireAndForgetService;
            }
        }

        public ThreadingService ThreadingService {
            get {
                if (runtimeEnvironment == null) {
                    throw new EPRuntimeDestroyedException(URI);
                }

                return runtimeEnvironment.Services.ThreadingService;
            }
        }

        public EPCompilerPathable RuntimePath {
            get {
                var services = runtimeEnvironment.Services;

                var variables = new VariableRepositoryPreconfigured();
                foreach (var entry in services.VariableManagementService.DeploymentsWithVariables) {
                    foreach (var variableEntry in entry.Value.Variables) {
                        if (variableEntry.Value.MetaData.IsPreconfigured) {
                            variables.AddVariable(variableEntry.Key, variableEntry.Value.MetaData);
                        }
                    }
                }

                var eventTypes = new EventTypeRepositoryImpl(true);
                foreach (var entry in services.EventTypeRepositoryBus.NameToTypeMap) {
                    if (entry.Value.Metadata.AccessModifier == NameAccessModifier.PRECONFIGURED) {
                        eventTypes.AddType(entry.Value);
                    }
                }

                return new EPCompilerPathableImpl(
                    services.VariablePathRegistry,
                    services.EventTypePathRegistry,
                    services.ExprDeclaredPathRegistry,
                    services.NamedWindowPathRegistry,
                    services.TablePathRegistry,
                    services.ContextPathRegistry,
                    services.ScriptPathRegistry,
                    eventTypes,
                    variables);
            }
        }

        private void InitializeInternal(long? currentTime)
        {
            DoInitialize(currentTime);
            PostInitialize();
        }

        /// <summary>
        ///     Performs the initialization.
        /// </summary>
        /// <param name="startTime">optional start time</param>
        protected void DoInitialize(long? startTime)
        {
            Log.Info("Initializing runtime URI '" + URI + "' version " + RuntimeVersion.VERSION);

            // Retain config-at-initialization since config-last-provided can be set to new values and "initialize" can be called
            configAtInitialization = configLastProvided;

            // Verify settings
            if (configLastProvided.Runtime.Threading.IsInternalTimerEnabled &&
                configLastProvided.Common.TimeSource.TimeUnit != TimeUnit.MILLISECONDS) {
                throw new ConfigurationException("Internal timer requires millisecond time resolution");
            }

            // This setting applies to all runtimes in a given VM
            ExecutionPathDebugLog.IsDebugEnabled = configLastProvided.Runtime.Logging.IsEnableExecutionDebug;
            ExecutionPathDebugLog.IsTimerDebugEnabled = configLastProvided.Runtime.Logging.IsEnableTimerDebug;

            // This setting applies to all runtimes in a given VM
            AuditPath.AuditPattern = configLastProvided.Runtime.Logging.AuditPattern;

            if (runtimeEnvironment != null) {
                if (ServiceStatusProvider != null) {
                    ServiceStatusProvider.Set(false);
                }

                runtimeEnvironment.Services.TimerService.StopInternalClock(false);

                if (configLastProvided.Runtime.MetricsReporting.IsRuntimeMetrics) {
                    DestroyEngineMetrics(runtimeEnvironment.Services.RuntimeURI);
                }

                runtimeEnvironment.Runtime.Initialize();

                runtimeEnvironment.Services.Destroy();
            }

            ServiceStatusProvider = new AtomicBoolean(true);
            // Make EP services context factory
            var epServicesContextFactoryClassName = configLastProvided.Runtime.EPServicesContextFactoryClassName;
            EPServicesContextFactory epServicesContextFactory;
            if (epServicesContextFactoryClassName == null) {
                // Check system properties
                epServicesContextFactoryClassName = Environment.GetEnvironmentVariable("ESPER_EPSERVICE_CONTEXT_FACTORY_CLASS");
            }

            if (epServicesContextFactoryClassName == null) {
                epServicesContextFactory = new EPServicesContextFactoryDefault(
                    ServicesContext.Container);
            }
            else {
                Type clazz;
                try {
                    clazz = TransientConfigurationResolver.ResolveClassForNameProvider(configLastProvided.Common.TransientConfiguration)
                        .ClassForName(epServicesContextFactoryClassName);
                }
                catch (TypeLoadException) {
                    throw new ConfigurationException("Class '" + epServicesContextFactoryClassName + "' cannot be loaded");
                }

                object obj;
                try {
                    obj = TypeHelper.Instantiate(clazz);
                }
                catch (Exception) {
                    throw new ConfigurationException("Class '" + clazz + "' cannot be instantiated");
                }

                epServicesContextFactory = (EPServicesContextFactory) obj;
            }

            EPServicesContext services;
            try {
                services = epServicesContextFactory.CreateServicesContext(this, configLastProvided);
            }
            catch (Exception ex) {
                throw new ConfigurationException("Failed runtime startup: " + ex.Message, ex);
            }

            // new runtime
            var eventService = epServicesContextFactory.CreateEPRuntime(services, ServiceStatusProvider);

            eventService.InternalEventRouter = services.InternalEventRouter;
            services.InternalEventRouteDest = eventService;

            // set current time, if applicable
            if (startTime != null) {
                services.SchedulingService.Time = startTime.Value;
            }

            // Configure services to use the new runtime
            services.TimerService.Callback = eventService;

            // New services
            EPDeploymentServiceSPI deploymentService = new EPDeploymentServiceImpl(services, this);
            var eventTypeService = new EPEventTypeServiceImpl(services);
            EPContextPartitionService contextPartitionService = new EPContextPartitionServiceImpl(services);
            EPVariableService variableService = new EPVariableServiceImpl(services);
            EPMetricsService metricsService = new EPMetricsServiceImpl(services);
            EPFireAndForgetService fireAndForgetService = new EPFireAndForgetServiceImpl(services, ServiceStatusProvider);

            // Build runtime environment
            runtimeEnvironment = new EPRuntimeEnv(
                services, eventService, deploymentService, eventTypeService, contextPartitionService, variableService, metricsService,
                fireAndForgetService);

            // Deployment Recovery
            var deploymentIterator = services.DeploymentRecoveryService.Deployments();
            ISet<EventType> protectedVisibleTypes = new LinkedHashSet<EventType>();
            while (deploymentIterator.MoveNext()) {
                var entry = deploymentIterator.Current;

                StatementUserObjectRuntimeOption userObjectResolver = new ProxyStatementUserObjectRuntimeOption {
                    ProcGetUserObject = env => entry.Value.UserObjectsRuntime.Get(env.StatementId)
                };

                StatementNameRuntimeOption statementNameResolver = new ProxyStatementNameRuntimeOption {
                    ProcGetStatementName = env => entry.Value.StatementNamesWhenProvidedByAPI.Get(env.StatementId)
                };

                StatementSubstitutionParameterOption substitutionParameterResolver = new ProxyStatementSubstitutionParameterOption {
                    ProcSetStatementParameters = env => {
                        var param = entry.Value.SubstitutionParameters.Get(env.StatementId);
                        if (param == null) {
                            return;
                        }

                        if (env.SubstitutionParameterNames != null) {
                            foreach (var name in env.SubstitutionParameterNames) {
                                env.SetObject(name.Key, param.Get(name.Value));
                            }
                        }
                        else {
                            for (var i = 0; i < env.SubstitutionParameterTypes.Length; i++) {
                                env.SetObject(i + 1, param.Get(i + 1));
                            }
                        }
                    }
                };

                DeploymentInternal deployerResult;
                try {
                    deployerResult = Deployer.DeployRecover(
                        entry.Key, entry.Value.StatementIdFirstStatement, entry.Value.Compiled, statementNameResolver, userObjectResolver,
                        substitutionParameterResolver, this);
                }
                catch (EPDeployException ex) {
                    throw new EPException(ex.Message, ex);
                }

                foreach (var eventType in deployerResult.DeploymentTypes.Values) {
                    if (eventType.Metadata.BusModifier == EventTypeBusModifier.BUS ||
                        eventType.Metadata.TypeClass == EventTypeTypeClass.NAMED_WINDOW ||
                        eventType.Metadata.TypeClass == EventTypeTypeClass.STREAM) {
                        protectedVisibleTypes.Add(eventType);
                    }
                }
            }

            // Listener Recovery
            var listenerIterator = services.ListenerRecoveryService.Listeners;
            while (listenerIterator.MoveNext()) {
                var deployment = listenerIterator.Current;
                var epStatement = services.StatementLifecycleService.GetStatementById(deployment.Key);
                epStatement.RecoveryUpdateListeners(new EPStatementListenerSet(deployment.Value));
            }

            // Filter service init
            ISet<EventType> filterServiceTypes = new LinkedHashSet<EventType>(services.EventTypeRepositoryBus.AllTypes);
            filterServiceTypes.AddAll(protectedVisibleTypes);
            Supplier<ICollection<EventType>> availableTypes = () => filterServiceTypes;
            services.FilterService.Init(availableTypes);

            // Schedule service init
            services.SchedulingService.Init();

            // Start clocking
            if (configLastProvided.Runtime.Threading.IsInternalTimerEnabled) {
                services.TimerService.StartInternalClock();
            }

            // Load and initialize adapter loader classes
            LoadAdapters(services);

            // Initialize extension services
            if (services.RuntimeExtensionServices != null) {
                ((RuntimeExtensionServicesSPI) services.RuntimeExtensionServices).Init(services, eventService, deploymentService);
            }

            // Start metrics reporting, if any
            if (configLastProvided.Runtime.MetricsReporting.IsEnableMetricsReporting) {
                services.MetricReportingService.SetContext(services.FilterService, services.SchedulingService, eventService);
            }

            // Start runtimes metrics report
            if (configLastProvided.Runtime.MetricsReporting.IsRuntimeMetrics) {
                StartEngineMetrics(services, eventService);
            }

            // call initialize listeners
            foreach (var listener in serviceListeners) {
                try {
                    listener.OnEPRuntimeInitialized(this);
                }
                catch (Exception ex) {
                    Log.Error("Runtime exception caught during an onEPRuntimeInitialized callback:" + ex.Message, ex);
                }
            }
        }

        private void StartEngineMetrics(
            EPServicesContext services,
            EPEventService runtime)
        {
#if FALSE
            lock (this) {
                var filterName = MetricNameFactory.Name(services.RuntimeURI, "filter");
                CommonJMXUtil.RegisterMbean(services.FilterService, filterName);
                var scheduleName = MetricNameFactory.Name(services.RuntimeURI, "schedule");
                CommonJMXUtil.RegisterMbean(services.SchedulingService, scheduleName);
                var runtimeName = MetricNameFactory.Name(services.RuntimeURI, "runtime");
                CommonJMXUtil.RegisterMbean(runtime, runtimeName);
            }
#endif
        }

        private void DestroyEngineMetrics(string runtimeURI)
        {
#if FALSE
            lock (this) {
                CommonJMXUtil.UnregisterMbean(MetricNameFactory.Name(runtimeURI, "filter"));
                CommonJMXUtil.UnregisterMbean(MetricNameFactory.Name(runtimeURI, "schedule"));
                CommonJMXUtil.UnregisterMbean(MetricNameFactory.Name(runtimeURI, "runtime"));
            }
#endif
        }

        /// <summary>
        ///     Loads and initializes adapter loaders.
        /// </summary>
        /// <param name="services">is the runtime instance services</param>
        private void LoadAdapters(EPServicesContext services)
        {
            var pluginLoaders = configLastProvided.Runtime.PluginLoaders;
            if (pluginLoaders == null || pluginLoaders.Count == 0) {
                return;
            }

            foreach (var config in pluginLoaders) {
                var className = config.ClassName;
                Type pluginLoaderClass;
                try {
                    pluginLoaderClass = services.ClassForNameProvider.ClassForName(className);
                }
                catch (TypeLoadException ex) {
                    throw new ConfigurationException("Failed to load adapter loader class '" + className + "'", ex);
                }

                object pluginLoaderObj;
                try {
                    pluginLoaderObj = TypeHelper.Instantiate(pluginLoaderClass);
                }
                catch (Exception ex) {
                    throw new ConfigurationException("Failed to instantiate adapter loader class '" + className + "' via default constructor", ex);
                }

                if (!(pluginLoaderObj is PluginLoader)) {
                    throw new ConfigurationException("Failed to cast adapter loader class '" + className + "' to " + typeof(PluginLoader).Name);
                }

                var pluginLoader = (PluginLoader) pluginLoaderObj;
                var context = new PluginLoaderInitContext(config.LoaderName, config.ConfigProperties, config.ConfigurationXML, this);
                pluginLoader.Init(context);

                // register adapter loader in JNDI context tree
                try {
                    services.RuntimeEnvContext.Bind("plugin-loader/" + config.LoaderName, pluginLoader);
                }
                catch (Exception e) {
                    throw new EPException("Failed to use context to bind adapter loader", e);
                }
            }
        }

        private Configuration TakeSnapshot(Configuration configuration)
        {
            try {
                // Allow variables to have non-serializable values by copying their initial value
                IDictionary<string, object> variableInitialValues = null;
                if (!configuration.Common.Variables.IsEmpty()) {
                    variableInitialValues = new Dictionary<string, object>();
                    foreach (var variable in configuration.Common.Variables) {
                        var initializationValue = variable.Value.InitializationValue;
                        if (initializationValue != null) {
                            variableInitialValues.Put(variable.Key, initializationValue);
                            variable.Value.InitializationValue = null;
                        }
                    }
                }

                // Avro schemas are not serializable
                IDictionary<string, ConfigurationCommonEventTypeAvro> avroSchemas = null;
                if (!configuration.Common.EventTypesAvro.IsEmpty()) {
                    avroSchemas = new LinkedHashMap<string, ConfigurationCommonEventTypeAvro>(
                        configuration.Common.EventTypesAvro);
                    configuration.Common.EventTypesAvro.Clear();
                }

                // Transient configuration may not be copy-able
                IDictionary<string, object> transients = null;
                if (!configuration.Common.TransientConfiguration.IsEmpty()) {
                    transients = new Dictionary<string, object>(
                        configuration.Common.TransientConfiguration);
                    // no need to clear, it is marked as transient
                }

                var copy = SerializableObjectCopier
                    .GetInstance(Container)
                    .Copy(configuration);

                // Restore transient
                if (transients != null) {
                    copy.Common.TransientConfiguration = transients;
                }
                else {
                    copy.Common.TransientConfiguration = new EmptyDictionary<string, object>();
                }

                // Restore variable with initial values
                if (variableInitialValues != null && !variableInitialValues.IsEmpty()) {
                    foreach (var entry in variableInitialValues) {
                        var config = copy.Common.Variables.Get(entry.Key);
                        config.InitializationValue = entry.Value;
                    }

                    foreach (var entry in variableInitialValues) {
                        var config = configuration.Common.Variables.Get(entry.Key);
                        config.InitializationValue = entry.Value;
                    }
                }

                // Restore Avro schemas
                if (avroSchemas != null) {
                    copy.Common.EventTypesAvro.PutAll(avroSchemas);
                    configuration.Common.EventTypesAvro.PutAll(avroSchemas);
                }

                return copy;
            }
            catch (IOException e) {
                throw new ConfigurationException("Failed to snapshot configuration instance through serialization : " + e.Message, e);
            }
            catch (TypeLoadException e) {
                throw new ConfigurationException("Failed to snapshot configuration instance through serialization : " + e.Message, e);
            }
        }

        public IContainer Container => ServicesContext.Container;
    }
} // end of namespace