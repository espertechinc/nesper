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
    /// Holder for script settings.
    /// </summary>
    [Serializable]
    public class ConfigurationCompilerScripts
    {
        /// <summary>
        /// Returns the default script dialect.
        /// </summary>
        /// <value>dialect</value>
        public string DefaultDialect { get; set; } = "js";
    }
} // end of namespace