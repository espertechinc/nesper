///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.regressionlib.support.filter;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.support.SupportBean;
using static com.espertech.esper.regressionlib.support.filter.FilterTestMultiStmtAssertItem;
using static com.espertech.esper.regressionlib.support.filter.FilterTestMultiStmtPermutable;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
    [TestFixture]
    public class ExprFilterPlanOneFilterTwoPathNonNested
    {
        public static IList<FilterTestMultiStmtExecution> Executions()
        {
            IList<FilterTestMultiStmtPermutable> cases = new List<FilterTestMultiStmtPermutable>();

            var stats = FilterTestMultiStmtAssertStats.MakeSingleStat("P0=(fh:1,fi:2),P1=(fh:0,fi:0,fipar:0)");

            AddCase(
                cases,
                stats,
                "IntPrimitive = 0 or LongPrimitive = 0",
                MakeItem(MakeBean("E1", 0, 1), true),
                MakeItem(MakeBean("E2", 1, 0), true),
                MakeItem(MakeBean("E3", 1, 1), false));

            return FilterTestMultiStmtRunner.ComputePermutations(
                typeof(ExprFilterPlanOneFilterTwoPathNonNested),
                new PermutationSpec(true),
                cases);
        }
    }
} // end of namespace