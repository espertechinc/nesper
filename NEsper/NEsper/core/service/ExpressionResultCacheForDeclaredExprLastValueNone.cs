///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;

namespace com.espertech.esper.core.service
{
	public class ExpressionResultCacheForDeclaredExprLastValueNone : ExpressionResultCacheForDeclaredExprLastValue
    {
	    public bool CacheEnabled()
        {
	        return false;
	    }

	    public ExpressionResultCacheEntry<EventBean[], object> GetDeclaredExpressionLastValue(object node, EventBean[] eventsPerStream)
        {
	        return null;
	    }

	    public void SaveDeclaredExpressionLastValue(object node, EventBean[] eventsPerStream, object result)
        {
	    }
	}
} // end of namespace
