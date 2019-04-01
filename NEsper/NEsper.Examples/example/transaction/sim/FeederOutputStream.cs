///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace NEsper.Examples.Transaction.sim
{
    public class FeederOutputStream : OutputStream
    {
        private readonly EPRuntime runtime;
        private readonly long startTimeMSec;

        // We keep increasing the current time to simulate a 30 minute window
        private long currentTimeMSec;

        public FeederOutputStream(EPRuntime runtime)
        {
            this.runtime = runtime;
            startTimeMSec = DateTimeHelper.CurrentTimeMillis;
            currentTimeMSec = startTimeMSec;
        }

        public void Output(IList<TxnEventBase> bucket)
        {
            Log.Info(".output Feeding " + bucket.Count + " events");

            long startTimeMSec = currentTimeMSec;
            long timePeriodLength = FindMissingEventStmt.TIME_WINDOW_TXNC_IN_SEC * 1000;
            long endTimeMSec = startTimeMSec + timePeriodLength;
            SendTimerEvent(startTimeMSec);

            int count = 0, total = 0;
            foreach (TxnEventBase eventBean in bucket)
            {
                runtime.SendEvent(eventBean);
                count++;
                total++;

                if (count % 1000 == 0)
                {
                    SendTimerEvent(startTimeMSec + timePeriodLength * total / bucket.Count);
                    count = 0;
                }

                if (count == 10000)
                {
                    Log.Info(".output Completed " + total + " events");
                    count = 0;
                }
            }

            SendTimerEvent(endTimeMSec);
            currentTimeMSec = endTimeMSec;

            Log.Info(".output Completed bucket");
        }

        private void SendTimerEvent(long msec)
        {
            Log.Info(".sendTimerEvent Setting time to now + " + (msec - startTimeMSec));
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
