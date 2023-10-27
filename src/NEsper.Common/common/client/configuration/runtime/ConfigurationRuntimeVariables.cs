///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.configuration.runtime
{
    /// <summary>
    ///     Holds variables settings.
    /// </summary>
    [Serializable]
    public class ConfigurationRuntimeVariables
    {
        /// <summary>
        ///     Ctor - sets up defaults.
        /// </summary>
        public ConfigurationRuntimeVariables()
        {
            MsecVersionRelease = 15000;
        }

        /// <summary>
        ///     Returns the number of milliseconds that a version of a variables is held stable for
        ///     use by very long-running atomic statement execution.
        ///     <para />
        ///     A slow-executing statement such as an SQL join may use variables that, at the time
        ///     the statement starts to execute, have certain values. The runtime guarantees that during
        ///     statement execution the value of the variables stays the same as long as the statement
        ///     does not take longer then the given number of milliseconds to execute. If the statement does take longer
        ///     to execute then the variables release time, the current variables value applies instead.
        /// </summary>
        /// <returns>
        ///     millisecond time interval that a variables version is guaranteed to be stablein the context of an atomic statement
        ///     execution
        /// </returns>
        public long MsecVersionRelease { get; set; }
    }
} // end of namespace