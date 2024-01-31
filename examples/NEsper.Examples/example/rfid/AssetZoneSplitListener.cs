///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using net.esper.client;
using net.esper.event;

using org.apache.commons.logging;

namespace net.esper.example.rfid
{
	public class AssetZoneSplitListener implements UpdateListener
	{
	    private static final Log log = LogFactory.GetLog(typeof(AssetZoneSplitListener));

	    private List<Integer> callbacks = new ArrayList<Integer>();

	    public void Update(EventBean[] newEvents, EventBean[] oldEvents)
	    {
	        int groupId = (Integer) newEvents[0].Get("a.groupId");
	        callbacks.Add(groupId);
	        log.Info(".update Received event from group id " + groupId);
	    }

	    public List<Integer> GetCallbacks()
	    {
	        return callbacks;
	    }

	    public void Reset()
	    {
	        callbacks.Clear();
	    }
	}
} // End of namespace
