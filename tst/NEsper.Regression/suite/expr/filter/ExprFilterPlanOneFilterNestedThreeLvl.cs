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
    public class ExprFilterPlanOneFilterNestedThreeLvl
    {
        public static IList<FilterTestMultiStmtExecution> Executions(bool withStats)
        {
            IList<FilterTestMultiStmtPermutable> cases = new List<FilterTestMultiStmtPermutable>();

            var stats = FilterTestMultiStmtAssertStats.MakeSingleStat("P0=(fh:1, fi:3),P1=(fh:0, fi:0, fipar:0)");

            // simple three-param equals
            FilterTestMultiStmtPermutable.AddCase(
                cases,
                stats,
                "IntPrimitive=1, LongPrimitive=10, DoublePrimitive=100",
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("A", 1, 10, 100), true),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("A", 1, 10, -1), false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("A", 0, 10, 100), false));

            // two-param equals with boolean
            FilterTestMultiStmtPermutable.AddCase(
                cases,
                stats,
                "IntPrimitive=1, LongPrimitive=10, TheString like 'A%'",
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("A", 1, 10), true),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("B", 1, 10), false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("A", 1, 0), false));

            return FilterTestMultiStmtRunner.ComputePermutations(
                typeof(ExprFilterPlanOneFilterNestedThreeLvl),
                new PermutationSpec(true),
                cases,
                withStats);
        }
    }
} // end of namespace