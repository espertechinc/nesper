///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using net.esper.client;
using net.esper.event;

using org.apache.commons.logging;

namespace net.esper.example.rfid
{
	public class AssetGroupCountListener implements UpdateListener
	{
	    private static final Log log = LogFactory.GetLog(typeof(AssetGroupCountListener));

	    public void Update(EventBean[] newEvents, EventBean[] oldEvents)
	    {
	        int groupId = (Integer) newEvents[0].Get("groupId");
	        int zone = (Integer) newEvents[0].Get("zone");
	        long cnt = (Long) newEvents[0].Get("cnt");

	        log.Info(".update " +
	                " groupId=" + groupId +
	                " zone=" + zone +
	                " cnt=" + cnt);
	    }
	}
} // End of namespace
