///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.configuration.compiler
{
    /// <summary>
    ///     Holds view logging settings other then the Apache commons or Log4J settings.
    /// </summary>
    [Serializable]
    public class ConfigurationCompilerLogging
    {
        /// <summary>
        ///     Ctor - sets up defaults.
        /// </summary>
        internal ConfigurationCompilerLogging()
        {
            IsEnableCode = false;
            IsEnableFilterPlan = false;
        }

        /// <summary>
        ///     Returns indicator whether code generation logging is enabled or not.
        /// </summary>
        /// <value>indicator</value>
        public bool IsEnableCode { get; private set; }
        
        /// <summary>
        /// Returns indicator whether filter plan logging is enabled or not.
        /// </summary>
        public bool IsEnableFilterPlan { get; set; }
        
        /// <summary>
        /// Gets or sets the location for audit code to be written.
        /// </summary>
        public string AuditDirectory { get; set; }

        /// <summary>
        ///     Set indicator whether code generation logging is enabled, by default it is disabled.
        /// </summary>
        /// <value>indicator</value>
        public bool EnableCode {
            get => IsEnableCode;
            set => IsEnableCode = value;
        }
    }
} // end of namespace