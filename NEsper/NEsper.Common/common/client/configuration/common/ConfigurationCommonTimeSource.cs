///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.client.configuration.common
{
    /// <summary>
    ///     Time source configuration, the default in MILLI (millisecond resolution from System.currentTimeMillis).
    /// </summary>
    [Serializable]
    public class ConfigurationCommonTimeSource
    {
        /// <summary>
        ///     Returns the time unit time resolution level of time tracking
        /// </summary>
        /// <returns>time resolution</returns>
        public TimeUnit TimeUnit { get; set; } = TimeUnit.MILLISECONDS;
    }
} // end of namespace