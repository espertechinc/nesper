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
    ///     Holds pattern settings.
    /// </summary>
    [Serializable]
    public class ConfigurationRuntimePatterns
    {
        /// <summary>
        ///     Returns true, the default, to indicate that if there is a maximum defined
        ///     it is being enforced and new subexpressions are not allowed.
        /// </summary>
        /// <returns>indicate whether enforced or not</returns>
        public bool IsMaxSubexpressionPreventStart { get; set; } = true;

        /// <summary>
        ///     Returns the maximum number of subexpressions
        /// </summary>
        /// <value>subexpression count</value>
        public long? MaxSubexpressions { get; set; }
    }
} // end of namespace