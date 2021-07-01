///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Threading;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.support.multithread
{
    public class StmtInsertIntoCallable : ICallable<object>
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int numRepeats;
        private readonly EPRuntime runtime;
        private readonly string threadKey;

        public StmtInsertIntoCallable(
            string threadKey,
            EPRuntime runtime,
            int numRepeats)
        {
            this.runtime = runtime;
            this.numRepeats = numRepeats;
            this.threadKey = threadKey;
        }

        public object Call()
        {
            try {
                for (var loop = 0; loop < numRepeats; loop++) {
                    var eventOne = new SupportBean();
                    eventOne.TheString = "E1_" + threadKey;
                    runtime.EventService.SendEventBean(eventOne, eventOne.GetType().Name);

                    var eventTwo = new SupportMarketDataBean("E2_" + threadKey, 0d, null, null);
                    runtime.EventService.SendEventBean(eventTwo, eventTwo.GetType().Name);
                }
            }
            catch (Exception ex) {
                log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return false;
            }

            return true;
        }
    }
} // end of namespace