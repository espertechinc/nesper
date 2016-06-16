///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.view;

namespace com.espertech.esper.epl.core
{
    public interface ResultSetProcessorGroupedOutputFirstHelper {
	    OutputConditionPolled GetOrAllocate(object mk, AgentInstanceContext agentInstanceContext, OutputConditionPolledFactory optionalOutputFirstConditionFactory);
	    void Remove(object key);
	    void Destroy();
	}
} // end of namespace
