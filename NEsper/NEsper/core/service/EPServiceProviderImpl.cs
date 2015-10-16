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
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.deploy;
using com.espertech.esper.core.thread;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.metric;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.filter;
using com.espertech.esper.plugin;
using com.espertech.esper.schedule;
using com.espertech.esper.script;
using com.espertech.esper.timer;
using com.espertech.esper.util;

namespace com.espertech.esper.core.service
{
    using TimerCallback = timer.TimerCallback;
    using Version = util.Version;

    /// <summary>
    /// Service provider encapsulates the engine's services for runtime and administration interfaces.
    /// </summary>
    public class EPServiceProviderImpl : EPServiceProviderSPI
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Occurs before an <seealso cref="EPServiceProvider"/> is destroyed.
        /// </summary>
        public event EventHandler<ServiceProviderEventArgs> ServiceDestroyRequested;
        /// <summary>
        /// Occurs after an <seealso cref="EPServiceProvider"/> is initialized.
        /// </summary>
        public event EventHandler<ServiceProviderEventArgs> ServiceInitialized;
        /// <summary>
        /// Occurs when a statement created.
        /// </summary>
        public event EventHandler<StatementStateEventArgs> StatementCreate;
        /// <summary>
        /// Occurs when a statement state changes.
        /// </summary>
        public event EventHandler<StatementStateEventArgs> StatementStateChange;

        private volatile EPServiceEngine _engine;
        private ConfigurationInformation _configSnapshot;
        private StatementEventDispatcherUnthreaded _stmtEventDispatcher;
        private readonly IDictionary<String, EPServiceProviderSPI> _runtimes;
    
        /// <summary>Constructor - initializes services. </summary>
        /// <param name="configuration">is the engine configuration</param>
        /// <param name="engineURI">is the engine URI or "default" (or null which it assumes as "default") if this is the default provider</param>
        /// <param name="runtimes">map of URI and runtime</param>
        /// <throws>ConfigurationException is thrown to indicate a configuraton error</throws>
        public EPServiceProviderImpl(Configuration configuration, String engineURI, IDictionary<String, EPServiceProviderSPI> runtimes)
        {
            if (configuration == null)
            {
                throw new NullReferenceException("Unexpected null value received for configuration");
            }
            if (engineURI == null)
            {
            	throw new NullReferenceException("Engine URI should not be null at this stage");
            }
            _runtimes = runtimes;
            URI = engineURI;
            VerifyConfiguration(configuration);
    
            _configSnapshot = TakeSnapshot(configuration);
            DoInitialize(null);
        }
    
        public EPServiceProviderIsolated GetEPServiceIsolated(String name)
        {
            lock (this)
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }

                if (!_engine.Services.ConfigSnapshot.EngineDefaults.ExecutionConfig.IsAllowIsolatedService)
                {
                    throw new EPServiceNotAllowedException("Isolated runtime requires execution setting to allow isolated services, please change execution settings under engine defaults");
                }
                if (_engine.Services.ConfigSnapshot.EngineDefaults.ViewResourcesConfig.IsShareViews)
                {
                    throw new EPServiceNotAllowedException("Isolated runtime requires view sharing disabled, set engine defaults under view resources and share views to false");
                }

                if (_engine.Services.ConfigSnapshot.EngineDefaults.ViewResourcesConfig.IsShareViews)
                {
                    throw new EPException(
                        "Isolated runtime requires view sharing disabled, set engine defaults under view resources and share views to false");
                }
                if (name == null)
                {
                    throw new ArgumentException("Name parameter does not have a value provided");
                }

