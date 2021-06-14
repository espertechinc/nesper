///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core
{
    public abstract partial class Clock
    {
        /// <summary>
        ///     A clock implementation which returns the current time in epoch nanoseconds.
        /// </summary>
        public class UserTimeClock : Clock
        {
            public override long Tick => DateTimeHelper.CurrentTimeNanos;
        }
    }
}