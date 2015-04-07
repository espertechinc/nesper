///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.core.service
{
    /// <summary>Context for administrative services. </summary>
    public class EPAdministratorContext
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="runtimeSPI">The runtime SPI.</param>
        /// <param name="services">engine services</param>
        /// <param name="configurationOperations">configuration snapshot</param>
        /// <param name="defaultStreamSelector">default stream selection</param>
        public EPAdministratorContext(EPRuntimeSPI runtimeSPI,
                                      EPServicesContext services,
                                      ConfigurationOperations configurationOperations,
                                      SelectClauseStreamSelectorEnum defaultStreamSelector
                                      )
        {
            RuntimeSPI = runtimeSPI;
            ConfigurationOperations = configurationOperations;
            DefaultStreamSelector = defaultStreamSelector;
            Services = services;
        }

        /// <summary>Returns configuration. </summary>
        /// <value>configuration</value>
        public ConfigurationOperations ConfigurationOperations { get; private set; }

        /// <summary>Returns the default stream selector. </summary>
        /// <value>default stream selector</value>
        public SelectClauseStreamSelectorEnum DefaultStreamSelector { get; private set; }

        /// <summary>Returns the engine services context. </summary>
        /// <value>engine services</value>
        public EPServicesContext Services { get; private set; }

        /// <summary>
        /// Gets or sets the runtime SPI.
        /// </summary>
        /// <value>The runtime SPI.</value>
        public EPRuntimeSPI RuntimeSPI { get; private set; }
    }
}
