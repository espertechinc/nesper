///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.support.multithread
{
    public class StmtNamedWindowDeleteCallable : ICallable<object>
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int numRepeats;
        private readonly EPRuntime runtime;
        private readonly string threadKey;

        public StmtNamedWindowDeleteCallable(
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
            IList<string> eventKeys = new List<string>(numRepeats);
            try {
                for (var loop = 0; loop < numRepeats; loop++) {
                    // Insert event into named window
                    var theEvent = "E" + threadKey + "_" + loop;
                    eventKeys.Add(theEvent);
                    SendMarketBean(theEvent, 0);

                    // delete same event
                    SendSupportBean_A(theEvent);
                }
            }
            catch (Exception ex) {
                log.Error("Error in thread " + Thread.CurrentThread.ManagedThreadId, ex);
                return null;
            }

            return eventKeys;
        }

        private SupportBean_A SendSupportBean_A(string id)
        {
            var bean = new SupportBean_A(id);
            runtime.EventService.SendEventBean(bean, bean.GetType().Name);
            return bean;
        }

        private void SendMarketBean(
            string symbol,
            long volume)
        {
            var bean = new SupportMarketDataBean(symbol, 0, volume, "");
            runtime.EventService.SendEventBean(bean, bean.GetType().Name);
        }
    }
} // end of namespace