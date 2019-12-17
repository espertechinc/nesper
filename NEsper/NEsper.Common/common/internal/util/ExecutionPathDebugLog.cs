///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    /// Utility class that control debug-level logging in the execution path
    /// beyond which is controlled by logging infrastructure.
    /// </summary>
    public class ExecutionPathDebugLog
    {
        /// <summary>
        /// Gets or sets a flag that allows execution path debug logging.
        /// </summary>
        public static bool IsDebugEnabled { get; set; }

        /// <summary>
        /// Public access.
        /// </summary>
        public static bool IsTimerDebugEnabled { get; set; }

        /// <summary>
        /// Initializes the <see cref="ExecutionPathDebugLog"/> class.
        /// </summary>
        static ExecutionPathDebugLog()
        {
            IsDebugEnabled = false;
            IsTimerDebugEnabled = true;
        }
    }
}