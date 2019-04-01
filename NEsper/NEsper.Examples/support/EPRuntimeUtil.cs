///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace NEsper.Examples.Support
{
	/// <summary>
	/// Utility methods for monitoring a EPRuntime instance.
	/// </summary>

	public class EPRuntimeUtil
	{
	    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	    public static bool AwaitCompletion(
		    EPRuntime epRuntime,
	        int numEventsExpected,
	        int numSecAwait,
	        int numSecThreadSleep,
	        int numSecThreadReport)
	    {
	        Log.Info(".awaitCompletion Waiting for completion, expecting " + numEventsExpected +
	                 " events within " + numSecAwait + " sec");
	        
	        int secondsWaitTotal = numSecAwait;
	        long lastNumEventsProcessed = 0;
	        int secondsUntilReport = 0;
	
	        long startTimeMSec = DateTimeHelper.CurrentTimeMillis;
	        long endTimeMSec = 0;
	        long numPerSec = 0;
	
	        while (secondsWaitTotal > 0)
	        {
                Thread.Sleep(numSecThreadSleep * 1000);
	
	            secondsWaitTotal -= numSecThreadSleep;
	            secondsUntilReport += numSecThreadSleep;
	            long currNumEventsProcessed = epRuntime.NumEventsEvaluated;
	
	            if (secondsUntilReport > numSecThreadReport)
	            {
	                numPerSec = (currNumEventsProcessed - lastNumEventsProcessed) / numSecThreadReport;
                    Log.Info(".awaitCompletion received=" + epRuntime.NumEventsEvaluated +
	                         "  processed=" + currNumEventsProcessed +
	                         "  perSec=" + numPerSec);
	                lastNumEventsProcessed = currNumEventsProcessed;
	                secondsUntilReport = 0;
	            }
	
	            // Completed loop if the total event count has been reached
                if (epRuntime.NumEventsEvaluated == numEventsExpected)
	            {
	                endTimeMSec = DateTimeHelper.CurrentTimeMillis;
	                break;
	            }
	        }
	
	        if (endTimeMSec == 0)
	        {
	            Log.Info(".awaitCompletion Not completed within " + numSecAwait + " seconds");
	            return false;
	        }

            long totalUnitsProcessed = epRuntime.NumEventsEvaluated;
	        long deltaTimeSec = (endTimeMSec - startTimeMSec) / 1000;
	
	        numPerSec = 0;
	        if (deltaTimeSec > 0)
	        {
	            numPerSec = (totalUnitsProcessed) / deltaTimeSec;
	        }
	        else
	        {
	            numPerSec = -1;
	        }
	
	        Log.Info(".awaitCompletion Completed, sec=" + deltaTimeSec + "  avgPerSec=" + numPerSec);

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
	
	        Log.Info(".awaitCompletion Runtime reports, numReceived=" + numReceived +
                     "  numProcessed=" + epRuntime.NumEventsEvaluated +
	                 "  perSec=" +  numReceivedPerSec +
                     "  numEmitted=" + epRuntime.NumEventsEvaluated
	                 );
	
	        return true;
	    }
	}
}
