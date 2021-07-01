///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class EPEventServiceThreadLocalEntry
    {
        public EPEventServiceThreadLocalEntry(
            DualWorkQueue<object> dualWorkQueue,
            ArrayBackedCollection<FilterHandle> matchesArrayThreadLocal,
            ArrayBackedCollection<ScheduleHandle> scheduleArrayThreadLocal,
            IDictionary<EPStatementAgentInstanceHandle, object> matchesPerStmtThreadLocal,
            IDictionary<EPStatementAgentInstanceHandle, object> schedulePerStmtThreadLocal,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            DualWorkQueue = dualWorkQueue;
            MatchesArrayThreadLocal = matchesArrayThreadLocal;
            ScheduleArrayThreadLocal = scheduleArrayThreadLocal;
            MatchesPerStmtThreadLocal = matchesPerStmtThreadLocal;
            SchedulePerStmtThreadLocal = schedulePerStmtThreadLocal;
            ExprEvaluatorContext = exprEvaluatorContext;
        }

        public DualWorkQueue<object> DualWorkQueue { get; }

        public ArrayBackedCollection<FilterHandle> MatchesArrayThreadLocal { get; }

        public ArrayBackedCollection<ScheduleHandle> ScheduleArrayThreadLocal { get; }

        public IDictionary<EPStatementAgentInstanceHandle, object> MatchesPerStmtThreadLocal { get; }

        public IDictionary<EPStatementAgentInstanceHandle, object> SchedulePerStmtThreadLocal { get; }

        public ExprEvaluatorContext ExprEvaluatorContext { get; }
    }
} // end of namespace