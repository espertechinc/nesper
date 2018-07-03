///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.compat.logging;

namespace NEsper.Examples.StockTicker
{
    /// <summary>
    /// Utility methods for monitoring a EPRuntime instance.
    /// </summary>
    public class EPRuntimeUtil
    {
        public static bool AwaitCompletion(EPRuntime epRuntime,
                                           int numEventsExpected,
                                           int numSecAwait,
                                           int numSecThreadSleep,
                                           int numSecThreadReport)
        {
            log.Info(".awaitCompletion Waiting for completion, expecting " + numEventsExpected +
                     " events within " + numSecAwait + " sec");
    
            int secondsWaitTotal = numSecAwait;
            long lastNumEventsProcessed = 0;
            int secondsUntilReport = 0;
    
            long startTimeMSec = Environment.TickCount;
            long endTimeMSec = 0;
    
            while (secondsWaitTotal > 0)
            {
                Thread.Sleep(numSecThreadSleep * 1000);
    
                secondsWaitTotal -= numSecThreadSleep;
                secondsUntilReport += numSecThreadSleep;
                long currNumEventsProcessed = epRuntime.NumEventsEvaluated;
    
                if (secondsUntilReport > numSecThreadReport)
                {
                    long numPerSec = (currNumEventsProcessed - lastNumEventsProcessed) / numSecThreadReport;
                    log.Info(".awaitCompletion received=" + epRuntime.NumEventsEvaluated +
                             "  processed=" + currNumEventsProcessed +
                             "  perSec=" + numPerSec);
                    lastNumEventsProcessed = currNumEventsProcessed;
                    secondsUntilReport = 0;
                }
    
                // Completed loop if the total event count has been reached
                if (epRuntime.NumEventsEvaluated == numEventsExpected)
                {
                    endTimeMSec = Environment.TickCount;
                    break;
                }
            }
    
            if (endTimeMSec == 0)
            {
                log.Info(".awaitCompletion Not completed within " + numSecAwait + " seconds");
                return false;
            }
    
            long totalUnitsProcessed = epRuntime.NumEventsEvaluated;
            long deltaTimeSec = (endTimeMSec - startTimeMSec) / 1000;
    
            long perSec;
            if (deltaTimeSec > 0)
            {
                perSec = (totalUnitsProcessed) / deltaTimeSec;
            }
            else
            {
                perSec = -1;
            }
    
            log.Info(".awaitCompletion Completed, sec=" + deltaTimeSec + "  avgPerSec=" + perSec);
    
            long numReceived = epRuntime.NumEventsEvaluated;
            long numReceivedPerSec = 0;
            if (deltaTimeSec > 0)
            {
                numReceivedPerSec = (numReceived) / deltaTimeSec;
            }
            else
            {
                numReceivedPerSec = -1;
            }
    
            log.Info(".awaitCompletion Runtime reports, numReceived=" + numReceived +
                     "  numProcessed=" + epRuntime.NumEventsEvaluated +
                     "  perSec=" +  numReceivedPerSec
                     );
    
            return true;
        }
    
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
