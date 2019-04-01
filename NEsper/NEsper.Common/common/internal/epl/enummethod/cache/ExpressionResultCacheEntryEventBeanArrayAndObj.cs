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
	/// <summary>
	/// Cache entry bean-to-collection-of-bean.
	/// </summary>
	public class ExpressionResultCacheEntryEventBeanArrayAndObj {
	    private EventBean[] reference;
	    private object result;

	    public ExpressionResultCacheEntryEventBeanArrayAndObj(EventBean[] reference, object result) {
	        this.reference = reference;
	        this.result = result;
	    }

	    public EventBean[] GetReference() {
	        return reference;
	    }

	    public void SetReference(EventBean[] reference) {
	        this.reference = reference;
	    }

	    public object Result {
	        get => result;
	    }

	    public void SetResult(object result) {
	        this.result = result;
	    }
	}
} // end of namespace