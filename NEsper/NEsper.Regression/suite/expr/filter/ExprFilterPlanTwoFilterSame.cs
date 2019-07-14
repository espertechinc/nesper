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
    public class ExprFilterPlanTwoFilterSame
    {
        public static IList<FilterTestMultiStmtExecution> Executions()
        {
            IList<FilterTestMultiStmtPermutable> cases = new List<FilterTestMultiStmtPermutable>();

            var stats = FilterTestMultiStmtAssertStats.MakeTwoSameStat(
                "P0=(fh:1, fi:1),P1=(fh:2, fi:1),P2=(fh:1, fi:1),P3=(fh:0, fi:0, fipar:0)");

            // same equals-index, same value
            AddCase(
                cases,
                stats,
                "IntPrimitive = 0",
                "IntPrimitive = 0",
                MakeItem(MakeBean("E1", 0), true, true),
                MakeItem(MakeBean("E2", 1), false, false));

            // boolean-index
            AddCase(
                cases,
                stats,
                "theString like 'A%'",
                "theString like 'A%'",
                MakeItem(MakeBean("A1"), true, true),
                MakeItem(MakeBean("B1"), false, false));

            return FilterTestMultiStmtRunner.ComputePermutations(
                typeof(ExprFilterPlanTwoFilterSame),
                new PermutationSpec(0, 1),
                cases);
        }
    }
} // end of namespace