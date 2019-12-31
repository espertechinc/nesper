///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

namespace NEsper.Examples.Support
{
	/// <summary>
	/// Utility methods for monitoring a EPRuntime instance.
	/// </summary>

	public static class EPRuntimeUtil
	{
	    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	    public static EPStatement DeployStatement(
		    this EPRuntime runtime,
		    string epl)
	    {
		    return CompileDeploy(runtime, epl).Statements[0];
	    }

	    public static EPDeployment CompileDeploy(
		    this EPRuntime runtime,
		    string epl)
	    {
		    try {
			    var args = new CompilerArguments(runtime.ConfigurationDeepCopy);
			    args.Path.Add(runtime.RuntimePath);
			    
			    var compiled = EPCompilerProvider.Compiler.Compile(epl, args);
			    return runtime.DeploymentService.Deploy(compiled);
		    }
		    catch (Exception ex) {
			    throw new EPRuntimeException(ex);
		    }
	    }

	    public static bool AwaitCompletion(
		    this EPRuntime runtime,
	        int numEventsExpected,
	        int numSecAwait,
	        int numSecThreadSleep,
	        int numSecThreadReport)
	    {
	        Log.Info(".awaitCompletion Waiting for completion, expecting " + numEventsExpected +
	                 " events within " + numSecAwait + " sec");
	        
	        var secondsWaitTotal = numSecAwait;
	        var lastNumEventsProcessed = 0L;
	        var secondsUntilReport = 0;
	
	        var startTimeMSec = DateTimeHelper.CurrentTimeMillis;
	        var endTimeMSec = 0L;
	        var numPerSec = 0L;

	        var eventService = runtime.EventService;
	        
	        while (secondsWaitTotal > 0)
	        {
                Thread.Sleep(numSecThreadSleep * 1000);
	
	            secondsWaitTotal -= numSecThreadSleep;
	            secondsUntilReport += numSecThreadSleep;
	            var currNumEventsProcessed = eventService.NumEventsEvaluated;
	
	            if (secondsUntilReport > numSecThreadReport)
	            {
	                numPerSec = (currNumEventsProcessed - lastNumEventsProcessed) / numSecThreadReport;
                    Log.Info(".awaitCompletion received=" + eventService.NumEventsEvaluated +
	                         "  processed=" + currNumEventsProcessed +
	                         "  perSec=" + numPerSec);
	                lastNumEventsProcessed = currNumEventsProcessed;
	                secondsUntilReport = 0;
	            }
	
	            // Completed loop if the total event count has been reached
                if (eventService.NumEventsEvaluated == numEventsExpected)
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

            var totalUnitsProcessed = eventService.NumEventsEvaluated;
	        var deltaTimeSec = (endTimeMSec - startTimeMSec) / 1000;
	
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

            var numReceived = eventService.NumEventsEvaluated;
	        var numReceivedPerSec = 0L;
	        if (deltaTimeSec > 0)
	        {
	            numReceivedPerSec = (numReceived) / deltaTimeSec;
	        }
	        else
	        {
	            numReceivedPerSec = -1;
	        }
	
	        Log.Info(".awaitCompletion Runtime reports, numReceived=" + numReceived +
                     "  numProcessed=" + eventService.NumEventsEvaluated +
	                 "  perSec=" +  numReceivedPerSec +
                     "  numEmitted=" + eventService.NumEventsEvaluated
	                 );
	
	        return true;
	    }
	}
}
