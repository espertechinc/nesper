///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client;

namespace com.espertech.esper.plugin
{
    /// <summary>
    /// Interface for loaders of input/output adapters or any other adapter that may participate
    /// in an engine lifecycle.
    /// </summary>
    public interface PluginLoader : IDisposable
    {
        /// <summary>
        /// Initializes the adapter loader.
        /// <para/> 
        /// Invoked before the engine instance is fully initialized. Thereby this is not the place 
        /// to look up an engine instance from <seealso cref="com.espertech.esper.client.EPServiceProviderManager"/> 
        /// and use it. Use the {@link #postInitialize} method instead.
        /// </summary>
        /// <param name="context">the plug in context</param>
        void Init(PluginLoaderInitContext context);

        /// <summary>
        /// Called after an engine instances has fully initialized and is already registered 
        /// with <seealso cref="EPServiceProviderManager"/>.
        /// </summary>
        void PostInitialize();
    }
}
