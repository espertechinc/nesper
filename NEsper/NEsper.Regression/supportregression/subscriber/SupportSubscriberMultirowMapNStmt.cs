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
    
    public class SupportSubscriberMultirowMapNStmt : SupportSubscriberMultirowMapBase
	{
	    public SupportSubscriberMultirowMapNStmt() : base(false)
        {
	    }

        public void Update(DataMap[] newEvents, DataMap[] oldEvents)
        {
	        AddIndication(newEvents, oldEvents);
	    }
	}
} // end of namespace
