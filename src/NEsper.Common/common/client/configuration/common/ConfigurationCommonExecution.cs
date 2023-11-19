///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.util;

namespace com.espertech.esper.common.client.configuration.common
{
    /// <summary>
    ///     Holds common execution-related settings.
    /// </summary>
    public class ConfigurationCommonExecution
    {
        public ConfigurationCommonExecution()
        {
            ThreadingProfile = ThreadingProfile.NORMAL;
        }

        /// <summary>
        ///     Returns the threading profile
        /// </summary>
        /// <returns>profile</returns>
        public ThreadingProfile ThreadingProfile { get; set; }
    }
} // end of namespace