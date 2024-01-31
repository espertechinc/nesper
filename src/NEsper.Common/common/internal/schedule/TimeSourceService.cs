///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.schedule
{
    public interface TimeSourceService
    {
        /// <summary>
        /// Returns time in millis.
        /// </summary>
        long TimeMillis { get; }
    }

    public class ProxyTimeSourceService : TimeSourceService
    {
        public Func<long> ProcTimeMillis { get; set; }

        public ProxyTimeSourceService()
        {
        }

        public ProxyTimeSourceService(Func<long> procTimeMillis)
        {
            ProcTimeMillis = procTimeMillis;
        }

        public long TimeMillis => ProcTimeMillis.Invoke();
    }
}