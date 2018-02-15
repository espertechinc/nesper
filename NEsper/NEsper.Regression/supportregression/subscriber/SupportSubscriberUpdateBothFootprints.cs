///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.subscriber
{
	public class SupportSubscriberUpdateBothFootprints : SupportSubscriberRowByRowSpecificBase
    {
	    public SupportSubscriberUpdateBothFootprints() : base(true)
        {
	    }

	    public void Update(string theString, int intPrimitive)
        {
	        Assert.Fail();
	    }

	    public void Update(EPStatement stmt, string theString, int intPrimitive)
        {
	        AddIndication(stmt, new object[] {theString, intPrimitive});
	    }
	}
} // end of namespace
