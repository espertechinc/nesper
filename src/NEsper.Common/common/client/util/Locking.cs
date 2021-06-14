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
    public enum Locking
    {
        /// <summary>
        ///     Spin lock blocking is good for locks held very shortly or generally uncontended locks and
        ///     is therefore the default.
        /// </summary>
        SPIN,

        /// <summary>
        ///     Blocking that suspends a thread and notifies a thread to wake up can be
        ///     more expensive then spin locks.
        /// </summary>
        SUSPEND
    }
} // end of namespace