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
    public class ExprFilterPlanThreeFilterIndexReuse
    {
        public static IList<FilterTestMultiStmtExecution> Executions()
        {
            IList<FilterTestMultiStmtPermutable> cases = new List<FilterTestMultiStmtPermutable>();

            // Permutations:
            // [0, 1, 2]
            // [0, 2, 1]
            // [1, 0, 2]
            // [1, 2, 0]
            // [2, 0, 1]
            // [2, 1, 0]

            var perm012 =
                "P0=(fh:1, fi:1),P1=(fh:2, fi:2),P2=(fh:3, fi:3),P3=(fh:2, fi:3),P4=(fh:1, fi:3),P5=(fh:0, fi:0, fipar:0)";
            var perm021 =
                "P0=(fh:1, fi:1),P1=(fh:2, fi:3),P2=(fh:3, fi:3),P3=(fh:2, fi:3),P4=(fh:1, fi:2),P5=(fh:0, fi:0, fipar:0)";
            var perm102 =
                "P0=(fh:1, fi:2),P1=(fh:2, fi:2),P2=(fh:3, fi:3),P3=(fh:2, fi:3),P4=(fh:1, fi:3),P5=(fh:0, fi:0, fipar:0)";
            var perm120 =
                "P0=(fh:1, fi:2),P1=(fh:2, fi:3),P2=(fh:3, fi:3),P3=(fh:2, fi:3),P4=(fh:1, fi:1),P5=(fh:0, fi:0, fipar:0)";
            var perm201 =
                "P0=(fh:1, fi:3),P1=(fh:2, fi:3),P2=(fh:3, fi:3),P3=(fh:2, fi:2),P4=(fh:1, fi:2),P5=(fh:0, fi:0, fipar:0)";
            var perm210 =
                "P0=(fh:1, fi:3),P1=(fh:2, fi:3),P2=(fh:3, fi:3),P3=(fh:2, fi:2),P4=(fh:1, fi:1),P5=(fh:0, fi:0, fipar:0)";
            FilterTestMultiStmtAssertStats[] reuseStats = {
                new FilterTestMultiStmtAssertStats(perm012, 0, 1, 2),
                new FilterTestMultiStmtAssertStats(perm021, 0, 2, 1),
                new FilterTestMultiStmtAssertStats(perm102, 1, 0, 2),
                new FilterTestMultiStmtAssertStats(perm120, 1, 2, 0),
                new FilterTestMultiStmtAssertStats(perm201, 2, 0, 1),
                new FilterTestMultiStmtAssertStats(perm210, 2, 1, 0)
            };

            // equals
            FilterTestMultiStmtPermutable.AddCase(
                cases,
                reuseStats,
                "IntPrimitive = 1",
                "IntPrimitive = 1 and LongPrimitive = 10",
                "IntPrimitive = 1 and LongPrimitive = 10 and DoublePrimitive=100",
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E1", 1, 10, 100), true, true, true),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E2", 0, 10, 100), false, false, false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E2", 1, 0, 100), true, false, false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E3", 1, 10, 0), true, true, false));

            return FilterTestMultiStmtRunner.ComputePermutations(
                typeof(ExprFilterPlanThreeFilterIndexReuse),
                new PermutationSpec(2, 1, 0),
                cases);
        }
    }
} // end of namespace