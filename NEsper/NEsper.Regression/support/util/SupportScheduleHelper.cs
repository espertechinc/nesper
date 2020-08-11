///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.service;
using com.espertech.esper.runtime.@internal.kernel.statement;
using com.espertech.esper.runtime.@internal.schedulesvcimpl;

namespace com.espertech.esper.regressionlib.support.util
{
    public class SupportScheduleHelper
    {
        public static int ScheduleCount(EPStatement statement)
        {
            if (statement == null) {
                throw new IllegalStateException("Statement is null");
            }

            var spi = (EPStatementSPI) statement;
            var schedulingServiceSPI = (SchedulingServiceSPI) spi.StatementContext.SchedulingService;
            var visitor = new ScheduleVisitorStatement(spi.StatementId);
            schedulingServiceSPI.VisitSchedules(visitor);
            return visitor.Count;
        }

        public static int ScheduleCountOverall(RegressionEnvironment env)
        {
            var spi = (EPRuntimeSPI) env.Runtime;
            var visitor = new ScheduleVisitorAll();
            spi.ServicesContext.SchedulingServiceSPI.VisitSchedules(visitor);
            return visitor.Count;
        }

        internal class ScheduleVisitorStatement : ScheduleVisitor
        {
            private readonly int statementId;

            public ScheduleVisitorStatement(int statementId)
            {
                this.statementId = statementId;
            }

            public int Count { get; private set; }

            public void Visit(ScheduleVisit visit)
            {
                if (visit.StatementId == statementId) {
                    Count++;
                }
            }
        }

        internal class ScheduleVisitorAll : ScheduleVisitor
        {
            public int Count { get; private set; }

            public void Visit(ScheduleVisit visit)
            {
                Count++;
            }
        }
    }
} // end of namespace