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

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
    [TestFixture]
    public class ExprFilterPlanTwoFilterNestedTwoDiff
    {
        public static IList<FilterTestMultiStmtExecution> Executions(bool withStats)
        {
            IList<FilterTestMultiStmtPermutable> cases = new List<FilterTestMultiStmtPermutable>();

            var stats = FilterTestMultiStmtAssertStats.MakeTwoSameStat(
                "P0=(fh:1, fi:2),P1=(fh:2, fi:3),P2=(fh:1, fi:2),P3=(fh:0, fi:0, fipar:0)");

            // same equals-indexes
            FilterTestMultiStmtPermutable.AddCase(
                cases,
                stats,
                "TheString != 'x' and TheString != 'y' and DoubleBoxed is not null",
                "TheString != 'x' and TheString != 'y' and LongBoxed is not null",
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBeanWBoxed("E1", -1, null, null), false, false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBeanWBoxed("x", -1, 1d, 1L), false, false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBeanWBoxed("x", -1, 1d, null), false, false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBeanWBoxed("y", -1, 1d, 1L), false, false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBeanWBoxed("E2", -1, 1d, 1L), true, true),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBeanWBoxed("E3", -1, 1d, null), true, false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBeanWBoxed("E4", -1, null, 1L), false, true));

            // we permute only [0, 1] as all filters are the same
            return FilterTestMultiStmtRunner.ComputePermutations(
                typeof(ExprFilterPlanTwoFilterNestedTwoDiff),
                new PermutationSpec(0, 1),
                cases,
                withStats);
        }
    }
} // end of namespace