///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.supportregression.subscriber
{
    using DataMap = IDictionary<string, object>;

	public class SupportSubscriberRowByRowMapNStmt : SupportSubscriberRowByRowMapBase
	{
	    public SupportSubscriberRowByRowMapNStmt() : base(false)
        {
	    }

	    public void Update(DataMap row)
	    {
	        AddIndicationIStream(row);
	    }

	    public void UpdateRStream(DataMap row)
	    {
	        AddIndicationRStream(row);
	    }
	}
} // end of namespace
