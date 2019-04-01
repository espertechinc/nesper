///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.supportunit.util;

namespace com.espertech.esper.supportunit.pattern
{
    public class SupportPatternContextFactory
    {
        public static PatternAgentInstanceContext MakePatternAgentInstanceContext()
        {
            return MakePatternAgentInstanceContext(null);
        }

        public static PatternAgentInstanceContext MakePatternAgentInstanceContext(SchedulingService scheduleService)
        {
            var container = SupportContainer.Instance;

            StatementContext stmtContext;
            if (scheduleService == null)
            {
                stmtContext = SupportStatementContextFactory.MakeContext(container);
            }
            else
            {
                stmtContext = SupportStatementContextFactory.MakeContext(container, scheduleService);
            }
            PatternContext context = new PatternContext(stmtContext, 1, new MatchedEventMapMeta(new String[0], false), false);
            return new PatternAgentInstanceContext(context, SupportStatementContextFactory.MakeAgentInstanceContext(container), false);
        }

        public static PatternContext MakeContext()
        {
            var container = SupportContainer.Instance;
            StatementContext stmtContext = SupportStatementContextFactory.MakeContext(container);
            return new PatternContext(stmtContext, 1, new MatchedEventMapMeta(new String[0], false), false);
        }
    }
}
