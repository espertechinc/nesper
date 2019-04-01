///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.output.view
{
	public class OutputProcessViewAfterStateImpl : OutputProcessViewAfterState {
	    private readonly long? afterConditionTime;
	    private readonly int? afterConditionNumberOfEvents;
	    protected bool isAfterConditionSatisfied;
	    private int afterConditionEventsFound;

	    public OutputProcessViewAfterStateImpl(long? afterConditionTime, int? afterConditionNumberOfEvents) {
	        this.afterConditionTime = afterConditionTime;
	        this.afterConditionNumberOfEvents = afterConditionNumberOfEvents;
	    }

	    /// <summary>
	    /// Returns true if the after-condition is satisfied.
	    /// </summary>
	    /// <param name="newEvents">is the view new events</param>
	    /// <returns>indicator for output condition</returns>
	    public bool CheckUpdateAfterCondition(EventBean[] newEvents, StatementContext statementContext) {
	        return isAfterConditionSatisfied || CheckAfterCondition(newEvents == null ? 0 : newEvents.Length, statementContext);
	    }

	    /// <summary>
	    /// Returns true if the after-condition is satisfied.
	    /// </summary>
	    /// <param name="newEvents">is the join new events</param>
	    /// <returns>indicator for output condition</returns>
	    public bool CheckUpdateAfterCondition(ISet<MultiKey<EventBean>> newEvents, StatementContext statementContext) {
	        return isAfterConditionSatisfied || CheckAfterCondition(newEvents == null ? 0 : newEvents.Count, statementContext);
	    }

	    /// <summary>
	    /// Returns true if the after-condition is satisfied.
	    /// </summary>
	    /// <param name="newOldEvents">is the new and old events pair</param>
	    /// <returns>indicator for output condition</returns>
	    public bool CheckUpdateAfterCondition(UniformPair<EventBean[]> newOldEvents, StatementContext statementContext) {
	        return isAfterConditionSatisfied || CheckAfterCondition(newOldEvents == null ? 0 : (newOldEvents.First == null ? 0 : newOldEvents.First.Length), statementContext);
	    }

	    public void Destroy() {
	        // no action required
	    }

	    private bool CheckAfterCondition(int numOutputEvents, StatementContext statementContext) {
	        if (afterConditionTime != null) {
	            long time = statementContext.TimeProvider.Time;
	            if (time < afterConditionTime) {
	                return false;
	            }

	            isAfterConditionSatisfied = true;
	            return true;
	        } else if (afterConditionNumberOfEvents != null) {
	            afterConditionEventsFound += numOutputEvents;
	            if (afterConditionEventsFound <= afterConditionNumberOfEvents) {
	                return false;
	            }

	            isAfterConditionSatisfied = true;
	            return true;
	        } else {
	            isAfterConditionSatisfied = true;
	            return true;
	        }
	    }
	}
} // end of namespace