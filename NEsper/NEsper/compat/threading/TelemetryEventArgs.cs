///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

namespace com.espertech.esper.compat.threading
{
    public class TelemetryEventArgs : EventArgs
    {
        /// <summary>
        /// Unique lock identifier.
        /// </summary>
        public string Id;
        /// <summary>
        /// Time lock was requested.
        /// </summary>
        public long RequestTime;
        /// <summary>
        /// Time lock was acquired.
        /// </summary>
        public long AcquireTime;
        /// <summary>
        /// Time lock was released.
        /// </summary>
        public long ReleaseTime;
        /// <summary>
        /// Stack trace associated with lock.
        /// </summary>
        public StackTrace StackTrace;
    }
}
