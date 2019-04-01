///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;

namespace com.espertech.esper.supportregression.subscriber
{
	public class SupportSubscriberMultirowObjectArrayWStmt : SupportSubscriberMultirowObjectArrayBase
	{
	    public SupportSubscriberMultirowObjectArrayWStmt() : base(true)
        {
	    }

	    public void Update(EPStatement stmt, object[][] newEvents, object[][] oldEvents)
	    {
	        AddIndication(stmt, newEvents, oldEvents);
	    }
	}
} // end of namespace
