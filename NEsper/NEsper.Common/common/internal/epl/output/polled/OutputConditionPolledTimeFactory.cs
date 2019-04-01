///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.output.polled
{
	public sealed class OutputConditionPolledTimeFactory : OutputConditionPolledFactory {
	    internal readonly TimePeriodCompute timePeriodCompute;

	    public OutputConditionPolledTimeFactory(TimePeriodCompute timePeriodCompute) {
	        this.timePeriodCompute = timePeriodCompute;
	    }

	    public OutputConditionPolled MakeNew(AgentInstanceContext agentInstanceContext) {
	        return new OutputConditionPolledTime(this, agentInstanceContext, new OutputConditionPolledTimeState(null));
	    }

	    public OutputConditionPolled MakeFromState(AgentInstanceContext agentInstanceContext, OutputConditionPolledState state) {
	        OutputConditionPolledTimeState timeState = (OutputConditionPolledTimeState) state;
	        return new OutputConditionPolledTime(this, agentInstanceContext, timeState);
	    }
	}
} // end of namespace