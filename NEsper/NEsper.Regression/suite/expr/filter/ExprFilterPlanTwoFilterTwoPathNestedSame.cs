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
    public class ExprFilterPlanTwoFilterTwoPathNestedSame
    {
        public static IList<FilterTestMultiStmtExecution> Executions()
        {
            IList<FilterTestMultiStmtPermutable> cases = new List<FilterTestMultiStmtPermutable>();

            var stats = FilterTestMultiStmtAssertStats.MakeTwoSameStat(
                "P0=(fh:1, fi:3),P1=(fh:2, fi:3),P2=(fh:1, fi:3),P3=(fh:0, fi:0, fipar:0)");

            AddCase(
                cases,
                stats,
                "theString = 'A' and (IntPrimitive = 0 or LongPrimitive = 0)",
                "theString = 'A' and (IntPrimitive = 0 or LongPrimitive = 0)",
                MakeItem(MakeBean("A", 0, 1), true, true),
                MakeItem(MakeBean("A", 1, 0), true, true),
                MakeItem(MakeBean("A", 1, 1), false, false),
                MakeItem(MakeBean("B", 0, 0), false, false));

            // we permute only [0, 1] as all filters are the same
            return FilterTestMultiStmtRunner.ComputePermutations(
                typeof(ExprFilterPlanTwoFilterTwoPathNestedSame),
                new PermutationSpec(0, 1),
                cases);
        }
    }
} // end of namespace