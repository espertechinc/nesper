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
    ///     Holds execution-related settings.
    /// </summary>
    [Serializable]
    public class ConfigurationCompilerExecution
    {
        /// <summary>
        ///     Returns the maximum width for breaking up "or" expression in filters to
        ///     subexpressions for reverse indexing.
        /// </summary>
        /// <value>max filter width</value>
        public int FilterServiceMaxFilterWidth { get; set; } = 16;

        /// <summary>
        ///     Returns indicator whether declared-expression-value-cache is enabled (true by default)
        /// </summary>
        /// <value>indicator</value>
        public bool IsEnabledDeclaredExprValueCache { get; private set; } = true;

        /// <summary>
        ///     Sets indicator whether declared-expression-value-cache is enabled (true by default)
        /// </summary>
        /// <value>indicator</value>
        public bool EnabledDeclaredExprValueCache {
            get => IsEnabledDeclaredExprValueCache;
            set => IsEnabledDeclaredExprValueCache = value;
        }
    }
} // end of namespace