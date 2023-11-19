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
        /// Returns the setting instructing the compiler which level of filter index planning to perform (default is ADVANCED).
        /// Please check the documentation for information on advanced planning.
        /// </summary>
        public FilterIndexPlanningEnum FilterIndexPlanning { get; set; } = FilterIndexPlanningEnum.ADVANCED;

        /// <summary>
        ///     Sets indicator whether declared-expression-value-cache is enabled (true by default)
        /// </summary>
        /// <value>indicator</value>
        public bool EnabledDeclaredExprValueCache {
            get => IsEnabledDeclaredExprValueCache;
            set => IsEnabledDeclaredExprValueCache = value;
        }

        /// <summary>
        /// Controls the level of planning of filter indexes from filter expressions.
        /// </summary>
        public enum FilterIndexPlanningEnum
        {
            /// <summary>No planning for filter indexes</summary>
            NONE,

            /// <summary>Only basic planning for filter indexes</summary>
            BASIC,

            /// <summary>Advanced planning</summary>
            ADVANCED
        }
    }
} // end of namespace