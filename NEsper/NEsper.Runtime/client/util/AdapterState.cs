///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.client.util
{
    /// <summary>The state of a Adapter. </summary>
    public enum AdapterState
    {
        /// <summary>Opened state. </summary>
        OPENED,

        /// <summary>Started state. </summary>
        STARTED,

        /// <summary>Paused state. </summary>
        PAUSED,

        /// <summary>Destroyed state. </summary>
        DESTROYED
    }
}