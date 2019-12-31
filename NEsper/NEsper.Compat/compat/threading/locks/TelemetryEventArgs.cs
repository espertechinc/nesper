///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

namespace com.espertech.esper.compat.threading.locks
{
    public class TelemetryEventArgs : EventArgs
    {
        /// <summary>
        /// Unique lock identifier.
        /// </summary>
        public string Id;
        /// <summary>
        /// TimeInMillis lock was requested.
        /// </summary>
        public long RequestTime;
        /// <summary>
        /// TimeInMillis lock was acquired.
        /// </summary>
        public long AcquireTime;
        /// <summary>
        /// TimeInMillis lock was released.
        /// </summary>
        public long ReleaseTime;
        /// <summary>
        /// Stack TRACE associated with lock.
        /// </summary>
        public StackTrace StackTrace;
    }
}
