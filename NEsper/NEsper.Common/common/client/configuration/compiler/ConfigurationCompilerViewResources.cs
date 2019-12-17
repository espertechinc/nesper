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
    ///     Holds view resources settings.
    /// </summary>
    [Serializable]
    public class ConfigurationCompilerViewResources
    {
        /// <summary>
        ///     Ctor - sets up defaults.
        /// </summary>
        internal ConfigurationCompilerViewResources()
        {
            IsIterableUnbound = false;
            IsOutputLimitOpt = true;
        }

        /// <summary>
        ///     Returns flag to indicate unbound statements are iterable and return the last event.
        /// </summary>
        /// <value>indicator</value>
        public bool IsIterableUnbound { get; set; }

        /// <summary>
        ///     Sets flag to indicate unbound statements are iterable and return the last event.
        /// </summary>
        /// <value>to set</value>
        public bool IterableUnbound {
            get => IsIterableUnbound;
            set => IsIterableUnbound = value;
        }

        /// <summary>
        ///     Returns indicator whether for output limiting the options are enabled by default.
        ///     Has the same effect as adding "@hint("ENABLE_OUTPUTLIMIT_OPT") to all statements (true by default).
        /// </summary>
        /// <value>flag</value>
        public bool IsOutputLimitOpt { get; private set; }

        /// <summary>
        ///     Sets indicator whether for output limiting the options are enabled by default.
        ///     Has the same effect as adding "@hint("ENABLE_OUTPUTLIMIT_OPT") to all statements (true by default).
        /// </summary>
        /// <value>flag</value>
        public bool OutputLimitOpt {
            get => IsOutputLimitOpt;
            set => IsOutputLimitOpt = value;
        }
    }
} // end of namespace