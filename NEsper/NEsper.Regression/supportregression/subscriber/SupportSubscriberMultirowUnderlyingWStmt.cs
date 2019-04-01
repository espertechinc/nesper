///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.supportregression.bean;

namespace com.espertech.esper.supportregression.subscriber
{
	public class SupportSubscriberMultirowUnderlyingWStmt : SupportSubscriberMultirowUnderlyingBase
	{
	    public SupportSubscriberMultirowUnderlyingWStmt() : base(true)
        {
	    }

	    public void Update(EPStatement stmt, SupportBean[]newEvents, SupportBean[] oldEvents)
	    {
	        AddIndication(stmt, newEvents, oldEvents);
	    }
	}
} // end of namespace
