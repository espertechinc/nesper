///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.support
{
	public class SendEventRunnable : IRunnable
	{
	    private readonly Object _eventToSend;
        private readonly EPServiceProvider _epService;
	
	    public SendEventRunnable(EPServiceProvider epService, Object eventToSend)
	    {
	        _epService = epService;
	        _eventToSend = eventToSend;
	    }
	
	    public void Run()
	    {
	        try
	        {
	            _epService.EPRuntime.SendEvent(_eventToSend);
	        }
	        catch (Exception ex)
	        {
	            Log.Fatal(string.Empty, ex);
	        }
	    }
	
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	}
}
