///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.service;
using com.espertech.esper.core.support;
using com.espertech.esper.pattern;
using com.espertech.esper.schedule;

namespace com.espertech.esper.support.pattern
{
    public class SupportPatternContextFactory
    {
        public static PatternAgentInstanceContext MakePatternAgentInstanceContext()
        {
            return MakePatternAgentInstanceContext(null);
        }

        public static PatternAgentInstanceContext MakePatternAgentInstanceContext(SchedulingService scheduleService)
        {
            StatementContext stmtContext;
            if (scheduleService == null)
            {
                stmtContext = SupportStatementContextFactory.MakeContext();
            }
            else
            {
                stmtContext = SupportStatementContextFactory.MakeContext(scheduleService);
            }
            PatternContext context = new PatternContext(stmtContext, 1, new MatchedEventMapMeta(new String[0], false), false);
            return new PatternAgentInstanceContext(context, SupportStatementContextFactory.MakeAgentInstanceContext(), false);
        }

        public static PatternContext MakeContext()
        {
            StatementContext stmtContext = SupportStatementContextFactory.MakeContext();
            return new PatternContext(stmtContext, 1, new MatchedEventMapMeta(new String[0], false), false);
        }
    }
}
