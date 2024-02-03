///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.configuration.common
{
    /// <summary>
    ///     Holds view logging settings other then the Apache commons or Log4J settings.
    /// </summary>
    public class ConfigurationCommonLogging
    {
        /// <summary>
        ///     Ctor - sets up defaults.
        /// </summary>
        public ConfigurationCommonLogging()
        {
            IsEnableQueryPlan = false;
            IsEnableADO = false;
        }

        /// <summary>
        ///     Returns indicator whether query plan logging is enabled or not.
        /// </summary>
        /// <value>indicator</value>
        public bool IsEnableQueryPlan { get; set; }

        /// <summary>
        ///     Returns an indicator whether ADO query reporting is enabled.
        /// </summary>
        /// <value>indicator</value>
        public bool IsEnableADO { get; set; }
    }
} // end of namespace