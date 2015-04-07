///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
using System.Threading;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.schedule
{
    /// <summary>
    /// Implements the schedule service by simply keeping a sorted set of long
    /// millisecond values and a set of handles for each.
    /// <para/>
    /// Synchronized since statement creation and event evaluation by multiple (event
    /// send) threads can lead to callbacks added/removed asynchronously.
    /// </summary>
    public sealed class SchedulingMgmtServiceImpl : SchedulingMgmtService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Current bucket number - for use in ordering handles by bucket
        /// </summary>
        private int curBucketNum;

        #region SchedulingMgmtService Members

        public void Dispose()
        {
            Log.Debug("Destroying scheduling management service");
        }

        public ScheduleBucket AllocateBucket()
        {
            int bucket = Interlocked.Increment(ref curBucketNum);
            return new ScheduleBucket(bucket);
        }

        #endregion
    }
}
