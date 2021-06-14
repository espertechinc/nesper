///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.util
{
    /// <summary>
    ///     Enumeration of blocking techniques.
    /// </summary>
    public enum UndeployRethrowPolicy
    {
        /// <summary>
        ///     Warn.
        /// </summary>
        WARN,

        /// <summary>
        ///     Rethrow First Encountered Exception.
        /// </summary>
        RETHROW_FIRST
    }
} // end of namespace