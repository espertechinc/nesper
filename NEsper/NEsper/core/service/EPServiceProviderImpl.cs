///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
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
    using Directory = com.espertech.esper.client.Directory;
    using TimerCallback = com.espertech.esper.timer.TimerCallback;
    using Version = com.espertech.esper.util.Version;

    /// <summary>
    /// Service provider encapsulates the engine's services for runtime and administration interfaces.
    /// </summary>
    public class EPServiceProviderImpl : EPServiceProviderSPI
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
        private readonly IContainer _container;
        private ConfigurationInformation _configSnapshot;
        private readonly string _engineURI;
        private readonly IDictionary<string, EPServiceProviderSPI> _runtimes;

        /// <summary>
        /// Constructor - initializes services.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="configuration">is the engine configuration</param>
        /// <param name="engineURI">is the engine URI or "default" (or null which it assumes as "default") if this is the default provider</param>
        /// <param name="runtimes">map of URI and runtime</param>
        /// <exception cref="System.ArgumentNullException">
        /// configuration - Unexpected null value received for configuration
        /// or
        /// engineURI - Engine URI should not be null at this stage
        /// </exception>
        /// <exception cref="ConfigurationException">is thrown to indicate a configuraton error</exception>
        public EPServiceProviderImpl(
            IContainer container,
            Configuration configuration,
            string engineURI,
            IDictionary<string, EPServiceProviderSPI> runtimes)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration), "Unexpected null value received for configuration");
            }

            _container = container;
            _runtimes = runtimes;
            _engineURI = engineURI ?? throw new ArgumentNullException(nameof(engineURI), "Engine URI should not be null at this stage");
            VerifyConfiguration(configuration);

            _configSnapshot = TakeSnapshot(configuration);
            DoInitialize(null);
        }

        public EPServiceProviderIsolated GetEPServiceIsolated(string name)
        {
            lock (this)
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(_engineURI);
                }

                if (!_engine.Services.ConfigSnapshot.EngineDefaults.Execution.IsAllowIsolatedService)
                {
                    throw new EPServiceNotAllowedException(
                        "Isolated runtime requires execution setting to allow isolated services, please change execution settings under engine defaults");
                }
                if (_engine.Services.ConfigSnapshot.EngineDefaults.ViewResources.IsShareViews)
                {
                    throw new EPServiceNotAllowedException(
                        "Isolated runtime requires view sharing disabled, set engine defaults under view resources and share views to false");
                }
                if (name == null)
                {
                    throw new ArgumentException("Name parameter does not have a value provided");
                }

                return _engine.Services.StatementIsolationService.GetIsolationUnit(name, null);
            }
        }

        /// <summary>Invoked after an initialize operation.</summary>
        public void PostInitialize()
        {
            // plugin-loaders
            var pluginLoaders = _engine.Services.ConfigSnapshot.PluginLoaders;
            // in the order configured
            foreach (var config in pluginLoaders)
            {
                try
                {
                    var plugin =
                        (PluginLoader) _engine.Services.EngineEnvContext.Lookup("plugin-loader/" + config.LoaderName);
                    plugin.PostInitialize();
                }
                catch (Exception ex)
                {
                    var message = "Error post-initializing plugin class " + config.TypeName + ": " + ex.Message;
                    Log.Error(message, ex);
                    throw new EPException(message, ex);
                }
            }
        }

        /// <summary>
        /// Sets engine configuration information for use in the next initialize.
        /// </summary>
        /// <param name="configuration">is the engine configs</param>
        public void SetConfiguration(Configuration configuration)
        {
            VerifyConfiguration(configuration);
            _configSnapshot = TakeSnapshot(configuration);
        }

        private void VerifyConfiguration(Configuration configuration)
        {
            if (configuration.EngineDefaults.Execution.IsPrioritized)
            {
                if (!configuration.EngineDefaults.ViewResources.IsShareViews)
                {
                    Log.Info("Setting engine setting for share-views to false as execution is prioritized");
                }
                configuration.EngineDefaults.ViewResources.IsShareViews = false;
            }
        }

        public string URI
        {
            get { return _engineURI; }
        }

        public IContainer Container
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(_engineURI);
                }
                return _engine.Container;
            }
        }

        public EPRuntime EPRuntime
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(_engineURI);
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
                    throw new EPServiceDestroyedException(_engineURI);
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
                    throw new EPServiceDestroyedException(_engineURI);
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
                    throw new EPServiceDestroyedException(_engineURI);
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
                    throw new EPServiceDestroyedException(_engineURI);
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
                    throw new EPServiceDestroyedException(_engineURI);
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
                    throw new EPServiceDestroyedException(_engineURI);
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
                    throw new EPServiceDestroyedException(_engineURI);
                }
                return _engine.Services.TimerService;
            }
        }

        public ConfigurationInformation ConfigurationInformation
        {
            get { return _configSnapshot; }
        }

        public NamedWindowMgmtService NamedWindowMgmtService
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(_engineURI);
                }
                return _engine.Services.NamedWindowMgmtService;
            }
        }

        public TableService TableService
        {
            get
            {
                if (_engine == null)
                {
                    throw new EPServiceDestroyedException(_engineURI);
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
                    throw new EPServiceDestroyedException(_engineURI);
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
                    throw new EPServiceDestroyedException(_engineURI);
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
                    throw new EPServiceDestroyedException(_engineURI);
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
                    throw new EPServiceDestroyedException(_engineURI);
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
                    throw new EPServiceDestroyedException(_engineURI);
                }
                return _engine.Services.StatementEventTypeRefService;
            }
        }

        public Directory EngineEnvContext
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

        public Directory Directory
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
                    throw new EPServiceDestroyedException(_engineURI);
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
                    throw new EPServiceDestroyedException(_engineURI);
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
                    throw new EPServiceDestroyedException(_engineURI);
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
            lock (this)
            {
                if (_engine != null)
                {
                    Log.Info("Destroying engine URI '" + _engineURI + "'");

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

                    if (_configSnapshot.EngineDefaults.MetricsReporting.IsEnableMetricsReporting)
                    {
                        DestroyEngineMetrics(_engine.Services.EngineURI);
                    }

                    // assign null value
                    var engineToDestroy = _engine;

                    engineToDestroy.Services.TimerService.StopInternalClock(false);
                    // Give the timer thread a little moment to catch up
                    Thread.Sleep(100);

                    // plugin-loaders - destroy in opposite order
                    var pluginLoaders = engineToDestroy.Services.ConfigSnapshot.PluginLoaders;
                    if (!pluginLoaders.IsEmpty())
                    {
                        var reversed = new List<ConfigurationPluginLoader>(pluginLoaders);
                        reversed.Reverse();
                        foreach (var config in reversed)
                        {
                            try
                            {
                                using ((PluginLoader) engineToDestroy.Services.EngineEnvContext.Lookup("plugin-loader/" + config.LoaderName))
                                {
                                }
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
                    _runtimes.Remove(_engineURI);

                    engineToDestroy.Services.Initialize();
                }
            }
        }

        public bool IsDestroyed
        {
            get { return _engine == null; }
        }

        public void Initialize()
        {
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

        /// <summary>
        /// Performs the initialization.
        /// </summary>
        /// <param name="startTime">optional start time</param>
        protected void DoInitialize(long? startTime)
        {
            Log.Info("Initializing engine URI '" + _engineURI + "' version " + Version.VERSION);

            // This setting applies to all engines in a given VM
            ExecutionPathDebugLog.IsEnabled = _configSnapshot.EngineDefaults.Logging.IsEnableExecutionDebug;
            ExecutionPathDebugLog.IsTimerDebugEnabled = _configSnapshot.EngineDefaults.Logging.IsEnableTimerDebug;

            // This setting applies to all engines in a given VM
            MetricReportingPath.IsMetricsEnabled =
                _configSnapshot.EngineDefaults.MetricsReporting.IsEnableMetricsReporting;

            // This setting applies to all engines in a given VM
            AuditPath.AuditPattern = _configSnapshot.EngineDefaults.Logging.AuditPattern;

            // This setting applies to all engines in a given VM
            ThreadingOption.IsThreadingEnabled = (
                ThreadingOption.IsThreadingEnabled ||
                _configSnapshot.EngineDefaults.Threading.IsThreadPoolTimerExec ||
                _configSnapshot.EngineDefaults.Threading.IsThreadPoolInbound ||
                _configSnapshot.EngineDefaults.Threading.IsThreadPoolRouteExec ||
                _configSnapshot.EngineDefaults.Threading.IsThreadPoolOutbound
                );

            if (_engine != null)
            {
                _engine.Services.TimerService.StopInternalClock(false);
                // Give the timer thread a little moment to catch up
                Thread.Sleep(100);

                if (_configSnapshot.EngineDefaults.MetricsReporting.IsEnableMetricsReporting)
                {
                    DestroyEngineMetrics(_engine.Services.EngineURI);
                }

                _engine.Runtime.Initialize();

                _engine.Services.Dispose();
            }

            // Make EP services context factory
            var epServicesContextFactoryClassName = _configSnapshot.EPServicesContextFactoryClassName;
            EPServicesContextFactory epServicesContextFactory;
            if (epServicesContextFactoryClassName == null)
            {
                // Check system properties
                epServicesContextFactoryClassName =
                    Environment.GetEnvironmentVariable("ESPER_EPSERVICE_CONTEXT_FACTORY_CLASS");
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
                    clazz =
                        TransientConfigurationResolver.ResolveClassForNameProvider(
                            _configSnapshot.TransientConfiguration).ClassForName(epServicesContextFactoryClassName);
                }
                catch (TypeLoadException)
                {
                    throw new ConfigurationException(
                        "Type '" + epServicesContextFactoryClassName + "' cannot be loaded");
                }

                Object obj;
                try
                {
                    obj = _engine.Container.CreateInstance<object>(clazz);
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

            var services = epServicesContextFactory.CreateServicesContext(
                _container, this, _configSnapshot);

            // New runtime
            EPRuntimeSPI runtimeSPI;
            InternalEventRouteDest routeDest;
            TimerCallback timerCallback;
            var runtimeClassName = _configSnapshot.EngineDefaults.AlternativeContext.Runtime;
            if (runtimeClassName == null)
            {
                // Check system properties
                runtimeClassName = Environment.GetEnvironmentVariable("ESPER_EPRUNTIME_CLASS");
            }

            if (runtimeClassName == null)
            {
                var runtimeImpl = new EPRuntimeImpl(services);
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
                    var c = clazz.GetConstructor(new[] { typeof (EPServicesContext) });
                    obj = c.Invoke(new object[]{ services });
                }
                catch (TypeLoadException)
                {
                    throw new ConfigurationException("Class '" + clazz + "' cannot be instantiated");
                }
                catch (MissingMethodException)
                {
                    throw new ConfigurationException(
                        "Class '" + clazz + "' cannot be instantiated, constructor accepting services was not found");
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
                services.EventAdapterService, 
                services.EventTypeIdGenerator, 
                services.EngineImportService,
                services.VariableService, 
                services.EngineSettingsService, 
                services.ValueAddEventService,
                services.MetricsReportingService, 
                services.StatementEventTypeRefService,
                services.StatementVariableRefService, 
                services.PlugInViews, 
                services.FilterService,
                services.PatternSubexpressionPoolSvc, 
                services.MatchRecognizeStatePoolEngineSvc,
                services.TableService,
                services.ResourceManager, 
                _configSnapshot.TransientConfiguration);
            var defaultStreamSelector = SelectClauseStreamSelectorEnumExtensions.MapFromSODA(
                _configSnapshot.EngineDefaults.StreamSelection.DefaultStreamSelector);
            EPAdministratorSPI adminSPI;
            var adminClassName = _configSnapshot.EngineDefaults.AlternativeContext.Admin;
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
                    throw new ConfigurationException(
                        "Class '" + epServicesContextFactoryClassName + "' cannot be loaded");
                }

                Object obj;
                try
                {
                    var c = clazz.GetConstructor(new[] { typeof (EPAdministratorContext) });
                    obj = c.Invoke(new[] { adminContext });
                }
                catch (TypeLoadException)
                {
                    throw new ConfigurationException("Class '" + clazz + "' cannot be instantiated");
                }
                catch (MissingMethodException)
                {
                    throw new ConfigurationException(
                        "Class '" + clazz + "' cannot be instantiated, constructor accepting context was not found");
                }
                catch (MethodAccessException)
                {
                    throw new ConfigurationException("Illegal access instantiating class '" + clazz + "'");
                }

                adminSPI = (EPAdministratorSPI) obj;
            }

            // Start clocking
            if (_configSnapshot.EngineDefaults.Threading.IsInternalTimerEnabled)
            {
                if (_configSnapshot.EngineDefaults.TimeSource.TimeUnit != TimeUnit.MILLISECONDS)
                {
                    throw new ConfigurationException("Internal timer requires millisecond time resolution");
                }
                services.TimerService.StartInternalClock();
            }

            // Give the timer thread a little moment to start up
            Thread.Sleep(100);

            // Save engine instance
            _engine = new EPServiceEngine(_container, services, runtimeSPI, adminSPI);

            // Load and initialize adapter loader classes
            LoadAdapters(services);

            // Initialize extension services
            if (services.EngineLevelExtensionServicesContext != null)
            {
                services.EngineLevelExtensionServicesContext.Init(services, runtimeSPI, adminSPI);
            }

            // Start metrics reporting, if any
            if (_configSnapshot.EngineDefaults.MetricsReporting.IsEnableMetricsReporting)
            {
                services.MetricsReportingService.SetContext(runtimeSPI, services);
            }

            // Start engine metrics report
            if (_configSnapshot.EngineDefaults.MetricsReporting.IsEnableMetricsReporting)
            {
                StartEngineMetrics(services, runtimeSPI);
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
        /// Raises the <see cref="StatementStateChange"/> event.
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

        private void StartEngineMetrics(EPServicesContext services, EPRuntime runtime)
        {
            lock (this)
            {
#if false
                MetricName filterName = MetricNameFactory.Name(services.EngineURI, "filter");
                CommonJMXUtil.RegisterMbean(services.FilterService, filterName);
                MetricName scheduleName = MetricNameFactory.Name(services.EngineURI, "schedule");
                CommonJMXUtil.RegisterMbean(services.SchedulingService, scheduleName);
                MetricName runtimeName = MetricNameFactory.Name(services.EngineURI, "runtime");
                CommonJMXUtil.RegisterMbean(runtime, runtimeName);
#endif
            }
        }

        private void DestroyEngineMetrics(string engineURI)
        {
            lock (this)
            {
#if false
                CommonJMXUtil.UnregisterMbean(MetricNameFactory.Name(engineURI, "filter"));
                CommonJMXUtil.UnregisterMbean(MetricNameFactory.Name(engineURI, "schedule"));
                CommonJMXUtil.UnregisterMbean(MetricNameFactory.Name(engineURI, "runtime"));
#endif
            }
        }

        /// <summary>
        /// Loads and initializes adapter loaders.
        /// </summary>
        /// <param name="services">is the engine instance services</param>
        private void LoadAdapters(EPServicesContext services)
        {
            var pluginLoaders = _configSnapshot.PluginLoaders;
            if ((pluginLoaders == null) || (pluginLoaders.Count == 0))
            {
                return;
            }
            foreach (var config in pluginLoaders)
            {
                var className = config.TypeName;
                Type pluginLoaderClass;
                try
                {
                    pluginLoaderClass = services.EngineImportService.GetClassForNameProvider().ClassForName(className);
                }
                catch (TypeLoadException ex)
                {
                    throw new ConfigurationException("Failed to load adapter loader class '" + className + "'", ex);
                }

                Object pluginLoaderObj;
                try
                {
                    pluginLoaderObj = _engine.Container.CreateInstance<object>(pluginLoaderClass);
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

                if (!(pluginLoaderObj is PluginLoader))
                {
                    throw new ConfigurationException(
                        "Failed to cast adapter loader class '" + className + "' to " + typeof (PluginLoader).FullName);
                }

                var pluginLoader = (PluginLoader) pluginLoaderObj;
                var context = new PluginLoaderInitContext(
                    config.LoaderName, config.ConfigProperties, config.ConfigurationXML, this);
                pluginLoader.Init(context);

                // register adapter loader in JNDI context tree
                services.EngineEnvContext.Bind("plugin-loader/" + config.LoaderName, pluginLoader);
            }
        }

        private ConfigurationInformation TakeSnapshot(Configuration configuration)
        {
            try
            {
                // Allow variables to have non-serializable values by copying their initial value
                IDictionary<string, Object> variableInitialValues = null;
                if (!configuration.Variables.IsEmpty())
                {
                    variableInitialValues = new Dictionary<string, object>();
                    foreach (var variable in configuration.Variables)
                    {
                        var initializationValue = variable.Value.InitializationValue;
                        if (initializationValue != null)
                        {
                            variableInitialValues.Put(variable.Key, initializationValue);
                            variable.Value.InitializationValue = null;
                        }
                    }
                }

                // Avro schemas are not serializable
                IDictionary<string, ConfigurationEventTypeAvro> avroSchemas = null;
                if (!configuration.EventTypesAvro.IsEmpty())
                {
                    avroSchemas = new LinkedHashMap<string, ConfigurationEventTypeAvro>(configuration.EventTypesAvro);
                    configuration.EventTypesAvro.Clear();
                }

                var copy = (Configuration) SerializableObjectCopier.Copy(_container, configuration);
                copy.TransientConfiguration = configuration.TransientConfiguration;
                copy.Container = _container; // transition to this container??

                // Restore variable with initial values
                if (variableInitialValues != null && !variableInitialValues.IsEmpty())
                {
                    foreach (var entry in variableInitialValues)
                    {
                        var config = copy.Variables.Get(entry.Key);
                        config.InitializationValue = entry.Value;
                    }
                }

                // Restore Avro schemas
                if (avroSchemas != null)
                {
                    copy.EventTypesAvro.PutAll(avroSchemas);
                }

                return copy;
            }
            catch (IOException e)
            {
                throw new ConfigurationException(
                    "Failed to snapshot configuration instance through serialization : " + e.Message, e);
            }
            catch (TypeLoadException e)
            {
                throw new ConfigurationException(
                    "Failed to snapshot configuration instance through serialization : " + e.Message, e);
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
                    throw new EPServiceDestroyedException(_engineURI);
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
                    throw new EPServiceDestroyedException(_engineURI);
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
                    throw new EPServiceDestroyedException(_engineURI);
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
                    throw new EPServiceDestroyedException(_engineURI);
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
                    throw new EPServiceDestroyedException(_engineURI);
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
                    throw new EPServiceDestroyedException(_engineURI);
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
                    throw new EPServiceDestroyedException(_engineURI);
                }
                return _engine.Services.EventProcessingRWLock;
            }
        }

        private class EPServiceEngine
        {
            public EPServiceEngine(
                IContainer container,
                EPServicesContext services, 
                EPRuntimeSPI runtimeSPI, 
                EPAdministratorSPI admin)
            {
                Container = container;
                Services = services;
                Runtime = runtimeSPI;
                Admin = admin;
            }

            public IContainer Container { get; private set; }

            public EPServicesContext Services { get; private set; }

            public EPRuntimeSPI Runtime { get; private set; }

            public EPAdministratorSPI Admin { get; private set; }
        }
    }
} // end of namespace
