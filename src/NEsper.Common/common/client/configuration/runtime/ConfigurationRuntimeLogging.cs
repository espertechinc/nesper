///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.configuration.runtime
{
    /// <summary>
    ///     Holds view logging settings other then the Apache commons or Log4J settings.
    /// </summary>
    public class ConfigurationRuntimeLogging
    {
        /// <summary>
        ///     Ctor - sets up defaults.
        /// </summary>
        public ConfigurationRuntimeLogging()
        {
            IsEnableExecutionDebug = false;
            IsEnableTimerDebug = true;
        }

        /// <summary>
        ///     Returns the pattern that formats audit logs.
        ///     <para />
        ///     Available conversion characters are:
        ///     <para>
        ///     %m      - Used to output the audit message.
        ///     %s      - Used to output the statement name.
        ///     %u      - Used to output the runtime URI.
        ///     </para>
        /// </summary>
        /// <returns>audit formatting pattern</returns>
        public string AuditPattern { get; set; }

        /// <summary>
        ///     Returns true if execution path debug logging is enabled.
        ///     <para>
        ///     Only if this flag is set to true, in addition to LOG4J settings set to DEBUG, does a runtime instance,
        ///     produce debug out.
        ///     </para>
        /// </summary>
        /// <value>true if debug logging through Log4j is enabled for any event processing execution paths</value>
        public bool IsEnableExecutionDebug { get; set; }

        /// <summary>
        ///     Returns true if timer debug level logging is enabled (true by default).
        ///     <para>
        ///     Set this value to false to reduce the debug-level logging output for the timer thread(s).
        ///     For use only when debug-level logging is enabled.
        ///     </para>
        /// </summary>
        /// <value>indicator whether timer execution is noisy in debug or not</value>
        public bool IsEnableTimerDebug { get; set; }

        /// <summary>
        ///     Returns indicator whether lock activity logging is enabled.
        ///     <para>
        ///     When lock logging is enabled the runtime logs all activity
        ///     in respect to context partition read-write locks
        ///     at INFO level to the log destination "com.espertech.esper.lock".
        ///     </para>
        /// </summary>
        public bool IsEnableLockActivity { get; set; }
    }
} // end of namespace