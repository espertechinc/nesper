///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.client.hook
{
    /// <summary>
    /// Factory for <see cref="VirtualDataWindow"/>.
    /// <para/>
    /// Register an implementation of this interface with the engine before use:
    ///     configuration.AddPlugInVirtualDataWindow("test", "vdw", typeof(SupportVirtualDWFactory).FullName);
    /// </summary>
    public interface VirtualDataWindowFactory
    {
        /// <summary>
        /// Invoked once after instantiation of the factory, exactly once per named window.
        /// </summary>
        /// <param name="factoryContext">factory context provides contextual information such as event type, named window name and parameters.</param>
        void Initialize(VirtualDataWindowFactoryContext factoryContext);

        /// <summary>
        /// Invoked for each context partition (or once if not using contexts),
        /// return a virtual data window to handle the specific event type, named window or paramaters
        /// as provided in the context.
        /// <p>
        ///      This method is invoked for each named window instance after the initialize method.
        ///      If using context partitions, the method is invoked once per context partition per named window.
        ///  </p>
        /// </summary>
        /// <param name="context">provides contextual information such as event type, named window name and parameters and including context partition information</param>
        /// <returns>virtual data window</returns>
        VirtualDataWindow Create(VirtualDataWindowContext context);

        /// <summary>
        /// Invoked to indicate the named window is destroyed.
        /// <p>
        ///     This method is invoked once per named window (and not once per context partition).
        /// </p>
        /// <p>
        ///     For reference, the VirtualDataWindow destroy method is called once per context partition,
        ///     before this method is invoked.
        /// </p>
        /// </summary>
        void DestroyAllContextPartitions();

        /// <summary>
        /// Return the names of properties that taken together (combined, composed, not individually) are the unique keys of a row,
        /// return null if there are no unique keys that can be identified.
        /// </summary>
        /// <returns>set of unique key property names</returns>
        ICollection<string> UniqueKeyPropertyNames { get; }
    }
}
