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
    public class ExprFilterPlanTwoFilterDifferent
    {
        public static IList<FilterTestMultiStmtExecution> Executions(bool withStats)
        {
            IList<FilterTestMultiStmtPermutable> cases = new List<FilterTestMultiStmtPermutable>();

            var stats = FilterTestMultiStmtAssertStats.MakeTwoSameStat(
                "P0=(fh:1, fi:1),P1=(fh:2, fi:1),P2=(fh:1, fi:1),P3=(fh:0, fi:0, fipar:0)");

            // same equals-index, different value
            FilterTestMultiStmtPermutable.AddCase(
                cases,
                stats,
                "IntPrimitive = 0",
                "IntPrimitive = 1",
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E1", 0), true, false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E2", 1), false, true),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("E3", -1), false, false));

            return FilterTestMultiStmtRunner.ComputePermutations(
                typeof(ExprFilterPlanTwoFilterDifferent),
                new PermutationSpec(true),
                cases,
                withStats);
        }
    }
} // end of namespace