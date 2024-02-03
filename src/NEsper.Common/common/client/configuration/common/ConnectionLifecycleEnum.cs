///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.configuration.common
{
    /// <summary>
    ///     Enum controlling connection lifecycle.
    /// </summary>
    public enum ConnectionLifecycleEnum
    {
        /// <summary>
        ///     Retain connection between lookups, not getting a new connection each lookup.
        /// </summary>
        RETAIN,

        /// <summary>
        ///     Obtain a new connection each lookup closing the connection when done.
        /// </summary>
        POOLED
    }
}