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
    ///     Expression evaluation settings in the runtime are for results of expressions.
    /// </summary>
    [Serializable]
    public class ConfigurationRuntimeExpression
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public ConfigurationRuntimeExpression()
        {
            TimeZone = TimeZoneInfo.Local;
            //TimeZone = TimeZone.CurrentTimeZone;
            IsSelfSubselectPreeval = true;
        }

        /// <summary>
        ///     Returns the time zone for calendar operations.
        /// </summary>
        /// <returns>time zone</returns>
        public TimeZoneInfo TimeZone { get; set; }

        /// <summary>
        ///     Set to true (the default) to indicate that sub-selects within a statement are updated first when a new
        ///     event arrives. This is only relevant for statements in which both subselects
        ///     and the from-clause may react to the same exact event.
        /// </summary>
        /// <value>indicator whether to evaluate sub-selects first or last on new event arrival</value>
        public bool IsSelfSubselectPreeval { get; set; }
    }
} // end of namespace