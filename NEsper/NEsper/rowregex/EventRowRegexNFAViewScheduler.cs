///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;

namespace com.espertech.esper.rowregex
{
    public interface EventRowRegexNFAViewScheduler
	{
	    void SetScheduleCallback(AgentInstanceContext agentInstanceContext, EventRowRegexNFAViewScheduleCallback scheduleCallback);
	    void AddSchedule(long msecAfterCurrentTime);
	    void ChangeSchedule(long msecAfterCurrentTime);
	    void RemoveSchedule();
	}
} // end of namespace
