///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.support.filter;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
    [TestFixture]
    public class ExprFilterPlanOneFilterTwoPathNested
    {
        public static IList<FilterTestMultiStmtExecution> Executions()
        {
            IList<FilterTestMultiStmtPermutable> cases = new List<FilterTestMultiStmtPermutable>();

            var stats = FilterTestMultiStmtAssertStats.MakeSingleStat("P0=(fh:1,fi:3),P1=(fh:0,fi:0,fipar:0)");

            FilterTestMultiStmtPermutable.AddCase(
                cases,
                stats,
                "theString = 'A' and (IntPrimitive = 0 or LongPrimitive = 0)",
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("A", 0, 1), true),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("A", 1, 0), true),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("A", 1, 1), false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("B", 0, 0), false));

            return FilterTestMultiStmtRunner.ComputePermutations(
                typeof(ExprFilterPlanOneFilterTwoPathNested),
                new PermutationSpec(true),
                cases);
        }
    }
} // end of namespace