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
    /// <summary>
    ///     Index reuse with different-key
    ///     - index: {key: [filter, index: {filter}]}
    /// </summary>
    public class ExprFilterPlanTwoFilterIndexReuse
    {
        public static ICollection<FilterTestMultiStmtExecution> Executions(bool withStats)
        {
            IList<FilterTestMultiStmtPermutable> cases = new List<FilterTestMultiStmtPermutable>();

            FilterTestMultiStmtAssertStats[] reuseStats = {
                new FilterTestMultiStmtAssertStats(
                    "P0=(fh:1, fi:1),P1=(fh:2, fi:2),P2=(fh:1, fi:2),P3=(fh:0, fi:0, fipar:0)",
                    0,
                    1),
                new FilterTestMultiStmtAssertStats(
                    "P0=(fh:1, fi:2),P1=(fh:2, fi:2),P2=(fh:1, fi:1),P3=(fh:0, fi:0, fipar:0)",
                    1,
                    0)
            };

            // equals
            AddCase(
                cases,
                reuseStats,
                "IntPrimitive = 1",
                "IntPrimitive = 0 and LongPrimitive = 0",
                MakeItem(MakeBean("E1", 0, 0), false, true),
                MakeItem(MakeBean("E2", 1, 0), true, false),
                MakeItem(MakeBean("E3", 0, 1), false, false));

            // not-equals
            AddCase(
                cases,
                reuseStats,
                "IntPrimitive != 5",
                "IntPrimitive != 0 and LongPrimitive != 0",
                MakeItem(MakeBean("E1", 0, 0), true, false),
                MakeItem(MakeBean("E2", 5, 0), false, false),
                MakeItem(MakeBean("E3", 5, 5), false, true),
                MakeItem(MakeBean("E4", -1, -1), true, true));

            // greater
            AddCase(
                cases,
                reuseStats,
                "IntPrimitive >= 5",
                "IntPrimitive >= 0 and LongPrimitive >= 0",
                MakeItem(MakeBean("E1", 0, 0), false, true),
                MakeItem(MakeBean("E2", -1, 0), false, false),
                MakeItem(MakeBean("E3", 10, -1), true, false),
                MakeItem(MakeBean("E4", 10, 1), true, true));

            // 'range'
            AddCase(
                cases,
                reuseStats,
                "IntPrimitive between 0 and 10",
                "IntPrimitive between 10 and 20 and LongPrimitive between 10 and 20",
                MakeItem(MakeBean("E1", -1, 16), false, false),
                MakeItem(MakeBean("E2", 10, -1), true, false),
                MakeItem(MakeBean("E3", 10, 15), true, true),
                MakeItem(MakeBean("E4", 15, 15), false, true));

            // 'in'
            AddCase(
                cases,
                reuseStats,
                "IntPrimitive in (0, 1)",
                "IntPrimitive in (0, 2) and LongPrimitive in (0, 2)",
                MakeItem(MakeBean("E1", 0, 0), true, true),
                MakeItem(MakeBean("E2", -1, 0), false, false),
                MakeItem(MakeBean("E3", 1, -1), true, false),
                MakeItem(MakeBean("E4", 2, 2), false, true));

            // 'not in'
            AddCase(
                cases,
                reuseStats,
                "IntPrimitive not in (0, 1)",
                "IntPrimitive not in (0, 2) and LongPrimitive not in (0, 2)",
                MakeItem(MakeBean("E1", 0, 0), false, false),
                MakeItem(MakeBean("E2", -1, 0), true, false),
                MakeItem(MakeBean("E3", 1, 1), false, true),
                MakeItem(MakeBean("E4", 3, 3), true, true));

            // boolean with equals
            AddCase(
                cases,
                reuseStats,
                "IntPrimitive = 1",
                "IntPrimitive = 0 and TheString like 'B%'",
                MakeItem(MakeBean("A", 1), true, false),
                MakeItem(MakeBean("B", 0), false, true),
                MakeItem(MakeBean("B", 2), false, false));

            return FilterTestMultiStmtRunner.ComputePermutations(
                typeof(ExprFilterPlanTwoFilterIndexReuse),
                new PermutationSpec(true),
                cases,
                withStats);
        }
    }
} // end of namespace