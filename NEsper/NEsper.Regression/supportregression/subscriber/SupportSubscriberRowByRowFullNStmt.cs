///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.supportregression.subscriber
{
	public class SupportSubscriberRowByRowFullNStmt : SupportSubscriberRowByRowFullBase
	{
	    public SupportSubscriberRowByRowFullNStmt() : base(false)
        {
	    }

	    public void UpdateStart(int lengthIStream, int lengthRStream)
	    {
	        AddUpdateStart(lengthIStream, lengthRStream);
	    }

	    public void Update(string theString, int intPrimitive)
	    {
	        AddUpdate(new object[] {theString, intPrimitive});
	    }

	    public void UpdateRStream(string theString, int intPrimitive)
	    {
	        AddUpdateRStream(new object[] {theString, intPrimitive});
	    }

	    public void UpdateEnd()
	    {
	        AddUpdateEnd();
	    }
	}
} // end of namespace
