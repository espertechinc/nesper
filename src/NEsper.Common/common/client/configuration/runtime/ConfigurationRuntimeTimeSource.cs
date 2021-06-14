///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.util;

namespace com.espertech.esper.common.client.configuration.runtime
{
    /// <summary>
    ///     Time source configuration, the default in MILLI (millisecond resolution from System.currentTimeMillis).
    /// </summary>
    [Serializable]
    public class ConfigurationRuntimeTimeSource
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        internal ConfigurationRuntimeTimeSource()
        {
            TimeSourceType = TimeSourceType.MILLI;
        }

        /// <summary>
        ///     Returns the time source type.
        /// </summary>
        /// <returns>time source type enum</returns>
        public TimeSourceType TimeSourceType { get; set; }
    }
} // end of namespace