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
    ///     Holds match-reconize settings.
    /// </summary>
    [Serializable]
    public class ConfigurationRuntimeMatchRecognize
    {
        /// <summary>
        ///     Returns the maximum number of states
        /// </summary>
        /// <value>state count</value>
        public long? MaxStates { get; set; }

        /// <summary>
        ///     Returns true, the default, to indicate that if there is a maximum defined
        ///     it is being enforced and new states are not allowed.
        /// </summary>
        /// <value>indicate whether enforced or not</value>
        public bool IsMaxStatesPreventStart { get; set; } = true;
    }
} // end of namespace