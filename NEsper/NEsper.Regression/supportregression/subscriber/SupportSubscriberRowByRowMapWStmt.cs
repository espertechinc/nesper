///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;

namespace com.espertech.esper.supportregression.subscriber
{
    using DataMap = IDictionary<string, object>;

	public class SupportSubscriberRowByRowMapWStmt : SupportSubscriberRowByRowMapBase
	{
	    public SupportSubscriberRowByRowMapWStmt() : base(true)
        {
	    }

	    public void Update(EPStatement stmt, DataMap row)
	    {
	        AddIndicationIStream(stmt, row);
	    }

	    public void UpdateRStream(EPStatement stmt, DataMap row)
	    {
	        AddIndicationRStream(stmt, row);
	    }
	}
} // end of namespace
