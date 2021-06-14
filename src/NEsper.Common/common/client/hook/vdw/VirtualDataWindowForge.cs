///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.client.hook.vdw
{
    /// <summary>
    ///     Factory for <seealso cref="VirtualDataWindow" />.
    ///     <para />
    ///     Register an implementation of this interface with the runtime before use:
    ///     configuration.addPlugInVirtualDataWindow("test", "vdw", SupportVirtualDWFactory.class.getName());
    /// </summary>
    public interface VirtualDataWindowForge
    {
        /// <summary>
        ///     Describes to the compiler how it should manage code for the virtual data window factory.
        /// </summary>
        /// <value>mode object</value>
        VirtualDataWindowFactoryMode FactoryMode { get; }

        /// <summary>
        ///     Return the names of properties that taken together (combined, composed, not individually) are the unique keys of a
        ///     row,
        ///     return null if there are no unique keys that can be identified.
        /// </summary>
        /// <value>set of unique key property names</value>
        ISet<string> UniqueKeyPropertyNames { get; }

        /// <summary>
        ///     Invoked once after instantiation of the forge, exactly once per named window.
        /// </summary>
        /// <param name="initializeContext">provides contextual information such as event type, named window name and parameters.</param>
        void Initialize(VirtualDataWindowForgeContext initializeContext);
    }
} // end of namespace