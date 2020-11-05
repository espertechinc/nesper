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
    public class ExprFilterPlanOneFilterNestedTwoLvl
    {
        public static IList<FilterTestMultiStmtExecution> Executions(bool withStats)
        {
            IList<FilterTestMultiStmtPermutable> cases = new List<FilterTestMultiStmtPermutable>();

            var stats = FilterTestMultiStmtAssertStats.MakeSingleStat("P0=(fh:1, fi:2),P1=(fh:0, fi:0, fipar:0)");

            // simple two-param equals and not-equals
            FilterTestMultiStmtPermutable.AddCase(
                cases,
                stats,
                "IntPrimitive=1, LongPrimitive=10",
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("A", 1, 10), true),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("A", 1, 0), false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("A", 0, 10), false));

            FilterTestMultiStmtPermutable.AddCase(
                cases,
                stats,
                "IntPrimitive != 0, LongPrimitive != 1",
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E1", 0, 1), false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E2", -1, -1), true),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E3", 0, -1), false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E4", -1, 1), false));

            // simple two-param greater
            FilterTestMultiStmtPermutable.AddCase(
                cases,
                stats,
                "IntPrimitive>1, LongPrimitive>10",
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("A", 1, 10), false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("A", 2, 11), true),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("A", 2, 10), false));

            // two-param range
            FilterTestMultiStmtPermutable.AddCase(
                cases,
                stats,
                "IntPrimitive between 0 and 2, LongPrimitive between 0 and 2",
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E1", 0, 1), true),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E2", 0, 2), true),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E3", 2, 0), true),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E10", 5, 1), false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E11", 1, 5), false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E12", -1, -1), false));

            // two-param 'in'
            FilterTestMultiStmtPermutable.AddCase(
                cases,
                stats,
                "IntPrimitive in (0,1), LongPrimitive in (2, 3)",
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E1", 0, 2), true),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E2", 1, 3), true),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E3", 0, 3), true),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E10", 2, 2), false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E11", 1, 4), false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E12", -1, -1), false));

            // two-param 'not-in'
            FilterTestMultiStmtPermutable.AddCase(
                cases,
                stats,
                "IntPrimitive not in (0,1), LongPrimitive not in (2, 3)",
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E1", 2, 0), true),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E2", -1, -1), true),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E3", 3, 1), true),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E10", 0, 2), false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E11", 1, 4), false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E12", -1, 2), false));

            // equals with boolean
            FilterTestMultiStmtPermutable.AddCase(
                cases,
                stats,
                "IntPrimitive=1 and TheString like 'A%B'",
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("A B", 1), true),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("A", 1), false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("A B", 0), false));

            return FilterTestMultiStmtRunner.ComputePermutations(
                typeof(ExprFilterPlanOneFilterNestedTwoLvl),
                new PermutationSpec(true),
                cases,
                withStats);
        }
    }
} // end of namespace