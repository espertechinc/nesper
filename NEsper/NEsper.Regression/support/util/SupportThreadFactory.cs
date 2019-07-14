///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.compat;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.regressionlib.support.util
{
    public class SupportThreadFactory
    {
        private readonly Type user;

        public SupportThreadFactory(Type user)
        {
            this.user = user;
        }

        public ThreadFactory ThreadFactory => NewThread;

        public Thread NewThread(ThreadStart ts)
        {
            return new Thread(ts) {
                Name = user.Name,
                IsBackground = true
            };
        }

        public Thread NewThread(Runnable r)
        {
            return new Thread(r.Invoke) {
                Name = user.Name,
                IsBackground = true
            };
        }
    }
} // end of namespace