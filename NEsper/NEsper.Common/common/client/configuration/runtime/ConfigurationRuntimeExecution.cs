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
    ///     Holds runtime execution-related settings.
    /// </summary>
    [Serializable]
    public class ConfigurationRuntimeExecution
    {
        /// <summary>
        ///     Ctor - sets up defaults.
        /// </summary>
        internal ConfigurationRuntimeExecution()
        {
            IsPrioritized = false;
            FilterServiceProfile = FilterServiceProfile.READMOSTLY;
        }

        /// <summary>
        ///     Returns the filter service profile for tuning filtering operations.
        /// </summary>
        /// <returns>filter service profile</returns>
        public FilterServiceProfile FilterServiceProfile { get; set; }

        /// <summary>
        ///     Returns the cache size for declared expression values
        /// </summary>
        /// <returns>value</returns>
        public int DeclaredExprValueCacheSize { get; set; } = 1;

        /// <summary>
        ///     Returns false (the default) if the runtimedoes not consider statement priority and preemptive instructions,
        ///     or true to enable priority-based statement execution order.
        /// </summary>
        /// <value>false by default to indicate unprioritized statement execution</value>
        public bool IsPrioritized { get; set; }

        /// <summary>
        ///     Returns true for fair locking, false for unfair locks.
        /// </summary>
        /// <value>fairness flag</value>
        public bool IsFairlock { get; set; }

        /// <summary>
        ///     Returns indicator whether statement-level locks are disabled.
        ///     The default is false meaning statement-level locks are taken by default and depending on EPL optimizations.
        ///     If set to true statement-level locks are never taken.
        /// </summary>
        /// <value>indicator for statement-level locks</value>
        public bool IsDisableLocking { get; set; }
    }
} // end of namespace