                return _engine.Services.StatementIsolationService.GetIsolationUnit(name, null);
            }
        }
    
        /// <summary>Invoked after an initialize operation. </summary>
        public void PostInitialize()
        {
            // plugin-loaders
            var pluginLoaders = _engine.Services.ConfigSnapshot.PluginLoaders;
            foreach (ConfigurationPluginLoader config in pluginLoaders)  // in the order configured
            {
                try
                {
                    var plugin = (PluginLoader) _engine.Services.EngineEnvContext.Lookup("plugin-loader/" + config.LoaderName);
                    plugin.PostInitialize();
                }
                catch (Exception ex)
                {
                    String message = "Error post-initializing plugin class " + config.TypeName + ": " + ex.Message;
                    Log.Error(message, ex);
                    throw new EPException(message, ex);
                }
            }
        }
    
        /// <summary>Sets engine configuration information for use in the next initialize. </summary>
        /// <param name="configuration">is the engine configs</param>
        public void SetConfiguration(Configuration configuration)
        {
            VerifyConfiguration(configuration);
            _configSnapshot = TakeSnapshot(configuration);
        }
    
        private void VerifyConfiguration(Configuration configuration)
        {
            if (configuration.EngineDefaults.ExecutionConfig.IsPrioritized)
            {
                if (!configuration.EngineDefaults.ViewResourcesConfig.IsShareViews)
                {
                    Log.Info("Setting engine setting for share-views to false as execution is prioritized");
                }
                configuration.EngineDefaults.ViewResourcesConfig.IsShareViews = false;
            }
        }

        public string URI { get; private set; }

        public EPRuntime EPRuntime
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Runtime;
            }
        }

        public EPAdministrator EPAdministrator
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Admin;
            }
        }

        public EPServicesContext ServicesContext
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services;
            }
        }

        public ThreadingService ThreadingService
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.ThreadingService;
            }
        }

        public EventAdapterService EventAdapterService
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.EventAdapterService;
            }
        }

        public SchedulingService SchedulingService
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.SchedulingService;
            }
        }

        public FilterService FilterService
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.FilterService;
            }
        }

        public TimerService TimerService
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.TimerService;
            }
        }

        public ConfigurationInformation ConfigurationInformation
        {
            get { return _configSnapshot; }
        }

        public NamedWindowService NamedWindowService
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.NamedWindowService;
            }
        }

        public TableService TableService
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.TableService;
            }
        }

        public EngineLevelExtensionServicesContext ExtensionServicesContext
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.EngineLevelExtensionServicesContext;
            }
        }

        public StatementLifecycleSvc StatementLifecycleSvc
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.StatementLifecycleSvc;
            }
        }

        public MetricReportingService MetricReportingService
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.MetricsReportingService;
            }
        }

        public ValueAddEventService ValueAddEventService
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.ValueAddEventService;
            }
        }

        public StatementEventTypeRef StatementEventTypeRef
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.StatementEventTypeRefService;
            }
        }

        public client.Directory EngineEnvContext
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.EngineEnvContext;
            }
        }

        public client.Directory Directory
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.EngineEnvContext;
            }
        }

        public StatementContextFactory StatementContextFactory
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.StatementContextFactory;
            }
        }

        public StatementIsolationService StatementIsolationService
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.StatementIsolationService;
            }
        }

        public DeploymentStateService DeploymentStateService
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.DeploymentStateService;
            }
        }

        /// <summary>
        /// Gets the scripting service.
        /// </summary>
        /// <value>The scripting service.</value>
        public ScriptingService ScriptingService
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }

                return _engine.Services.ScriptingService;
            }
        }

        public void Dispose()
        {
            lock(this)
            {
                if (_engine != null)
                {
                    Log.Info("Destroying engine URI '" + URI + "'");

                    try
                    {
                        if (ServiceDestroyRequested != null)
                        {
                            ServiceDestroyRequested(this, new ServiceProviderEventArgs(this));
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Exception caught during an ServiceDestroyRequested event:" + ex.Message, ex);
                    }

                    // assign null value
                    EPServiceEngine engineToDestroy = _engine;

                    engineToDestroy.Services.TimerService.StopInternalClock(false);
                    // Give the timer thread a little moment to catch up
                    Thread.Sleep(100);

                    // plugin-loaders - destroy in opposite order
                    IList<ConfigurationPluginLoader> pluginLoaders =
                        engineToDestroy.Services.ConfigSnapshot.PluginLoaders;
                    if (pluginLoaders.IsNotEmpty())
                    {
                        var reversed = new List<ConfigurationPluginLoader>(pluginLoaders);
                        reversed.Reverse();
                        foreach (ConfigurationPluginLoader config in reversed)
                        {
                            PluginLoader plugin;
                            try
                            {
                                plugin =
                                    (PluginLoader)
                                    engineToDestroy.Services.EngineEnvContext.Lookup("plugin-loader/" +
                                                                                     config.LoaderName);
                                plugin.Dispose();
                            }
                            catch (Exception e)
                            {
                                Log.Error("Error destroying plug-in loader: " + config.LoaderName, e);
                            }
                        }
                    }

                    engineToDestroy.Services.ThreadingService.Dispose();

                    // assign null - making EPRuntime and EPAdministrator unobtainable
                    _engine = null;

                    engineToDestroy.Runtime.Dispose();
                    engineToDestroy.Admin.Dispose();
                    engineToDestroy.Services.Dispose();
                    _runtimes.Remove(URI);

                    engineToDestroy.Services.Initialize();
                }
            }
        }

        public bool IsDestroyed
        {
            get { return _engine == null; }
        }

        public void Initialize() {
            InitializeInternal(null);
        }

        public void Initialize(long? currentTime)
        {
            InitializeInternal(currentTime);
        }

        private void InitializeInternal(long? currentTime)
        {
            DoInitialize(currentTime);
            PostInitialize();
        }
    
        /// <summary>Performs the initialization. </summary>
        protected void DoInitialize(long? startTime)
        {
            Log.Info("Initializing engine URI '{0}' version {1}", URI, Version.VERSION);
    
            // This setting applies to all engines in a given VM
            ExecutionPathDebugLog.IsEnabled = _configSnapshot.EngineDefaults.LoggingConfig.IsEnableExecutionDebug;
            ExecutionPathDebugLog.IsTimerDebugEnabled = _configSnapshot.EngineDefaults.LoggingConfig.IsEnableTimerDebug;
    
            // This setting applies to all engines in a given VM
            MetricReportingPath.IsMetricsEnabled = _configSnapshot.EngineDefaults.MetricsReportingConfig.IsEnableMetricsReporting;
    
            // This setting applies to all engines in a given VM
            AuditPath.AuditPattern = _configSnapshot.EngineDefaults.LoggingConfig.AuditPattern;

            // This setting applies to all engines in a given VM
            ThreadingOption.IsThreadingEnabled = ThreadingOption.IsThreadingEnabled ||
                                                 _configSnapshot.EngineDefaults.ThreadingConfig.IsThreadPoolTimerExec ||
                                                 _configSnapshot.EngineDefaults.ThreadingConfig.IsThreadPoolInbound ||
                                                 _configSnapshot.EngineDefaults.ThreadingConfig.IsThreadPoolRouteExec ||
                                                 _configSnapshot.EngineDefaults.ThreadingConfig.IsThreadPoolOutbound;
            
            if (_engine != null)
            {
                _engine.Services.TimerService.StopInternalClock(false);
                // Give the timer thread a little moment to catch up
                Thread.Sleep(100);
    
                _engine.Runtime.Initialize();
    
                _engine.Services.Dispose();
            }
    
            // Make EP services context factory
            String epServicesContextFactoryClassName = _configSnapshot.EPServicesContextFactoryClassName;
            EPServicesContextFactory epServicesContextFactory;
            if (epServicesContextFactoryClassName == null)
            {
                // Check system properties
                epServicesContextFactoryClassName = Environment.GetEnvironmentVariable("ESPER_EPSERVICE_CONTEXT_FACTORY_CLASS");
            }
            if (epServicesContextFactoryClassName == null)
            {
                epServicesContextFactory = new EPServicesContextFactoryDefault();
            }
            else
            {
                Type clazz;
                try
                {
                    clazz = TypeHelper.ResolveType(epServicesContextFactoryClassName, true);
                }
                catch (TypeLoadException)
                {
                    throw new ConfigurationException("Class '" + epServicesContextFactoryClassName + "' cannot be loaded");
                }
    
                Object obj;
                try
                {
                    obj = Activator.CreateInstance(clazz);
                }
                catch (TypeLoadException)
                {
                    throw new ConfigurationException("Class '" + clazz + "' cannot be instantiated");
                }
                catch (MissingMethodException e)
                {
                    throw new ConfigurationException(
                        "Error instantiating class - Default constructor was not found", e);
                }
                catch (MethodAccessException e)
                {
                    throw new ConfigurationException(
                        "Error instantiating class - Caller does not have permission to use constructor", e);
                }
                catch (ArgumentException e)
                {
                    throw new ConfigurationException("Error instantiating class - Type is not a RuntimeType", e);
                }
    
                epServicesContextFactory = (EPServicesContextFactory) obj;
            }
    
            EPServicesContext services = epServicesContextFactory.CreateServicesContext(this, _configSnapshot);
    
            // New runtime
            EPRuntimeSPI runtimeSPI;
            InternalEventRouteDest routeDest;
            TimerCallback timerCallback;
            String runtimeClassName = _configSnapshot.EngineDefaults.AlternativeContextConfig.Runtime;
            if (runtimeClassName == null)
            {
                // Check system properties
                runtimeClassName = Environment.GetEnvironmentVariable("ESPER_EPRUNTIME_CLASS");
            }

            if (runtimeClassName == null)
            {
                EPRuntimeImpl runtimeImpl = new EPRuntimeImpl(services);
                runtimeSPI = runtimeImpl;
                routeDest = runtimeImpl;
                timerCallback = runtimeImpl.TimerCallback;
            }
            else
            {
                Type clazz;
                try
                {
                    clazz = TypeHelper.ResolveType(runtimeClassName, true);
                }
                catch (TypeLoadException)
                {
                    throw new ConfigurationException("Class '" + runtimeClassName + "' cannot be loaded");
                }
    
                Object obj;
                try
                {
                    ConstructorInfo c = clazz.GetConstructor(new[] {typeof (EPServicesContext)});
                    obj = c.Invoke(new object[] {services});
                }
                catch (TypeLoadException)
                {
                    throw new ConfigurationException("Class '" + clazz + "' cannot be instantiated");
                }
                catch (MissingMethodException)
                {
                    throw new ConfigurationException("Class '" + clazz + "' cannot be instantiated, constructor accepting services was not found");
                }
                catch (MethodAccessException)
                {
                    throw new ConfigurationException("Illegal access instantiating class '" + clazz + "'");
                }
    
                runtimeSPI = (EPRuntimeSPI) obj;
                routeDest = (InternalEventRouteDest) obj;
                timerCallback = (TimerCallback) obj;
            }

            routeDest.InternalEventRouter = services.InternalEventRouter;
            services.InternalEventEngineRouteDest = routeDest;
    
            // set current time, if applicable
            if (startTime != null)
            {
                services.SchedulingService.Time = startTime.Value;
            }
    
            // Configure services to use the new runtime
            services.TimerService.Callback = timerCallback;
    
            // Statement lifecycle init
            services.StatementLifecycleSvc.Init();
            // Filter service init
            services.FilterService.Init();

            // Schedule service init
            services.SchedulingService.Init();
    
            // New admin
            var configOps = new ConfigurationOperationsImpl(
                services.EventAdapterService, services.EventTypeIdGenerator, services.EngineImportService,
                services.VariableService, services.EngineSettingsService, services.ValueAddEventService,
                services.MetricsReportingService, services.StatementEventTypeRefService,
                services.StatementVariableRefService, services.PlugInViews, services.FilterService,
                services.PatternSubexpressionPoolSvc,
                services.MatchRecognizeStatePoolEngineSvc, 
                services.TableService);
            var defaultStreamSelector = _configSnapshot.EngineDefaults.StreamSelectionConfig.DefaultStreamSelector.MapFromSODA();
            EPAdministratorSPI adminSPI;
            var adminClassName = _configSnapshot.EngineDefaults.AlternativeContextConfig.Admin;
            var adminContext = new EPAdministratorContext(runtimeSPI, services, configOps, defaultStreamSelector);
            if (adminClassName == null)
            {
                // Check system properties
                adminClassName = Environment.GetEnvironmentVariable("ESPER_EPADMIN_CLASS");
            }
            if (adminClassName == null)
            {
                adminSPI = new EPAdministratorImpl(adminContext);
            }
            else
            {
                Type clazz;
                try
                {
                    clazz = TypeHelper.ResolveType(adminClassName, true);
                }
                catch (TypeLoadException)
                {
                    throw new ConfigurationException("Class '" + epServicesContextFactoryClassName + "' cannot be loaded");
                }
    
                Object obj;
                try
                {
                    ConstructorInfo c = clazz.GetConstructor(new[] {typeof (EPAdministratorContext)});
                    obj = c.Invoke(new[] {adminContext});
                }
                catch (TypeLoadException)
                {
                    throw new ConfigurationException("Class '" + clazz + "' cannot be instantiated");
                }
                catch (MissingMethodException)
                {
                    throw new ConfigurationException("Class '" + clazz + "' cannot be instantiated, constructor accepting context was not found");
                }
                catch (MethodAccessException)
                {
                    throw new ConfigurationException("Illegal access instantiating class '" + clazz + "'");
                }
    
                adminSPI = (EPAdministratorSPI) obj;
            }
    
            // Start clocking
            if (_configSnapshot.EngineDefaults.ThreadingConfig.IsInternalTimerEnabled)
            {
                services.TimerService.StartInternalClock();
            }
    
            // Give the timer thread a little moment to start up
            Thread.Sleep(100);
    
            // Save engine instance
            _engine = new EPServiceEngine(services, runtimeSPI, adminSPI);
    
            // Load and initialize adapter loader classes
            LoadAdapters(services);
    
            // Initialize extension services
            if (services.EngineLevelExtensionServicesContext != null)
            {
                services.EngineLevelExtensionServicesContext.Init(services, runtimeSPI, adminSPI);
            }
    
            // Start metrics reporting, if any
            if (_configSnapshot.EngineDefaults.MetricsReportingConfig.IsEnableMetricsReporting)
            {
                services.MetricsReportingService.SetContext(runtimeSPI, services);
            }

            // register with the statement lifecycle service
            services.StatementLifecycleSvc.LifecycleEvent += HandleLifecycleEvent;
    
            // call initialize listeners
            try
            {
                if (ServiceInitialized != null)
                {
                    ServiceInitialized(this, new ServiceProviderEventArgs(this));
                }
            }
            catch (Exception ex)
            {
                Log.Error("Runtime exception caught during an ServiceInitialized event:" + ex.Message, ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="StatementCreate"/> event.
        /// </summary>
        /// <param name="e">The <see cref="StatementLifecycleEvent"/> instance containing the event data.</param>
        protected virtual void OnStatementCreate(StatementStateEventArgs e)
        {
            if (StatementCreate != null)
                StatementCreate(this, new StatementStateEventArgs(this, e.Statement));
        }

        /// <summary>
        /// Raises the <see cref="StatementStateChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="StatementLifecycleEvent"/> instance containing the event data.</param>
        protected virtual void OnStatementStateChanged(StatementLifecycleEvent e)
        {
            if (StatementStateChange != null)
                StatementStateChange(this, new StatementStateEventArgs(this, e.Statement));
        }

        /// <summary>
        /// Handles the lifecycle event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="StatementLifecycleEvent"/> instance containing the event data.</param>
        private void HandleLifecycleEvent(object sender, StatementLifecycleEvent e)
        {
            switch (e.EventType)
            {
                case StatementLifecycleEvent.LifecycleEventType.CREATE:
                    OnStatementCreate(new StatementStateEventArgs(this, e.Statement));
                    break;
                case StatementLifecycleEvent.LifecycleEventType.STATECHANGE:
                    OnStatementStateChanged(e);
                    break;
            }
        }

        /// <summary>Loads and initializes adapter loaders. </summary>
        /// <param name="services">is the engine instance services</param>
        private void LoadAdapters(EPServicesContext services)
        {
            IList<ConfigurationPluginLoader> pluginLoaders = _configSnapshot.PluginLoaders;
            if ((pluginLoaders == null) || (pluginLoaders.Count == 0))
            {
                return;
            }
            foreach (ConfigurationPluginLoader config in pluginLoaders)
            {
                String className = config.TypeName;
                Type pluginLoaderClass;
                try
                {
                    pluginLoaderClass = TypeHelper.ResolveType(className, true);
                }
                catch (TypeLoadException e)
                {
                    throw new ConfigurationException("Failed to load adapter loader class '" + className + "'", e);
                }

                Object pluginLoaderObj;
                try
                {
                    pluginLoaderObj = Activator.CreateInstance(pluginLoaderClass);
                }
                catch (TypeLoadException)
                {
                    throw new ConfigurationException("Class '" + pluginLoaderClass + "' cannot be instantiated");
                }
                catch (MissingMethodException e)
                {
                    throw new ConfigurationException(
                        "Error instantiating class - Default constructor was not found", e);
                }
                catch (MethodAccessException e)
                {
                    throw new ConfigurationException(
                        "Error instantiating class - Caller does not have permission to use constructor", e);
                }
                catch (ArgumentException e)
                {
                    throw new ConfigurationException("Error instantiating class - Type is not a RuntimeType", e);
                }
    
                if (!(pluginLoaderObj is PluginLoader)) {
                    throw new ConfigurationException("Failed to cast adapter loader class '" + className + "' to " + typeof(PluginLoader).FullName);
                }
    
                var pluginLoader = (PluginLoader) pluginLoaderObj;
                var context = new PluginLoaderInitContext(config.LoaderName, config.ConfigProperties, config.ConfigurationXML, this);
                pluginLoader.Init(context);
    
                // register adapter loader in JNDI context tree
                services.EngineEnvContext.Bind("plugin-loader/" + config.LoaderName, pluginLoader);
            }
        }
    
        private class EPServiceEngine
        {
            public EPServiceEngine(EPServicesContext services, EPRuntimeSPI runtimeSPI, EPAdministratorSPI admin)
            {
                Services = services;
                Runtime = runtimeSPI;
                Admin = admin;
            }

            public EPServicesContext Services { get; private set; }

            public EPRuntimeSPI Runtime { get; private set; }

            public EPAdministratorSPI Admin { get; private set; }
        }
    
        private static ConfigurationInformation TakeSnapshot(Configuration configuration)
        {
            try
            {
                return (ConfigurationInformation) SerializableObjectCopier.Copy(configuration);
            }
            catch (IOException e)
            {
                throw new ConfigurationException("Failed to snapshot configuration instance through serialization : " + e.Message, e);
            }
            catch (TypeLoadException e)
            {
                throw new ConfigurationException("Failed to snapshot configuration instance through serialization : " + e.Message, e);
            }
        }
    
        /// <summary>
        /// Clears the service state event handlers.  For internal use only.
        /// </summary>
        public virtual void RemoveAllServiceStateEventHandlers()
        {
            ServiceInitialized = null;
            ServiceDestroyRequested = null;
        }

        public IList<string> EPServiceIsolatedNames
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.StatementIsolationService.IsolationUnitNames;
            }
        }

        public SchedulingMgmtService SchedulingMgmtService
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.SchedulingMgmtService;
            }
        }

        public EngineImportService EngineImportService
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.EngineImportService;
            }
        }

        public TimeProvider TimeProvider
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.SchedulingService;
            }
        }

        public VariableService VariableService
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.VariableService;
            }
        }

        public ContextManagementService ContextManagementService
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.ContextManagementService;
            }
        }

        public IReaderWriterLock EngineInstanceWideLock
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(URI);
                }
                return _engine.Services.EventProcessingRwLock;
            }
        }
    }
}
