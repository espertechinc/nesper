///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.enummethod.cache
{
	public class ExpressionResultCacheForDeclaredExprLastValueNone : ExpressionResultCacheForDeclaredExprLastValue {

	    public bool CacheEnabled() {
	        return false;
	    }

	    public ExpressionResultCacheEntryEventBeanArrayAndObj GetDeclaredExpressionLastValue(object node, EventBean[] eventsPerStream) {
	        return null;
	    }

	    public void SaveDeclaredExpressionLastValue(object node, EventBean[] eventsPerStream, object result) {
	    }
	}
} // end of namespace