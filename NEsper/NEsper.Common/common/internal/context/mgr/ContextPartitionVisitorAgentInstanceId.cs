///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.mgr
{
	public class ContextPartitionVisitorAgentInstanceId : ContextPartitionVisitor {
	    private readonly int numLevels;
	    private readonly ISet<int> ids = new HashSet<>();

	    public ContextPartitionVisitorAgentInstanceId(int numLevels) {
	        this.numLevels = numLevels;
	    }

	    public void Add(int id, int nestingLevel) {
	        if (nestingLevel == numLevels) {
	            ids.Add(id);
	        }
	    }

	    public ISet<int> Ids
	    {
	        get => ids;
	    }
	}
} // end of namespace