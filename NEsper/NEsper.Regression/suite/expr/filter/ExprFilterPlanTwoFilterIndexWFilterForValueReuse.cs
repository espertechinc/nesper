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
    ///     Index reuse with same-key
    ///     - index: {key: [filter, index: {filter}]}
    /// </summary>
    [TestFixture]
    public class ExprFilterPlanTwoFilterIndexWFilterForValueReuse
    {
        public static IList<FilterTestMultiStmtExecution> Executions()
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
                "IntPrimitive = 0",
                "IntPrimitive = 0 and longPrimitive = 0",
                MakeItem(MakeBean("E1", 0, 0), true, true),
                MakeItem(MakeBean("E2", 1, 0), false, false),
                MakeItem(MakeBean("E3", 0, 1), true, false),
                MakeItem(MakeBean("E4", 1, 1), false, false));

            // not-equals
            AddCase(
                cases,
                reuseStats,
                "IntPrimitive != 0",
                "IntPrimitive != 0 and longPrimitive != 0",
                MakeItem(MakeBean("E1", 0, 0), false, false),
                MakeItem(MakeBean("E2", -1, 0), true, false),
                MakeItem(MakeBean("E3", 0, -1), false, false),
                MakeItem(MakeBean("E4", -1, -1), true, true));

            // greater
            AddCase(
                cases,
                reuseStats,
                "IntPrimitive >= 0",
                "IntPrimitive >= 0 and longPrimitive >= 0",
                MakeItem(MakeBean("E1", 0, 0), true, true),
                MakeItem(MakeBean("E2", -1, 0), false, false),
                MakeItem(MakeBean("E3", 0, -1), true, false));

            // 'range'
            AddCase(
                cases,
                reuseStats,
                "IntPrimitive between 0 and 10",
                "IntPrimitive between 0 and 10 and longPrimitive between 10 and 20",
                MakeItem(MakeBean("E1", -1, 16), false, false),
                MakeItem(MakeBean("E2", -1, -1), false, false),
                MakeItem(MakeBean("E3", 5, 15), true, true),
                MakeItem(MakeBean("E4", 2, 2), true, false));

            // 'in'
            AddCase(
                cases,
                reuseStats,
                "IntPrimitive in (0, 1)",
                "IntPrimitive in (0, 1) and longPrimitive in (0, 1)",
                MakeItem(MakeBean("E1", 0, 0), true, true),
                MakeItem(MakeBean("E2", -1, 0), false, false),
                MakeItem(MakeBean("E3", 1, -1), true, false),
                MakeItem(MakeBean("E4", 1, 1), true, true));

            // 'not in'
            AddCase(
                cases,
                reuseStats,
                "IntPrimitive not in (0, 1)",
                "IntPrimitive not in (0, 1) and longPrimitive not in (0, 1)",
                MakeItem(MakeBean("E1", 0, 0), false, false),
                MakeItem(MakeBean("E2", -1, 0), true, false),
                MakeItem(MakeBean("E3", 2, 2), true, true));

            // boolean with equals
            AddCase(
                cases,
                reuseStats,
                "IntPrimitive = 0",
                "IntPrimitive = 0 and theString like 'B%'",
                MakeItem(MakeBean("A", 0), true, false),
                MakeItem(MakeBean("B", 0), true, true),
                MakeItem(MakeBean("B", 1), false, false));

            return FilterTestMultiStmtRunner.ComputePermutations(
                typeof(ExprFilterPlanTwoFilterIndexWFilterForValueReuse),
                new PermutationSpec(true),
                cases);
        }
    }
} // end of namespace