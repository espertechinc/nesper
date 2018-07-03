///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.client
{
    /// <summary>
    /// This class provides access to the EPRuntime and EPAdministrator implementations.
    /// </summary>
    public interface EPServiceProvider : IDisposable
    {
        IContainer Container { get; }

        /// <summary> Returns a class instance of EPRuntime.</summary>
        /// <returns> an instance of EPRuntime
        /// </returns>
        EPRuntime EPRuntime { get; }

        /// <summary> Returns a class instance of EPAdministrator.</summary>
        /// <returns> an instance of EPAdministrator
        /// </returns>
        EPAdministrator EPAdministrator { get; }

        /// <summary>
        /// Returns the engine environment directory for engine-external
        /// resources such as adapters.
        /// </summary>
        /// <returns>engine environment directory</returns>
        Directory Directory { get; }

        /// <summary>
        /// Frees any resources associated with this engine instance, and leaves
        /// the engine instance ready for further use.
        /// <para/>
        /// Do not use the <see cref="EPAdministrator" /> administrative and <see cref="EPRuntime" />
        /// runtime instances obtained before the initialize (including related services such as configuration, 
        /// module management, etc.).  Your application must obtain new administrative and runtime instances.
        /// <para/>
        /// Retains the existing configuration of the engine instance but forgets any runtime configuration changes.
        /// <para/>
        /// Stops and destroys any existing statement resources such as filters, patterns, expressions, views.
        /// </summary>
        void Initialize();

        /// <summary>Returns the provider URI, or "default" if this is the default provider.</summary>
        /// <returns>provider URI</returns>
        String URI { get; }

        /// <summary>
        /// Returns true if the service is in destroyed state, or false if not.
        /// </summary>
        /// <returns>indicator whether the service has been destroyed</returns>
        bool IsDestroyed { get; }

        /// <summary>
        /// Clears the service state event handlers.  For internal use only.
        /// </summary>
        void RemoveAllServiceStateEventHandlers();

        /// <summary>
        /// Returns the isolated service provider for that name, creating an isolated
        /// service if the name is a new name, or returning an existing isolated service for an
        /// existing name.
        /// <para>
        ///     Note: Requires configuration setting.
        /// </para>
        /// </summary>
        /// <param name="name">to return isolated service for</param>
        /// <returns>isolated service</returns>
        EPServiceProviderIsolated GetEPServiceIsolated(String name);

        /// <summary>
        /// Returns the names of isolated service providers currently allocated.
        /// </summary>
        /// <returns>
        /// isolated service provider names
        /// </returns>
        IList<string> EPServiceIsolatedNames { get; }

        /// <summary>
        /// Returns the engine-instance global read-write lock.
        /// </summary>
        /// <para>The EPRuntime.SendEvent method takes a read lock.</para>
        /// <para>The EPRuntime.CreateEPL methods take a write lock.</para>
        /// <returns>engine instance global read-write lock</returns>
        IReaderWriterLock EngineInstanceWideLock { get; }

        /// <summary>
        /// Occurs before an <seealso cref="EPServiceProvider"/> is destroyed.
        /// </summary>
        event EventHandler<ServiceProviderEventArgs> ServiceDestroyRequested;

        /// <summary>
        /// Occurs after an <seealso cref="EPServiceProvider"/> is initialized.
        /// </summary>
        event EventHandler<ServiceProviderEventArgs> ServiceInitialized;

        /// <summary>
        /// Occurs when a statement created.
        /// </summary>
        event EventHandler<StatementStateEventArgs> StatementCreate;

        /// <summary>
        /// Occurs when a statement state changes.
        /// </summary>
        event EventHandler<StatementStateEventArgs> StatementStateChange;       
    }

    public class ServiceProviderEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the service provider.
        /// </summary>
        /// <value>The service provider.</value>
        public EPServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderEventArgs"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public ServiceProviderEventArgs(EPServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
    }
}
