///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.namedwindow.consume
{
	public abstract class NamedWindowConsumerLatch {
	    private readonly NamedWindowDeltaData deltaData;
	    private readonly IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> dispatchTo;

	    public abstract void Await();

	    public abstract void Done();

	    public abstract NamedWindowConsumerLatch Earlier { get; }

	    public NamedWindowConsumerLatch(NamedWindowDeltaData deltaData, IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> dispatchTo) {
	        this.deltaData = deltaData;
	        this.dispatchTo = dispatchTo;
	    }

	    public NamedWindowDeltaData DeltaData
	    {
	        get => deltaData;
	    }

	    public IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> GetDispatchTo() {
	        return dispatchTo;
	    }
	}
} // end of namespace