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
	public class SupportSubscriberRowByRowFullWStmt : SupportSubscriberRowByRowFullBase
	{
	    public SupportSubscriberRowByRowFullWStmt() : base(true)
        {
	    }

	    public void UpdateStart(EPStatement statement, int lengthIStream, int lengthRStream)
	    {
	        AddUpdateStart(statement, lengthIStream, lengthRStream);
	    }

	    public void Update(EPStatement statement, string theString, int intPrimitive)
	    {
	        AddUpdate(statement, new object[] {theString, intPrimitive});
	    }

	    public void UpdateRStream(EPStatement statement, string theString, int intPrimitive)
	    {
	        AddUpdateRStream(statement, new object[] {theString, intPrimitive});
	    }

	    public void UpdateEnd(EPStatement statement)
	    {
	        AddUpdateEnd(statement);
	    }
	}
} // end of namespace
