///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.hook.vdw
{
    /// <summary>
    ///     Factory for <see cref="VirtualDataWindow" />.
    ///     <para />
    ///     Register an implementation of this interface with the engine before use:
    ///     configuration.AddPlugInVirtualDataWindow("test", "vdw", typeof(SupportVirtualDWFactory).FullName);
    /// </summary>
    public interface VirtualDataWindowFactory
    {
        /// <summary>
        ///     Invoked once after instantiation of the factory, exactly once per named window.
        /// </summary>
        /// <param name="factoryContext">
        ///     factory context provides contextual information such as event type, named window name and
        ///     parameters.
        /// </param>
        void Initialize(VirtualDataWindowFactoryContext factoryContext);

        /// <summary>
        ///     Invoked for each context partition (or once if not using contexts),
        ///     return a virtual data window to handle the specific event type, named window or paramaters
        ///     as provided in the context.
        ///     <p>
        ///         This method is invoked for each named window instance after the initialize method.
        ///         If using context partitions, the method is invoked once per context partition per named window.
        ///     </p>
        /// </summary>
        /// <param name="context">
        ///     provides contextual information such as event type, named window name and parameters and
        ///     including context partition information
        /// </param>
        /// <returns>virtual data window</returns>
        VirtualDataWindow Create(VirtualDataWindowContext context);

        /// <summary>
        /// Invoked upon undeployment of the virtual data window.
        /// </summary>
        void Destroy();
    }
}