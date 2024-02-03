///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.support.filter;

using static com.espertech.esper.regressionlib.support.filter.FilterTestMultiStmtAssertItem;
using static com.espertech.esper.regressionlib.support.filter.FilterTestMultiStmtAssertStats;
using static com.espertech.esper.regressionlib.support.filter.FilterTestMultiStmtPermutable;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
    public class ExprFilterPlanNoFilter
    {
        public static IList<FilterTestMultiStmtExecution> Executions(bool withStats)
        {
            IList<FilterTestMultiStmtPermutable> cases = new List<FilterTestMultiStmtPermutable>();

            // no filter
            AddCase(
                cases,
                MakeSingleStat("P0=(fh:1,fi:0),P1=(fh:0,fi:0)"),
                "",
                MakeItem(SupportBean.MakeBean("E1"), true)); // no filter

            return FilterTestMultiStmtRunner.ComputePermutations(
                typeof(ExprFilterPlanNoFilter),
                new PermutationSpec(true),
                cases,
                withStats);
        }
    }
} // end of namespace