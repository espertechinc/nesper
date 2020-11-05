///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.schedule
{
    /// <summary>
    /// Provider of internal system time.
    /// <para>
    /// Internal system time is controlled either by a timer function or by external time events.
    /// </para>
    /// </summary>
    public interface TimeProvider
    {
        /// <summary>Returns the current engine time.</summary>
        /// <returns>time that has last been set</returns>
        long Time { get; set; }
    }

    /// <summary>
    /// A proxy implementation of the time provider
    /// </summary>
    public class ProxyTimeProvider : TimeProvider
    {
        public Func<long> Get { get; set; }
        public Action<long> Set { get; set; }

        public long Time {
            get { return Get(); }
            set { Set(value); }
        }
    }
} // End of namespace