///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.compat.threading.locks
{
    /// <summary>
    /// Constants we keep for our locking algorithms.
    /// </summary>

    public class LockConstants
    {
        /// <summary>
        /// Default number of milliseconds for default timeouts.
        /// </summary>
        public const int DefaultTimeout = 5000;

        /// <summary>
        /// Number of milliseconds until read locks timeout
        /// </summary>
        public const int DefaultReaderTimeout = 5000;

        /// <summary>
        /// Number of milliseconds until write locks timeout
        /// </summary>
        public const int DefaultWriterTimeout = 5000;
    }
}
