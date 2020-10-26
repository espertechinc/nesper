///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.metric;
using com.espertech.esper.common.client.render;
using com.espertech.esper.common.client.variable;
using com.espertech.esper.compat.directory;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.runtime.client
{
	/// <summary>
	/// The runtime for deploying and executing EPL.
	/// </summary>
	public interface EPRuntime {
        /// <summary>
        /// Returns the event service, for sending events to the runtime and for controlling time
        /// </summary>
        /// <value>event service</value>
        /// <throws>EPRuntimeDestroyedException thrown when the runtime has been destroyed</throws>
        EPEventService EventService { get; }

        /// <summary>
        /// Returns the data flow service, for managing dataflows
        /// </summary>
        /// <value>data flow service</value>
        /// <throws>EPRuntimeDestroyedException thrown when the runtime has been destroyed</throws>
        EPDataFlowService DataFlowService { get; }

        /// <summary>
        /// Returns the context partition service, for context partition information
        /// </summary>
        /// <value>context partition service</value>
        /// <throws>EPRuntimeDestroyedException thrown when the runtime has been destroyed</throws>
        EPContextPartitionService ContextPartitionService { get; }

        /// <summary>
        /// Returns the variable service, for reading and writing variables
        /// </summary>
        /// <value>variable service</value>
        /// <throws>EPRuntimeDestroyedException thrown when the runtime has been destroyed</throws>
        EPVariableService VariableService { get; }

        /// <summary>
        /// Returns the metrics service, for managing runtime and statement metrics reporting
        /// </summary>
        /// <value>metrics service</value>
        /// <throws>EPRuntimeDestroyedException thrown when the runtime has been destroyed</throws>
        EPMetricsService MetricsService { get; }

        /// <summary>
        /// Returns the event type service, for obtaining information on event types
        /// </summary>
        /// <value>event type service</value>
        /// <throws>EPRuntimeDestroyedException thrown when the runtime has been destroyed</throws>
        EPEventTypeService EventTypeService { get; }

        /// <summary>
        /// Returns the event rendering service, for rendering events to JSON and XML
        /// </summary>
        /// <value>render event service</value>
        /// <throws>EPRuntimeDestroyedException thrown when the runtime has been destroyed</throws>
        EPRenderEventService RenderEventService { get; }

        /// <summary>
        /// Returns the fire-and-forget service, for executing fire-and-forget queries
        /// </summary>
        /// <value>fire-and-forget service</value>
        /// <throws>EPRuntimeDestroyedException thrown when the runtime has been destroyed</throws>
        EPFireAndForgetService FireAndForgetService { get; }

        /// <summary>
        /// Returns the deployment service, for deploying and undeploying compiled modules
        /// </summary>
        /// <value>deployment service</value>
        /// <throws>EPRuntimeDestroyedException thrown when the runtime has been destroyed</throws>
        EPDeploymentService DeploymentService { get; }

		/// <summary>
		/// Returns the stage service, for managing stages
		/// </summary>
		/// <value>stage service</value>
		/// <throws>EPRuntimeDestroyedException thrown when the runtime has been destroyed</throws>
        EPStageService StageService { get; }

        /// <summary>
        /// Returns true if the runtime is in destroyed state, or false if not.
        /// </summary>
        /// <value>indicator whether the runtime has been destroyed</value>
        bool IsDestroyed { get; }

        /// <summary>
	    /// Frees any resources associated with this runtime instance, and leaves the runtime instance
	    /// ready for further use.
	    /// <para />Do not use the <seealso cref="EPDeploymentService" /> administrative and <seealso cref="EPEventService" /> runtime instances obtained before the
	    /// initialize (including related services such as configuration, module management, etc.).
	    /// Your application must obtain new administrative and runtime instances.
	    /// <para />Retains the existing configuration of the runtime instance but forgets any runtime configuration changes.
	    /// <para />Stops and destroys any existing statement resources such as filters, patterns, expressions, views.
	    /// </summary>
	    void Initialize();

	    /// <summary>
	    /// Returns the runtime URI, or "default" if this is the default runtime.
	    /// </summary>
	    /// <returns>runtime URI</returns>
	    string URI { get; }

        /// <summary>
        /// Provides naming context for public named objects.
        /// <para />An extension point designed for use by input and output adapters as well as
        /// other extension services.
        /// </summary>
        /// <value>naming context providing name-to-object bindings</value>
        /// <throws>EPRuntimeDestroyedException thrown when the runtime instance has been destroyed</throws>
        INamingContext Context { get; }

        /// <summary>
	    /// Destroys the runtime.
	    /// <para />Releases any resources held by the runtime. The runtime enteres a state in
	    /// which operations provided by the runtime
	    /// are not guaranteed to operate properly.
	    /// <para />Removes the runtime URI from the known URIs. Allows configuration to change for the instance.
	    /// <para />When destroying a runtime your application must make sure that threads that are sending events into the runtime
	    /// have completed their work. More generally, the runtime should not be currently in use during or after the destroy operation.
	    /// </summary>
	    void Destroy();

        /// <summary>
        /// Returns the runtime-instance global read-write lock.
        /// The send-event methods takes a read lock.
        /// The {@link EPDeploymentService#deploy(EPCompiled)} and {@link EPDeploymentService#undeploy(String)} methods take a write lock.
        /// </summary>
        /// <value>runtime instance global read-write lock</value>
        /// <throws>EPRuntimeDestroyedException thrown when the runtime has been destroyed</throws>
        IReaderWriterLock RuntimeInstanceWideLock { get; }

        /// <summary>
	    /// Add a listener to runtime state changes that receives a before-destroy event.
	    /// The listener collection applies set-semantics.
	    /// </summary>
	    /// <param name="listener">to add</param>
	    void AddRuntimeStateListener(EPRuntimeStateListener listener);

	    /// <summary>
	    /// Removate a listener to runtime state changes.
	    /// </summary>
	    /// <param name="listener">to remove</param>
	    /// <returns>true to indicate the listener was removed, or fals</returns>
	    bool RemoveRuntimeStateListener(EPRuntimeStateListener listener);

	    /// <summary>
	    /// Remove all listeners to runtime state changes.
	    /// </summary>
	    void RemoveAllRuntimeStateListeners();

	    /// <summary>
	    /// Returns a deep-copy of the configuration that is actively in use by the runtime.
	    /// <para />Note: This can be an expensive operation.
	    /// </summary>
	    /// <returns>deep copy of the configuration</returns>
	    Configuration ConfigurationDeepCopy { get; }

	    /// <summary>
	    /// Returns the transient configuration, which are configuration values that are passed by reference (and not by value)
	    /// </summary>
	    /// <returns>transient configuration</returns>
	    IDictionary<string, object> ConfigurationTransient { get; }

	    /// <summary>
	    /// Returns a path object for use by the compiler that represents a snapshot of the EPL objects deployed into the runtime
	    /// at the time of this call. The EPL objects deployed after a call to this method are not included.
	    /// </summary>
	    /// <returns>path</returns>
	    EPCompilerPathable RuntimePath { get; }
	}
} // end of namespace