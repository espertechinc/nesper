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
    public class ExprFilterPlanTwoFilterNestedTwoSame
    {
        public static IList<FilterTestMultiStmtExecution> Executions()
        {
            IList<FilterTestMultiStmtPermutable> cases = new List<FilterTestMultiStmtPermutable>();

            var stats = FilterTestMultiStmtAssertStats.MakeTwoSameStat(
                "P0=(fh:1, fi:2),P1=(fh:2, fi:2),P2=(fh:1, fi:2),P3=(fh:0, fi:0, fipar:0)");

            // same equals-indexes
            AddCase(
                cases,
                stats,
                "IntPrimitive = 0 and longPrimitive = 0",
                "IntPrimitive = 0 and longPrimitive = 0",
                MakeItem(MakeBean("E1", 0, 0), true, true),
                MakeItem(MakeBean("E2", 1, 0), false, false),
                MakeItem(MakeBean("E3", 0, 1), false, false));

            // same not-equals-index
            AddCase(
                cases,
                stats,
                "IntPrimitive != 1 and longPrimitive != 2",
                "IntPrimitive != 1 and longPrimitive != 2",
                MakeItem(MakeBean("E1", 1, 2), false, false),
                MakeItem(MakeBean("E2", 2, 3), true, true),
                MakeItem(MakeBean("E3", 1, -1), false, false),
                MakeItem(MakeBean("E4", -1, 2), false, false));

            // same greater-indexes
            AddCase(
                cases,
                stats,
                "IntPrimitive > 0 and longPrimitive > 0",
                "IntPrimitive > 0 and longPrimitive > 0",
                MakeItem(MakeBean("E1", 1, 1), true, true),
                MakeItem(MakeBean("E2", 1, 0), false, false),
                MakeItem(MakeBean("E3", 0, 1), false, false));

            // same range-index
            AddCase(
                cases,
                stats,
                "IntPrimitive between 0 and 10 and longPrimitive between 0 and 10",
                "IntPrimitive between 0 and 10 and longPrimitive between 0 and 10",
                MakeItem(MakeBean("E1", 1, 1), true, true),
                MakeItem(MakeBean("E2", 1, -1), false, false),
                MakeItem(MakeBean("E3", -1, 1), false, false));

            // same in-index
            AddCase(
                cases,
                stats,
                "IntPrimitive in (1, 2) and longPrimitive in (2, 3)",
                "IntPrimitive in (1, 2) and longPrimitive in (2, 3)",
                MakeItem(MakeBean("E1", 1, 2), true, true),
                MakeItem(MakeBean("E2", 2, 3), true, true),
                MakeItem(MakeBean("E3", 1, -1), false, false),
                MakeItem(MakeBean("E4", -1, 1), false, false));

            // same not-in-index
            AddCase(
                cases,
                stats,
                "IntPrimitive not in (1, 2) and longPrimitive not in (2, 3)",
                "IntPrimitive not in (1, 2) and longPrimitive not in (2, 3)",
                MakeItem(MakeBean("E1", 1, 2), false, false),
                MakeItem(MakeBean("E2", 2, 3), false, false),
                MakeItem(MakeBean("E3", -1, -1), true, true),
                MakeItem(MakeBean("E4", -1, 2), false, false));

            // we permute only [0, 1] as all filters are the same
            return FilterTestMultiStmtRunner.ComputePermutations(
                typeof(ExprFilterPlanTwoFilterNestedTwoSame),
                new PermutationSpec(0, 1),
                cases);
        }
    }
} // end of namespace