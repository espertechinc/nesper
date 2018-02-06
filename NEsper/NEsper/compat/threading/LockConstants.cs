///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.compat.threading
{
    /// <summary>
    /// Constants we keep for our locking algorithms.
    /// </summary>

    public class LockConstants
    {
        /// <summary>
        /// Number of milliseconds until write locks timeout
        /// </summary>
        public const int WriterTimeout = 50000;
    }
}
